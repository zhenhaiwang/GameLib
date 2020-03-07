using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    /// <summary>
    /// 继承自 UGUI ScrollRect Component
    /// 适用场景: 组合ScrollRect页面，Child-ScrollRect(this) 垂直/水平滚动，而 Parent-ScrollRect 水平/垂直滚动
    /// 主要功能: DragEvents 透传
    /// </summary>
    public sealed class UIChildScrollRect : ScrollRect
    {
        private ScrollRect m_ParentScrollRect;
        private bool m_FireToParent;

        protected override void Start()
        {
            base.Start();

            // can't do this in Awake function when working with UISimpleObjectPool component
            if (!(m_ParentScrollRect = transform.parent.GetComponentInParent<ScrollRect>()))
            {
                Log.Error("Get null when getting parent ScrollRect");
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            m_FireToParent = IsFireToParentScrollRect(eventData);

            if (m_FireToParent)
            {
                m_ParentScrollRect.OnBeginDrag(eventData);
                m_ParentScrollRect.SendMessage("OnBeginDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (m_FireToParent)
            {
                m_ParentScrollRect.OnDrag(eventData);
                m_ParentScrollRect.SendMessage("OnDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                base.OnDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            if (m_FireToParent)
            {
                m_ParentScrollRect.OnEndDrag(eventData);
                m_ParentScrollRect.SendMessage("OnEndDrag", eventData, SendMessageOptions.DontRequireReceiver);
            }
        }

        private bool IsFireToParentScrollRect(PointerEventData eventData)
        {
            if (m_ParentScrollRect == null)
                return false;

            if (!m_ParentScrollRect.vertical && !m_ParentScrollRect.horizontal)
                return false;

            if (!vertical && !horizontal)
            {
                return true;
            }
            else
            {
                if (horizontal)
                {
                    return Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);
                }
                else // if (vertical)
                {
                    return Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
                }
            }
        }
    }
}
