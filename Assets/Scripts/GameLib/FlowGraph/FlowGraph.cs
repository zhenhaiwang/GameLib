using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

namespace GameLib
{
    public sealed class FlowGraph : ScriptableObject
    {
        [HideInInspector]
        [SerializeField]
        private List<string> m_NodeJsonList;
        [HideInInspector]
        [SerializeField]
        private List<GameObject> m_PrefabList;
        [HideInInspector]
        [SerializeField]
        private Vector2 m_GraphOffset;

        [NonSerialized]
        private List<FlowNode> m_NodeList = new List<FlowNode>();
        [NonSerialized]
        private int m_NodeNextID;
        [NonSerialized]
        private bool m_Valid;

        public GameObject actor { get; set; }

        public List<string> nodeJsonList
        {
            get { return m_NodeJsonList; }
        }

        public List<GameObject> prefabList
        {
            get { return m_PrefabList; }
        }

        public Vector2 graphOffset
        {
            get { return m_GraphOffset; }
            set { m_GraphOffset = value; }
        }

        public List<FlowNode> nodeList
        {
            get { return m_NodeList; }
        }

        public int nodeCount
        {
            get { return m_NodeList.Count; }
        }

        public int nodeNextID
        {
            get
            {
                if (m_NodeNextID == 0)
                {
                    m_NodeNextID = nodeCount + 1;
                }

                return m_NodeNextID++;
            }
        }

        public bool valid
        {
            get { return m_Valid; }
        }

        public void AddNode(FlowNode node)
        {
            if (node == null)
            {
                return;
            }

            m_NodeList.Add(node);
        }

        public FlowNode GetNode(int nodeID)
        {
            return m_NodeList.Find(node => node.id == nodeID);
        }

        public void RemoveNode(FlowNode node)
        {
            foreach (var flowNode in m_NodeList)
            {
                flowNode.RemoveLinkNode(node);
                node.RemovePreNode(flowNode);
            }

            m_NodeList.Remove(node);
        }

        public HashSet<FlowNode> GetStartNodes()
        {
            HashSet<FlowNode> startNodeHs = null;

            foreach (var node in m_NodeList)
            {
                if (node.type == FlowNodeType.Start)
                {
                    if (startNodeHs == null)
                    {
                        startNodeHs = new HashSet<FlowNode>();
                    }

                    startNodeHs.Add(node);
                }
            }

            return startNodeHs;
        }

        public bool Initialize()
        {
            if (!m_Valid && m_NodeJsonList != null && m_PrefabList != null)
            {
                m_NodeNextID = 0;
                m_NodeList.Clear();

                int index = 0;

                foreach (string json in m_NodeJsonList)
                {
                    var node = FlowNode.CreateFromJson(json);
                    node.flowGraph = this;
                    node.prefab = m_PrefabList[index++];
                    m_NodeList.Add(node);
                }

                return m_Valid = true;
            }

            return false;
        }

        public static FlowGraph Load(string path)
        {
            var graph = AssetDatabase.LoadAssetAtPath<FlowGraph>(path);

            if (graph != null)
            {
                graph.Initialize();
            }

            return graph;
        }

        public static FlowGraph LoadFromAsset(Object graphAsset)
        {
            return Load(AssetDatabase.GetAssetPath(graphAsset));
        }

        public Object Save(string path, bool create)
        {
            m_NodeJsonList = new List<string>();
            m_PrefabList = new List<GameObject>();

            foreach (var node in m_NodeList)
            {
                m_NodeJsonList.Add(JsonConvert.SerializeObject(node, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
                m_PrefabList.Add(node.prefab);
            }

            if (create)
            {
                AssetDatabase.CreateAsset(this, path);
            }
            else
            {
                EditorUtility.SetDirty(this);
            }

            return this;
        }
    }
}