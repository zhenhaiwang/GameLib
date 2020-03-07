using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Framework.UI
{
    public sealed class UICutImage : Image
    {
        private Vector2[] m_UVs;

        private bool m_Changed;

        private float m_Width;
        private float m_Height;

        private float m_PivotX;
        private float m_PivotY;

        protected override void Start()
        {
            base.Start();

            if (m_UVs != null && m_UVs.Length > 0)
            {
                SetUV(m_UVs);
            }
        }

        public void SetUV(Vector2[] uvs)
        {
            this.m_Changed = true;
            this.m_UVs = uvs;

            SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (m_Changed)
            {
                m_Width = rectTransform.rect.width;
                m_Height = rectTransform.rect.height;
                m_PivotX = rectTransform.pivot.x;
                m_PivotY = rectTransform.pivot.y;

                Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
                float uvCenterX = (uv.x + uv.z) * 0.5f;
                float uvCenterY = (uv.y + uv.w) * 0.5f;
                float uvScaleX = (uv.z - uv.x) / m_Width;
                float uvScaleY = (uv.w - uv.y) / m_Height;

                Color32 color32 = color;

                toFill.Clear();

                int length = m_UVs.Length;

                for (int i = 0; i < length; i++)
                {
                    Vector3 pos = new Vector3(m_Width * m_UVs[i].x - m_PivotX * m_Width, m_Height * m_UVs[i].y - m_PivotY * m_Height, 0.0f);
                    Vector2 tmpuv = new Vector2((pos.x - (m_PivotX - 0.5f) * m_Width) * uvScaleX + uvCenterX, (pos.y - (m_PivotY - 0.5f) * m_Height) * uvScaleY + uvCenterY);
                    toFill.AddVert(pos, color32, tmpuv);
                }

                int start = 1;

                for (int i = 0; i < length - 2; i++)
                {
                    toFill.AddTriangle(0, start, start + 1);
                    start++;
                }

                m_Changed = false;
            }
            else
            {
                base.OnPopulateMesh(toFill);
            }
        }
    }
}