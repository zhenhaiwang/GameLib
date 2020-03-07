using System.IO;
using UnityEngine;
using UnityEditor;

namespace GameLib.Editor
{
    public sealed class FlowEditorWindow : EditorWindow
    {
        // Path
        private const string GRAPH_FILE_PATH = "Assets/Resources/Flow/";
        private const string RESOURCE_PATH = "Assets/Editor/FlowEditor/__Resources/";
        // Window
        private const float WINDOW_MIN_WIDTH = 1280f;
        private const float WINDOW_MIN_HEIGHT = 720f;
        // Splitter
        private const float SPLITTER_WIDTH = 4f;
        // Inspector
        private const float INSPECTOR_MIN_WIDTH = 250f;
        // MiniMap
        private const float MINIMAP_SCALE = 0.1f;
        // Texture2D
        private const float LINK_ICON_WIDTH = 16f;

        private static Texture2D texLinkin;
        private static Texture2D texLinkout;
        private static Texture2D texUnlink;

        // GUIStyle
        private static GUISkin windowSkin;
        private static GUIStyle iconButtonStyle;
        private static Color nodeSelectedColor = Color.red;
        // Rect
        private Rect m_RectMain = new Rect(0f, 0f, WINDOW_MIN_WIDTH - INSPECTOR_MIN_WIDTH - SPLITTER_WIDTH, WINDOW_MIN_HEIGHT);
        private Rect m_RectInspector = new Rect(WINDOW_MIN_WIDTH - INSPECTOR_MIN_WIDTH, 0f, INSPECTOR_MIN_WIDTH, WINDOW_MIN_HEIGHT);
        private Rect m_RectSplitter = new Rect(WINDOW_MIN_WIDTH - INSPECTOR_MIN_WIDTH - SPLITTER_WIDTH, 0f, SPLITTER_WIDTH, WINDOW_MIN_HEIGHT);

        private Vector2 m_InspectorScroll = Vector2.zero;

        private float m_SplitterX;
        private float m_PreWindowWidth;

        private bool m_MainDragging;
        private bool m_SplitterDragging;

        private FlowGraph m_CurFlowGraph;
        private FlowNode m_CurSelectFlowNode;
        private FlowNode m_CurLinkingFlowNode;

        private string m_CurAssetPath = "";
        private Object m_CurSelectAsset;

        [MenuItem("Window/Flow Editor", priority = 3)]
        public static FlowEditorWindow Open()
        {
            var window = GetWindow<FlowEditorWindow>("Flow Editor", true);
            window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
            window.wantsMouseMove = true;
            return window;
        }

        private void OnGUI()
        {
            LoadResources();
            SetGUIStyle();

            HandleWindowSizeChanged();

            GUILayout.BeginArea(m_RectMain);
            DrawMain();
            GUILayout.EndArea();

            GUI.Box(m_RectSplitter, GUIContent.none);

            GUILayout.BeginArea(m_RectInspector);
            DrawInspector();
            GUILayout.EndArea();

            HandleEvents();
        }

        private void OnLostFocus()
        {
            m_CurLinkingFlowNode = null;
            m_MainDragging = false;
            m_SplitterDragging = false;
        }

        private void OnDestroy()
        {
            SaveGraph();
        }

        private void LoadResources()
        {
            if (texLinkin == null)
            {
                texLinkin = AssetDatabase.LoadAssetAtPath(RESOURCE_PATH + "linkin.png", typeof(Texture2D)) as Texture2D;
            }
            if (texLinkout == null)
            {
                texLinkout = AssetDatabase.LoadAssetAtPath(RESOURCE_PATH + "linkout.png", typeof(Texture2D)) as Texture2D;
            }
            if (texUnlink == null)
            {
                texUnlink = AssetDatabase.LoadAssetAtPath(RESOURCE_PATH + "unlink.png", typeof(Texture2D)) as Texture2D;
            }
        }

        private void SetGUIStyle()
        {
            if (windowSkin == null)
            {
                windowSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

                iconButtonStyle = new GUIStyle
                {
                    name = "ButtonStyle",
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    fixedWidth = 16,
                    fixedHeight = 16,
                    imagePosition = ImagePosition.ImageOnly,
                };

                GUIStyle[] customStyles = windowSkin.customStyles;
                ArrayUtility.Add(ref customStyles, iconButtonStyle);
                windowSkin.customStyles = customStyles;
            }

            GUI.skin = windowSkin;
        }

        #region Handle

