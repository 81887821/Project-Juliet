using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : Enemy
{
    private float attackEnableTimer;
    private bool attackDisabled;

    protected override void Update()
    {
        base.Update();

        if (attackDisabled && Time.time > attackEnableTimer)
            attackDisabled = false;
    }

    public override void OnAttack(IInteractable target)
    {
        if (!attackDisabled)
            target.OnDamaged(this, 1);
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);

        attackDisabled = true;
        attackEnableTimer = Time.time + 1f;
    }

    protected override void UpdateVelocity()
    {
        // TODO : Patrol moving.
        velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
