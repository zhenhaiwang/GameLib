using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public class UIScrollList : UIBaseList
    {
        public ScrollRect scroll;
        public float scrollSmoothing = 8f;
        public int visibleCount = 1;

        private bool m_IsHorizantal;
        private bool m_IsVertical;
        private bool m_IsAutoScrolling;

        private float m_HorizantalTargetPos = 1f;
        private float m_VerticalTargetPos = 1f;

        private int m_Length = 1;
        private float m_StepLength;

        protected override void OnStart()
        {
            m_IsHorizantal = scroll.horizontal;
            m_IsVertical = scroll.vertical;
        }

        protected override void OnDataChanged()
        {
            m_Length = datas.Length();

            if (m_Length > visibleCount)
            {
                m_StepLength = 1f / (m_Length - visibleCount);
            }
            else
            {
                m_StepLength = 0f;
            }
        }

        protected override void OnUpdate()
        {
            if (m_IsAutoScrolling)
            {
                if (m_IsVertical)
                {
                    if (Mathf.Abs(scroll.verticalNormalizedPosition - m_VerticalTargetPos) <= 0.001f)
                    {
                        scroll.verticalNormalizedPosition = m_VerticalTargetPos;
                        m_IsAutoScrolling = false;
                    }
                    else
                    {
                        scroll.verticalNormalizedPosition = Mathf.Lerp(scroll.verticalNormalizedPosition, m_VerticalTargetPos, Time.deltaTime * scrollSmoothing);
                    }
                }

                if (m_IsHorizantal)
                {
                    if (Mathf.Abs(scroll.horizontalNormalizedPosition - m_HorizantalTargetPos) <= 0.001f)
                    {
                        scroll.horizontalNormalizedPosition = m_HorizantalTargetPos;
                        m_IsAutoScrolling = false;
                    }
                    else
                    {
                        scroll.horizontalNormalizedPosition = Mathf.Lerp(scroll.horizontalNormalizedPosition, m_HorizantalTargetPos, Time.deltaTime * scrollSmoothing);
                    }
                }
            }
        }

        public void ScrollToTopSmooth()
        {
            m_IsAutoScrolling = true;
            m_VerticalTargetPos = 1f;
            m_HorizantalTargetPos = 0f;
        }

        public void ScrollToTopImmediate()
        {
            if (m_IsVertical)
                scroll.verticalNormalizedPosition = 1f;
            if (m_IsHorizantal)
                scroll.horizontalNormalizedPosition = 0f;
        }

        public void ScrollToBottomSmooth()
        {
            m_IsAutoScrolling = true;
            m_VerticalTargetPos = 0f;
            m_HorizantalTargetPos = 1f;
        }

        public void ScrollToBottomImmediate()
        {
            if (m_IsVertical)
                scroll.verticalNormalizedPosition = 0f;
            if (m_IsHorizantal)
                scroll.horizontalNormalizedPosition = 1f;
        }

        public void ScrollToIndex(int index, bool immediate = false)
        {
            if (index <= 0)
            {
                if (immediate)
                {
                    if (m_IsVertical)
                        scroll.verticalNormalizedPosition = 1f;
                    if (m_IsHorizantal)
                        scroll.horizontalNormalizedPosition = 0f;
                }
                else
                {
                    m_IsAutoScrolling = true;
                    m_VerticalTargetPos = 1f;
                    m_HorizantalTargetPos = 0f;
                }
            }
            else if (index >= m_Length - 1)
            {
                if (immediate)
                {
                    if (m_IsVertical)
                        scroll.verticalNormalizedPosition = 0f;
                    if (m_IsHorizantal)
                        scroll.horizontalNormalizedPosition = 1f;
                }
                else
                {
                    m_IsAutoScrolling = true;
                    m_VerticalTargetPos = 0f;
                    m_HorizantalTargetPos = 1f;
                }
            }
            else if (m_Length > 0)
            {
                int clampIndex = Mathf.Clamp(index, 0, m_Length - visibleCount);

                if (immediate)
                {
                    if (m_IsVertical)
                        scroll.verticalNormalizedPosition = 1f - clampIndex * m_StepLength;
                    if (m_IsHorizantal)
                        scroll.horizontalNormalizedPosition = clampIndex * m_StepLength;
                }
                else
                {
                    m_IsAutoScrolling = true;
                    m_VerticalTargetPos = 1f - clampIndex * m_StepLength;
                    m_HorizantalTargetPos = clampIndex * m_StepLength;
                }
            }
        }
    }
}