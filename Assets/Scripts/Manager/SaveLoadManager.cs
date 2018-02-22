using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public static class SaveLoadManager
{
    public const string SCENE_LIST_KEY = "Scene list";
    public const string SCENARIO_TRIGGER_PREFIX = "ScenarioTrigger";

    private const string SAVE_FORMAT_VERSION_KEY = "Save format version";
    private const int SAVE_FORMAT_VERSION = 0;

    static SaveLoadManager()
    {
        ReportManager.ReportCollected += Save;
        ScenarioTriggerManager.Triggered += Save;
    }

    public static void Save()
    {
        PlayerPrefs.SetInt(SAVE_FORMAT_VERSION_KEY, SAVE_FORMAT_VERSION);
        PlayerPrefs.Save();
    }

    public static void Load()
    {
        if (PlayerPrefs.GetInt(SAVE_FORMAT_VERSION_KEY) == SAVE_FORMAT_VERSION)
        {
            ReportManager.LoadPlayerPrefs();
        }
        else
        {
            Debug.LogError("Unsupported save format version : " + PlayerPrefs.GetInt(SAVE_FORMAT_VERSION_KEY));
        }
    }
}
