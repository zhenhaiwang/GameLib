using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    public sealed class UIDotPageList : UIScrollPage
    {
        public UISimpleObjectPool pageObjectPool;
        public UISimpleObjectPool dotObjectPool;
        public Transform dotContent;

        private ToggleGroup m_DotToggleGroup;

        private object[] m_Datas;

        public object[] datas
        {
            set
            {
                m_Datas = value;

                InvalidView();
            }
        }

        public int dataLength
        {
            get { return m_Datas.Length(); }
        }

        protected override void OnStart()
        {
            m_DotToggleGroup = dotContent.GetComponent<ToggleGroup>();
            m_DotToggleGroup.allowSwitchOff = false;
        }

        public override void UpdateView()
        {
            base.UpdateView();

            RemoveCells();
            AddCells();
        }

        private void RemoveCells()
        {
            while (currentPageCount > 0)
            {
                var toRemove = pageContent.GetChild(0).gameObject;
                toRemove.GetComponent<UIBaseCell>().Recycle();
                pageObjectPool.ReturnObject(toRemove);
            }

            while (dotContent.childCount > 0)
            {
                var toRemove = dotContent.GetChild(0).gameObject;
                toRemove.GetComponent<UIBaseCell>().Recycle();
                dotObjectPool.ReturnObject(toRemove);
            }
        }

        private void AddCells()
        {
            int length = dataLength;

            for (int i = 0; i < length; i++)
            {
                var pageGo = pageObjectPool.GetObject(i);
                pageGo.name = i.ToString();
                pageGo.transform.SetParent(pageContent, false);

                var dotGo = dotObjectPool.GetObject(i);
                dotGo.name = i.ToString();
                dotGo.transform.SetParent(dotContent, false);

                var dotToggle = dotGo.GetComponent<Toggle>();

                if (dotToggle != null)
                {
                    dotToggle.group = m_DotToggleGroup;
                    dotToggle.isOn = i == 0 ? true : false;
                }

                pageGo.GetComponent<UIBaseCell>().Provide(i, m_Datas[i]);
            }

            dotContent.gameObject.SetActive(length > 1);

            pageScrollRect.enabled = length > 1;
            pageScrollRect.content.anchoredPosition = Vector2.zero;
        }

        private void SetDotToggle(int index)
        {
            if (index >= 0 && index < dotContent.childCount)
            {
                dotContent.GetChild(index).GetComponent<Toggle>().isOn = true;
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            SetDotToggle(currentPageIndex);
        }
    }
}
