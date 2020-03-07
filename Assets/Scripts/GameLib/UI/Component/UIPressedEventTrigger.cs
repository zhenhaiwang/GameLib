using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameLib
{
    [Serializable]
    internal class PressedEvent : UnityEvent<bool, string> { }

    public sealed class UIPressedEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Range(0.1f, 1.0f)]
        private float m_Delay = 0.25f;

        [SerializeField]
        private PressedEvent m_OnPressed = new PressedEvent();

        public string param = string.Empty;

        private bool m_Pressed;
        private bool m_PressedInvoked;
        private float m_PressedTime;

        private void Update()
        {
            if (m_Pressed && Time.time - m_PressedTime > m_Delay)
            {
                m_Pressed = false;
                m_PressedInvoked = true;
                m_OnPressed.Invoke(true, param);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_PressedTime = Time.time;
            m_Pressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            m_Pressed = false;

            if (m_PressedInvoked)
            {
                m_PressedInvoked = false;
                m_OnPressed.Invoke(false, param);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Pressed = false;

            if (m_PressedInvoked)
            {
                m_PressedInvoked = false;
                m_OnPressed.Invoke(false, param);
            }
        }
    }
}