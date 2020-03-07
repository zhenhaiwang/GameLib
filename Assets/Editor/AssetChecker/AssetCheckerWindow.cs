using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace GameLib.Editor
{
    public sealed class AssetCheckerWindow : EditorWindow
    {
        private static readonly string texturePath = "Assets/Editor/AssetChecker/__Resources/";

        private static Texture2D collapseTexture2D;
        private static Texture2D expandTexture2D;

        private static Dictionary<string, AssetInfo> assetInfoDict = new Dictionary<string, AssetInfo>();
        private static Dictionary<string, HashSet<string>> assetRefDict = new Dictionary<string, HashSet<string>>();

        private static AssetInfoComparer comparer = new AssetInfoComparer();

        private Object m_ClickObject;
        private Object m_SelectObject;

        private Vector2 m_ScrollViewRect = Vector2.zero;
        private GUIStyle m_TextStyle;

        private AssetInfo m_AssetRef;

        [MenuItem("Window/Asset Checker", priority = 2)]
        private static void Open()
        {
            var window = GetWindow<AssetCheckerWindow>("Asset Checker", true);
            window.wantsMouseMove = true;
            window.minSize = new Vector2(400f, 200f);
        }

        private void OnGUI()
        {
            float width = position.width;
            float height = position.height;
            int assetCount = assetInfoDict.Count;

            var newSelectObject = EditorGUILayout.ObjectField(m_SelectObject, typeof(Object), false);

            EditorGUILayout.BeginHorizontal();

            if (GUI.Button(new Rect(width / 2f - 60f, 25f, 120f, 30f), string.Format("Update <{0}>", assetCount)))
            {
                UpdateAssets();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginArea(new Rect(5f, 55f, width - 5f, height - 30f));

            EditorGUILayout.Separator();

            m_ScrollViewRect = EditorGUILayout.BeginScrollView(m_ScrollViewRect, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (newSelectObject != null && newSelectObject != m_SelectObject && assetCount > 0)
            {
                m_AssetRef = GetAssetInfo(m_SelectObject = newSelectObject);
            }

            if (m_AssetRef != null)
            {
                int index = -1;

                UpdateScrollView(m_AssetRef, 0, ref index);
            }

            EditorGUILayout.EndScrollView();

            GUILayout.EndArea();

            OnClickAsset();
        }

        private void OnClickAsset()
        {
            if (m_ClickObject != null)
            {
                var curObject = m_ClickObject;

                m_ClickObject = null;

                if (AssetDatabase.Contains(curObject))
                {
                    EditorApplication.ExecuteMenuItem("Window/Project");
                    EditorUtility.FocusProjectWindow();
                }

                if (curObject is GameObject && AssetDatabase.Contains(curObject))
                {
                    var go = curObject as GameObject;

                    while (go.transform.parent != null)
                    {
                        go = go.transform.parent.gameObject;

                        ProjectWindowUtil.ShowCreatedAsset(go);
                        EditorGUIUtility.PingObject(go);
                    }
                }

                Selection.activeObject = curObject;
                EditorGUIUtility.PingObject(curObject);
            }
        }

        private void UpdateScrollView(AssetInfo asset, int next, ref int index)
        {
            if (string.IsNullOrEmpty(asset.GUID))
            {
                return;
            }

            index++;

            var references = GetAssetRefs(asset.GUID);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(24f));
            GUILayout.Space(next * 24f);

            int refCount = references.Count;
            bool opened = asset.opened;

            if (refCount > 0)
            {
                var style = GUIStyle.none;

                style.margin.top = 5;

                if (collapseTexture2D == null)
                {
                    collapseTexture2D = AssetDatabase.LoadAssetAtPath(texturePath + "collapse.png", typeof(Texture2D)) as Texture2D;
                }

                if (expandTexture2D == null)
                {
                    expandTexture2D = AssetDatabase.LoadAssetAtPath(texturePath + "expand.png", typeof(Texture2D)) as Texture2D;
                }

                if (GUILayout.Button(opened ? collapseTexture2D : expandTexture2D, style, GUILayout.Width(24f)))
                {
                    opened = !opened;
                    asset.opened = opened;
                }
            }
            else
            {
                GUILayout.Space(28f);
            }

            if (m_TextStyle == null)
            {
                m_TextStyle = new GUIStyle { richText = true, margin = new RectOffset(0, 0, 5, 0), normal = { textColor = Color.white } };
            }

            var icon = GetIcon(asset.path);

            GUILayout.Label(icon, GUILayout.Width(20f), GUILayout.Height(20f));
            GUILayout.Label(string.Format("{0} <b><color=#00ff00ff>{1} 个引用</color></b>", Path.GetFileNameWithoutExtension(asset.path), refCount), m_TextStyle, GUILayout.Width(200f));
            GUILayout.Label(asset.path, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            if (Event.current.clickCount == 2 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                m_ClickObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset.GUID), typeof(Object));
            }

            if (opened)
            {
                foreach (var reference in references)
                {
                    UpdateScrollView(reference, next + 1, ref index);
                }
            }
        }

        private void UpdateAssets()
        {
            assetInfoDict.Clear();

            string[] assetPaths = AssetDatabase.GetAllAssetPaths();
            if (assetPaths == null) return;

            int total = assetPaths.Length;
            int current = 0;

            foreach (string assetPath in assetPaths)
            {
                if (++current > total)
                {
                    current = total;
                }

                if (EditorUtility.DisplayCancelableProgressBar("Update", "Please wait ... " + current.ToString() + "/" + total.ToString(), (float)current / total))
                {
                    EditorUtility.ClearProgressBar();
                    assetInfoDict.Clear();

                    break;
                }

                string guid = AssetDatabase.AssetPathToGUID(assetPath);

                InsertToAssetInfoDict(guid, assetPath);

                string[] depPaths = AssetDatabase.GetDependencies(new[] { assetPath });

                if (depPaths != null && depPaths.Length > 0)
                {
                    foreach (string depPath in depPaths)
                    {
                        if (!string.IsNullOrEmpty(depPath))
                        {
                            string depGuid = AssetDatabase.AssetPathToGUID(depPath);

                            InsertToAssetInfoDict(depGuid, depPath);

                            if (guid != depGuid)
                            {
                                if (assetRefDict.ContainsKey(depGuid))
                                {
                                    assetRefDict[depGuid].Add(guid);
                                }
                                else
                                {
                                    var depHash = new HashSet<string>();
                                    depHash.Add(guid);
                                    assetRefDict.Add(depGuid, depHash);
                                }
                            }
                        }
                    }
                }

                if (current == total)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private AssetInfo GetAssetInfo(Object selectObject)
        {
            string path = AssetDatabase.GetAssetPath(selectObject);
            string guid = AssetDatabase.AssetPathToGUID(path);

            if (assetInfoDict.ContainsKey(guid))
            {
                return assetInfoDict[guid];
            }

            return null;
        }

        private static List<AssetInfo> GetAssetRefs(string guid)
        {
            var refList = new List<AssetInfo>();

            if (assetRefDict.ContainsKey(guid))
            {
                foreach (string refGuid in assetRefDict[guid])
                {
                    if (assetInfoDict.ContainsKey(refGuid))
                    {
                        refList.Add(assetInfoDict[refGuid]);
                    }
                }
            }

            refList.Sort(comparer);

            return refList;
        }

        private static void InsertToAssetInfoDict(string guid, string path)
        {
            if (!assetInfoDict.ContainsKey(guid))
            {
                assetInfoDict.Add(guid, new AssetInfo(guid, path));
            }
        }

        private static Texture2D GetIcon(string fileName)
        {
            var lastDot = fileName.LastIndexOf('.');
            var extension = (lastDot != -1) ? fileName.Substring(lastDot + 1).ToLower() : string.Empty;

            switch (extension)
            {
                case "boo":
                    return EditorGUIUtility.FindTexture("boo Script Icon");
                case "cginc":
                    return EditorGUIUtility.FindTexture("CGProgram Icon");
                case "cs":
                    return EditorGUIUtility.FindTexture("cs Script Icon");
                case "guiskin":
                    return EditorGUIUtility.FindTexture("GUISkin Icon");
                case "js":
                    return EditorGUIUtility.FindTexture("Js Script Icon");
                case "mat":
                    return EditorGUIUtility.FindTexture("Material Icon");
                case "prefab":
                    return EditorGUIUtility.FindTexture("PrefabNormal Icon");
                case "shader":
                    return EditorGUIUtility.FindTexture("Shader Icon");
                case "txt":
                    return EditorGUIUtility.FindTexture("TextAsset Icon");
                case "unity":
                    return EditorGUIUtility.FindTexture("SceneAsset Icon");
                case "asset":
                case "prefs":
                    return EditorGUIUtility.FindTexture("GameManager Icon");
                case "anim":
                    return EditorGUIUtility.FindTexture("Animation Icon");
                case "meta":
                    return EditorGUIUtility.FindTexture("MetaFile Icon");
                case "ttf":
                case "otf":
                case "fon":
                case "fnt":
                    return EditorGUIUtility.FindTexture("Font Icon");
                case "aac":
                case "aif":
                case "aiff":
                case "au":
                case "mid":
                case "midi":
                case "mp3":
                case "mpa":
                case "ra":
                case "ram":
                case "wma":
                case "wav":
                case "wave":
                case "ogg":
                    return EditorGUIUtility.FindTexture("AudioClip Icon");
                case "ai":
                case "apng":
                case "png":
                case "bmp":
                case "cdr":
                case "dib":
                case "eps":
                case "exif":
                case "gif":
                case "ico":
                case "icon":
                case "j":
                case "j2c":
                case "j2k":
                case "jas":
                case "jiff":
                case "jng":
                case "jp2":
                case "jpc":
                case "jpe":
                case "jpeg":
                case "jpf":
                case "jpg":
                case "jpw":
                case "jpx":
                case "jtf":
                case "mac":
                case "omf":
                case "qif":
                case "qti":
                case "qtif":
                case "tex":
                case "tfw":
                case "tga":
                case "tif":
                case "tiff":
                case "wmf":
                case "psd":
                case "exr":
                    return EditorGUIUtility.FindTexture("Texture Icon");
                case "3df":
                case "3dm":
                case "3dmf":
                case "3ds":
                case "3dv":
                case "3dx":
                case "blend":
                case "c4d":
                case "lwo":
                case "lws":
                case "ma":
                case "max":
                case "mb":
                case "mesh":
                case "obj":
                case "vrl":
                case "wrl":
                case "wrz":
                case "fbx":
                    return EditorGUIUtility.FindTexture("Mesh Icon");
                case "asf":
                case "asx":
                case "avi":
                case "dat":
                case "divx":
                case "dvx":
                case "mlv":
                case "m2l":
                case "m2t":
                case "m2ts":
                case "m2v":
                case "m4e":
                case "m4v":
                case "mjp":
                case "mov":
                case "movie":
                case "mp21":
                case "mp4":
                case "mpe":
                case "mpeg":
                case "mpg":
                case "mpv2":
                case "ogm":
                case "qt":
                case "rm":
                case "rmvb":
                case "wmw":
                case "xvid":
                    return EditorGUIUtility.FindTexture("MovieTexture Icon");
                case "colors":
                case "gradients":
                case "curves":
                case "curvesnormalized":
                case "particlecurves":
                case "particlecurvessigned":
                case "particledoublecurves":
                case "particledoublecurvessigned":
                    return EditorGUIUtility.FindTexture("ScriptableObject Icon");
            }

            return EditorGUIUtility.FindTexture("DefaultAsset Icon");
        }
    }

    internal sealed class AssetInfo
    {
        public string GUID { get; private set; }
        public string path { get; private set; }
        public bool opened { get; set; }

        public AssetInfo(string guid, string path)
        {
            this.GUID = guid;
            this.path = path;
            this.opened = false;
        }
    }

    internal sealed class AssetInfoComparer : IComparer<AssetInfo>
    {
        private static readonly string sceneExtension = ".unity";

        public int Compare(AssetInfo left, AssetInfo right)
        {
            bool sceneFileLeft = left.path.EndsWith(sceneExtension, StringComparison.OrdinalIgnoreCase);
            bool sceneFileRight = right.path.EndsWith(sceneExtension, StringComparison.OrdinalIgnoreCase);

            if (sceneFileLeft == sceneFileRight)
            {
                return string.Compare(left.path, right.path, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return sceneFileLeft == true ? 1 : -1;
            }
        }
    }
}