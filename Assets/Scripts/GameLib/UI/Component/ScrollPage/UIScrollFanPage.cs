using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    public sealed class UIScrollFanPage : UIBaseView, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public RectTransform content;
        public UISimpleObjectPool objectPool;

        public float deltaEulerAngle;

        public float inscribedPolygonWidth;
        public float inscribedPolygonHeight;

        public float scrollSmoothing = 8f;
        public float nextFanThreshold = 0.3f;

        private float m_Radius;

        private float m_ScreenRatio = 1f;

        private float m_TargetEulerAngle;

        private float m_BeginDragEulerAngle;
        private float m_BeginDragTime;

        private bool m_IsAutoScrolling;

        private List<GameObject> m_FanPageList = new List<GameObject>();

        private int m_CurrentIndex;

        private int currentIndex
        {
            get
            {
                return m_CurrentIndex;
            }
            set
            {
                m_CurrentIndex = value;

                CalculatePageEnabled();
            }
        }

        private float deltaRadianAngle
        {
            get { return Mathf.Deg2Rad * deltaEulerAngle; }
        }

        private object[] m_Datas;

        public object[] datas
        {
            get
            {
                return m_Datas;
            }
            set
            {
                m_Datas = value;

                InvalidView();
            }
        }

        protected override void OnAwake()
        {
            if (content != null)
            {
                CalculateScreenRatio();
                CalculateRadius();
                CalculateContentRect();
            }
        }

        protected override void OnUpdate()
        {
            if (m_IsAutoScrolling)
            {
                if (Mathf.Abs(content.localEulerAngles.z - m_TargetEulerAngle) <= 0.001f)
                {
                    m_IsAutoScrolling = false;
                    content.localEulerAngles = new Vector3(0f, 0f, m_TargetEulerAngle);
                }
                else
                {
                    content.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(content.localEulerAngles.z, m_TargetEulerAngle, Time.deltaTime * scrollSmoothing));
                }
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();
            RemovePages();
            AddPages();
        }

        private void AddPages()
        {
            m_FanPageList.Clear();

            int length = m_Datas.Length();

            for (int i = 0; i < length; i++)
            {
                var pageData = m_Datas[i];
                var pageGo = objectPool.GetObject(i);
                pageGo.transform.SetParent(content, false);

                m_FanPageList.Add(pageGo);

                pageGo.GetComponent<UIBaseCell>().Provide(i, pageData);
            }

            StartCoroutine(CalculatePagePositionAndAngle());
        }

        private void RemovePages()
        {
            while (content.childCount > 0)
            {
                var toRemove = content.transform.GetChild(0).gameObject;
                toRemove.GetComponent<UIBaseCell>().Recycle();
                objectPool.ReturnObject(toRemove);
            }
        }

        private void CalculateScreenRatio()
        {
            var scaler = GetComponentInParent<CanvasScaler>();

            if (scaler != null)
            {
                m_ScreenRatio = Screen.width / scaler.referenceResolution.x;
            }
        }

        private IEnumerator CalculatePagePositionAndAngle()
        {
            yield return null;

            int length = m_FanPageList.Count();

            for (int i = 0; i < length; i++)
            {
                var rect = m_FanPageList[i].GetComponent<RectTransform>();

                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(m_Radius * Mathf.Sin(deltaRadianAngle * i), m_Radius * Mathf.Cos(deltaRadianAngle * i));
                    rect.localEulerAngles = new Vector3(0f, 0f, -deltaEulerAngle * i);
                }
            }

            CalculatePageEnabled();
        }

        private void CalculatePageEnabled()
        {
            int length = m_FanPageList.Count();

            for (int i = 0; i < length; i++)
            {
                m_FanPageList[i].SetActive(i >= m_CurrentIndex - 1 && i <= m_CurrentIndex + 1);
            }
        }

        private void CalculateRadius()
        {
            if (deltaEulerAngle > 0f &&
                inscribedPolygonWidth > 0f &&
                inscribedPolygonHeight > 0f)
            {
                m_Radius = inscribedPolygonWidth / (2f * Mathf.Tan(deltaRadianAngle / 2f)) + inscribedPolygonHeight / 2f;
            }
            else
            {
                m_Radius = 0f;
            }
        }

        private void CalculateContentRect()
        {
            if (content != null && m_Radius > 0f)
            {
                content.anchorMin = content.anchorMax = content.pivot = new Vector2(0.5f, 0.5f);
                content.anchoredPosition = new Vector2(0f, -m_Radius);
                content.localEulerAngles = Vector2.zero;
                content.sizeDelta = new Vector2(m_Radius * 2f + inscribedPolygonHeight, m_Radius * 2f + inscribedPolygonHeight);
            }
        }

        private bool CheckDraggable()
        {
            return m_Radius > 0 && m_FanPageList != null && m_FanPageList.Count > 1;
        }

        public void SetPage(int index)
        {
            if (m_Datas != null && index >= 0 && index < m_Datas.Length)
            {
                currentIndex = index;
                m_TargetEulerAngle = m_CurrentIndex * deltaEulerAngle;
                content.localEulerAngles = new Vector3(0f, 0f, m_TargetEulerAngle);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CheckDraggable())
            {
                m_IsAutoScrolling = false;
                m_BeginDragTime = Time.unscaledTime;
                m_BeginDragEulerAngle = content.localEulerAngles.z;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (CheckDraggable() && !m_IsAutoScrolling)
            {
                float dragOffset = Mathf.Clamp((eventData.position.x - eventData.pressPosition.x) / m_ScreenRatio, -inscribedPolygonWidth, inscribedPolygonWidth);
                float deltaAngle = Mathf.Clamp01(Mathf.Abs(dragOffset) / inscribedPolygonWidth) * deltaEulerAngle;
                content.localEulerAngles = new Vector3(0f, 0f, dragOffset > 0f ? m_BeginDragEulerAngle - deltaAngle : m_BeginDragEulerAngle + deltaAngle);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CheckDraggable() && !m_IsAutoScrolling)
            {
                float speed = (eventData.position.x - eventData.pressPosition.x) / ((Time.unscaledTime - m_BeginDragTime) * 1000f);

                if (speed >= nextFanThreshold && currentIndex > 0)
                {
                    m_TargetEulerAngle = --currentIndex * deltaEulerAngle;
                }
                else if (speed <= -nextFanThreshold && currentIndex < m_FanPageList.Count - 1)
                {
                    m_TargetEulerAngle = ++currentIndex * deltaEulerAngle;
                }

                m_IsAutoScrolling = true;

                m_BeginDragEulerAngle = 0f;
                m_BeginDragTime = 0f;
            }
        }
    }
}
