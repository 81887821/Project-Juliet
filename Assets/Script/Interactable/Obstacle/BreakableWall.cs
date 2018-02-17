using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IInteractable
{
    public int CurrentHealth = 1;

    public void Die()
    {
        Destroy(gameObject);
    }

    public void OnAttack(IInteractable target)
    {
        throw new InvalidOperationException("Wall cannot attack.");
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
            Die();
    }

    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        // Wall ignores knockback.
        OnDamaged(attacker, damage);
    }
}
