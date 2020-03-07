using UnityEngine;

namespace GameLib.Effect
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Framework Effect/BlurOptimized")]
    public class BlurOptimized : PostEffectsBase
    {
        [Range(0, 2)]
        public int downsample = 1;

        public enum BlurType
        {
            StandardGauss = 0,
            SgxGauss = 1,
        }

        [Range(0.0f, 10.0f)]
        public float blurSize = 3.0f;

        [Range(1, 4)]
        public int blurIterations = 2;

        public BlurType blurType = BlurType.StandardGauss;

        public Shader blurShader;

        private Material m_BlurMaterial;

        public override bool CheckResources()
        {
            CheckSupport(false);

            m_BlurMaterial = CheckShaderAndCreateMaterial(blurShader, m_BlurMaterial);

            if (!m_Supported)
                ReportAutoDisable();

            return m_Supported;
        }

        public void OnDisable()
        {
            if (m_BlurMaterial)
                DestroyImmediate(m_BlurMaterial);
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!CheckResources())
            {
                Graphics.Blit(source, destination);
                return;
            }

            float widthMod = 1.0f / (1.0f * (1 << downsample));

            m_BlurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
            source.filterMode = FilterMode.Bilinear;

            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;

            // downsample
            var rt1 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt1.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, rt1, m_BlurMaterial, 0);

            var passOffset = blurType == BlurType.StandardGauss ? 0 : 2;

            for (int i = 0; i < blurIterations; i++)
            {
                float iterationOffset = i * 1f;
                m_BlurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffset, -blurSize * widthMod - iterationOffset, 0.0f, 0.0f));

                // vertical blur
                var rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt1, rt2, m_BlurMaterial, 1 + passOffset);
                RenderTexture.ReleaseTemporary(rt1);
                rt1 = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt1, rt2, m_BlurMaterial, 2 + passOffset);
                RenderTexture.ReleaseTemporary(rt1);
                rt1 = rt2;
            }

            Graphics.Blit(rt1, destination);

            RenderTexture.ReleaseTemporary(rt1);
        }
    }
}