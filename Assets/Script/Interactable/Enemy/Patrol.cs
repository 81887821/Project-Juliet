using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Patrol : Enemy
{
    private enum PatrolState { NONE, NORMAL, TURNING, ALERT_DELAY, ALERT, SEARCHING_LEFT, SEARCHING_RIGHT, POST_ATTACK_DELAY, HIT, DEAD }

    public float AlertModeSpeedMultiplier = 1.5f;
    public Vector2 MoveBackAfterAttack = new Vector2(60f, 0f);
    [Space]
    public float DelayOnAlert = 1f;
    public float DelayAfterAttack = 2f;
    public float DelayAfterHit = 1f;

    private TargetDetector playerDetector;
    private Animator animator;

    private bool attackDisabled;

    private PatrolState state = PatrolState.NORMAL;
    private PatrolState nextState = PatrolState.NORMAL;

    protected override void Awake()
    {
        base.Awake();
        playerDetector = GetComponentInChildren<TargetDetector>();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    private void UpdateState()
    {
        PatrolState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
            UpdateAnimationState(state);
        }

        nextState = PatrolState.NONE;
    }

    private PatrolState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PatrolState.NORMAL:
                if (controller.collisions.front || CliffOnFront())
                    return PatrolState.TURNING;
                else if (playerDetector.TargetFound)
                    return PatrolState.ALERT_DELAY;
                else
                    return PatrolState.NORMAL;
            case PatrolState.TURNING:
                return PatrolState.NORMAL;
            case PatrolState.ALERT_DELAY:
                if (stateEndTime > Time.time)
                    return PatrolState.ALERT_DELAY;
                else
                    return PatrolState.ALERT;
            case PatrolState.ALERT:
                if (playerDetector.TargetFound)
                    return PatrolState.ALERT;
                else
                    return PatrolState.SEARCHING_LEFT;
            case PatrolState.SEARCHING_LEFT:
                if (playerDetector.TargetFound)
                    return PatrolState.ALERT;
                else if (stateEndTime > Time.time)
                    return PatrolState.SEARCHING_LEFT;
                else
                    return PatrolState.SEARCHING_RIGHT;
            case PatrolState.SEARCHING_RIGHT:
                if (playerDetector.TargetFound)
                    return PatrolState.ALERT;
                else if (stateEndTime > Time.time)
                    return PatrolState.SEARCHING_RIGHT;
                else
                    return PatrolState.NORMAL;
            case PatrolState.POST_ATTACK_DELAY:
                if (stateEndTime > Time.time)
                    return PatrolState.POST_ATTACK_DELAY;
                else
                    return PatrolState.ALERT;
            case PatrolState.HIT:
                if (stateEndTime > Time.time)
                    return PatrolState.HIT;
                else
                    return PatrolState.ALERT;
            case PatrolState.DEAD:
                return PatrolState.DEAD;
            default:
                throw new Exception("Forbidden state for Cleaner : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(PatrolState oldState, PatrolState newState)
    {
        switch (oldState)
        {
            case PatrolState.ALERT:
            case PatrolState.SEARCHING_LEFT:
            case PatrolState.SEARCHING_RIGHT:
                maxSpeed /= AlertModeSpeedMultiplier;
                break;
            case PatrolState.HIT:
                attackDisabled = false;
                break;
        }

        switch (newState)
        {
            case PatrolState.TURNING:
                HeadingRight = !HeadingRight;
                velocity.x = 0f;
                break;
            case PatrolState.ALERT_DELAY:
                stateEndTime = Time.time + DelayAfterAttack;
                break;
            case PatrolState.ALERT:
                maxSpeed *= AlertModeSpeedMultiplier;
                break;
            case PatrolState.SEARCHING_LEFT:
                maxSpeed *= AlertModeSpeedMultiplier;
                HeadingRight = false;
                stateEndTime = Time.time + 1f;
                break;
            case PatrolState.SEARCHING_RIGHT:
                maxSpeed *= AlertModeSpeedMultiplier;
                HeadingRight = true;
                stateEndTime = Time.time + 1f;
                break;
            case PatrolState.POST_ATTACK_DELAY:
                stateEndTime = Time.time + DelayAfterAttack;
                break;
            case PatrolState.HIT:
                attackDisabled = true;
                stateEndTime = Time.time + DelayAfterHit;
                break;
            case PatrolState.DEAD:
                attackDisabled = true;
                Destroy(gameObject, 1f);
                break;
        }
    }

    private void UpdateAnimationState(PatrolState state)
    {
        switch (state)
        {
            case PatrolState.NORMAL:
            case PatrolState.TURNING:
                animator.Play("PatrolNormal");
                break;
            case PatrolState.ALERT_DELAY:
            case PatrolState.ALERT:
            case PatrolState.SEARCHING_LEFT:
            case PatrolState.SEARCHING_RIGHT:
            case PatrolState.POST_ATTACK_DELAY:
            case PatrolState.HIT:
                animator.Play("PatrolAlert");
                break;
            case PatrolState.DEAD:
                animator.Play("PatrolNormalDead");
                break;
            default:
                throw new Exception("Forbidden state for Patrol : " + state);
        }
    }

    public override void OnAttack(IInteractable target)
    {
        if (!attackDisabled)
        {
            target.OnDamaged(this, 1);
            velocity -= MoveBackAfterAttack;
            nextState = PatrolState.POST_ATTACK_DELAY;
        }
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);
        nextState = PatrolState.HIT;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        float targetVelocityX;

        switch (state)
        {
            case PatrolState.NORMAL:
            case PatrolState.ALERT:
            case PatrolState.SEARCHING_LEFT:
            case PatrolState.SEARCHING_RIGHT:
                targetVelocityX = maxSpeed;
                break;
            default:
                targetVelocityX = 0f;
                break;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public override void Die()
    {
        nextState = PatrolState.DEAD;
        UpdateState();
    }
}
