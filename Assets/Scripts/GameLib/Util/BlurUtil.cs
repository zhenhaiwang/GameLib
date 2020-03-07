using UnityEngine;

namespace GameLib
{
    public static class BlurUtil
    {
        public static void BlurCamera(Camera camera)
        {
            if (camera == null) return;

            var blur = camera.gameObject.GetComponent<Effect.BlurOptimized>();

            if (blur == null)
            {
                blur = camera.gameObject.AddComponent<Effect.BlurOptimized>();
                blur.blurShader = Shader.Find("Hidden/FastBlur");
            }

            blur.enabled = true;
        }

        public static void UnBlurCamera(Camera camera)
        {
            if (camera == null) return;

            var blur = camera.gameObject.GetComponent<Effect.BlurOptimized>();

            if (blur != null)
            {
                blur.enabled = false;
            }
        }

        public static void BlurCameras(Camera[] cameras)
        {
            if (cameras == null || cameras.Length == 0) return;

            for (int i = 0; i < cameras.Length; i++)
            {
                BlurCamera(cameras[i]);
            }
        }

        public static void UnBlurCameras(Camera[] cameras)
        {
            if (cameras == null || cameras.Length == 0) return;

            for (int i = 0; i < cameras.Length; i++)
            {
                UnBlurCamera(cameras[i]);
            }
        }
    }
}