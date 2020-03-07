using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLib
{
    public sealed class PopupContainer : MonoBehaviour
    {
        public GameObject child { get; private set; }
        public RawImage mask { get; private set; }
        public bool modal { get; private set; }

        public Action destroyDelegate;

        private void Awake()
        {
            var containerRect = gameObject.AddComponent<RectTransform>();
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var maskObject = UnityUtil.AddChild(gameObject);
            maskObject.name = "Mask";

            var maskRect = maskObject.AddComponent<RectTransform>();
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.anchorMin = Vector2.zero;
            maskRect.anchorMax = Vector2.one;
            maskRect.offsetMin = Vector2.zero;
            maskRect.offsetMax = Vector2.zero;

            mask = maskObject.AddComponent<RawImage>();
            mask.color = new Color(0f, 0f, 0f, 0f);
            mask.raycastTarget = true;

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(OnMaskPointerClick);

            var eventTrigger = maskObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(entry);
        }

        private void OnMaskPointerClick(BaseEventData eventData)
        {
            if (modal)
            {
                return;
            }

            DestroyContainer();
        }

        public GameObject AddChild(GameObject prefab)
        {
            child = UnityUtil.AddChild(gameObject, prefab);
            child.name = "Prefab";
            return child;
        }

        public void DestroyContainer()
        {
            destroyDelegate.Call();
            Destroy(gameObject);
        }

        public void SetModal(bool modal)
        {
            this.modal = modal;
        }

        public void SetTop()
        {
            transform.SetAsLastSibling();
        }

        public void SetAlpha(float alpha)
        {
            if (alpha <= 0f)
            {
                mask.color = new Color(0f, 0f, 0f, 0f);
            }
            else
            {
                mask.color = new Color(0f, 0f, 0f, alpha);
            }
        }

        public void SetBlur()
        {
            var cameras = new Camera[] { Camera.main };

            BlurUtil.BlurCameras(cameras);

            var texture2D = ScreenshotUtil.Screenshot(cameras, TextureFormat.RGB24);
            mask.color = Color.white;
            mask.texture = texture2D;

            BlurUtil.UnBlurCameras(cameras);
        }
    }
}
