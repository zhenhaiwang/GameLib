using System;
using UnityEngine;

namespace GameLib
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIKeyboardComponent : MonoBehaviour
    {
        public TouchScreenKeyboard Keyboard { get; private set; }

        private Action<string> m_OnInput;
        private Action m_OnCancel;
        private Action<float> m_OnHeightChanged;

        private float m_CanvasHeight = 1080f;

        private TouchScreenKeyboard.Status m_LastStatus = TouchScreenKeyboard.Status.LostFocus;

        private int m_LastCharacterLength;
        private float m_LastHeight;

        private int m_CharacterLimit;           // 输入长度限制
        private bool m_IgnoreSurrogate;         // 过滤特殊字符
        private bool m_ForceHide;               // 强制关闭键盘

        private void Awake()
        {
            enabled = TouchScreenKeyboard.isSupported;
        }

        private void Start()
        {
            var canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                Log.Error("Can not find canvas component in parent");
            }
            else
            {
                m_CanvasHeight = canvas.GetComponent<RectTransform>().rect.height;
            }
        }

        private void Update()
        {
            if (!TouchScreenKeyboard.isSupported || Keyboard == null)
            {
                return;
            }

            HandleCharacterLimit();
            HandleSurrogate();
            HandleMaskHide();

            CheckCharacterLengthChanged();

            if (CheckHeightChanged() || CheckStatusChanged())
            {
                HandleInputDone();
            }
        }

        private void HandleCharacterLimit()
        {
            if (!Keyboard.active)
            {
                return;
            }

            if (m_CharacterLimit == 0)
            {
                return;
            }

            if (GetCharacterLength() > m_CharacterLimit)
            {
                Keyboard.text = Keyboard.text.Substring(0, m_CharacterLimit);
            }
        }

        private void HandleSurrogate()
        {
            if (!Keyboard.active)
            {
                return;
            }

            if (!m_IgnoreSurrogate)
            {
                return;
            }

            int characterLength = GetCharacterLength();

            if (characterLength <= m_LastCharacterLength)
            {
                return;
            }

            for (int i = characterLength; i > m_LastCharacterLength; i--)
            {
                if (char.IsSurrogate(Keyboard.text[i - 1]))
                {
                    Keyboard.text = Keyboard.text.Substring(0, m_LastCharacterLength);

                    break;
                }
            }
        }

        private void HandleMaskHide()
        {
#if UNITY_IPHONE
            if (Keyboard.active && Input.GetMouseButtonDown(0))
            {
                Hide();
            }
#endif
        }

        private bool CheckCharacterLengthChanged()
        {
            if (!Keyboard.active)
            {
                return false;
            }

            int characterLength = GetCharacterLength();

            if (characterLength == m_LastCharacterLength)
            {
                return false;
            }

            m_LastCharacterLength = characterLength;

            return true;
        }

        private bool CheckHeightChanged()
        {
            float height = GetKeyboardHeight();

            if (Math.Abs(height - m_LastHeight) < 0.01f)
            {
                return false;
            }

            float deltaHeight = height - m_LastHeight;
            float aspect = deltaHeight / Screen.height;

            m_OnHeightChanged.Call(aspect * m_CanvasHeight);

            m_LastHeight = height;

            return true;
        }

        private bool CheckStatusChanged()
        {
            if (Keyboard.status == m_LastStatus)
            {
                return false;
            }

            m_LastStatus = Keyboard.status;

            return true;
        }

        private void HandleInputDone()
        {
            if (m_ForceHide)
            {
                m_OnCancel.Call();

                m_ForceHide = false;
            }
            else
            {
                switch (Keyboard.status)
                {
                    case TouchScreenKeyboard.Status.Visible:
                        return;
                    case TouchScreenKeyboard.Status.Done:
                        {
                            m_OnInput.Call(Keyboard.text);
                        }
                        break;
                    case TouchScreenKeyboard.Status.Canceled:
                    case TouchScreenKeyboard.Status.LostFocus:
                        {
                            m_OnCancel.Call();
                        }
                        break;
                }
            }

            m_OnInput = null;
            m_OnCancel = null;

            m_LastCharacterLength = 0;
        }

        private int GetCharacterLength()
        {
            return string.IsNullOrEmpty(Keyboard.text) ? 0 : Keyboard.text.Length;
        }

        private static float GetKeyboardHeight()
        {
#if UNITY_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            var view = unityPlayer.Call<AndroidJavaObject>("getView");
            var dialog = unityPlayer.Get<AndroidJavaObject>("b");

            if (view == null || dialog == null)
            {
                return 0;
            }

            int decorHeight = 0;

            var decorView = dialog.Call<AndroidJavaObject>("getWindow")
                .Call<AndroidJavaObject>("getDecorView");

            if (decorView != null)
            {
                decorHeight = decorView.Call<int>("getHeight");
            }

            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);

                return Screen.height - rect.Call<int>("height") + decorHeight;
            }
        }
#else
            return TouchScreenKeyboard.area.height;
#endif
        }

        public UIKeyboardComponent CharacterLimit(int limit)
        {
            m_CharacterLimit = Mathf.Max(0, limit);

            return this;
        }

        public UIKeyboardComponent IgnoreSurrogate(bool ignore = true)
        {
            m_IgnoreSurrogate = ignore;

            return this;
        }

        public UIKeyboardComponent ListenInput(Action<string> onInput)
        {
            m_OnInput = onInput;

            return this;
        }

        public UIKeyboardComponent ListenCancel(Action onCancel)
        {
            m_OnCancel = onCancel;

            return this;
        }

        public UIKeyboardComponent ListenHeightChanged(Action<float> onHeightChanged)
        {
            m_OnHeightChanged = onHeightChanged;

            return this;
        }

        public UIKeyboardComponent Pop(string text = "", string textPlaceholder = "")
        {
            if (TouchScreenKeyboard.isSupported)
            {
                TouchScreenKeyboard.hideInput = false;

                Keyboard = TouchScreenKeyboard.Open(
                    text,
                    TouchScreenKeyboardType.Default,
                    false,
                    false,
                    false,
                    false,
                    textPlaceholder);
            }
            else
            {
                Log.Debug("TouchScreenKeyboard not supported");
            }

            return this;
        }

        public void Hide()
        {
            if (Keyboard == null)
            {
                return;
            }

            m_ForceHide = true;

            Keyboard.active = false;
        }
    }
}