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
   public class VirtualGridList : AbstractVirtualList
   {
      public enum Axis { Horizontal = 0, Vertical = 1 }

      public RectOffset padding;
      public Axis axis;
      public Vector2 cellSize = new Vector2(100, 100);
      public Vector2 spacing;
      public int limit = 1;
      private int _axis;

      protected override void OnInvalidate()
      {
         _axis = (int)axis;
         RecalculateSize();
      }

      private void RecalculateSize()
      {
         int primary = Mathf.CeilToInt(ItemCount() / (float)limit);
         int otherAxis = 1 - _axis;

         Vector2 size = Vector2.zero;
         size[_axis] = cellSize[_axis] * primary + Mathf.Max(0, primary - 1) * spacing[_axis];
         size[otherAxis] = cellSize[otherAxis] * limit + Mathf.Min(0, limit - 1) * spacing[otherAxis];
         size.x += padding.horizontal;
         size.y += padding.vertical;
         scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
         scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      }

      protected override void PositionCell(GameObject cell, int index)
      {
         var trans = cell.GetComponent<RectTransform>();
         trans.SetParent(scrollRect.content, false);

         int otherAxis = 1 - _axis;
         int primary = index / limit;
         int secondary = index % limit;

         float primaryPos = primary * (cellSize[_axis] + spacing[_axis]) + PaddingForAxis(_axis);
         float secondaryPos = secondary * (cellSize[otherAxis] + spacing[otherAxis]) + PaddingForAxis(otherAxis);

         trans.SetInsetAndSizeFromParentEdge(EdgeForAxis(_axis), primaryPos, cellSize[_axis]);
         trans.SetInsetAndSizeFromParentEdge(EdgeForAxis(otherAxis), secondaryPos, cellSize[otherAxis]);
      }

      private float PaddingForAxis(int ax)
      {
         return ax == 0 ? padding.left : padding.top;
      }

      private RectTransform.Edge EdgeForAxis(int ax)
      {
         return ax == 1 ? RectTransform.Edge.Top : RectTransform.Edge.Left;
      }

      protected override Vector2 CalculateRawIndices(Rect window)
      {
         var pos = window.position;
         var size = window.size;

         float pad = PaddingForAxis(_axis);
         float lowestPosVisible = pos[_axis] - pad;
         float highestPosVisible = pos[_axis] + size[_axis] + cellSize[_axis] - pad;
         float rowSize = cellSize[_axis] + spacing[_axis];

         int min = limit * RowAtPos(lowestPosVisible, rowSize);
         int max = limit * RowAtPos(highestPosVisible, rowSize);
         return new Vector2(min, max);
      }

      private int RowAtPos(float pos, float rowSize)
      {
         return (int)(pos / rowSize);
      }

      public override Vector2 GetCenteredScrollPosition(int index)
      {
         int primary = index / limit;
         float primaryPos = primary * (cellSize[_axis] + spacing[_axis]) + PaddingForAxis(_axis);

         float offset = primaryPos - ((Viewport.rect.size[_axis] - cellSize[_axis]) * 0.5f);
         offset = Mathf.Clamp(offset, 0, scrollRect.content.rect.size[_axis] - Viewport.rect.size[_axis]);

         if (axis == Axis.Vertical)
         {
            return new Vector2(0, offset);
         }
         else
         {
            return new Vector2(-offset, 0);
         }
      }
   }
}
