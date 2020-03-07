using UnityEngine;

namespace GameLib
{
    public static class ScreenshotUtil
    {
        public static Texture2D Screenshot(Camera camera, TextureFormat textureFormat = TextureFormat.RGB24)
        {
            var cameraShot = camera;

            if (cameraShot == null)
            {
                cameraShot = Camera.main;
            }

            int width = Screen.width;
            int heigth = Screen.height;
            var rect = new Rect(0, 0, width, heigth);

            return Screenshot(new Camera[1] { cameraShot }, width, heigth, rect, textureFormat);
        }

        public static Texture2D Screenshot(Camera[] cameras, TextureFormat textureFormat = TextureFormat.RGB24)
        {
            int width = Screen.width;
            int heigth = Screen.height;
            var rect = new Rect(0, 0, width, heigth);

            return Screenshot(cameras, width, heigth, rect, textureFormat);
        }

        public static Texture2D Screenshot(Camera camera, int width, int heigth, Rect rect, TextureFormat textureFormat = TextureFormat.RGB24)
        {
            var cameraShot = camera;

            if (cameraShot == null)
            {
                cameraShot = Camera.main;
            }

            return Screenshot(new Camera[1] { cameraShot }, width, heigth, rect, textureFormat);
        }

        public static Texture2D Screenshot(Camera[] cameras, int width, int heigth, Rect rect, TextureFormat textureFormat = TextureFormat.RGB24)
        {
            var rt = new RenderTexture(width, heigth, 0, RenderTextureFormat.ARGB32);

            for (int i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];

                if (camera != null)
                {
                    camera.targetTexture = rt;
                    camera.Render();
                }
            }

            RenderTexture.active = rt;

            var screenShot = new Texture2D((int)rect.width, (int)rect.height, textureFormat, false);
            screenShot.ReadPixels(rect, 0, 0);
            screenShot.Apply();

            for (int i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];

                if (camera != null)
                {
                    camera.targetTexture = null;
                }
            }

            RenderTexture.active = null;

            Object.DestroyImmediate(rt);

            return screenShot;
        }
    }
}
