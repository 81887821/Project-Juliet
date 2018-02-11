using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// Callback to be called by attacker's OnAttack method.
    /// This method uses target's default knockback value.
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="damage"></param>
    void OnDamaged(IInteractable attacker, int damage);
    /// <summary>
    /// Callback to be called by attacker's OnAttack method.
    /// This method ignore target's default knockback value, and uses given value.
    /// Attacker must give knockback.x with positive values, and target have to decide the direction.
    /// Knockback.y can be negative value, and target uses it as is.
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="damage"></param>
    /// <param name="knockback"></param>
    void OnDamaged(IInteractable attacker, int damage, Vector2 knockback);
    /// <summary>
    /// Callback to be called by AttackDetector, when intersects object with DamageDetector.
    /// OnAttack method must call target's OnDamaged method.
    /// </summary>
    /// <param name="target"></param>
    void OnAttack(IInteractable target);
    /// <summary>
    /// Kill this interactable object.
    /// </summary>
    void Die();

    Transform transform
    {
        get;
    }
    GameObject gameObject
    {
        get;
    }
}
