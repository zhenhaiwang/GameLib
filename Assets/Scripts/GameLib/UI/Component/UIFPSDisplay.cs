using UnityEngine;

namespace GameLib
{
    public sealed class FPSDisplay : MonoBehaviour
    {
        private float m_DeltaTime;

        private GUIStyle m_Style;
        private Rect m_Rect;

        private void Start()
        {
            m_Rect = new Rect(0, 0, Screen.width, Screen.height * 2 / 100);

            m_Style = new GUIStyle();
            m_Style.alignment = TextAnchor.UpperRight;
            m_Style.fontSize = Screen.height * 2 / 75;
            m_Style.normal.textColor = new Color(0f, 1f, 0f, 1f);
        }

        private void Update()
        {
            m_DeltaTime += (Time.deltaTime - m_DeltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            float ms = m_DeltaTime * 1000f;
            float fps = 1f / m_DeltaTime;

            GUI.Label(m_Rect, string.Format("{0:0.0} ms ({1:0.} fps)", ms, fps), m_Style);
        }
    }
}