using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    [RequireComponent(typeof(Image))]
    public class UIImageDrop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<GameObject> onDrop;

        private Image m_DroppableImage;

        private void OnEnable()
        {
            m_DroppableImage = GetComponent<Image>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var dropGameObject = eventData.pointerDrag;

            if (dropGameObject != null)
            {
                var dropImage = dropGameObject.GetComponent<Image>();

                if (dropImage != null && dropImage.sprite != null)
                {
                    m_DroppableImage.overrideSprite = dropImage.sprite;
                    m_DroppableImage.enabled = true;
                    m_DroppableImage.color = Color.white;

                    onDrop.Call(dropGameObject);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // to do
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // to do
        }
    }
}