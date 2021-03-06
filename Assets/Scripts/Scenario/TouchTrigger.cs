﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TouchTrigger : MonoBehaviour, IScenarioTrigger
{
    public LayerMask InteractableMask;
    public ScenarioScript Script;
    public TriggerMode Mode;
    public float MinimumTriggerInterval = 0.5f;

    private float lastContact = float.MinValue;

    public string TriggerId
    {
        get
        {
            return GetInstanceID().ToString();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (InteractableMask.Contains(collision.gameObject.layer))
        {
            if (lastContact + MinimumTriggerInterval > Time.time)
                return;
            else
                lastContact = Time.time;

            switch (Mode)
            {
                case TriggerMode.Never:
                    break;
                case TriggerMode.Once:
                    if (!ScenarioTriggerManager.IsTriggered(this))
                        ScenarioInterpreter.Instance.EnqueueScript(Script);
                    Destroy(this);
                    break;
                case TriggerMode.EveryOnceInScene:
                    ScenarioInterpreter.Instance.EnqueueScript(Script);
                    Destroy(this);
                    break;
                case TriggerMode.Always:
                    ScenarioInterpreter.Instance.EnqueueScript(Script);
                    break;
                default:
                    Debug.LogError("Unknown trigger mode : " + Mode);
                    break;
            }
        }
    }
}
