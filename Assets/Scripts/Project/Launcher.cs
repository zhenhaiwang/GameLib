using UnityEngine;
using GameLib;

public sealed class Launcher : MonoBehaviour
{
    private void Awake()
    {
        new GameObject().AddComponent<SingletonManager>();

        Log.Debug("Game launcher awake, os: " + Application.platform.ToString());
    }
}
