using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TargetDetector : MonoBehaviour
{
    public LayerMask Targets;

    private int targetCount = 0;

    public bool TargetFound
    {
        get
        {
            return targetCount > 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Targets.Contains(collision.gameObject.layer))
            targetCount++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Targets.Contains(collision.gameObject.layer) && targetCount > 0)
            targetCount--;
    }
}
