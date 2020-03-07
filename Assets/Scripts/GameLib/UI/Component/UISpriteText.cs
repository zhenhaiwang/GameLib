using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLib
{
    [ExecuteInEditMode]
    public sealed class UISpriteText : UIBaseView
    {
        [Serializable]
        public struct CharSpritePair
        {
            public string letterChar;
            public Sprite letterSprite;
        }

        public enum TextAlign { LEFT, CENTER, RIGHT };

        public CharSpritePair[] charSpritePairs;

        public int fontHeight;
        public int maxWidth;
        public int padding;
        public int veticalPadding;
        public TextAlign textAlign;
        public string text = string.Empty;
        public Color fontColor = new Color(1f, 1f, 1f, 1f);

        private Dictionary<char, Sprite> m_CharSpriteDic;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;

                InvalidView();
            }
        }

        public void SetText(string text)
        {
            this.text = text;

            InvalidView();
        }

        public void SetFontColor(Color color)
        {
            fontColor = color;

            var images = GetComponentsInChildren<Image>();

            for (int j = 0; j < images.Length; j++)
            {
                images[j].color = fontColor;
            }
        }

        protected override void OnStart()
        {
            InitCharSpritesDict();
        }

        private void InitCharSpritesDict()
        {
            if (m_CharSpriteDic == null)
            {
                m_CharSpriteDic = new Dictionary<char, Sprite>();
            }

            if (charSpritePairs != null)
            {
                foreach (var pair in charSpritePairs)
                {
                    if (pair.letterChar.Length > 0 && pair.letterSprite != null)
                    {
                        m_CharSpriteDic[pair.letterChar[0]] = pair.letterSprite;
                    }
                }
            }
        }

        private void InitCharSpritesDictInEditorMode()
        {
            if (charSpritePairs == null || charSpritePairs.Length == 0)
            {
                var spriteRenders = transform.GetComponentsInChildren<SpriteRenderer>(true);

                if (spriteRenders.Length > 0)
                {
                    charSpritePairs = new CharSpritePair[spriteRenders.Length];

                    int i = 0;

                    foreach (var spriteRender in spriteRenders)
                    {
                        CharSpritePair pair = new CharSpritePair();
                        pair.letterChar = spriteRender.name;
                        pair.letterSprite = spriteRender.sprite;
                        charSpritePairs[i++] = pair;
                        DestroyImmediate(spriteRender.gameObject);
                    }
                }
            }

            InitCharSpritesDict();
        }

        private Image AddSprite(Transform spritesContainer, Sprite letterSprite, int horDock, int spriteIndex)
        {
            GameObject letterObject;
            Image image;

            if (spriteIndex < spritesContainer.childCount)
            {
                letterObject = spritesContainer.GetChild(spriteIndex).gameObject;
                image = letterObject.GetComponent<Image>();
            }
            else
            {
                letterObject = UnityUtil.AddChild(spritesContainer.gameObject, null);
                letterObject.name = "Letter";
                image = letterObject.AddComponent<Image>();
            }

            image.sprite = letterSprite;
            image.color = fontColor;
            image.SetNativeSize();
            letterObject.transform.localPosition = new Vector3(horDock + (int)image.preferredWidth / 2, spriteIndex * veticalPadding, 0);

            return image;
        }

        protected override void OnUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                InitCharSpritesDictInEditorMode();
                UpdateView();
            }
#endif
        }

        public override void UpdateView()
        {
            if (transform.childCount == 0)
            {
                UnityUtil.AddChild(gameObject, null);
            }

            var spritesContainer = transform.GetChild(0);
            spritesContainer.name = "SpritesContainer";

            // default left align
            int currentHorDock = 0;
            int spriteHeight = 0;
            int spriteAddIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (m_CharSpriteDic.ContainsKey(text[i]))
                {
                    var image = AddSprite(spritesContainer, m_CharSpriteDic[text[i]], currentHorDock, spriteAddIndex++);
                    spriteHeight = Math.Max(spriteHeight, (int)image.preferredHeight);
                    currentHorDock += ((int)image.preferredWidth + padding);
                }
            }

            if (Application.isPlaying)
                UnityUtil.RemoveAllChildrenFrom(spritesContainer.gameObject, false, spriteAddIndex);
            else
                UnityUtil.RemoveAllChildrenFrom(spritesContainer.gameObject, true, spriteAddIndex);

            int currentTotalWidth = currentHorDock - padding;

            float scaleFact;
            int calWidth;

            if (fontHeight > 0 && spriteHeight > 0)
            {
                scaleFact = fontHeight / (float)spriteHeight;
                calWidth = (int)(scaleFact * currentTotalWidth);
            }
            else
            {
                scaleFact = 1.0f;
                calWidth = currentTotalWidth;
            }

            if (maxWidth > 0 && calWidth > maxWidth)
            {
                scaleFact = (float)maxWidth / currentTotalWidth;
            }

            spritesContainer.localScale = new Vector3(scaleFact, scaleFact, scaleFact);

            // text alignment
            if (textAlign == TextAlign.RIGHT)
            {
                spritesContainer.localPosition = new Vector3(-currentTotalWidth * scaleFact, 0, 0);
            }
            else if (textAlign == TextAlign.CENTER)
            {
                spritesContainer.localPosition = new Vector3(-currentTotalWidth / 2 * scaleFact, 0, 0);
            }
            else if (textAlign == TextAlign.LEFT)
            {
                spritesContainer.localPosition = new Vector3(0, 0, 0);
            }
        }
    }
}
