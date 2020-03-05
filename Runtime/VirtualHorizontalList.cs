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

using UnityEngine;

namespace VirtualList
{
   public class VirtualHorizontalList : AbstractVirtualList
   {
      public RectOffset padding;
      public float cellSize;
      public float spacing;

      protected override void OnInvalidate()
      {
         RecalculateSize();
      }

      private void RecalculateSize()
      {
         int primary = ItemCount();
         float size = padding.horizontal + cellSize * primary + Mathf.Max(0, primary - 1) * spacing;
         scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
      }

      protected override void PositionCell(GameObject cell, int index)
      {
         var trans = cell.GetComponent<RectTransform>();
         trans.SetParent(scrollRect.content, false);

         float primaryPos = index * (cellSize + spacing) + padding.left;
         
         trans.anchorMin = new Vector2(0, 0); // bottom-left
         trans.anchorMax = new Vector2(0, 1); // bottom-right
         trans.sizeDelta = new Vector2(cellSize, -padding.vertical);
         trans.pivot = new Vector2(0f, 1f); // anchor to bottom-right
         trans.anchoredPosition = new Vector2(primaryPos, padding.top);
      }

      protected override Vector2 CalculateRawIndices(Rect window)
      {
         var pos = window.position;
         var size = window.size;

         const int axis = 0;
         float pad = padding.left;
         float lowestPosVisible = pos[axis] - pad;
         float highestPosVisible = pos[axis] + size[axis] + cellSize - pad;
         float colSize = cellSize + spacing;

         int min = (int)(lowestPosVisible / colSize);
         int max = (int)(highestPosVisible / colSize);
         return new Vector2(min, max);
      }

      public override Vector2 GetCenteredScrollPosition(int index)
      {
         float primaryPos = -(index * (cellSize + spacing) + padding.left);
         var rect = Viewport.rect;
         float offset = primaryPos + ((rect.size.x- cellSize) * 0.5f);
         return new Vector2(Mathf.Clamp(offset, -Mathf.Max(0,scrollRect.content.rect.size.x - rect.size.x), 0), 0);
      }
   }
}
