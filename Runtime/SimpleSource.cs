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

namespace VirtualList
{
   // An interface used by the SimpleSource
   public interface IViewFor<T>
   {
      void Set(T value);
   }

   // A simple data source backed by an IList (which can be an array)
   public class SimpleSource<TData, TView> : IListSource
      where TView : Component, IViewFor<TData>
   {
      private readonly IList<TData> _list;

      public SimpleSource(IList<TData> list)
      {
         _list = list;
      }

      // Number of items
      public int Count
      {
         get
            {
               if (_list != null)
                  return _list.Count;
               else
                  return 0;
            }
      }

      public void SetItem(GameObject view, int index)
      {
         var element = _list[index];
         var display = view.GetComponent<TView>();
         display.Set(element);
      }
   }

   public class SimpleSourceWithPrefab<TData, TView> : SimpleSource<TData, TView>, IPrefabSource
      where TView : Component, IViewFor<TData>
   {
      private readonly GameObject _prefab;

      public SimpleSourceWithPrefab(IList<TData> list, GameObject prefab) : base(list)
      {
         _prefab = prefab;
      }

      public GameObject PrefabAt(int _index)
      {
         return _prefab;
      }
   }
}
