using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public abstract class Enemy : MonoBehaviour, IInteractable
{
    #region Variables shown in Unity Editor
    public int currentHealth;

    public float accelerationTimeGrounded = .1f;
    public float accelerationTimeAirborne = .2f;
    public float gravity = -50f;
    public Vector2 defaultKnockback = new Vector2(15f, 4f);
    #endregion

    #region State flags
    protected bool headingLeft;

    protected abstract bool HeadingLeft
    {
        get;
        set;
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

    public abstract void OnDamaged(IInteractable attacker, int damage);

    public abstract void OnDamaged(IInteractable attacker, int damage, Vector2 knockback);

    /// <summary>
    /// Set velocity for next frame.
    /// Subclasses must use this method to implement their own movements.
    /// </summary>
    protected abstract void UpdateVelocity();
}
