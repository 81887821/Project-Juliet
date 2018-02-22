using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ReportManager
{
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
            savedSceneList = savedSceneList + Environment.NewLine + stageName;
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

        if (savedSceneList != string.Empty)
        {
            foreach (string stageName in savedSceneList.Split(Environment.NewLine.ToCharArray()))
            {
                sceneList.Add(stageName);
            }
        }
    }
}
