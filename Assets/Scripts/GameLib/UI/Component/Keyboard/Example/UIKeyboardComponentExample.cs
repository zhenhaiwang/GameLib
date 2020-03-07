using UnityEngine;
using UnityEngine.UI;
using GameLib;

public sealed class KeyboardComponentExample : MonoBehaviour
{
    public UIKeyboardComponent keyboard;
    public Text input;
    public RectTransform target;

    public void OnClickPop()
    {
        keyboard.ListenInput((value) =>
        {
            input.text = value;
        })
        .ListenCancel(() =>
        {
            // do nothing
        })
        .ListenHeightChanged((value) =>
        {
            target.localPosition += new Vector3(0f, value);
        })
        .CharacterLimit(100)
        .Pop();
    }

    public void OnClickHide()
    {
        keyboard.Hide();
    }
}
