using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace GameLib
{
    public sealed class UICircleImage : Image
    {
        private List<Vector3> m_InnerVertices;
        private List<Vector3> m_OuterVertices;

        [Tooltip("Fill percent")]
        [Range(0, 1)]
        [SerializeField]
        private float m_FillPercent = 1f;

        [Tooltip("Circle or ring")]
        [SerializeField]
        private bool m_Fill = true;

        [Tooltip("Ring thickness")]
        [SerializeField]
        private float m_Thickness = 5;

        [Tooltip("Smooth")]
        [Range(3, 100)]
        [SerializeField]
        private int m_Segments = 20;

        protected override void Awake()
        {
            base.Awake();

            m_InnerVertices = new List<Vector3>();
            m_OuterVertices = new List<Vector3>();
        }

        private void Update()
        {
            m_Thickness = Mathf.Clamp(m_Thickness, 0, rectTransform.rect.width / 2);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();

            m_InnerVertices.Clear();
            m_OuterVertices.Clear();

            float degreeDelta = 2 * Mathf.PI / m_Segments;
            int curSegements = (int)(m_Segments * m_FillPercent);

            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;
            float outerRadius = rectTransform.pivot.x * tw;
            float innerRadius = rectTransform.pivot.x * tw - m_Thickness;

            Vector4 uv = overrideSprite != null ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;

            float uvCenterX = (uv.x + uv.z) * 0.5f;
            float uvCenterY = (uv.y + uv.w) * 0.5f;
            float uvScaleX = (uv.z - uv.x) / tw;
            float uvScaleY = (uv.w - uv.y) / th;

            float curDegree = 0;
            UIVertex uiVertex;
            int verticeCount;
            int triangleCount;
            Vector2 curVertice;

            if (m_Fill)
            {// Circle
                curVertice = Vector2.zero;
                verticeCount = curSegements + 1;
                uiVertex = new UIVertex();
                uiVertex.color = color;
                uiVertex.position = curVertice;
                uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                toFill.AddVert(uiVertex);

                for (int i = 1; i < verticeCount; i++)
                {
                    float cosA = Mathf.Cos(curDegree);
                    float sinA = Mathf.Sin(curDegree);
                    curVertice = new Vector2(cosA * outerRadius, sinA * outerRadius);
                    curDegree += degreeDelta;

                    uiVertex = new UIVertex();
                    uiVertex.color = color;
                    uiVertex.position = curVertice;
                    uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                    toFill.AddVert(uiVertex);

                    m_OuterVertices.Add(curVertice);
                }

                triangleCount = curSegements * 3;

                for (int i = 0, v = 1; i < triangleCount - 3; i += 3, v++)
                {
                    toFill.AddTriangle(v, 0, v + 1);
                }

                if (m_FillPercent == 1f)
                {// Connect head and tail
                    toFill.AddTriangle(verticeCount - 1, 0, 1);
                }
            }
            else
            {// Ring
                verticeCount = curSegements * 2;

                for (int i = 0; i < verticeCount; i += 2)
                {
                    float cosA = Mathf.Cos(curDegree);
                    float sinA = Mathf.Sin(curDegree);
                    curDegree += degreeDelta;

                    curVertice = new Vector3(cosA * innerRadius, sinA * innerRadius);
                    uiVertex = new UIVertex();
                    uiVertex.color = color;
                    uiVertex.position = curVertice;
                    uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                    toFill.AddVert(uiVertex);
                    m_InnerVertices.Add(curVertice);

                    curVertice = new Vector3(cosA * outerRadius, sinA * outerRadius);
                    uiVertex = new UIVertex();
                    uiVertex.color = color;
                    uiVertex.position = curVertice;
                    uiVertex.uv0 = new Vector2(curVertice.x * uvScaleX + uvCenterX, curVertice.y * uvScaleY + uvCenterY);
                    toFill.AddVert(uiVertex);
                    m_OuterVertices.Add(curVertice);
                }

                triangleCount = curSegements * 3 * 2;

                for (int i = 0, v = 0; i < triangleCount - 6; i += 6, v += 2)
                {
                    toFill.AddTriangle(v + 1, v, v + 3);
                    toFill.AddTriangle(v, v + 2, v + 3);
                }

                if (m_FillPercent == 1f)
                {// Connect head and tail
                    toFill.AddTriangle(verticeCount - 1, verticeCount - 2, 1);
                    toFill.AddTriangle(verticeCount - 2, 0, 1);
                }
            }
        }
    }
}
