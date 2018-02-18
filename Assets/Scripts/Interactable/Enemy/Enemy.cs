using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(BoxCollider2D))]
public abstract class Enemy : MonoBehaviour, IInteractable
{
    #region Variables shown in Unity Editor
    public int CurrentHealth = 6;

    public float AccelerationTimeGrounded = .1f;
    public float AccelerationTimeAirborne = .2f;
    public float Gravity = -150f;
    public Vector2 DefaultKnockback = new Vector2(45f, 12f);

    public float SpeedRelativeToJulia = 1f;
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
                Controller2D.Contacts temp = controller.Collisions.back;
                controller.Collisions.back = controller.Collisions.front;
                controller.Collisions.front = temp;
            }
        }
    }
    #endregion

    #region Other variables
    protected Controller2D controller;
    protected BoxCollider2D boxCollider;

    protected float maxSpeed;

    protected Vector2 velocity = Vector2.zero;
    protected float velocityXSmoothing = 0f;

    protected float stateEndTime;
    #endregion

    protected virtual void Awake()
    {
        controller = GetComponent<Controller2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        headingRight = transform.rotation == new Quaternion(0f, 0f, 0f, 1f);
    }

    protected virtual void Start()
    {
        maxSpeed = PlayerData.Instance.JuliaMoveSpeed * SpeedRelativeToJulia;
    }

    protected virtual void Update()
    {
        UpdateVelocity();
        controller.Move(velocity * Time.deltaTime);

        if (controller.Collisions.above || controller.Collisions.below)
        {
            velocity.y = 0;
        }
    }

    public abstract void OnAttack(IInteractable target);

    public virtual void OnDamaged(IInteractable attacker, int damage)
    {
        OnDamaged(attacker, damage, DefaultKnockback);
    }

    public virtual void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        bool attackerOnRight = attacker.transform.position.x > transform.position.x;

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
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

    protected bool CliffOnFront()
    {
        Bounds physicalBounds = boxCollider.bounds;
        int directionX = headingRight ? 1 : -1;
        Vector2 origin = new Vector2(physicalBounds.center.x + directionX * physicalBounds.extents.x, physicalBounds.center.y + physicalBounds.extents.y);
        Vector2 rayDirection = new Vector2(directionX * 0.01f, -physicalBounds.size.y - 0.01f);
#if DEBUG
        Debug.DrawRay(origin, rayDirection);
#endif
        return !Physics2D.Raycast(origin, rayDirection, rayDirection.magnitude, controller.CollisionMask); ;
    }
}
