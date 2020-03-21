using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameLib
{
    /// <summary>
    /// 超链接文本组件
    /// </summary>
    [AddComponentMenu("UI/HrefText", 10)]
    public sealed class UIHrefText : Text, IPointerClickHandler
    {
        private sealed class HrefEntry
        {
            public string name { get; private set; }
            public int startIndex { get; private set; }
            public int endIndex { get; private set; }

            public readonly List<Rect> rects = new List<Rect>();

            public HrefEntry(string name, int startIndex, int endIndex)
            {
                this.name = name;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }
        }

        private static readonly Regex s_HrefRegex = new Regex(@"<a([\s\u00a0])href=([^>\n\s]+)>(.*?)<\/a>", RegexOptions.Singleline);

        private static readonly StringBuilder s_TextBuilder = new StringBuilder();

        private readonly List<HrefEntry> m_HrefEntrys = new List<HrefEntry>();

        private string m_OutputText;

        [Serializable]
        public class HrefClickEvent : UnityEvent<string> { }

        [SerializeField]
        private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

        public HrefClickEvent onHrefClick
        {
            get { return m_OnHrefClick; }
            set { m_OnHrefClick = value; }
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();

#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
            {
                return;
            }
#endif

            m_HrefEntrys.Clear();
            s_TextBuilder.Length = 0;

            int index = 0;

            foreach (Match match in s_HrefRegex.Matches(text))
            {
                s_TextBuilder.Append(text.Substring(index, match.Index - index));
                s_TextBuilder.Append("<color=blue>");

                m_HrefEntrys.Add(new HrefEntry(match.Groups[2].Value, s_TextBuilder.Length * 4, (s_TextBuilder.Length + match.Groups[3].Length - 1) * 4 + 3));

                s_TextBuilder.Append(match.Groups[3].Value);
                s_TextBuilder.Append("</color>");

                index = match.Index + match.Length;
            }

            s_TextBuilder.Append(text.Substring(index, text.Length - index));

            m_OutputText = s_TextBuilder.ToString();
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            string originText = m_Text;
            m_Text = m_OutputText;
            base.OnPopulateMesh(toFill);
            m_Text = originText;

            var vert = new UIVertex();

            int hrefEntryCount = m_HrefEntrys.Count;

            for (int i = 0; i < hrefEntryCount; i++)
            {
                var hrefEntry = m_HrefEntrys[i];

                hrefEntry.rects.Clear();

                if (hrefEntry.startIndex >= toFill.currentVertCount)
                {
                    continue;
                }

                toFill.PopulateUIVertex(ref vert, hrefEntry.startIndex);

                var pos = vert.position;
                var bounds = new Bounds(pos, Vector3.zero);

                for (int ii = hrefEntry.startIndex; ii < hrefEntry.endIndex; ii++)
                {
                    if (ii >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref vert, ii);

                    pos = vert.position;

                    if (pos.x < bounds.min.x)
                    {
                        hrefEntry.rects.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos);
                    }
                }

                hrefEntry.rects.Add(new Rect(bounds.min, bounds.size));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);

            int hrefEntryCount = m_HrefEntrys.Count;

            for (int i = 0; i < hrefEntryCount; i++)
            {
                var hrefEntry = m_HrefEntrys[i];
                var hrefRects = hrefEntry.rects;
                int hrefRectCount = hrefRects.Count;

                for (int ii = 0; ii < hrefRectCount; ii++)
                {
                    if (hrefRects[ii].Contains(localPoint))
                    {
                        m_OnHrefClick.Invoke(hrefEntry.name);

                        return;
                    }
                }
            }
        }
    }
}
