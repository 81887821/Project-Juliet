using System;
using UnityEngine;

public static class SettingManager
{
    public static Action<bool> UseScrollbarChanged = delegate {};

    private const string USING_SCROLLBAR_KEY = "Using scrollbar";

    public static bool UsingScrollbar
    {
        get
        {
            return usingScrollbar;
        }

        set
        {
            if (usingScrollbar != value)
            {
                usingScrollbar = value;
                PlayerPrefs.SetInt(USING_SCROLLBAR_KEY, value ? 1 : 0);
                SaveLoadManager.Save();
                UseScrollbarChanged(value);
            }
        }
    }

    private static bool usingScrollbar;

    public static void LoadPlayerPrefs()
    {
        usingScrollbar = PlayerPrefs.GetInt(USING_SCROLLBAR_KEY, 1) != 0;
    }
}