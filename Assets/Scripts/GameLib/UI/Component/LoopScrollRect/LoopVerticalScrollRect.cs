using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    [AddComponentMenu("UI/Loop Vertical Scroll Rect", 51)]
    [DisallowMultipleComponent]
    public class LoopVerticalScrollRect : LoopScrollRect
    {
        public enum LoadingStage
        {
            None = 0,
            MoveToBottom,
            Loading,
        }

        [SerializeField] private bool m_AutoLoading;
        [SerializeField] private float m_LoadingDistance = 30f;
        [SerializeField] private GameObject m_LoadingObject;

        private Action m_OnLoadingStart;
        private LoadingStage m_LoadingStage = LoadingStage.None;

        public void ListenLoadingStart(Action onLoadingStart)
        {
            m_OnLoadingStart = onLoadingStart;
        }

        public void ShowOrHideLoadingObject(bool show)
        {
            if (m_LoadingObject == null)
            {
                return;
            }

            m_LoadingObject.SetActive(show);
        }

        protected override void OnDataChanged()
        {
            if (m_LoadingStage == LoadingStage.Loading)
            {
                m_LoadingStage = LoadingStage.None;

                ShowOrHideLoadingObject(false);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            if (m_LoadingStage == LoadingStage.None && m_AutoLoading)
            {
                if (verticalNormalizedPosition > 1f && (content.anchoredPosition.y + viewRect.rect.height - content.rect.height) > m_LoadingDistance)
                {
                    m_LoadingStage = LoadingStage.MoveToBottom;
                }
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            if (m_LoadingStage == LoadingStage.MoveToBottom)
            {
                m_LoadingStage = LoadingStage.Loading;

                ShowOrHideLoadingObject(true);

                m_OnLoadingStart.Call();
            }
        }

        protected override float GetSize(RectTransform cell)
        {
            float size = contentSpacing;

            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.y;
            }
            else
            {
                size += LayoutUtility.GetPreferredHeight(cell);
            }

            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return vector.y;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(0f, value);
        }

        protected override void Awake()
        {
            base.Awake();

            m_DirectionSign = -1;

            GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();

            if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
            {
                Debug.LogError("[LoopVerticalScrollRect] Unsupported GridLayoutGroup constraint");
            }
        }

        protected override bool UpdateCells(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;

            if (viewBounds.min.y < contentBounds.min.y)
            {
                float size = NewCellAtEnd(), totalSize = size;

                while (size > 0f && viewBounds.min.y < contentBounds.min.y - totalSize)
                {
                    size = NewCellAtEnd();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }
            else if (viewBounds.min.y > contentBounds.min.y + m_Threshold)
            {
                if (m_LoadingStage == LoadingStage.Loading &&
                    content.childCount > 0 &&
                    m_LoadingObject == content.GetChild(content.childCount - 1).gameObject)
                {
                    ShowOrHideLoadingObject(false);
                }

                float size = DeleteCellAtEnd(), totalSize = size;

                while (size > 0f && viewBounds.min.y > contentBounds.min.y + m_Threshold + totalSize)
                {
                    size = DeleteCellAtEnd();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }

            if (viewBounds.max.y > contentBounds.max.y)
            {
                float size = NewCellAtStart(), totalSize = size;

                while (size > 0f && viewBounds.max.y > contentBounds.max.y + totalSize)
                {
                    size = NewCellAtStart();
                    totalSize += size;
                }

                if (totalSize > 0f)
                {
                    changed = true;
                }
            }
            else if (viewBounds.max.y < contentBounds.max.y - m_Threshold)
            {
                if (m_LoadingStage == LoadingStage.Loading &&
                    content.childCount > 0 &&
                    m_LoadingObject == content.GetChild(0).gameObject)
                {
                    ShowOrHideLoadingObject(false);
                }

                float size = DeleteCellAtStart(), totalSize = size;

                while (size > 0f && viewBounds.max.y < contentBounds.max.y - m_Threshold - totalSize)
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