using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    /// <summary>
    /// Dynamic Horizontal TabPage Component
    /// 1.tab node tree: ScrollRect / Mask / ToggleGroup(Content) / Toggle(tabitem) ...
    /// 2.Page node tree: ScrollRect / Mask / ... / ...
    /// </summary>
    public sealed class UITabPageList : UIBaseView
    {
        public UISimpleObjectPool tabPool;
        public UISimpleObjectPool pagePool;
        public Transform tabContentPanel;
        public Transform pageContentPanel;
        public Button arrowPrevious;
        public Button arrowNext;

        // tab count we can see in panel
        public int tabSizeInPanel;
        // scroll speed
        public float scrollSmoothing = 10f;

        private ToggleGroup m_TabToggleGroup;
        private ScrollRect m_TabScrollRect;
        private ScrollRect m_PageScrollRect;

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

        // default tab index, support jump to any tab page
        private int m_JumpIndex = -1;

        public int jumpIndex
        {
            get { return m_JumpIndex; }
            set { m_JumpIndex = value; }
        }

        private bool m_IsTabHorizontalScrolling;
        private bool m_IsPageHorizontalScrolling;
        private float m_TargetTabHorizontalPosition;
        private float m_TargetPageHorizontalPosition;

        private int m_CurrentPageIndex;

        private bool m_Init;

        protected override void OnStart()
        {
            // tab ToggleGroup
            m_TabToggleGroup = tabContentPanel.GetComponent<ToggleGroup>();
            m_TabToggleGroup.allowSwitchOff = false;

            // tab ScrollRect
            m_TabScrollRect = tabContentPanel.GetComponentInParent<ScrollRect>();
            m_TabScrollRect.onValueChanged.AddListener(TabRectChange);
            m_TabScrollRect.vertical = false;
            m_TabScrollRect.horizontal = true;

            // page ScrollRect
            m_PageScrollRect = pageContentPanel.GetComponentInParent<ScrollRect>();
            m_PageScrollRect.vertical = false;   // disable vertical scroll temporarily
            m_PageScrollRect.horizontal = false;

            // arrow click event & default hide arrow
            arrowPrevious.onClick.AddListener(TabPrevious);
            arrowNext.onClick.AddListener(TabNext);
            arrowPrevious.gameObject.SetActive(false);
            arrowNext.gameObject.SetActive(false);
        }

        public override void UpdateView()
        {
            base.UpdateView();

            RemoveCells();
            AddCells();
        }

        protected override void OnUpdate()
        {
            // tab scroll
            if (m_IsTabHorizontalScrolling)
            {
                if (Mathf.Abs(m_TabScrollRect.horizontalNormalizedPosition - m_TargetTabHorizontalPosition) <= 0.001f)    // Magic number based on what "feels right"
                {
                    m_TabScrollRect.horizontalNormalizedPosition = m_TargetTabHorizontalPosition;
                    m_IsTabHorizontalScrolling = false;
                }
                else
                {
                    m_TabScrollRect.horizontalNormalizedPosition = Mathf.Lerp(m_TabScrollRect.horizontalNormalizedPosition, m_TargetTabHorizontalPosition, Time.deltaTime * scrollSmoothing);
                }
            }

            // page scroll
            if (m_IsPageHorizontalScrolling)
            {
                if (Mathf.Abs(m_PageScrollRect.horizontalNormalizedPosition - m_TargetPageHorizontalPosition) <= 0.001f)
                {
                    m_PageScrollRect.horizontalNormalizedPosition = m_TargetPageHorizontalPosition;
                    m_IsPageHorizontalScrolling = false;
                }
                else
                {
                    m_PageScrollRect.horizontalNormalizedPosition = Mathf.Lerp(m_PageScrollRect.horizontalNormalizedPosition, m_TargetPageHorizontalPosition, Time.deltaTime * scrollSmoothing);
                }
            }
        }

        private void RemoveCells()
        {
            while (tabContentPanel.childCount > 0)
            {
                var toRemove = tabContentPanel.GetChild(0).gameObject;
                toRemove.GetComponent<UIBaseCell>().Recycle();
                tabPool.ReturnObject(toRemove);
            }

            while (pageContentPanel.childCount > 0)
            {
                var toRemove = pageContentPanel.GetChild(0).gameObject;
                toRemove.GetComponent<UIBaseCell>().Recycle();
                pagePool.ReturnObject(toRemove);
            }
        }

        private void AddCells()
        {
            int length = dataLength;

            for (int i = 0; i < length; i++)
            {
                var tabGo = tabPool.GetObject(i);
                tabGo.name = i.ToString();
                tabGo.transform.SetParent(tabContentPanel, false);

                var pageGo = pagePool.GetObject(i);
                pageGo.name = i.ToString();
                pageGo.transform.SetParent(pageContentPanel, false);

                var tabToggle = tabGo.GetComponent<Toggle>();

                if (tabToggle != null)
                {
                    tabToggle.group = m_TabToggleGroup;
                    tabToggle.onValueChanged.AddListener(TabOnChange);
                    tabToggle.isOn = !m_Init && i == 0;
                }

                tabGo.GetComponent<UIBaseCell>().Provide(i, m_Datas[i]);
                pageGo.GetComponent<UIBaseCell>().Provide(i, m_Datas[i]);
            }

            // jump to specific tab
            if (m_JumpIndex >= 0 && !m_Init)
            {
                iTweenUtil.CreateTimeout(gameObject, 0.5f, "UITabPageList_ScrollToTab", delegate ()
                {
                    SetTab(m_JumpIndex);
                    ScrollToTab(m_JumpIndex);
                    m_JumpIndex = -1;
                });
            }

            // forbid scroll if tab count less than tabSizeInPanel
            if (length <= tabSizeInPanel)
                m_TabScrollRect.enabled = false;
            else
                m_TabScrollRect.enabled = true;

            m_TabScrollRect.content.anchoredPosition = Vector2.zero;
            m_PageScrollRect.content.anchoredPosition = Vector2.zero;

            m_Init = length > 0;
        }

        private void TabOnChange(bool active)
        {
            if (active)
            {
                var toggles = m_TabToggleGroup.ActiveToggles().GetEnumerator();

                while (toggles.MoveNext())
                {
                    var toggle = toggles.Current as Toggle;

                    if (toggle.isOn)
                    {
                        ScrollToPage(int.Parse(toggle.gameObject.name));

                        break;
                    }
                }
            }
        }

        private void TabRectChange(Vector2 position)
        {
            if (dataLength <= tabSizeInPanel)
            {
                arrowPrevious.gameObject.SetActive(false);
                arrowNext.gameObject.SetActive(false);

                return;
            }

            int scope = dataLength - tabSizeInPanel;
            float left = 1.0f / (scope * 2.0f);
            float right = 1.0f - left;

            arrowPrevious.gameObject.SetActive(position.x >= left);
            arrowNext.gameObject.SetActive(position.x <= right);
        }

        private void ScrollToTab(int index)
        {
            if (dataLength <= tabSizeInPanel)
                return;

            if (index >= 0 && index < dataLength)
            {
                m_TargetTabHorizontalPosition = Mathf.Clamp01(index / (float)(dataLength - tabSizeInPanel));
                m_IsTabHorizontalScrolling = true;
            }
        }

        private void ScrollToPage(int index)
        {
            if (dataLength <= 1)
                return;

            if (index >= 0 && index < dataLength)
            {
                m_TargetPageHorizontalPosition = Mathf.Clamp01(index / (float)(dataLength - 1));
                m_IsPageHorizontalScrolling = true;

                m_CurrentPageIndex = index;
            }
        }

        private void SetTab(int index)
        {
            if (index >= 0 && index < tabContentPanel.childCount)
            {
                tabContentPanel.GetChild(index).GetComponent<Toggle>().isOn = true;
            }
        }

        private void TabPrevious()
        {
            if (m_CurrentPageIndex > 0)
            {
                SetTab(--m_CurrentPageIndex);

                if (m_TabScrollRect.horizontalNormalizedPosition > 0.0f)
                {
                    m_TargetTabHorizontalPosition = Mathf.Clamp01(m_TabScrollRect.horizontalNormalizedPosition - 1.0f / (dataLength - tabSizeInPanel));
                    m_IsTabHorizontalScrolling = true;
                }
            }
        }

        private void TabNext()
        {
            if (m_CurrentPageIndex < tabContentPanel.childCount)
            {
                SetTab(++m_CurrentPageIndex);

                if (m_TabScrollRect.horizontalNormalizedPosition < 1.0f)
                {
                    m_TargetTabHorizontalPosition = Mathf.Clamp01(m_TabScrollRect.horizontalNormalizedPosition + 1.0f / (dataLength - tabSizeInPanel));
                    m_IsTabHorizontalScrolling = true;
                }
            }
        }

        protected override void OnDestroy()
        {
            m_Datas = null;

            iTweenUtil.ClearTimeout(gameObject, "UITabPageList_ScrollToTab");

            base.OnDestroy();
        }
    }
}
