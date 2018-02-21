using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ReportManager
{
    public static event Action ReportCollected = delegate {};

    /// <summary>
    /// Key is stage name.
    /// Value is bitwise or-ed collected report numbers.
    /// </summary>
    private static Dictionary<string, int> collectedReports = new Dictionary<string, int>();

    public static bool IsCollected(string stageName, int reportNumber)
    {
        try
        {
            return (collectedReports[stageName] & (1 << reportNumber)) != 0;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public static void MarkCollected(string stageName, int reportNumber)
    {
        try
        {
            collectedReports[stageName] |= (1 << reportNumber);
        }
        catch (KeyNotFoundException)
        {
            collectedReports[stageName] = (1 << reportNumber);
        }

        ReportCollected();
    }

    public static void SetPlayerPrefs()
    {
        StringBuilder scenes = new StringBuilder();
        foreach (var data in collectedReports)
        {
            PlayerPrefs.SetInt(data.Key, data.Value);
            scenes.AppendLine(data.Key);
        }
        PlayerPrefs.SetString(SaveLoadManager.SCENE_LIST_KEY, scenes.ToString());
    }

    public static void LoadPlayerPrefs()
    {
        string sceneList = PlayerPrefs.GetString(SaveLoadManager.SCENE_LIST_KEY, string.Empty);

        if (sceneList != string.Empty)
        {
            foreach (string stageName in sceneList.Split(Environment.NewLine.ToCharArray()))
            {
                collectedReports[stageName] = PlayerPrefs.GetInt(stageName);
            }
        }
    }
}
