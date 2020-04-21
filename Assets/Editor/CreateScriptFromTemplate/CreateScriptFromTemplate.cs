using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace GameLib.Editor
{
    public sealed class CreateScriptFromTemplate : EditorWindow
    {
        private string m_ProjectBrowserPath;

        private int m_Selected;
        private string[] m_MenuItems;

        private Dictionary<string, TemplateEntry> m_TemplateDict;

        [MenuItem("Assets/Create/C# Script From Template", false, 50)]
        public static void CreateScriptEditor()
        {
            var window = GetWindow<CreateScriptFromTemplate>();
            window.wantsMouseMove = true;
            window.titleContent = new GUIContent("C# Script");
            window.minSize = new Vector2(360f, 150f);
            window.Show();
            window.Focus();
            window.OnCreate();
        }

        private void OnCreate()
        {
            m_ProjectBrowserPath = GetProjectBrowserPath();

            LoadTemplateFiles();
        }

        private void OnGUI()
        {
            if (m_TemplateDict == null || m_MenuItems == null)
            {
                LoadTemplateFiles();
            }

            var entry = m_TemplateDict[m_MenuItems[m_Selected]];

            EditorGUILayout.BeginVertical();

            m_Selected = EditorGUILayout.Popup("Template", m_Selected, m_MenuItems, GUILayout.Width(350));

            var keys = new List<string>(entry.replacementDict.Keys);

            foreach (var key in keys)
            {
                if (key == "Year")
                {
                    continue;
                }

                entry.replacementDict[key] = EditorGUILayout.TextField(key, entry.replacementDict[key], GUILayout.Width(350));
            }

            GUILayout.Label("Creating file " + m_ProjectBrowserPath + "/" + entry.replacementDict["ClassName"] + entry.specialKeyDict["EXTENSION"]);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(entry.replacementDict["ClassName"]));

            if (GUILayout.Button("Create"))
            {
                CreateScriptFile(entry);
                Close();

                m_TemplateDict = null;
                m_MenuItems = null;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private string GetProjectBrowserPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            return path;
        }

        private void LoadTemplateFiles()
        {
            var paths = new List<string>();
            paths.AddRange(Directory.GetFiles(Path.Combine(Application.dataPath, "Editor/CreateScriptFromTemplate/Templates"), "*.txt"));

            m_TemplateDict = new Dictionary<string, TemplateEntry>();

            foreach (var template in paths)
            {
                var entry = ParseTemplate(template);

                if (entry == null)
                {
                    continue;
                }

                m_TemplateDict.Add(entry.menuName, entry);
            }

            m_MenuItems = m_TemplateDict.Keys.ToArray();
            m_MenuItems = m_MenuItems.OrderBy(menuName => m_TemplateDict[menuName].priority).ToArray();
        }

        private TemplateEntry ParseTemplate(string path)
        {
            var template = File.ReadAllText(path);

            if (string.IsNullOrEmpty(template))
            {
                return null;
            }

            var entry = new TemplateEntry();

            string pattern = @"&&(\w+) *= *(.?[\w/# ]+)&&\n?";

            var regex = new Regex(pattern, RegexOptions.Multiline);
            var match = regex.Match(template);

            entry.specialKeyDict = new Dictionary<string, string>();

            while (match.Success)
            {
                string key = match.Groups[1].Value.ToUpper();
                string value = match.Groups[2].Value;

                if (!entry.specialKeyDict.ContainsKey(key))
                {
                    entry.specialKeyDict.Add(key, value);
                }

                template = template.Replace(match.Groups[0].Value, "");
                match = match.NextMatch();
            }

            if (!entry.specialKeyDict.ContainsKey("EXTENSION"))
            {
                entry.specialKeyDict.Add("EXTENSION", ".cs");
            }

            pattern = @"##(\w+)##";

            regex = new Regex(pattern, RegexOptions.Multiline);
            match = regex.Match(template);

            entry.replacementDict = new Dictionary<string, string>();
            entry.replacementDict.Add("ClassName", "");

            while (match.Success)
            {
                string key = match.Groups[1].Value;

                if (!entry.replacementDict.ContainsKey(key))
                {
                    entry.replacementDict.Add(key, "");

                    switch (key)
                    {
                        case "Year":
                            {
                                entry.replacementDict[key] = System.DateTime.Now.Year.ToString();
                            }
                            break;
                        case "Month":
                            {
                                entry.replacementDict[key] = System.DateTime.Now.Month.ToString();
                            }
                            break;
                        case "Day":
                            {
                                entry.replacementDict[key] = System.DateTime.Now.Day.ToString();
                            }
                            break;
                    }
                }

                match = match.NextMatch();
            }

            if (!entry.specialKeyDict.TryGetValue("MENUNAME", out entry.menuName))
            {
                entry.menuName = Path.GetFileNameWithoutExtension(path);
            }

            entry.priority = 0;

            string priorityString;

            if (entry.specialKeyDict.TryGetValue("PRIORITY", out priorityString))
            {
                int priority;

                if (int.TryParse(priorityString, out priority))
                {
                    entry.priority = priority;
                }
            }

            entry.wholeTemplate = template;

            return entry;
        }

        private void CreateScriptFile(TemplateEntry entry)
        {
            string className = Path.GetFileNameWithoutExtension(entry.replacementDict["ClassName"]);
            string template = entry.wholeTemplate;
            string extension = entry.specialKeyDict["EXTENSION"];

            foreach (var pairs in entry.replacementDict)
            {
                template = template.Replace("##" + pairs.Key + "##", pairs.Value);
            }

            string finalPath = Path.Combine(m_ProjectBrowserPath, className + extension.ToLower());

            if (File.Exists(finalPath))
            {
                Debug.LogError("File already exists: " + finalPath);
            }
            else
            {
                File.WriteAllText(finalPath, template, System.Text.Encoding.UTF8);

                AssetDatabase.Refresh();
            }
        }

        private sealed class TemplateEntry
        {
            public string menuName;
            public int priority;
            public string wholeTemplate;
            public Dictionary<string, string> replacementDict;
            public Dictionary<string, string> specialKeyDict;
        }
    }
}