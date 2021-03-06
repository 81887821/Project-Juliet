﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaserController))]
public class Laser : MonoBehaviour, IInteractable
{
    public float SpeedRelativeToJulia = 5f;
    public float MaximumLength = 100f;

    private LaserController controller;
    private float speed;
    private bool stop = false;
    private List<IInteractable> attackedTargets = new List<IInteractable>();
    private float totalGrowLength = 0f;

    private void Awake()
    {
        controller = GetComponent<LaserController>();
        transform.parent = transform.parent.parent;
    }

    private void Start()
    {
        speed = PlayerData.Instance.JuliaMoveSpeed * SpeedRelativeToJulia;
    }

    private void Update()
    {
        if (totalGrowLength < MaximumLength)
        {
            float growLength = speed * Time.deltaTime;
            totalGrowLength += growLength;
            controller.Grow(growLength);
        }
        if (stop)
        {
            controller.Shrink(speed * Time.deltaTime);
            if (transform.localScale.x <= 0f)
                Destroy(gameObject);
        }
    }

    public void Stop()
    {
        stop = true;
    }

    public void Die()
    {
        throw new InvalidOperationException("Laser cannot die.");
    }

    public void OnAttack(IInteractable target)
    {
        if (!attackedTargets.Contains(target))
        {
            attackedTargets.Add(target);
            target.OnDamaged(this, 1);
        }
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        throw new InvalidOperationException("Laser cannot be damaged.");
    }

    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        throw new InvalidOperationException("Laser cannot be damaged.");
    }
}
