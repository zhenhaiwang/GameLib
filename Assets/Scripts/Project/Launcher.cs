using UnityEngine;
using GameLib;
using CE;

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

        if (Input.GetKeyDown(KeyCode.Q))
        {
            var configDict = CEConfig.GetElementDict();
            foreach (var config in configDict)
            {
                Log.Debug((config.Value as CEConfig).ToString());
            }

            CEManager.instance.Load(CEArea.CEName);

            var areaDict = CEArea.GetElementDict();
            foreach (var area in areaDict.CheckNull())
            {
                Log.Debug((area.Value as CEArea).ToString());
            }
        }
    }
}
