using UnityEngine;
using UnityEditor;

namespace GameLib.Editor
{
    [CustomEditor(typeof(FlowGraph))]
    public class FlowGraphInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Edit"))
            {
                var window = FlowEditorWindow.Open();
                window.CreateGraph(target);
            }
        }
    }
}