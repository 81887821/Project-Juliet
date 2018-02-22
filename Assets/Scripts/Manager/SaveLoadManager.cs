﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

public static class SaveLoadManager
{
    public const string SCENE_LIST_KEY = "Scene list";

    private const string SAVE_FORMAT_VERSION_KEY = "Save format version";
    private const int SAVE_FORMAT_VERSION = 0;

    static SaveLoadManager()
    {
        ReportManager.ReportCollected += Save;
    }

    public static void Save()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt(SAVE_FORMAT_VERSION_KEY, SAVE_FORMAT_VERSION);
        ReportManager.SetPlayerPrefs();
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