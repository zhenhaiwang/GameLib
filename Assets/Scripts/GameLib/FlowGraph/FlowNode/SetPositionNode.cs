using System.Collections;
using UnityEngine;
using UnityEditor;

namespace GameLib
{
    public sealed class SetPositionNode : FlowNode
    {
        public Vector3 targetPosition = Vector3.zero;

        public override string NodeName
        {
            get { return "SetPosition"; }
        }

        public override void OnDrawProperty()
        {
            base.OnDrawProperty();

            targetPosition = EditorGUILayout.Vector3Field("Target Position", targetPosition);
        }

        public override void OnDrawNode()
        {
            base.OnDrawNode();
        }

        public override IEnumerator OnExecute()
        {
            yield return base.OnExecute();

            if (actor != null)
            {
                actor.transform.localPosition = targetPosition;
            }

            FinishExecute();
        }

        public override bool CheckExecutable()
        {
            return base.CheckExecutable();
        }
    }
}