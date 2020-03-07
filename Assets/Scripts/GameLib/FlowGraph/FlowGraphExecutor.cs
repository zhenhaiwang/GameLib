using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameLib
{
    public class FlowGraphExecutor : MonoBehaviour
    {
        private const string FLOW_ROOT_NAME = "FlowRoot";
        private const string FLOW_GRAPH_NAME = "FlowGraph";

        private static GameObject flowRoot;

        public static FlowGraphExecutor Execute(Object flowGraphAsset, Action finishCallback = null)
        {
            return Execute(FlowGraph.LoadFromAsset(flowGraphAsset), finishCallback);
        }

        public static FlowGraphExecutor Execute(string flowGraphPath, Action finishCallback = null)
        {
            return Execute(FlowGraph.Load(flowGraphPath), finishCallback);
        }

        public static FlowGraphExecutor Execute(FlowGraph flowGraph, Action finishCallback = null)
        {
            if (flowGraph == null || !flowGraph.Initialize())
            {
                Log.Error("[FlowGraphExecutor] flow graph is null or invalid");

                return null;
            }

            var executor = GetOrCreateFlowGraphExecutor();

            executor.m_FlowGraph = flowGraph;
            executor.m_OnFinishDelegate = finishCallback;

            return executor.Execute();
        }

        private static FlowGraphExecutor GetOrCreateFlowGraphExecutor()
        {
            if (flowRoot == null)
            {
                flowRoot = new GameObject(FLOW_ROOT_NAME);
            }

            var flowGraphObject = new GameObject(FLOW_GRAPH_NAME);

            UnityUtil.SetChild(flowRoot, flowGraphObject);

            return flowGraphObject.AddComponent<FlowGraphExecutor>();
        }

        private FlowGraph m_FlowGraph;
        private Action m_OnFinishDelegate;

        private HashSet<FlowNode> m_CurNodeHs;
        private HashSet<FlowNode> m_NextNodeHs;
        private HashSet<FlowNode> m_FinishedNodeHs;

        public FlowGraphExecutor Execute()
        {
            m_CurNodeHs = m_FlowGraph.GetStartNodes();

            if (m_CurNodeHs != null)
            {
                StartCoroutine(ProcessGraph());
            }

            return this;
        }

        private IEnumerator ProcessGraph()
        {
            m_NextNodeHs = new HashSet<FlowNode>();
            m_FinishedNodeHs = new HashSet<FlowNode>();

            bool finished = false;

            while (m_CurNodeHs.Count > 0)
            {
                m_NextNodeHs.Clear();
                m_FinishedNodeHs.Clear();

                foreach (var curNode in m_CurNodeHs)
                {
                    var curState = curNode.GetCurState();

                    if (curState == FlowNode.State.Wait)
                    {
                        if (curNode.CheckExecutable() && curNode.StartExecute())
                        {
                            StartCoroutine(curNode.OnExecute());
                        }
                    }
                    else if (curState == FlowNode.State.Finish)
                    {
                        m_FinishedNodeHs.Add(curNode);

                        if (curNode.type == FlowNodeType.End)
                        {
                            finished = true;
                        }
                        else
                        {
                            int linkCount = curNode.linkList != null ? curNode.linkList.Count : 0;

                            for (int i = 0; i < linkCount; i++)
                            {
                                var linkNode = m_FlowGraph.GetNode(curNode.linkList[i]);

                                if (linkNode.GetCurState() == FlowNode.State.Wait)
                                {
                                    linkNode.NotifyPreFinish();
                                    m_NextNodeHs.Add(linkNode);
                                }
                            }
                        }
                    }
                }

                if (finished)
                {
                    break;
                }

                foreach (var finishedNode in m_FinishedNodeHs)
                {
                    m_CurNodeHs.Remove(finishedNode);
                }

                foreach (var nextNode in m_NextNodeHs)
                {
                    m_CurNodeHs.Add(nextNode);
                }

                yield return null;
            }

            m_OnFinishDelegate.Call();

            Destroy(gameObject);
        }
    }
}