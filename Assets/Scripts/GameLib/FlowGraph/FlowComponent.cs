using UnityEngine;

namespace GameLib
{
    public sealed class FlowComponent : MonoBehaviour
    {
        private enum TriggerType
        {
            None = 0,
            OnTriggerEnter,
            OnTriggerExit,
        }

        [SerializeField]
        private FlowGraph m_FlowGraph;
        //[SerializeField]
        //private TriggerType m_TriggerType = TriggerType.None;
        [SerializeField]
        private GameObject m_Actor;

        private void Awake()
        {
            if (m_FlowGraph == null)
            {
                Log.Error("[FlowComponent] flow graph is null");

                return;
            }

            m_FlowGraph.actor = m_Actor;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Execute();
            }
        }

        private void Execute()
        {
            FlowGraphExecutor.Execute(m_FlowGraph);
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (triggerType == TriggerType.OnTriggerEnter)
        //    {
        //        Execute();
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (triggerType == TriggerType.OnTriggerExit)
        //    {
        //        Execute();
        //    }
        //}
    }
}