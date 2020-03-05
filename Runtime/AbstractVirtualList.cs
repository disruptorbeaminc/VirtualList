// VirtualList
//
// Zach Kamsler
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualList
{
   public abstract class AbstractVirtualList : MonoBehaviour
   {
      public ScrollRect scrollRect;
      public GameObject tilePrefab;
      public int buffer;

      private Transform _poolParent;
      private Transform PoolParent
      {
         get
         {
            if (_poolParent == null)
            {
               // Cells that are moved to the pool are reparented with the following.
               // Because the following object is not active, reparenting has the effect
               //   of disabling the component hierarchy of the cell moving to the pool.
               var go = new GameObject("PoolParent", typeof(RectTransform));
               go.SetActive(false);
               _poolParent = go.transform;
               _poolParent.SetParent(transform, false);
            }

            return _poolParent;
         }
      }

      private RectTransform _viewport;
      protected RectTransform Viewport
      {
         get
         {
            if (_viewport == null)
            {
               _viewport = scrollRect.GetComponent<RectTransform>();
            }
            return _viewport;
         }
      }

      private IListSource _source;
      private IPrefabSource _prefabSource;

      private readonly List<Cell> _pool = new List<Cell>();
      private int _poolCommits; //< number of elements that have been commited to the the pool

      // TODO: consider using more compact representation (Queue<T>?)
      private readonly Dictionary<int, Cell> _activeCells = new Dictionary<int, Cell>();

      //Track the first and last index of currently active tiles [start, end)
      protected Vector2 _activeIndices = Vector2.zero;

      private class Cell
      {
         public readonly GameObject View;
         public readonly GameObject Prefab;

         public Cell(GameObject view, GameObject prefab)
         {
            this.View = view;
            this.Prefab = prefab;
         }
      }

#region Public Methods
      // Sets the data source for the virtual list
      public void SetSource(IListSource source)
      {
         _source = source;
         _prefabSource = source as IPrefabSource;
         Invalidate();
      }

      // Removes the data source for the virtual list. Note, this will not destroy any pooled elements so they can
      // be reused on a subsequent `SetSource()` call.
      public void RemoveSource()
      {
         SetSource(null);
      }

      public void SetSourceAndCenterOn(IListSource source, int index)
      {
         _source = source;
         _prefabSource = source as IPrefabSource;

         OnInvalidate();
         scrollRect.content.anchoredPosition = GetCenteredScrollPosition(index);
         UpdateVisibilityDisjoint();
      }

      // Refreshes view
      //
      // Call if contents of source changes in a way not handled by cells
      public void Invalidate()
      {
         OnInvalidate();
         UpdateVisibilityDisjoint();
      }

      // Clears the list and destroy pooled elements
      public void Clear()
      {
         _source = null;
         _prefabSource = null;
         foreach (var pair in _activeCells)
         {
            Object.Destroy(pair.Value.View);
         }
         _activeCells.Clear();
         _activeIndices = Vector2.zero;

         if (_poolParent != null)
         {
            Object.Destroy(_poolParent.gameObject);
            _poolParent = null;
         }
         _pool.Clear();
      }

      // methods for manually iterating over visible cells
      // use with care
      public int StartIndex
      {
         get { return (int) _activeIndices.x; }
      }

      // index after the last visible index
      public int EndIndex
      {
         get { return (int) _activeIndices.y; }
      }

      public GameObject GetCell(int index)
      {
         Cell cell;
         _activeCells.TryGetValue(index, out cell);
         return cell != null ? cell.View : null;
      }
#endregion

      protected virtual void Awake()
      {
         if (scrollRect == null)
         {
            Debug.LogError("VirtualList has no ScrollRect component. Please set one via Inspector.", this);
         }

         if (tilePrefab == null)
         {
            Debug.LogError("VirtualList does not have a Tile Prefab set.", this);
         }
      }

      protected virtual void Start()
      {
         Invalidate();
         scrollRect.onValueChanged.AddListener(OnScrollbarValue);
      }

      protected abstract void OnInvalidate();
      protected abstract void PositionCell(GameObject cell, int index);
      protected abstract Vector2 CalculateRawIndices(Rect window);
      public abstract Vector2 GetCenteredScrollPosition(int index);

      private Vector2 CalculateActiveIndices()
      {
         if (_source == null)
            return Vector2.zero;

         var viewport = Viewport;
         if (viewport == null)
         {
            Debug.LogWarning("no viewport", this);
            return Vector2.zero;
         }

         var viewportSize = viewport.rect.size;
         var content = scrollRect.content;
         var anchoredPos = content.anchoredPosition;
         var sizeDelta = content.sizeDelta;
         var pivot = content.pivot;
         var viewX = -anchoredPos.x + (sizeDelta.x * pivot.x);
         var viewY = anchoredPos.y + (sizeDelta.y * (1 - pivot.y));
         var viewportPosition = new Vector2(viewX, viewY);
         var raw = CalculateRawIndices(new Rect(viewportPosition, viewportSize));

         int count = ItemCount();
         int min = Mathf.Max((int)Mathf.Min(raw.x, raw.y) - buffer, 0);
         int max = Mathf.Min((int)Mathf.Max(raw.x, raw.y) + buffer, count);
         return new Vector2(min, max);
      }

      protected void OnScrollbarValue(Vector2 scrollValue)
      {
         UpdateVisibility();
      }

      private void UpdateVisibility()
      {
         if (scrollRect == null)
            return;

         Vector2 newActiveIndices = CalculateActiveIndices();

         if (_activeIndices == newActiveIndices)
            return;

         //Special case for no overlap
         if (_activeIndices.y <= newActiveIndices.x || _activeIndices.x >= newActiveIndices.y)
         {
            UpdateVisibilityDisjoint(newActiveIndices);
         }
         else
         {
            //Deactivate first
            for (int i = (int)_activeIndices.x; i < newActiveIndices.x; i++)
               ActivateCell(i, false);
            for (int i = (int)_activeIndices.y; i >= newActiveIndices.y; i--)
               ActivateCell(i, false);
                
            //Then activate
            for (int i = (int)Mathf.Min(newActiveIndices.y, _activeIndices.x) - 1; i >= newActiveIndices.x; i--)
               ActivateCell(i, true);
            for (int i = (int)Mathf.Max(newActiveIndices.x, _activeIndices.y); i < newActiveIndices.y; i++)
               ActivateCell(i, true);

         }
         _activeIndices = newActiveIndices;
         CommitToPool();
      }

      private void UpdateVisibilityDisjoint()
      {
         Vector2 newActiveIndices = CalculateActiveIndices();
         UpdateVisibilityDisjoint(newActiveIndices);
         _activeIndices = newActiveIndices;
         CommitToPool();
      }

      private void UpdateVisibilityDisjoint(Vector2 newActiveIndices)
      {
         for (int i = (int)_activeIndices.x; i < _activeIndices.y; i++)
         {
            if(i < newActiveIndices.x || i >= newActiveIndices.y)
            {
               ActivateCell(i, false);
            }
         }

         for (int i = (int)newActiveIndices.x; i < newActiveIndices.y; i++)
         {
            ActivateCell(i, true);
         }
      }

      private GameObject PrefabAt(int index)
      {
         if(_prefabSource != null) {
            return _prefabSource.PrefabAt(index) ?? tilePrefab;
         }
         return tilePrefab;
      }

      private void ActivateCell(int index, bool activate)
      {
         Cell cell;
         _activeCells.TryGetValue(index, out cell);

         if (activate)
         {
            var prefab = PrefabAt(index);
            if(cell != null && cell.Prefab != prefab)
            {
               ReturnCellToPool(cell);
               cell = null;
            }
            if (cell == null)
            {
               cell = GetCellFromPool(prefab);
               _activeCells[index] = cell;
               PositionCell(cell.View, index);
            }
            if (_source != null)
            {
               _source.SetItem(cell.View, index);
            }
         }
         else if (cell != null)
         {
            ReturnCellToPool(cell);
            _activeCells.Remove(index);
         }
      }

      protected int ItemCount()
      {
         return _source != null ? _source.Count : 0;
      }

      private Cell GetCellFromPool(GameObject prefab)
      {
         for(int i = _pool.Count - 1; i >= 0; --i)
         {
            var cell = _pool[i];
            if(cell.Prefab == prefab)
            {
               _pool.RemoveAt(i);
               if(i < _poolCommits)
               {
                  _poolCommits -= 1;
               }
               return cell;
            }
         }
         return new Cell(Instantiate(prefab), prefab);
      }

      private void ReturnCellToPool(Cell pooledObject)
      {
         if (pooledObject == null)
            return;

         _pool.Add(pooledObject);
      }

      private void CommitToPool()
      {
         var parent = PoolParent;
         for (int i = _poolCommits; i < _pool.Count; ++i)
         {
            var pooledObject = _pool[i].View;

            // @see the comments in the getter for PoolParent. The following
            //   effectively disables the component hierarchy of pooledObject.
            pooledObject.transform.SetParent(parent, false);
         }
         _poolCommits = _pool.Count;
      }

   #if UNITY_EDITOR
      public void PreviewLayout()
      {
         if(Application.isPlaying || tilePrefab == null) return;

         var trans = this.transform;
         while (trans.childCount > 0)
         {
            Transform t = trans.GetChild(0);
            t.SetParent(null);
            GameObject.DestroyImmediate(t.gameObject);
         }

         OnInvalidate();

         for (int i = 0; i < 20; i++)
         {
            var cell = Instantiate(tilePrefab);
            cell.name = "Temporary Preview";
            cell.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            PositionCell(cell, i);
         }
      }
   #endif
   }
}
