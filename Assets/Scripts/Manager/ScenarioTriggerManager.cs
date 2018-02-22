using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Static class for scenario script trigger management.
/// </summary>
public static class ScenarioTriggerManager
{
    public static bool IsTriggered(IScenarioTrigger trigger)
    {
        return PlayerPrefs.GetInt(SaveLoadManager.SCENARIO_TRIGGER_PREFIX + trigger.TriggerId, 0) != 0;
    }

    public static void MarkTriggered(IScenarioTrigger trigger)
    {
        PlayerPrefs.SetInt(SaveLoadManager.SCENARIO_TRIGGER_PREFIX + trigger.TriggerId, 1);
    }
}
