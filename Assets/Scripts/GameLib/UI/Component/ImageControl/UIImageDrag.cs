using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    [RequireComponent(typeof(Image))]
    public class UIImageDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool dragOnSurfaces;

        public Action<GameObject> onBeginDrag;
        public Action onEndDrag;

        private Image m_DraggableImage;
        private RectTransform m_DraggingRectTransform;
        private RectTransform m_DraggingPlane;

        private void OnEnable()
        {
            m_DraggableImage = GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var canvas = UnityUtil.FindInParent<Canvas>(gameObject);

            if (canvas != null)
            {
                var draggingGameObject = UnityUtil.AddChild(canvas.gameObject);
                draggingGameObject.name = gameObject.name;
                draggingGameObject.transform.SetAsLastSibling();

                var image = draggingGameObject.AddComponent<Image>();
                image.raycastTarget = false;
                image.sprite = m_DraggableImage.sprite;
                image.SetNativeSize();

                m_DraggingRectTransform = draggingGameObject.GetComponent<RectTransform>();

                if (dragOnSurfaces)
                {
                    m_DraggingPlane = transform as RectTransform;
                }
                else
                {
                    m_DraggingPlane = canvas.transform as RectTransform;
                }

                OnDrag(eventData);

                onBeginDrag.Call(draggingGameObject);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_DraggingRectTransform != null)
            {
                if (dragOnSurfaces && eventData.pointerEnter != null)
                {
                    var pointerEnterRectTransform = eventData.pointerEnter.transform as RectTransform;

                    if (pointerEnterRectTransform != null)
                    {
                        m_DraggingPlane = pointerEnterRectTransform;
                    }
                }

                Vector3 globalMousePosition;

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, eventData.position, eventData.pressEventCamera, out globalMousePosition))
                {
                    m_DraggingRectTransform.position = globalMousePosition;
                    m_DraggingRectTransform.rotation = m_DraggingPlane.rotation;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (m_DraggingRectTransform != null)
            {
                Destroy(m_DraggingRectTransform.gameObject);
                m_DraggingRectTransform = null;

                onEndDrag.Call();
            }
        }
    }
}