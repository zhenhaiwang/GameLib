using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace GameLib.Editor
{
    public sealed class FontChanger : EditorWindow
    {
        [MenuItem("Window/Font Changer", priority = 3)]
        private static void ShowWindow()
        {
            var window = GetWindow<FontChanger>(true, "Window/Font Changer");
            window.minSize = new Vector2(150f, 100f);
            window.Show();
            window.Focus();
        }

        Font m_DefaultFont;
        Font m_TargetFont;

        private void OnEnable()
        {
            m_DefaultFont = new Font("Arial");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("Target Font:");

            m_TargetFont = m_DefaultFont = (Font)EditorGUILayout.ObjectField(m_DefaultFont, typeof(Font), true, GUILayout.MinWidth(100f));

            if (GUILayout.Button("OK"))
            {
                ChangeFont();
            }
        }

        void ChangeFont()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                return;
            }

            var labels = Selection.GetFiltered(typeof(Text), SelectionMode.Deep);

            foreach (Object item in labels)
            {
                var label = item as Text;
                label.font = m_TargetFont;

                EditorUtility.SetDirty(item);
            }
        }
    }
}
