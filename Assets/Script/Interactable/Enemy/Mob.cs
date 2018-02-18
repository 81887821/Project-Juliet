using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Mob : Enemy
{
    private enum MobState { NONE, IDLE, PRESSED, HIT }

    public Sprite MobIdle;
    public Sprite MobPressed;
    public Sprite MobHit;

    private SpriteRenderer spriteRenderer;

    private MobState state = MobState.IDLE;
    private MobState nextState = MobState.IDLE;

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

        nextState = MobState.NONE;
    }

    private void UpdateAnimationState(MobState state)
    {
        switch (state)
        {
            case MobState.IDLE:
                spriteRenderer.sprite = MobIdle;
                break;
            case MobState.PRESSED:
                spriteRenderer.sprite = MobPressed;
                break;
            case MobState.HIT:
                spriteRenderer.sprite = MobHit;
                break;
        }
    }

    private MobState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case MobState.IDLE:
                return MobState.IDLE;
            case MobState.PRESSED:
            case MobState.HIT:
                if (stateEndTime > Time.time)
                    return state;
                else
                    return MobState.IDLE;
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
            nextState = MobState.PRESSED;
        else
            nextState = MobState.HIT;
        stateEndTime = Time.time + .3f;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
