using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    public sealed class UITabPage : UIScrollPage
    {
        public Toggle[] toggles;

        protected override void OnAwake()
        {
            int length = toggles.Length();

            for (int i = 0; i < length; i++)
            {
                toggles[i].onValueChanged.AddListener(OnTabChange);
            }
        }

        /// <summary>
        /// jump to target tab and page immediately
        /// </summary>
        public void SetTab(int index)
        {
            if (toggles != null &&
                index > 0 &&
                index < toggles.Length &&
                index < currentPageCount)
            {
                toggles[index].isOn = true;

                base.SetPage(index);
            }
        }

        /// <summary>
        /// scroll to target tab and page
        /// </summary>
        public void ScrollToTab(int index)
        {
            if (toggles != null &&
                index > 0 &&
                index < toggles.Length &&
                index < currentPageCount)
            {
                toggles[index].isOn = true;

                base.ScrollToPage(index);
            }
        }

        /// <summary>
        /// scroll to target tab and page, after delay time
        /// </summary>
        public void ScrollToTab(int index, float delay, Action callback = null)
        {
            if (toggles != null &&
                index > 0 &&
                index < toggles.Length &&
                index < currentPageCount &&
                delay > 0.0f)
            {
                if (delay > 0.0f)
                {
                    iTweenUtil.CreateTimeout(gameObject, delay, "UITabPage_ScrollToTab", delegate ()
                    {
                        ScrollToTab(index);

                        callback.Call();
                    });
                }
                else
                {
                    ScrollToTab(index);
                }
            }
        }

        private void OnTabChange(bool value)
        {
            if (value && toggles != null)
            {
                int length = toggles.Length;
                for (int i = 0; i < length; i++)
                {
                    if (toggles[i].isOn && i < currentPageCount)
                    {
                        base.ScrollToPage(i);

                        break;
                    }
                }
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            toggles[currentPageIndex].isOn = true;
        }

        /// <summary>
        /// do nothing, this feature is not supported
        /// </summary>
        public override void ScrollToPage(int index) { }

        /// <summary>
        /// do nothing, please use SetTab function instead
        /// </summary>
        public override void SetPage(int index) { }

        /// <summary>
        /// do nothing, this feature is not supported
        /// </summary>
        public override void ScrollToPage(int index, float delay, Action callback = null) { }
    }
}
