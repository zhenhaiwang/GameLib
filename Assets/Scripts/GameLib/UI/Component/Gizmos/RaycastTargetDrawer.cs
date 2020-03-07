#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    public sealed class RaycastTargetDrawer : MonoBehaviour
    {
        public Color lineColor = Color.red;

        private static Vector3[] m_FourCorners = new Vector3[4];

        private void OnDrawGizmos()
        {
            foreach (var graphic in FindObjectsOfType<MaskableGraphic>())
            {
                if (graphic.raycastTarget)
                {
                    var rectTransform = graphic.transform as RectTransform;
                    rectTransform.GetWorldCorners(m_FourCorners);

                    Gizmos.color = lineColor;

                    for (int i = 0; i < 4; i++)
                    {
                        Gizmos.DrawLine(m_FourCorners[i], m_FourCorners[(i + 1) % 4]);
                    }
                }
            }
        }
    }
}
#endif