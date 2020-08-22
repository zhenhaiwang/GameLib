using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    [AddComponentMenu("UI/Loop Horizontal Scroll Rect", 50)]
    [DisallowMultipleComponent]
    public class LoopHorizontalScrollRect : LoopScrollRect
    {
        protected override float GetSize(RectTransform cell)
        {
            float size = contentSpacing;

            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.x;
            }
            else
            {
                size += LayoutUtility.GetPreferredWidth(cell);
            }

            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return -vector.x;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(-value, 0f);
        }

        protected override void Awake()
        {
            base.Awake();

            m_DirectionSign = 1;

            GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();

            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
            {
                Debug.LogError("[LoopHorizontalScrollRect] Unsupported GridLayoutGroup constraint");
            }
        }

        protected override bool UpdateCells(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            if (viewBounds.max.x > contentBounds.max.x - padding.right)
            {
                float size = NewCellAtEnd(), totalSize = size;

                while (size > 0f && viewBounds.max.x > contentBounds.max.x - padding.right + totalSize)
                {
                    size = NewCellAtEnd();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }
            else if (viewBounds.max.x < contentBounds.max.x - padding.right - m_Threshold)
            {
                float size = DeleteCellAtEnd(), totalSize = size;

                while (size > 0f && viewBounds.max.x < contentBounds.max.x - padding.right - m_Threshold - totalSize)
                {
                    size = DeleteCellAtEnd();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }

            if (viewBounds.min.x < contentBounds.min.x + padding.left)
            {
                float size = NewCellAtStart(), totalSize = size;

                while (size > 0f && viewBounds.min.x < contentBounds.min.x + padding.left - totalSize)
                {
                    size = NewCellAtStart();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }
            else if (viewBounds.min.x > contentBounds.min.x + padding.left + m_Threshold)
            {
                float size = DeleteCellAtStart(), totalSize = size;

                while (size > 0f && viewBounds.min.x > contentBounds.min.x + padding.left + m_Threshold + totalSize)
                {
                    size = DeleteCellAtStart();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }

            return changed;
        }
    }
}