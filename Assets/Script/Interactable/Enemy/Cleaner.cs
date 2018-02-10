using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : Enemy
{
    private float attackEnableTimer;
    private bool attackDisabled;

    protected override bool HeadingLeft
    {
        get
        {
            return headingLeft;
        }

        set
        {
            if (headingLeft != value)
            {
                headingLeft = value;
                transform.rotation = (headingLeft ? new Quaternion(0f, 0f, 0f, 1f) : new Quaternion(0f, 1f, 0f, 0f));
            }
        }
    }

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

    public override void OnDamaged(IInteractable attacker, int damage)
    {
        OnDamaged(attacker, damage, defaultKnockback);
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        int direction = attacker.transform.position.x > transform.position.x ? 1 : -1;

        currentHealth -= damage;

        velocity.x += -direction * knockback.x;
        velocity.y += knockback.y;
        HeadingLeft = direction > 0;

        attackDisabled = true;
        attackEnableTimer = Time.time + 1f;
    }

    protected override void UpdateVelocity()
    {
        // TODO : Cleaner moving.
        velocity.x = Mathf.SmoothDamp(velocity.x, 60f, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
}
