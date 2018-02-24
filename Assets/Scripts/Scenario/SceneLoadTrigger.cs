using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTrigger : MonoBehaviour, IScenarioTrigger
{
    public ScenarioScript Script;
    public TriggerMode Mode;

    public string TriggerId
    {
        get
        {
            return GetInstanceID().ToString();
        }
    }

    private void Update()
    {
        switch (Mode)
        {
            case TriggerMode.Never:
                break;
            case TriggerMode.Once:
                if (!ScenarioTriggerManager.IsTriggered(this))
                    ScenarioInterpreter.Instance.EnqueueScript(Script);
                break;
            case TriggerMode.EveryOnceInScene:
            case TriggerMode.Always:
                ScenarioInterpreter.Instance.EnqueueScript(Script);
                break;
            default:
                Debug.LogError("Unknown trigger mode : " + Mode);
                break;
        }
        Destroy(gameObject);
    }
}
