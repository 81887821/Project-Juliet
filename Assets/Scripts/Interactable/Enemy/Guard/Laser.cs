using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaserController))]
public class Laser : MonoBehaviour, IInteractable
{
    public float SpeedRelativeToJulia = 5f;

    private LaserController controller;
    private float speed;
    private bool growing = true;
    private List<IInteractable> attackedTargets = new List<IInteractable>();

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
        if (growing)
            controller.Grow(speed * Time.deltaTime);
        else
        {
            controller.Shrink(speed * Time.deltaTime);
            if (transform.localScale.x <= 0f)
                Destroy(gameObject);
        }
    }

    public void Stop()
    {
        growing = false;
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
