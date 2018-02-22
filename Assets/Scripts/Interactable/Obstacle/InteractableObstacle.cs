using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObstacle : MonoBehaviour, IInteractable
{
    public int Damage = 1;
    public int CurrentHealth = 1;

    public void Die()
    {
        Destroy(gameObject);
    }

    public void OnAttack(IInteractable target)
    {
        target.OnDamaged(this, Damage);
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
            Die();
    }

    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        // Obstacle ignores knockback.
        OnDamaged(attacker, damage);
    }
}
