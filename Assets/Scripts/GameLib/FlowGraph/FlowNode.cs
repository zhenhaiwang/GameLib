using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

namespace GameLib
{
    public class FlowNode
    {
        public FlowNodeType type;
        public int id;
        public float x;
        public float y;
        public float[] color;
        public List<int> linkList;
        public List<int> preList;
        public float delay;
        public bool wait;

        public enum State
        {
            Wait = 0,
            Execute,
            Finish,
        }

        [NonSerialized]
        private GameObject m_Prefab;
        [NonSerialized]
        private State m_State = State.Wait;
        [NonSerialized]
        private int m_PreFinishCount;
        [NonSerialized]
        private bool m_PropertyFold = true;

        [NonSerialized]
        public FlowGraph flowGraph;

        public GameObject prefab
        {
            get { return m_Prefab; }
            set { m_Prefab = type > FlowNodeType.Start && type < FlowNodeType.End ? value : null; }
        }

        protected GameObject actor
        {
            get
            {
                return flowGraph.actor;
            }
        }

        public virtual string NodeName
        {
            get { return "FlowNode"; }
        }

        public virtual float NodeWidth
        {
            get { return 150f; }
        }

        public virtual float NodeHeight
        {
            get { return 50f; }
        }

        #region Create Node

        private static FlowNode CreateOrLoadFromJson(FlowNodeType type, string json = null)
        {
            FlowNode node = null;

            bool fromJson = !string.IsNullOrEmpty(json);

            switch (type)
            {
                case FlowNodeType.Start:
                    {
                        if (fromJson)
                        {
                            node = JsonConvert.DeserializeObject<StartNode>(json) as StartNode;
                        }
                        else
                        {
                            node = new StartNode();
                        }
                    }
                    break;
                case FlowNodeType.SetPosition:
                    {
                        if (fromJson)
                        {
                            node = JsonConvert.DeserializeObject<SetPositionNode>(json) as SetPositionNode;
                        }
                        else
                        {
                            node = new SetPositionNode();
                        }
                    }
                    break;
                case FlowNodeType.End:
                    {
                        if (fromJson)
                        {
                            node = JsonConvert.DeserializeObject<EndNode>(json) as EndNode;
                        }
                        else
                        {
                            node = new EndNode();
                        }
                    }
                    break;
            }

            if (!fromJson)
            {
                node.type = type;
                node.color = new float[] { 1f, 1f, 1f, 1f };
                node.linkList = new List<int>();
                node.preList = new List<int>();
                node.delay = 0f;
                node.wait = true;
            }

            return node;
        }

        private static FlowNode Create(FlowNodeType type, int id, Vector2 position)
        {
            var node = CreateOrLoadFromJson(type);

            node.id = id;
            node.x = position.x;
            node.y = position.y;

            return node;
        }

        public static FlowNode CreateFromJson(string json)
        {
            var node = JsonConvert.DeserializeObject<FlowNode>(json) as FlowNode;

            return CreateOrLoadFromJson(node.type, json);
        }

        public static FlowNode CreateFromGraph(FlowGraph graph, FlowNodeType type, int id, Vector2 position)
        {
            var node = Create(type, id, position);

            node.flowGraph = graph;
            node.SetRectInGraph(graph, node.x, node.y);

            graph.AddNode(node);

            return node;
        }

        #endregion

        #region Graph Process

        public virtual IEnumerator OnExecute()
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            yield break;
        }

        public virtual bool CheckExecutable()
        {
            return true;
        }

        public void NotifyPreFinish()
        {
            m_PreFinishCount++;
        }

        public State GetCurState()
        {
            return m_State;
        }

        public bool StartExecute()
        {
            if (wait && preList != null)
            {
                if (m_PreFinishCount >= preList.Count)
                {
                    m_State = State.Execute;
                }
            }
            else
            {
                m_State = State.Execute;
            }

            return m_State == State.Execute;
        }

        public void FinishExecute()
        {
            m_State = State.Finish;

            Log.DebugFormat("[FlowNode] {0} execute finish, delay {1}s", NodeName, delay);
        }

        #endregion

        #region On Draw

        public virtual void OnDrawProperty()
        {
            GUILayout.Label(NodeName, EditorStyles.whiteLargeLabel);

            EditorGUILayout.Space();

            m_PropertyFold = EditorGUILayout.Foldout(m_PropertyFold, "Base Setting");

            if (m_PropertyFold)
            {
                EditorGUI.indentLevel++;

                if (type == FlowNodeType.Start)
                {
                    delay = EditorGUILayout.FloatField("Delay", delay);
                }
                else if (type == FlowNodeType.End)
                {
                    wait = EditorGUILayout.Toggle("Wait", wait);
                }
                else
                {
                    SetColor(EditorGUILayout.ColorField("Color", GetColor()));
                    delay = EditorGUILayout.FloatField("Delay", delay);
                    wait = EditorGUILayout.Toggle("Wait", wait);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
        }

        public virtual void OnDrawNode() { }

        #endregion

        public Rect GetRectInGraph(FlowGraph graph)
        {
            return new Rect(x + graph.graphOffset.x, y + graph.graphOffset.y, NodeWidth, NodeHeight);
        }

        public void SetRectInGraph(FlowGraph graph, Vector2 position)
        {
            SetRectInGraph(graph, position.x, position.y);
        }

        public void SetRectInGraph(FlowGraph graph, float xPos, float yPos)
        {
            x = xPos - graph.graphOffset.x;
            y = yPos - graph.graphOffset.y;
        }

        public void AddLinkNode(FlowNode linkNode)
        {
            if (linkNode != this && !linkList.Contains(linkNode.id))
            {
                linkList.Add(linkNode.id);
            }
        }

        public void RemoveLinkNode(FlowNode linkNode)
        {
            if (linkList.Contains(linkNode.id))
            {
                linkList.Remove(linkNode.id);
            }
        }

        public void AddPreNode(FlowNode preNode)
        {
            if (preNode != this && !preList.Contains(preNode.id))
            {
                preList.Add(preNode.id);
            }
        }

        public void RemovePreNode(FlowNode preNode)
        {
            if (preList.Contains(preNode.id))
            {
                preList.Remove(preNode.id);
            }
        }

        public Color GetColor()
        {
            return new Color(color[0], color[1], color[2], color[3]);
        }

        private void SetColor(Color color)
        {
            this.color[0] = color.r;
            this.color[1] = color.g;
            this.color[2] = color.b;
            this.color[3] = color.a;
        }
    }
}