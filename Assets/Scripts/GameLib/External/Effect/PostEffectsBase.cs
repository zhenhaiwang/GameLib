using UnityEngine;

namespace GameLib.Effect
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PostEffectsBase : MonoBehaviour
    {
        protected bool m_SupportHDRTextures = true;
        protected bool m_SupportDX11;
        protected bool m_Supported = true;

        protected Material CheckShaderAndCreateMaterial(Shader shader, Material m2Create)
        {
            if (!shader)
            {
                Log.Debug("Missing shader in " + ToString());
                enabled = false;
                return null;
            }

            if (shader.isSupported && m2Create && m2Create.shader == shader)
                return m2Create;

            if (!shader.isSupported)
            {
                NotSupported();
                Log.Debug("The shader " + shader + " on effect " + ToString() + " is not supported on this platform");
                return null;
            }
            else
            {
                m2Create = new Material(shader);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }

        protected Material CreateMaterial(Shader shader, Material m2Create)
        {
            if (!shader)
            {
                Log.Debug("Missing shader in " + ToString());
                return null;
            }

            if (m2Create && (m2Create.shader == shader) && (shader.isSupported))
                return m2Create;

            if (!shader.isSupported)
            {
                return null;
            }
            else
            {
                m2Create = new Material(shader);
                m2Create.hideFlags = HideFlags.DontSave;

                if (m2Create)
                    return m2Create;
                else
                    return null;
            }
        }

        private void OnEnable()
        {
            m_Supported = true;
        }

        protected bool CheckSupport()
        {
            return CheckSupport(false);
        }

        public virtual bool CheckResources()
        {
            Log.Warning("Check resources for " + ToString() + " should be overwritten");

            return m_Supported;
        }

        protected void Start()
        {
            CheckResources();
        }

        protected bool CheckSupport(bool needDepth)
        {
            m_Supported = true;
            m_SupportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            m_SupportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

            if (!SystemInfo.supportsImageEffects)
            {
                NotSupported();
                return false;
            }

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                NotSupported();
                return false;
            }

            if (needDepth)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

            return true;
        }

        protected bool CheckSupport(bool needDepth, bool needHdr)
        {
            if (!CheckSupport(needDepth))
                return false;

            if (needHdr && !m_SupportHDRTextures)
            {
                NotSupported();
                return false;
            }

            return true;
        }

        public bool Dx11Support()
        {
            return m_SupportDX11;
        }

        protected void ReportAutoDisable()
        {
            Log.Warning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform");
        }

        /// <summary>
        /// deprecated but needed for old effects to survive upgrading
        /// </summary>
        protected bool CheckShader(Shader shader)
        {
            Log.Debug("The shader " + shader + " on effect " + ToString() + " is not part of the Unity 3.2+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package");

            if (!shader.isSupported)
            {
                NotSupported();
                return false;
            }
            else
            {
                return true;
            }
        }

        protected void NotSupported()
        {
            enabled = false;
            m_Supported = false;
            return;
        }

        protected void DrawBorder(RenderTexture dest, Material material)
        {
            float x1;
            float x2;
            float y1;
            float y2;

            RenderTexture.active = dest;
            bool invertY = true; // source.texelSize.y < 0.0ff;

            // Set up the simple Matrix
            GL.PushMatrix();
            GL.LoadOrtho();

            for (int i = 0; i < material.passCount; i++)
            {
                material.SetPass(i);

                float y1_; float y2_;

                if (invertY)
                {
                    y1_ = 1.0f; y2_ = 0.0f;
                }
                else
                {
                    y1_ = 0.0f; y2_ = 1.0f;
                }

                // left
                x1 = 0.0f;
                x2 = 0.0f + 1.0f / (dest.width * 1.0f);
                y1 = 0.0f;
                y2 = 1.0f;
                GL.Begin(GL.QUADS);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // right
                x1 = 1.0f - 1.0f / (dest.width * 1.0f);
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // top
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 0.0f + 1.0f / (dest.height * 1.0f);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // bottom
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 1.0f - 1.0f / (dest.height * 1.0f);
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                GL.End();
            }

            GL.PopMatrix();
        }
    }
}