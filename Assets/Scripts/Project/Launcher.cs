using UnityEngine;
using GameLib;

public sealed class Launcher : UIBaseView
{
    protected override void OnAwake()
    {
        new GameObject().AddComponent<SingletonManager>();

        Log.Debug("Game launcher awake, os: " + Application.platform.ToString());
    }

    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PopupManager.instance.Popup("UI/UIPopupTest_1", true, true);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            PopupManager.instance.Popup("UI/UIPopupTest_2", false, true);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            PopupManager.instance.Popup("UI/UIPopupTest_3", true, false);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            PopupManager.instance.Popup("UI/UIPopupTest_4", false, false);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            PopupManager.instance.Back();
        }
    }
}
