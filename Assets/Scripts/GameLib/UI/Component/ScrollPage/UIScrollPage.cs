using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLib
{
    public class UIScrollPage : UIBaseView, IBeginDragHandler, IEndDragHandler
    {
        public delegate void PageChangeDelegate(int currentPageIndex);

        public enum Direction { Vertical, Horizontal }

        // page direction, from left to right, or from top to bottom
        public Direction direction = Direction.Horizontal;

        // page ScrollRect component
        public ScrollRect pageScrollRect;

        // content transform (item container)
        public Transform pageContent;

        // the smoothness when auto scrolling to the next page
        public float scrollSmoothing = 8f;

        // the minimum sliding speed determining whether auto scroll to the next page or not
        public float nextPageThreshold = 0.3f;

        // enable drag scrolling to next page
        public bool dragScrollEnabled = true;

        // page OnValueChange delegate
        public PageChangeDelegate OnPageChange;

        // page current index
        protected int currentPageIndex
        {
            get
            {
                return m_CurrentPageIndex;
            }
            set
            {
                m_CurrentPageIndex = value;

                if (OnPageChange != null)
                {
                    OnPageChange(value);
                }
            }
        }

        // page total count
        protected int currentPageCount
        {
            get { return pageContent.childCount; }
        }

        private int m_CurrentPageIndex;

        private bool m_IsAutoScrolling;

        private float m_TargetPosition;

        private float m_BeginDragTime;

        protected override void OnStart()
        {
            EnableDragScroll(dragScrollEnabled);
        }

        protected override void OnUpdate()
        {
            if (!m_IsAutoScrolling)
            {
                return;
            }

            switch (direction)
            {
                case Direction.Vertical:
                    if (Mathf.Abs(pageScrollRect.verticalNormalizedPosition - m_TargetPosition) <= 0.001f) // Magic number based on what "feels right"
                    {
                        pageScrollRect.verticalNormalizedPosition = m_TargetPosition;
                        m_IsAutoScrolling = false;
                    }
                    else
                    {
                        pageScrollRect.verticalNormalizedPosition = Mathf.Lerp(pageScrollRect.verticalNormalizedPosition, m_TargetPosition, Time.deltaTime * scrollSmoothing);
                    }
                    break;
                case Direction.Horizontal:
                    if (Mathf.Abs(pageScrollRect.horizontalNormalizedPosition - m_TargetPosition) <= 0.001f) // Magic number based on what "feels right"
                    {
                        pageScrollRect.horizontalNormalizedPosition = m_TargetPosition;
                        m_IsAutoScrolling = false;
                    }
                    else
                    {
                        pageScrollRect.horizontalNormalizedPosition = Mathf.Lerp(pageScrollRect.horizontalNormalizedPosition, m_TargetPosition, Time.deltaTime * scrollSmoothing);
                    }
                    break;
            }
        }

        /// <summary>
        /// enable or disable drag scrolling to next page
        /// </summary>
        public void EnableDragScroll(bool enabled)
        {
            dragScrollEnabled = enabled;
            pageScrollRect.vertical = direction == Direction.Vertical && dragScrollEnabled;
            pageScrollRect.horizontal = direction == Direction.Horizontal && dragScrollEnabled;
        }

        /// <summary>
        /// jump to target page immediately
        /// </summary>
        public virtual void SetPage(int index)
        {
            if (index >= 0 && index < currentPageCount)
            {
                currentPageIndex = index;

                switch (direction)
                {
                    case Direction.Vertical:
                        pageScrollRect.verticalNormalizedPosition = Mathf.Clamp01(index / (float)(currentPageCount - 1));
                        break;
                    case Direction.Horizontal:
                        pageScrollRect.horizontalNormalizedPosition = Mathf.Clamp01(index / (float)(currentPageCount - 1));
                        break;
                }
            }
        }

        /// <summary>
        /// scroll to target page
        /// </summary>
        public virtual void ScrollToPage(int index)
        {
            if (index >= 0 && index < currentPageCount)
            {
                currentPageIndex = index;

                m_IsAutoScrolling = true;
                m_TargetPosition = Mathf.Clamp01(index / (float)(currentPageCount - 1));
            }
        }

        /// <summary>
        /// scroll to target page, after delay time
        /// </summary>
        public virtual void ScrollToPage(int index, float delay, Action callback = null)
        {
            if (index >= 0 && index < currentPageCount)
            {
                if (delay > 0.0f)
                {
                    iTweenUtil.CreateTimeout(gameObject, delay, "UIScrollPage_ScrollToPage", delegate ()
                    {
                        ScrollToPage(index);
                        callback.Call();
                    });
                }
                else
                {
                    ScrollToPage(index);
                }
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            EnableDragScroll(dragScrollEnabled);

            if (dragScrollEnabled)
            {
                m_IsAutoScrolling = false;
                m_BeginDragTime = Time.unscaledTime;
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (currentPageCount <= 1 || m_IsAutoScrolling)
                return;

            m_IsAutoScrolling = true;

            float speed = (eventData.position.x - eventData.pressPosition.x) / ((Time.unscaledTime - m_BeginDragTime) * 1000.0f);

            if (speed >= nextPageThreshold && currentPageIndex > 0)
            {
                m_TargetPosition = Mathf.Clamp01(--currentPageIndex / (float)(currentPageCount - 1));
            }
            else if (speed <= -nextPageThreshold && currentPageIndex < currentPageCount - 1)
            {
                m_TargetPosition = Mathf.Clamp01(++currentPageIndex / (float)(currentPageCount - 1));
            }
        }

        protected override void OnDestroy()
        {
            iTweenUtil.ClearTimeout(gameObject, "UIScrollPage_ScrollToPage");

            base.OnDestroy();
        }
    }
}
