using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public sealed class PopupManager : MonoSingleton<PopupManager>
    {
        public Camera popCamera { get; private set; }
        public Canvas popCanvas { get; private set; }

        private const string LayerName = "UIPopup";
        private const int LayerDepth = 1;
        private const int CanvasOrder = 1;
        private const float MaskAlpha = 0.75f;

        private Dictionary<string, PopupContainer> m_PopPath2ContainerDict = new Dictionary<string, PopupContainer>();
        private List<string> m_PopPathList = new List<string>();

        private void Awake()
        {
            var cameraObject = new GameObject("UIPopupCamera");

            DontDestroyOnLoad(cameraObject);

            UnityUtil.SetLayer(cameraObject, LayerMask.NameToLayer(LayerName));

            popCamera = cameraObject.AddComponent<Camera>();
            popCamera.cullingMask &= 1 << LayerMask.NameToLayer(LayerName);
            popCamera.clearFlags = CameraClearFlags.Depth;
            popCamera.orthographic = true;
            popCamera.depth = LayerDepth;

            var canvasObject = UnityUtil.AddChild(cameraObject);
            canvasObject.name = "Canvas";

            popCanvas = canvasObject.AddComponent<Canvas>();
            popCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popCanvas.worldCamera = popCamera;
            popCanvas.sortingOrder = CanvasOrder;

            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();
        }

        public GameObject Popup(string path, bool modal, bool blur, float alpha = MaskAlpha)
        {
            return Popup(path, (GameObject)Resources.Load(path), modal, blur, alpha);
        }

        public T Popup<T>(string path, bool modal, bool blur, float alpha = MaskAlpha)
        {
            return Popup(path, modal, blur, alpha).GetComponent<T>();
        }

        public void PopupAsync(string path, bool modal, bool blur, float alpha = MaskAlpha)
        {
            StartCoroutine(PopupAsyncCoroutine(path, modal, blur, alpha));
        }

        public void Back()
        {
            int popCount = m_PopPathList.Count;

            if (popCount == 0)
            {
                return;
            }

            PopupContainer container;

            if (m_PopPath2ContainerDict.TryGetValue(m_PopPathList[popCount - 1], out container))
            {
                container.DestroyContainer();
            }
        }

        private GameObject Popup(string path, GameObject prefab, bool modal, bool blur, float alpha)
        {
            if (prefab == null)
            {
                Log.Error("Can not load prefab from path ", path);

                return null;
            }

            PopupContainer container;

            if (m_PopPath2ContainerDict.TryGetValue(path, out container))
            {
                UnCache(path);
                Cache(path, container);

                container.SetTop();

                return container.child;
            }

            container = CreateContainer(path);

            if (blur)
            {
                container.SetBlur();
            }
            else
            {
                container.SetAlpha(alpha);
            }

            container.SetModal(modal);
            container.destroyDelegate = () =>
            {
                UnCache(path);
            };

            Cache(path, container);

            return container.AddChild(prefab);
        }

        private IEnumerator PopupAsyncCoroutine(string path, bool modal, bool blur, float alpha)
        {
            var request = Resources.LoadAsync(path);
            yield return request;
            Popup(path, request.asset as GameObject, modal, blur, alpha);
        }

        private PopupContainer CreateContainer(string path)
        {
            var popObject = UnityUtil.AddChild(popCanvas.gameObject);
            popObject.name = path;
            return popObject.AddComponent<PopupContainer>();
        }

        private void Cache(string path, PopupContainer container)
        {
            if (!m_PopPath2ContainerDict.ContainsKey(path))
            {
                m_PopPath2ContainerDict.Add(path, container);
                m_PopPathList.Add(path);
            }
        }

        private void UnCache(string path)
        {
            if (m_PopPath2ContainerDict.ContainsKey(path))
            {
                m_PopPath2ContainerDict.Remove(path);
                m_PopPathList.Remove(path);
            }
        }
    }
}