using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public abstract class Enemy : MonoBehaviour, IInteractable
{
    #region Variables shown in Unity Editor
    public int currentHealth = 6;

    public float accelerationTimeGrounded = .1f;
    public float accelerationTimeAirborne = .2f;
    public float gravity = -50f;
    public Vector2 defaultKnockback = new Vector2(15f, 4f);
    #endregion

    #region State flags
    protected bool headingRight = true;

    protected virtual bool HeadingRight
    {
        get
        {
            return headingRight;
        }
        set
        {
            if (headingRight != value)
            {
                headingRight = value;
                transform.rotation = (headingRight ? new Quaternion(0f, 0f, 0f, 1f) : new Quaternion(0f, 1f, 0f, 0f));
                velocity.x = -velocity.x;
            }
        }
    }
    #endregion

    #region Other variables
    protected Controller2D controller;

    protected Vector2 velocity = Vector2.zero;
    protected float velocityXSmoothing = 0f;

    protected float stateEndTime;
    #endregion

    protected virtual void Start()
    {
        controller = GetComponent<Controller2D>();
        headingRight = transform.rotation == new Quaternion(0f, 0f, 0f, 1f);
    }

    protected virtual void Update()
    {
        UpdateVelocity();
        controller.Move(velocity * Time.deltaTime);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }

    public abstract void OnAttack(IInteractable target);

    public virtual void OnDamaged(IInteractable attacker, int damage)
    {
        OnDamaged(attacker, damage, defaultKnockback);
    }

    public virtual void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        bool attackerOnRight = attacker.transform.position.x > transform.position.x;

        currentHealth--;
        if (currentHealth <= 0)
            Die();

        HeadingRight = attackerOnRight;
        velocity.x -= knockback.x;
        velocity.y += knockback.y;
    }

    public abstract void Die();

    /// <summary>
    /// Set velocity for next frame.
    /// Subclasses must use this method to implement their own movements.
    /// </summary>
    protected abstract void UpdateVelocity();
}
