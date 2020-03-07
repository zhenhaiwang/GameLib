using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameLib
{
    [AddComponentMenu("")]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        // *** patch --- //

        [Tooltip("Object pool, Prefab or GameObject both work")]
        [SerializeField]
        protected UIObjectPool m_ObjectPool;

        [Tooltip("Reverse direction for dragging")]
        [SerializeField]
        protected bool m_ReverseDirection;

        [Tooltip("Rubber scale for outside")]
        [SerializeField]
        protected float m_RubberScale = 1f;

        [Tooltip("Threshold for preloading")]
        [SerializeField]
        protected float m_Threshold = 100f;

        protected const float EPSILON = 0.01f;

        protected int m_DirectionSign;
        protected bool m_Draggable = true;

        protected int m_CellIndexStart;
        public int cellIndexStart { get { return m_CellIndexStart; } }

        protected int m_CellIndexEnd;
        public int cellIndexEnd { get { return m_CellIndexEnd; } }

        protected int m_TotalCount;
        public int totalCount { get { return m_TotalCount; } }

        [NonSerialized] private ArrayList m_Datas;
        [NonSerialized] private MonoBehaviour m_ParentView;

        protected GridLayoutGroup m_GridLayout;

        private float m_ContentSpacing = -1f;
        public float contentSpacing
        {
            get
            {
                if (m_ContentSpacing >= 0f)
                {
                    return m_ContentSpacing;
                }

                m_ContentSpacing = 0f;

                if (content != null)
                {
                    HorizontalOrVerticalLayoutGroup layout = content.GetComponent<HorizontalOrVerticalLayoutGroup>();

                    if (layout != null)
                    {
                        m_ContentSpacing = layout.spacing;
                    }

                    m_GridLayout = content.GetComponent<GridLayoutGroup>();

                    if (m_GridLayout != null)
                    {
                        m_ContentSpacing = Mathf.Abs(GetDimension(m_GridLayout.spacing));
                    }
                }

                return m_ContentSpacing;
            }
        }

        private int m_ContentConstraintCount;
        public int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCount > 0)
                {
                    return m_ContentConstraintCount;
                }

                m_ContentConstraintCount = 1;

                if (content != null)
                {
                    m_GridLayout = content.GetComponent<GridLayoutGroup>();

                    if (m_GridLayout != null)
                    {
                        if (m_GridLayout.constraint == GridLayoutGroup.Constraint.Flexible)
                        {
                            Log.Error("Flexible not supported yet");
                        }

                        m_ContentConstraintCount = m_GridLayout.constraintCount;
                    }
                }

                return m_ContentConstraintCount;
            }
        }

        public int gridStartLineIndex
        {
            get
            {
                return Mathf.CeilToInt((float)m_CellIndexStart / contentConstraintCount);
            }
        }

        public int gridViewLineCount
        {
            get
            {
                return Mathf.CeilToInt((float)(m_CellIndexEnd - m_CellIndexStart) / contentConstraintCount);
            }
        }

        public int gridTotalLineCount
        {
            get
            {
                return Mathf.CeilToInt((float)m_TotalCount / contentConstraintCount);
            }
        }

        protected abstract float GetSize(RectTransform cell);
        protected abstract float GetDimension(Vector2 vector);
        protected abstract Vector2 GetVector(float value);

        protected virtual bool UpdateCells(Bounds viewBounds, Bounds contentBounds) { return false; }

        // --- patch *** //

        public enum MovementType
        {
            Unrestricted,   // Unrestricted movement. The content can move forever.
            Elastic,        // Elastic movement. The content is allowed to temporarily move beyond the container, but is pulled back elastically.
            Clamped,        // Clamped movement. The content can not be moved beyond its container.
        }

        public enum ScrollbarVisibility
        {
            Permanent,                  // Always show the scrollbar.
            AutoHide,                   // Automatically hide the scrollbar when no scrolling is needed on this axis. The viewport rect will not be changed.
            AutoHideAndExpandViewport,  // Automatically hide the scrollbar when no scrolling is needed on this axis, and expand the viewport rect accordingly.
                                        // When this setting is used, the scrollbar and the viewport rect become driven, meaning that values in the RectTransform are calculated automatically and can't be manually edited.
        }

        [Serializable] public class ScrollRectEvent : UnityEvent<Vector2> { }

        [SerializeField] private RectTransform m_Content;
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }

        [SerializeField] private bool m_Horizontal = true;
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }

        [SerializeField] private bool m_Vertical = true;
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }

        [SerializeField] private MovementType m_MovementType = MovementType.Elastic;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }

        // Only used for MovementType.Elastic
        [SerializeField] private float m_Elasticity = 0.1f;
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

        [SerializeField] private bool m_Inertia = true;
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }

        // Only used when inertia is enabled
        [SerializeField] private float m_DecelerationRate = 0.135f;
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

        [SerializeField] private float m_ScrollSensitivity = 1.0f;
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }

        [SerializeField] private RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }

        [SerializeField] private Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);

                m_HorizontalScrollbar = value;

                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);

                SetDirtyCaching();
            }
        }

        [SerializeField] private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

                m_VerticalScrollbar = value;

                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

                SetDirtyCaching();
            }
        }

        [SerializeField] private ScrollbarVisibility m_HorizontalScrollbarVisibility;
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField] private ScrollbarVisibility m_VerticalScrollbarVisibility;
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }

        [SerializeField] private float m_HorizontalScrollbarSpacing;
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField] private float m_VerticalScrollbarSpacing;
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }

        [SerializeField] private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
        public ScrollRectEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;
        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;

                if (m_ViewRect == null)
                    m_ViewRect = transform as RectTransform;

                return m_ViewRect;
            }
        }

        private Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        private bool m_Dragging;
        private bool m_Scrolling;

        private Vector2 m_PrevPosition = Vector2.zero;
        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;

        [NonSerialized] private bool m_HasRebuiltLayout;

        private bool m_HorizontalSliderExpand;
        private bool m_VerticalSliderExpand;
        private float m_HorizontalSliderHeight;
        private float m_VerticalSliderWidth;

        [NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = transform as RectTransform;

                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;

        protected LoopScrollRect() { }

        // *** patch --- //

        protected virtual void OnDataChanged() { }

        protected virtual void OnApplicationPause(bool pause)
        {
            m_Dragging = false;
        }

        private void ProvideCellData(Transform cell, int index)
        {
            if (!m_Datas.IsNullOrEmpty() && index >= 0 && index < m_Datas.Count)
            {
                var cellScript = cell.GetComponent<UIBaseCell>();
                if (cellScript != null)
                {
                    cellScript.Provide(index, m_Datas[index], m_ParentView);
                }
            }
        }

        private void ReturnCellObject(Transform cell)
        {
            var cellScript = cell.GetComponent<UIBaseCell>();
            if (cellScript != null)
            {
                cellScript.Recycle();
            }

            m_ObjectPool.ReturnObject(cell.gameObject);
        }

        private RectTransform InstantiateCellObject(int index)
        {
            var poolObject = m_ObjectPool.GetObject(index);

            if (poolObject != null)
            {
                poolObject.transform.SetParent(content, false);
                poolObject.SetActive(true);

                ProvideCellData(poolObject.transform, index);

                return poolObject.transform as RectTransform;
            }

            return null;
        }

        public T GetObjectPool<T>() where T : UIObjectPool
        {
            return m_ObjectPool as T;
        }

        public void RegisterParentView<T>(T parentView) where T : MonoBehaviour
        {
            m_ParentView = parentView;
        }

        public void PushData(object[] data, bool forceRefillCells = false)
        {
            PushData(new ArrayList(data), forceRefillCells);
        }

        public void PushData(List<object> data, bool forceRefillCells = false)
        {
            PushData(new ArrayList(data), forceRefillCells);
        }

        public void PushData(ArrayList data, bool forceRefillCells = false)
        {
            if (data.IsNullOrEmpty())
            {
                return;
            }

            bool needFill = m_Datas.IsNullOrEmpty();

            if (needFill)
            {
                m_Datas = data;
            }
            else
            {
                m_Datas.AddRange(data);
            }

            m_TotalCount = m_Datas.Count;

            if (needFill || forceRefillCells || CheckContentInsideViewBounds())
            {
                RefillCells();
            }

            OnDataChanged();
        }

        public void PushData(object data, bool forceRefillCells = false)
        {
            if (data == null)
            {
                return;
            }

            bool needFill = m_Datas.IsNullOrEmpty();

            if (needFill)
            {
                m_Datas = new ArrayList();
            }

            m_Datas.Add(data);

            m_TotalCount = m_Datas.Count;

            if (needFill || forceRefillCells || CheckContentInsideViewBounds())
            {
                RefillCells();
            }

            OnDataChanged();
        }

        public void DeleteData(int index)
        {
            m_TotalCount = m_Datas.Count();

            if (index < 0 || index >= m_TotalCount)
            {
                return;
            }

            m_Datas.RemoveAt(index);

            m_TotalCount--;

            if (index >= m_CellIndexStart || index < m_CellIndexEnd)
            {
                RefillCellsWithoutMove(m_CellIndexStart);
            }

            OnDataChanged();
        }

        public void RefreshData(int index, object data)
        {
            if (index < 0 || index >= m_TotalCount)
            {
                return;
            }

            m_Datas[index] = data;

            if (index < m_CellIndexStart || index >= m_CellIndexEnd)
            {
                return;
            }

            ProvideCellData(content.GetChild(index - m_CellIndexStart), index);

            OnDataChanged();
        }

        public void RefreshData(ArrayList data, bool forceRefillCells = false)
        {
            if (data.IsNullOrEmpty())
            {
                return;
            }

            bool refillOrRefresh = m_Datas.IsNullOrEmpty();

            m_Datas = data;
            m_TotalCount = m_Datas.Count;

            if (refillOrRefresh || forceRefillCells)
            {
                RefillCells();
            }
            else
            {
                RefreshCells();
            }

            OnDataChanged();
        }

        public void RefillDataFromEnd(ArrayList data)
        {
            if (data.IsNullOrEmpty())
            {
                return;
            }

            m_Datas = data;
            m_TotalCount = m_Datas.Count;

            RefillCellsFromEnd();

            OnDataChanged();
        }

        public void ClearCells()
        {
            if (Application.isPlaying)
            {
                m_CellIndexStart = 0;
                m_CellIndexEnd = 0;
                m_TotalCount = 0;
                m_Datas = null;

                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    ReturnCellObject(content.GetChild(i));
                }

                OnDataChanged();
            }
        }

        public void JumpToCell(int index)
        {
            float value = 0f;
            int constraintCount = contentConstraintCount;

            if (constraintCount > 1)
            {
                int rowOrColumnIndex = index / constraintCount;
                int rowOrColumnTotalCount = (int)Mathf.Ceil((float)m_TotalCount / constraintCount);

                value = Mathf.Clamp01((float)rowOrColumnIndex / rowOrColumnTotalCount);
            }
            else
            {
                value = Mathf.Clamp01((float)index / m_TotalCount);
            }

            StartCoroutine(JumpToCellCoroutine(value));
        }

        private IEnumerator JumpToCellCoroutine(float value)
        {
            yield return null;

            if (m_Vertical)
            {
                verticalNormalizedPosition = value;
            }

            if (m_Horizontal)
            {
                horizontalNormalizedPosition = value;
            }
        }

        public void ScrollToTop(float speed = 3000f)
        {
            if (m_TotalCount == 0)
            {
                return;
            }

            ScrollToCell(0, speed);
        }

        public void ScrollToBottom(float speed = 3000f)
        {
            if (m_TotalCount == 0)
            {
                return;
            }

            ScrollToCell(m_TotalCount - 1, speed);
        }

        public void ScrollToCell(int index, float speed = 3000f)
        {
            if (m_TotalCount >= 0 && (index < 0 || index >= m_TotalCount))
            {
                Log.ErrorFormat("Invalid index {0}", index);
                return;
            }

            if (speed <= 0f)
            {
                Log.ErrorFormat("Invalid speed {0}", speed);
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ScrollToCellCoroutine(index, speed));
        }

        private IEnumerator ScrollToCellCoroutine(int index, float speed)
        {
            bool needMoving = true;

            while (needMoving)
            {
                yield return null;

                if (!m_Dragging)
                {
                    float move = 0f;

                    if (index < m_CellIndexStart)
                    {
                        move = -Time.deltaTime * speed;
                    }
                    else if (index >= m_CellIndexEnd)
                    {
                        move = Time.deltaTime * speed;
                    }
                    else
                    {
                        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

                        var m_CellBounds = GetBounds4Cell(index);
                        float offset = 0f;

                        if (m_DirectionSign == -1)
                            offset = m_ReverseDirection ? (m_ViewBounds.min.y - m_CellBounds.min.y) : (m_ViewBounds.max.y - m_CellBounds.max.y);
                        else if (m_DirectionSign == 1)
                            offset = m_ReverseDirection ? (m_CellBounds.max.x - m_ViewBounds.max.x) : (m_CellBounds.min.x - m_ViewBounds.min.x);

                        // check if we cannot move on
                        if (m_TotalCount >= 0)
                        {
                            if (offset > 0 && m_CellIndexEnd == m_TotalCount && !m_ReverseDirection)
                            {
                                m_CellBounds = GetBounds4Cell(m_TotalCount - 1);
                                // reach bottom
                                if ((m_DirectionSign == -1 && m_CellBounds.min.y > m_ViewBounds.min.y) ||
                                    (m_DirectionSign == 1 && m_CellBounds.max.x < m_ViewBounds.max.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                            else if (offset < 0 && m_CellIndexStart == 0 && m_ReverseDirection)
                            {
                                m_CellBounds = GetBounds4Cell(0);
                                // reach top
                                if ((m_DirectionSign == -1 && m_CellBounds.max.y < m_ViewBounds.max.y) ||
                                    (m_DirectionSign == 1 && m_CellBounds.min.x > m_ViewBounds.min.x))
                                {
                                    needMoving = false;
                                    break;
                                }
                            }
                        }

                        float maxMove = Time.deltaTime * speed;

                        if (Mathf.Abs(offset) < maxMove)
                        {
                            needMoving = false;
                            move = offset;
                        }
                        else
                        {
                            move = Mathf.Sign(offset) * maxMove;
                        }
                    }

                    if (Mathf.Abs(move) > EPSILON)
                    {
                        Vector2 offset = GetVector(move);

                        content.anchoredPosition += offset;
                        m_PrevPosition += offset;
                        m_ContentStartPosition += offset;
                    }
                }
            }

            StopMovement();
            UpdatePrevData();
        }

        public void EnableDrag(bool draggable)
        {
            m_Draggable = draggable;

            if (!draggable)
            {
                StopDecelerate();
            }
        }

        public void StopDecelerate(WaitForSeconds waitForSeconds = null)
        {
            StopMovement();

            if (m_DecelerationRate > 0f)
            {
                StartCoroutine(StopDecelerateCoroutine(waitForSeconds));
            }
        }

        private IEnumerator StopDecelerateCoroutine(WaitForSeconds waitForSeconds = null)
        {
            float preDecelerationRate = m_DecelerationRate;
            m_DecelerationRate = 0f;
            yield return waitForSeconds;
            m_DecelerationRate = preDecelerationRate;
        }

        public void AdaptAnchoredPosition(bool updateCells = false)
        {
            UpdateBounds(updateCells);

            float contentSize = m_ContentBounds.size.y;
            float viewSize = m_ViewBounds.size.y;

            Vector2 pos = m_Content.anchoredPosition;

            if (m_DirectionSign == -1)
                pos.y = Mathf.Max(0f, contentSize - viewSize);
            else if (m_DirectionSign == 1)
                pos.x = Mathf.Max(0f, contentSize - viewSize);

            m_Content.anchoredPosition = pos;
        }

        public void RefreshCells()
        {
            if (Application.isPlaying && this.isActiveAndEnabled)
            {
                m_CellIndexEnd = m_CellIndexStart;
                // recycle cells if we can
                for (int i = 0; i < content.childCount; i++)
                {
                    if (m_CellIndexEnd < m_TotalCount)
                    {
                        ProvideCellData(content.GetChild(i), m_CellIndexEnd);
                        m_CellIndexEnd++;
                    }
                    else
                    {
                        ReturnCellObject(content.GetChild(i));
                        i--;
                    }
                }

                UpdateBounds(true);
            }
        }

        public void RefillCellsFromEnd(int offset = 0)
        {
            if (!Application.isPlaying || m_ObjectPool == null)
            {
                return;
            }

            StopMovement();

            m_CellIndexEnd = m_ReverseDirection ? offset : m_TotalCount - offset;
            m_CellIndexStart = m_CellIndexEnd;

            if (m_TotalCount >= 0 && m_CellIndexStart % contentConstraintCount != 0)
            {
                Log.Error("Grid will become strange since we can't fill cells in the last line");
            }

            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnCellObject(m_Content.GetChild(i));
            }

            float sizeToFill = 0f, sizeFilled = 0f;

            if (m_DirectionSign == -1)
                sizeToFill = viewRect.rect.size.y;
            else
                sizeToFill = viewRect.rect.size.x;

            while (sizeToFill > sizeFilled)
            {
                float size = m_ReverseDirection ? NewCellAtEnd() : NewCellAtStart();
                if (size <= 0f) break;
                sizeFilled += size;
            }

            Vector2 pos = m_Content.anchoredPosition;
            float dist = Mathf.Max(0f, sizeFilled - sizeToFill);

            if (m_ReverseDirection)
                dist = -dist;

            if (m_DirectionSign == -1)
                pos.y = dist;
            else if (m_DirectionSign == 1)
                pos.x = -dist;

            m_Content.anchoredPosition = pos;
        }

        public void RefillCells(int offset = 0)
        {
            if (!Application.isPlaying || m_ObjectPool == null)
            {
                return;
            }

            RefillCellsWithoutMove(offset);

            Vector2 pos = m_Content.anchoredPosition;

            if (m_DirectionSign == -1)
                pos.y = 0f;
            else if (m_DirectionSign == 1)
                pos.x = 0f;

            m_Content.anchoredPosition = pos;
        }

        public void RefillCellsWithoutMove(int offset = 0)
        {
            if (!Application.isPlaying || m_ObjectPool == null)
            {
                return;
            }

            StopMovement();

            m_CellIndexStart = m_ReverseDirection ? m_TotalCount - offset : offset;
            m_CellIndexEnd = m_CellIndexStart;

            if (m_TotalCount >= 0 && m_CellIndexStart % contentConstraintCount != 0)
            {
                Log.Error("Grid will become strange since we can't fill cells in the first line");
            }

            // Don't `Canvas.ForceUpdateCanvases()` here, or it will new/delete cells to change cellTypeStart/End
            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnCellObject(m_Content.GetChild(i));
            }

            float sizeToFill = 0f, sizeFilled = 0f;
            // m_ViewBounds may be not ready when RefillCells on Start
            if (m_DirectionSign == -1)
                sizeToFill = viewRect.rect.size.y;
            else
                sizeToFill = viewRect.rect.size.x;

            while (sizeToFill > sizeFilled)
            {
                float size = m_ReverseDirection ? NewCellAtStart() : NewCellAtEnd();
                if (size <= 0f) break;
                sizeFilled += size;
            }
        }

        public bool CheckCellInsideViewport(int index)
        {
            return m_TotalCount > 0 && m_CellIndexStart <= index && index < m_CellIndexEnd;
        }

        protected float NewCellAtStart()
        {
            if (m_TotalCount >= 0 && m_CellIndexStart - contentConstraintCount < 0)
            {
                return 0;
            }

            float size = 0f;

            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform newCell = InstantiateCellObject(m_CellIndexStart - 1);

                if (newCell == null)
                {
                    continue;
                }

                m_CellIndexStart--;

                newCell.SetAsFirstSibling();

                size = Mathf.Max(GetSize(newCell), size);
            }

            m_Threshold = Mathf.Max(m_Threshold, size * 1.5f);

            if (!m_ReverseDirection)
            {
                Vector2 offset = GetVector(size);

                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }

            return size;
        }

        protected float DeleteCellAtStart()
        {
            // when moving or dragging, we cannot simply delete start when we've reached the end
            if (((m_Dragging || m_Velocity != Vector2.zero) && m_TotalCount >= 0 && m_CellIndexEnd >= m_TotalCount - 1) || content.childCount == 0)
            {
                return 0;
            }

            float size = 0f;

            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldCell = content.GetChild(0) as RectTransform;
                size = Mathf.Max(GetSize(oldCell), size);
                ReturnCellObject(oldCell);

                m_CellIndexStart++;

                if (content.childCount == 0)
                {
                    break;
                }
            }

            if (!m_ReverseDirection)
            {
                Vector2 offset = GetVector(size);

                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }

            return size;
        }


        protected float NewCellAtEnd()
        {
            if (m_TotalCount >= 0 && m_CellIndexEnd >= m_TotalCount)
            {
                return 0;
            }

            float size = 0f;
            // fill lines to end first
            int count = contentConstraintCount - (content.childCount % contentConstraintCount);

            for (int i = 0; i < count; i++)
            {
                RectTransform newCell = InstantiateCellObject(m_CellIndexEnd);

                if (newCell == null)
                {
                    continue;
                }

                size = Mathf.Max(GetSize(newCell), size);

                m_CellIndexEnd++;

                if (m_TotalCount >= 0 && m_CellIndexEnd >= m_TotalCount)
                {
                    break;
                }
            }

            m_Threshold = Mathf.Max(m_Threshold, size * 1.5f);

            if (m_ReverseDirection)
            {
                Vector2 offset = GetVector(size);

                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }

            return size;
        }

        protected float DeleteCellAtEnd()
        {
            if (((m_Dragging || m_Velocity != Vector2.zero) && m_TotalCount >= 0 && m_CellIndexStart < contentConstraintCount) || content.childCount == 0)
            {
                return 0;
            }

            float size = 0f;

            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldCell = content.GetChild(content.childCount - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldCell), size);
                ReturnCellObject(oldCell);

                m_CellIndexEnd--;

                if (m_CellIndexEnd % contentConstraintCount == 0 || content.childCount == 0)
                {
                    break; // just delete the whole row
                }
            }

            if (m_ReverseDirection)
            {
                Vector2 offset = GetVector(size);

                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }

            return size;
        }

        // --- patch *** //

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete() { }

        public virtual void GraphicUpdateComplete() { }

        private void UpdateCachedData()
        {
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HorizontalSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VerticalSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HorizontalSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VerticalSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);

            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_Dragging = false;
            m_Scrolling = false;
            m_HasRebuiltLayout = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        public void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
            {
                return;
            }

            // *** patch --- //

            if (!m_Draggable)
            {
                return;
            }

            // --- patch *** //

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1f;

            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0f;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0f;
            }

            if (data.IsScrolling())
            {
                m_Scrolling = true;
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;

            if (m_MovementType == MovementType.Clamped)
            {
                position += CalculateOffset(position - m_Content.anchoredPosition);
            }

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            m_Velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!IsActive())
            {
                return;
            }

            // *** patch --- //

            if (!m_Draggable)
            {
                return;
            }

            // --- patch *** //

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;

            m_Dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            m_Dragging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!m_Dragging)
            {
                return;
            }

            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!IsActive())
            {
                return;
            }

            // *** patch --- //

            if (!m_Draggable)
            {
                return;
            }

            // --- patch *** //

            Vector2 localCursor;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
            {
                return;
            }

            UpdateBounds();

            Vector2 pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;

            if (m_MovementType == MovementType.Elastic)
            {
                // *** patch --- //

                if (Mathf.Abs(offset.x) > EPSILON)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x) * m_RubberScale;
                if (Mathf.Abs(offset.y) > EPSILON)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y) * m_RubberScale;

                // --- patch *** //
            }

            SetContentAnchoredPosition(position);
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds(true);
            }
        }

        protected virtual void LateUpdate()
        {
            if (!m_Content)
            {
                return;
            }

            if (m_Horizontal && Mathf.Approximately(rectTransform.lossyScale.x, 0f))
            {
                return;
            }

            if (m_Vertical && Mathf.Approximately(rectTransform.lossyScale.y, 0f))
            {
                return;
            }

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);

            if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
            {
                Vector2 position = m_Content.anchoredPosition;

                for (int axis = 0; axis < 2; axis++)
                {
                    // Apply spring physics if movement is elastic and content has an offset from the view.
                    if (m_MovementType == MovementType.Elastic && Mathf.Abs(offset[axis]) > EPSILON)
                    {
                        float speed = m_Velocity[axis];
                        float smoothTime = m_Elasticity;

                        if (m_Scrolling)
                        {
                            smoothTime *= 3f;
                        }

                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);

                        if (Mathf.Abs(speed) < 1f)
                        {
                            speed = 0f;
                        }

                        m_Velocity[axis] = speed;
                    }
                    // Else move content according to velocity with deceleration applied.
                    else if (m_Inertia)
                    {
                        m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);

                        if (Mathf.Abs(m_Velocity[axis]) < 1f)
                        {
                            m_Velocity[axis] = 0f;
                        }

                        position[axis] += m_Velocity[axis] * deltaTime;
                    }
                    // If we have neither elaticity or friction, there shouldn't be any velocity.
                    else
                    {
                        m_Velocity[axis] = 0f;
                    }
                }

                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }

            if (m_Dragging && m_Inertia)
            {
                Vector2 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10f);
            }

            if (m_ViewBounds != m_PrevViewBounds ||
                m_ContentBounds != m_PrevContentBounds ||
                m_Content.anchoredPosition != m_PrevPosition)
            {
                UpdateScrollbars(offset);
                m_OnValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }

            UpdateScrollbarVisibility();

            m_Scrolling = false;
        }

        private void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;

            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                // *** patch --- //

                if (m_ContentBounds.size.x > 0f && m_TotalCount > 0)
                {
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x * gridViewLineCount / gridTotalLineCount);
                }

                // --- patch *** //

                else
                {
                    m_HorizontalScrollbar.size = 1;
                }

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                // *** patch --- //

                if (m_ContentBounds.size.y > 0f && m_TotalCount > 0)
                {
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y * gridViewLineCount / gridTotalLineCount);
                }

                // --- patch *** //

                else
                {
                    m_VerticalScrollbar.size = 1f;
                }

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();

                // *** patch --- //

                if (m_TotalCount > 0 && m_CellIndexEnd > m_CellIndexStart)
                {
                    float elementSize = m_ContentBounds.size.x / gridViewLineCount;
                    float totalSize = elementSize * gridTotalLineCount;
                    float offset = m_ContentBounds.min.x - elementSize * gridStartLineIndex;

                    if (totalSize <= m_ViewBounds.size.x || Mathf.Approximately(totalSize, m_ViewBounds.size.x))
                        return (m_ViewBounds.min.x > offset) ? 1f : 0f;

                    return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
                }
                else
                {
                    return 0.5f;
                }

                // --- patch *** //
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();

                // *** patch --- //

                if (m_TotalCount > 0 && m_CellIndexEnd > m_CellIndexStart)
                {
                    float elementSize = m_ContentBounds.size.y / gridViewLineCount;
                    float totalSize = elementSize * gridTotalLineCount;
                    float offset = m_ContentBounds.max.y + elementSize * gridStartLineIndex;

                    if (totalSize <= m_ViewBounds.size.y || Mathf.Approximately(totalSize, m_ViewBounds.size.y))
                        return (offset > m_ViewBounds.max.y) ? 1f : 0f;

                    return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
                }
                else
                {
                    return 0.5f;
                }

                // --- patch *** //
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        private void SetNormalizedPosition(float value, int axis)
        {
            // *** patch --- //

            if (m_TotalCount <= 0 || m_CellIndexEnd <= m_CellIndexStart)
            {
                return;
            }

            // --- patch *** //

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            // *** patch --- //

            Vector3 localPosition = m_Content.localPosition;
            float newLocalPosition = localPosition[axis];

            if (axis == 0)
            {
                float elementSize = m_ContentBounds.size.x / gridViewLineCount;
                float totalSize = elementSize * gridTotalLineCount;
                float offset = m_ContentBounds.min.x - elementSize * gridStartLineIndex;

                newLocalPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size[axis]) - offset;
            }
            else if (axis == 1)
            {
                float elementSize = m_ContentBounds.size.y / gridViewLineCount;
                float totalSize = elementSize * gridTotalLineCount;
                float offset = m_ContentBounds.max.y + elementSize * gridStartLineIndex;

                newLocalPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
            }

            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > EPSILON)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds(true);
            }

            // --- patch *** //
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1f - (1f / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1f))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool horizontalScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + EPSILON;

                return true;
            }
        }

        private bool verticalScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + EPSILON;

                return true;
            }
        }

        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        public virtual float minWidth { get { return -1f; } }
        public virtual float preferredWidth { get { return -1f; } }
        public virtual float flexibleWidth { get { return -1f; } }

        public virtual float minHeight { get { return -1f; } }
        public virtual float preferredHeight { get { return -1f; } }
        public virtual float flexibleHeight { get { return -1f; } }

        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HorizontalSliderExpand || m_VerticalSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VerticalSliderExpand && verticalScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VerticalSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HorizontalSliderExpand && horizontalScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HorizontalSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VerticalSliderExpand && verticalScrollingNeeded && Mathf.Abs(viewRect.sizeDelta.x) < EPSILON && viewRect.sizeDelta.y < 0f)
            {
                viewRect.sizeDelta = new Vector2(-(m_VerticalSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        private void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(horizontalScrollingNeeded, m_Horizontal, m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
            UpdateOneScrollbarVisibility(verticalScrollingNeeded, m_Vertical, m_VerticalScrollbarVisibility, m_VerticalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(bool scrollingNeeded, bool axisEnabled, ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar == null)
            {
                return;
            }

            if (scrollbarVisibility == ScrollbarVisibility.Permanent)
            {
                if (scrollbar.gameObject.activeSelf != axisEnabled)
                    scrollbar.gameObject.SetActive(axisEnabled);
            }
            else
            {
                if (scrollbar.gameObject.activeSelf != scrollingNeeded)
                    scrollbar.gameObject.SetActive(scrollingNeeded);
            }
        }

        private void UpdateScrollbarLayout()
        {
            if (m_VerticalSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                              DrivenTransformProperties.AnchorMinX |
                              DrivenTransformProperties.AnchorMaxX |
                              DrivenTransformProperties.SizeDeltaX |
                              DrivenTransformProperties.AnchoredPositionX);

                m_HorizontalScrollbarRect.anchorMin = new Vector2(0f, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1f, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0f, m_HorizontalScrollbarRect.anchoredPosition.y);

                if (verticalScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VerticalSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0f, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HorizontalSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                              DrivenTransformProperties.AnchorMinY |
                              DrivenTransformProperties.AnchorMaxY |
                              DrivenTransformProperties.SizeDeltaY |
                              DrivenTransformProperties.AnchoredPositionY);

                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0f);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1f);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0f);

                if (horizontalScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HorizontalSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0f);
            }
        }

        private void UpdateBounds(bool updateCells = false)
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
            {
                return;
            }

            // *** patch --- //

            // Don't do this in Rebuild
            if (Application.isPlaying && updateCells && UpdateCells(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = GetBounds();
            }

            // --- patch *** //

            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector2 contentPivot = m_Content.pivot;

            AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;

            if (movementType == MovementType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;

                if (m_ViewBounds.max.x > m_ContentBounds.max.x)
                {
                    delta.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }
                else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
                {
                    delta.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
                }

                if (m_ViewBounds.min.y < m_ContentBounds.min.y)
                {
                    delta.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }
                else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
                {
                    delta.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
                }

                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = m_Content.anchoredPosition + delta;

                    if (!m_Horizontal)
                    {
                        contentPos.x = m_Content.anchoredPosition.x;
                    }
                    if (!m_Vertical)
                    {
                        contentPos.y = m_Content.anchoredPosition.y;
                    }

                    AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 excess = viewBounds.size - contentSize;

            if (excess.x > 0f)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }
            if (excess.y > 0f)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];

        private Bounds GetBounds()
        {
            if (m_Content == null)
            {
                return new Bounds();
            }

            m_Content.GetWorldCorners(m_Corners);

            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;

            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                var v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                min = Vector3.Min(v, min);
                max = Vector3.Max(v, max);
            }

            var bounds = new Bounds(min, Vector3.zero);
            bounds.Encapsulate(max);

            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, m_Horizontal, m_Vertical, m_MovementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, MovementType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;

            if (movementType == MovementType.Unrestricted)
            {
                return offset;
            }

            Vector3 min = contentBounds.min;
            Vector3 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewBounds.max.x - max.x;
                float minOffset = viewBounds.min.x - min.x;

                if (minOffset < -EPSILON / 10f)
                    offset.x = minOffset;
                else if (maxOffset > EPSILON / 10f)
                    offset.x = maxOffset;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewBounds.max.y - max.y;
                float minOffset = viewBounds.min.y - min.y;

                if (maxOffset > EPSILON / 10f)
                    offset.y = maxOffset;
                else if (minOffset < -EPSILON / 10f)
                    offset.y = minOffset;
            }

            return offset;
        }

        // *** patch --- //

        private Bounds GetBounds4Cell(int index)
        {
            if (m_Content == null)
            {
                return new Bounds();
            }

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;

            int offset = index - m_CellIndexStart;
            if (offset < 0 || offset >= m_Content.childCount)
            {
                return new Bounds();
            }

            var rt = m_Content.GetChild(offset) as RectTransform;
            if (rt == null)
            {
                return new Bounds();
            }

            rt.GetWorldCorners(m_Corners);

            for (int i = 0; i < 4; i++)
            {
                var v = viewWorldToLocalMatrix.MultiplyPoint3x4(m_Corners[i]);
                min = Vector3.Min(v, min);
                max = Vector3.Max(v, max);
            }

            var bounds = new Bounds(min, Vector3.zero);
            bounds.Encapsulate(max);

            return bounds;
        }

        private bool CheckContentInsideViewBounds()
        {
            bool inside = true;

            m_ContentBounds = GetBounds();

            if (m_Horizontal)
                inside &= m_ContentBounds.size.x <= m_ViewBounds.size.x;
            if (m_Vertical)
                inside &= m_ContentBounds.size.y <= m_ViewBounds.size.y;

            return inside;
        }

        // --- patch *** //

        protected void SetDirty()
        {
            if (!IsActive())
            {
                return;
            }

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
            {
                return;
            }

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }
#endif
    }
}