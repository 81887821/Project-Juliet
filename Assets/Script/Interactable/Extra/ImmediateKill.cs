using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmediateKill : MonoBehaviour, IInteractable
{
    public void Die()
    {
        throw new InvalidOperationException("ImmediateKill cannot die.");
    }

    public void OnAttack(IInteractable target)
    {
        target.Die();
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        throw new InvalidOperationException("ImmediateKill cannot be damaged.");
    }

    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        throw new InvalidOperationException("ImmediateKill cannot be damaged.");
    }
}
