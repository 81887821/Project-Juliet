using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingWindow : PopupWindow
{
    public static SettingWindow Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        Close();
    }

    public void ExitGame()
    {
        StageManager.Instance.ExitGame();
    }

    public void LoadMainMenu()
    {
        StageManager.Instance.LoadMainMenu();
    }
}
