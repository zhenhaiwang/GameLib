using UnityEngine;
using UnityEditor;

namespace GameLib.Editor
{
    [CustomEditor(typeof(LoopScrollRect), true)]
    public sealed class LoopScrollRectInspector : UnityEditor.Editor
    {
        private int m_Index;
        private float m_Speed = 3000f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            var loopScrollRect = target as LoopScrollRect;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear"))
            {
                loopScrollRect.ClearCells();
            }
            if (GUILayout.Button("Refresh"))
            {
                loopScrollRect.RefreshCells();
            }
            if (GUILayout.Button("Refill"))
            {
                loopScrollRect.RefillCells();
            }
            if (GUILayout.Button("RefillFromEnd"))
            {
                loopScrollRect.RefillCellsFromEnd();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 45;

            EditorGUILayout.BeginHorizontal();

            float width = EditorGUIUtility.currentViewWidth / 2f;

            m_Index = EditorGUILayout.IntField("Index", m_Index, GUILayout.Width(width));
            m_Speed = EditorGUILayout.FloatField("Speed", m_Speed, GUILayout.Width(width));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Scroll"))
            {
                loopScrollRect.ScrollToCell(m_Index, m_Speed);
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}