using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace GameLib.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(UICircleImage), true)]
    public class UICircleImageEditor : ImageEditor
    {
        private SerializedProperty m_FillPercent;
        private SerializedProperty m_Fill;
        private SerializedProperty m_Thickness;
        private SerializedProperty m_Segements;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_FillPercent = serializedObject.FindProperty("m_FillPercent");
            m_Fill = serializedObject.FindProperty("m_Fill");
            m_Thickness = serializedObject.FindProperty("m_Thickness");
            m_Segements = serializedObject.FindProperty("m_Segments");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(m_FillPercent, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_Fill, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_Thickness, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_Segements, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