        private void HandleWindowSizeChanged()
        {
            float width = position.width;
            float height = position.height;

            if (width != m_PreWindowWidth)
            {
                MoveSplitter(width - m_PreWindowWidth);
                m_PreWindowWidth = width;
            }

            m_RectMain = new Rect(0f, 0f, m_SplitterX, height);
            m_RectInspector = new Rect(m_SplitterX + SPLITTER_WIDTH, 0f, width - m_SplitterX - SPLITTER_WIDTH, height);
            m_RectSplitter = new Rect(m_SplitterX, 0f, SPLITTER_WIDTH, height);
        }

        private void HandleEvents()
        {
            if (Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        {
                            if (m_CurLinkingFlowNode != null)
                            {
                                m_CurLinkingFlowNode = null;
                                Event.current.Use();
                                Repaint();
                            }
                            else
                            {
                                if (Event.current.button == 0)
                                {
                                    if (m_RectMain.Contains(Event.current.mousePosition))
                                    {
                                        m_MainDragging = true;
                                        GUI.FocusControl("");
                                        Event.current.Use();
                                    }
                                    else if (m_RectSplitter.Contains(Event.current.mousePosition))
                                    {
                                        m_SplitterDragging = true;
                                        Event.current.Use();
                                    }
                                    else if (m_RectInspector.Contains(Event.current.mousePosition))
                                    {
                                        GUI.FocusControl("");
                                        Event.current.Use();
                                    }
                                }
                                else if (Event.current.button == 1)
                                {
                                    GUI.FocusControl("");

                                    if (m_RectMain.Contains(Event.current.mousePosition))
                                    {
                                        HandlePopMenu();
                                        Event.current.Use();
                                        Repaint();
                                    }
                                    else if (m_RectInspector.Contains(Event.current.mousePosition))
                                    {
                                        Event.current.Use();
                                    }
                                }
                            }
                        }
                        break;
                    case EventType.MouseDrag:
                        {
                            if (m_MainDragging)
                            {
                                m_CurFlowGraph.graphOffset += new Vector2(Event.current.delta.x, Event.current.delta.y);
                                Event.current.Use();
                            }
                            else if (m_SplitterDragging)
                            {
                                MoveSplitter(Event.current.delta.x);
                                Event.current.Use();
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        {
                            m_MainDragging = false;
                            m_SplitterDragging = false;
                            Event.current.Use();
                        }
                        break;
                    case EventType.KeyDown:
                        {
                            if (!GUI.changed && m_CurSelectFlowNode != null)
                            {
                                if (Event.current.keyCode == KeyCode.Delete)
                                {
                                    DeleteNodeInGraph();
                                    Event.current.Use();
                                    Repaint();
                                }

                                if (Event.current.keyCode == KeyCode.Escape)
                                {
                                    m_CurSelectFlowNode = null;
                                    Event.current.Use();
                                    Repaint();
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void HandlePopMenu()
        {
            if (m_CurFlowGraph == null) return;

            var menu = new GenericMenu();

            for (var type = FlowNodeType.Start; type < FlowNodeType.Count; type++)
            {
                if (type == FlowNodeType.End) menu.AddSeparator("");

                menu.AddItem(new GUIContent(type.ToString()), false, HandleClickMenuItem, new object[] { type, Event.current.mousePosition });

                if (type == FlowNodeType.Start) menu.AddSeparator("");
            }

            menu.ShowAsContext();
        }

        private void HandleClickMenuItem(object args)
        {
            var argArray = args as object[];

            var type = (FlowNodeType)argArray[0];
            var mousePosition = (Vector2)argArray[1];

            FlowNode.CreateFromGraph(m_CurFlowGraph, type, m_CurFlowGraph.nodeNextID, new Vector2(mousePosition.x, mousePosition.y));
        }

        #endregion

        #region Draw

        private void DrawMain()
        {
            if (m_CurFlowGraph == null)
            {
                if (GUI.Button(new Rect(m_SplitterX / 2f - 50f, position.height / 2f - 15f, 100f, 30f), "Create"))
                {
                    m_CurFlowGraph = CreateInstance<FlowGraph>();
                }
            }
            else
            {
                DrawMiniMap();

                if (m_CurFlowGraph.nodeCount > 0)
                {
                    Handles.BeginGUI();

                    foreach (var node in m_CurFlowGraph.nodeList)
                    {
                        if (node == null)
                        {
                            Debug.Log("[FlowEditorWindow] node is null");

                            continue;
                        }

                        if (node.linkList == null)
                        {
                            continue;
                        }

                        FlowNode deleteNode = null;

                        foreach (int linkId in node.linkList)
                        {
                            var linkNode = m_CurFlowGraph.GetNode(linkId);
                            var nodeRect = node.GetRectInGraph(m_CurFlowGraph);
                            var linkRect = linkNode.GetRectInGraph(m_CurFlowGraph);

                            if (DrawBezier(new Vector2(nodeRect.x + nodeRect.width, nodeRect.y + LINK_ICON_WIDTH / 2f), new Vector2(linkRect.x, linkRect.y + LINK_ICON_WIDTH / 2f), Color.yellow))
                            {
                                deleteNode = linkNode;
                            }
                        }

                        if (deleteNode != null)
                        {
                            node.RemoveLinkNode(deleteNode);
                            deleteNode.RemovePreNode(node);
                        }
                    }

                    Handles.EndGUI();
                }

                BeginWindows();

                var nodeList = m_CurFlowGraph.nodeList;
                int nodeCount = m_CurFlowGraph.nodeCount;

                for (int i = 0; i < nodeCount; i++)
                {
                    var node = nodeList[i];

                    var rect = node.GetRectInGraph(m_CurFlowGraph);
                    var topLeft = new Vector2(rect.x, rect.y);
                    var topRight = new Vector2(rect.x + node.NodeWidth, rect.y);
                    var bottomLeft = new Vector2(rect.x, rect.y + node.NodeHeight);
                    var bottomRight = new Vector2(rect.x + node.NodeWidth, rect.y + node.NodeHeight);

                    if (m_RectMain.Contains(topLeft) ||
                        m_RectMain.Contains(topRight) ||
                        m_RectMain.Contains(bottomLeft) ||
                        m_RectMain.Contains(bottomRight))
                    {
                        if (node == m_CurSelectFlowNode)
                        {
                            GUI.color = nodeSelectedColor;
                        }
                        else
                        {
                            GUI.color = node.GetColor();
                        }

                        rect = GUI.Window(node.id, rect, DrawNode, node.NodeName);

                        GUI.color = Color.white;
                    }

                    node.SetRectInGraph(m_CurFlowGraph, rect.position);
                }

                DrawLinking();

                EndWindows();
            }
        }

        private void DrawInspector()
        {
            DrawObjectField();

            if (m_CurFlowGraph == null) return;

            EditorGUILayout.Space();

            if (GUILayout.Button("Save"))
            {
                SaveGraph();
            }

            m_InspectorScroll = GUILayout.BeginScrollView(m_InspectorScroll, GUILayout.Width(m_RectInspector.width), GUILayout.Height(m_RectInspector.height - 30f));
            {
                if (m_CurSelectFlowNode != null)
                {
                    m_CurSelectFlowNode.OnDrawProperty();
                }
            }

            GUILayout.EndScrollView();
        }

        private void DrawMiniMap()
        {
            if (!m_MainDragging) return;

            var preColor = GUI.color;
            var mapColor = GUI.color * new Color(1f, 1f, 1f, 0.05f);

            var mapCenter = new Vector2(m_RectMain.x + m_RectMain.width * 0.15f, m_RectMain.y + m_RectMain.height * 0.15f);
            mapCenter.x -= m_RectMain.width * MINIMAP_SCALE / 2f;
            mapCenter.y -= m_RectMain.height * MINIMAP_SCALE / 2f;

            GUI.color = mapColor;

            GUI.Box(new Rect(mapCenter.x, mapCenter.y, m_RectMain.width * MINIMAP_SCALE, m_RectMain.height * MINIMAP_SCALE), "");

            foreach (var node in m_CurFlowGraph.nodeList)
            {
                var rect = node.GetRectInGraph(m_CurFlowGraph);
                var nodeCenter = new Vector2(rect.x, rect.y);
                var nodeSize = new Vector2(rect.width, rect.height);

                nodeCenter *= MINIMAP_SCALE;
                nodeSize *= MINIMAP_SCALE;
                nodeCenter += mapCenter;

                GUI.color = node.GetColor();
                GUI.Box(new Rect(nodeCenter.x, nodeCenter.y, nodeSize.x, nodeSize.y), GUIContent.none);
            }

            GUI.color = preColor;
        }

        private void DrawObjectField()
        {
            var newSelectAsset = EditorGUILayout.ObjectField(m_CurSelectAsset, typeof(Object), false);

            if (newSelectAsset != m_CurSelectAsset)
            {
                if (newSelectAsset != null)
                {
                    CreateGraph(newSelectAsset);
                }
                else
                {
                    ClearGraph();
                }
            }
        }

        private void DrawNode(int id)
        {
            var node = m_CurFlowGraph.GetNode(id);

            node.OnDrawNode();

            if (node.type != FlowNodeType.End)
            {
                if (GUI.Button(new Rect(node.NodeWidth - LINK_ICON_WIDTH, 0f, LINK_ICON_WIDTH, LINK_ICON_WIDTH), new GUIContent(texLinkout), iconButtonStyle))
                {
                    m_CurLinkingFlowNode = node;
                }
            }

            if (node.type != FlowNodeType.Start)
            {
                if (GUI.Button(new Rect(0f, 0f, LINK_ICON_WIDTH, LINK_ICON_WIDTH), new GUIContent(texLinkin), iconButtonStyle))
                {
                    if (m_CurLinkingFlowNode != null)
                    {
                        m_CurLinkingFlowNode.AddLinkNode(node);
                        node.AddPreNode(m_CurLinkingFlowNode);
                        m_CurLinkingFlowNode = null;
                    }
                }
            }

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
            {
                m_CurSelectFlowNode = node;
                GUI.FocusWindow(id);
            }

            GUI.DragWindow();
        }

        private void DrawLinking()
        {
            if (m_CurLinkingFlowNode == null || Event.current == null) return;

            var nodeRect = m_CurLinkingFlowNode.GetRectInGraph(m_CurFlowGraph);

            DrawBezier(new Vector2(nodeRect.x + nodeRect.width, nodeRect.y + LINK_ICON_WIDTH / 2f), Event.current.mousePosition, Color.white);

            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }

        private bool DrawBezier(Vector3 startPos, Vector3 endPos, Color color)
        {
            float left = startPos.x < endPos.x ? startPos.x : endPos.x;
            float right = startPos.x > endPos.x ? startPos.x : endPos.x;
            float top = startPos.y < endPos.y ? startPos.y : endPos.y;
            float bottom = startPos.y > endPos.y ? startPos.y : endPos.y;

            var bounds = new Rect(left, top, right - left, bottom - top);

            if (bounds.xMin > m_RectMain.xMax ||
                bounds.xMax < m_RectMain.xMin ||
                bounds.yMin > m_RectMain.yMax ||
                bounds.yMax < m_RectMain.yMin)
            {
                return false;
            }

            float distance = Mathf.Abs(startPos.x - endPos.x);
            var startTangent = new Vector3(startPos.x + distance / 2.5f, startPos.y);
            var endTangent = new Vector3(endPos.x - distance / 2.5f, endPos.y);

            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, null, 4f);

            var deleteRect = new Rect(startPos.x + (endPos.x - startPos.x) * 0.5f - LINK_ICON_WIDTH / 2f, startPos.y + (endPos.y - startPos.y) * 0.5f - LINK_ICON_WIDTH / 2f, LINK_ICON_WIDTH, LINK_ICON_WIDTH);

            if (GUI.Button(deleteRect, new GUIContent(texUnlink), iconButtonStyle))
            {
                return true;
            }

            return false;
        }
        #endregion

        private void MoveSplitter(float deltaX)
        {
            m_SplitterX += deltaX;

            float curWindowWidth = position.width;

            if (m_SplitterX > curWindowWidth - INSPECTOR_MIN_WIDTH)
            {
                m_SplitterX = curWindowWidth - INSPECTOR_MIN_WIDTH;
            }
            else if (m_SplitterX < curWindowWidth / 2)
            {
                m_SplitterX = curWindowWidth / 2;
            }
        }

        private bool DeleteNodeInGraph()
        {
            if (m_CurSelectFlowNode != null)
            {
                m_CurFlowGraph.RemoveNode(m_CurSelectFlowNode);
                m_CurSelectFlowNode = null;

                return true;
            }

            return false;
        }

        private void ClearGraph()
        {
            m_CurSelectAsset = null;
            m_CurFlowGraph = null;
            m_CurAssetPath = "";
        }

        public void CreateGraph(Object asset)
        {
            m_CurSelectAsset = asset;
            m_CurAssetPath = AssetDatabase.GetAssetPath(m_CurSelectAsset);
            m_CurFlowGraph = FlowGraph.LoadFromAsset(m_CurSelectAsset);
        }

        private void SaveGraph()
        {
            if (m_CurFlowGraph != null)
            {
                if (m_CurSelectAsset == null)
                {
                    string path = EditorUtility.SaveFilePanel(
                        "Save flow graph as asset",
                        GRAPH_FILE_PATH,
                        "Flow Graph.asset",
                        "asset");

                    if (path.Length > 0)
                    {
                        m_CurAssetPath = GRAPH_FILE_PATH + Path.GetFileName(path);
                        m_CurSelectAsset = m_CurFlowGraph.Save(m_CurAssetPath, true);
                    }
                }
                else
                {
                    m_CurFlowGraph.Save(m_CurAssetPath, false);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}