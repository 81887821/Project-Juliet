using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ReportManager
{
    private const string CONTENT_RESOURCE_PREFIX = "Report";
    private const string CONTENT_RESOURCE_SEPERATOR = "_";

    public static event Action ReportCollected = delegate {};

    private static List<string> sceneList = new List<string>();

    // Report collection is saved to PlayerPrefs.
    // Key is stage name, value is bitwise or-ed collected report numbers.

    public static bool IsCollected(string stageName, int reportNumber)
    {
        return (PlayerPrefs.GetInt(stageName) & (1 << reportNumber)) != 0;
    }

    public static void MarkCollected(string stageName, int reportNumber)
    {
        if (!sceneList.Contains(stageName))
        {
            string savedSceneList = PlayerPrefs.GetString(SaveLoadManager.SCENE_LIST_KEY);
            savedSceneList = savedSceneList + '\n' + stageName;
            PlayerPrefs.SetString(SaveLoadManager.SCENE_LIST_KEY, savedSceneList);
            sceneList.Add(stageName);
        }

        int collectedReports = PlayerPrefs.GetInt(stageName) | (1 << reportNumber);
        PlayerPrefs.SetInt(stageName, collectedReports);
        ReportCollected();
    }

    public static void LoadPlayerPrefs()
    {
        string savedSceneList = PlayerPrefs.GetString(SaveLoadManager.SCENE_LIST_KEY, string.Empty);

        sceneList.Clear();
        if (savedSceneList != string.Empty)
        {
            foreach (string stageName in savedSceneList.Split('\n'))
            {
                sceneList.Add(stageName);
            }
        }
    }

    public static Dictionary<string, int> GetCollection()
    {
        var collection = new Dictionary<string, int>();

        foreach (string scene in sceneList)
            collection.Add(scene, PlayerPrefs.GetInt(scene));

        return collection;
    }

    public static string GetReportContent(string stage, int reportNumber)
    {
        string resourceName = string.Format("{0}{1}{2}{1}{3}", CONTENT_RESOURCE_PREFIX, CONTENT_RESOURCE_SEPERATOR, stage, reportNumber);
        var textAsset = Resources.Load<TextAsset>(resourceName);
        if (textAsset == null)
            return string.Format("Report resource file {0} not exists!", resourceName);
        string content = textAsset.text;
        Resources.UnloadAsset(textAsset);
        return content;
    }
}
