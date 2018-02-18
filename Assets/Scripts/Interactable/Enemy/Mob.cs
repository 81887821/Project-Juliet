using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Mob : Enemy
{
    private enum MobState { None, Idle, Pressed, Hit }

    public Sprite MobIdle;
    public Sprite MobPressed;
    public Sprite MobHit;

    private SpriteRenderer spriteRenderer;

    private MobState state = MobState.Idle;
    private MobState nextState = MobState.Idle;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    private void UpdateState()
    {
        MobState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            state = nextState;
            UpdateAnimationState(state);
        }

        nextState = MobState.None;
    }

    private void UpdateAnimationState(MobState state)
    {
        switch (state)
        {
            case MobState.Idle:
                spriteRenderer.sprite = MobIdle;
                break;
            case MobState.Pressed:
                spriteRenderer.sprite = MobPressed;
                break;
            case MobState.Hit:
                spriteRenderer.sprite = MobHit;
                break;
        }
    }

    private MobState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case MobState.Idle:
                return MobState.Idle;
            case MobState.Pressed:
            case MobState.Hit:
                if (stateEndTime > Time.time)
                    return state;
                else
                    return MobState.Idle;
            default:
                throw new Exception("Forbidden state for Mob : " + state);
        }
    }

    public override void OnAttack(IInteractable target)
    {
        throw new InvalidOperationException("Mob cannot attack.");
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);

        if (attacker is Julia)
            nextState = MobState.Pressed;
        else
            nextState = MobState.Hit;
        stateEndTime = Time.time + .3f;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref velocityXSmoothing, (controller.Collisions.below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        velocity.y += Gravity * Time.deltaTime;
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
