using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Patrol : Enemy
{
    private enum PatrolState { None, Normal, Turning, AlertDelay, Alert, SearchingLeft, SearchingRight, PostAttackDelay, Hit, Dead }

    public float AlertModeSpeedMultiplier = 1.3f;
    public Vector2 MoveBackAfterAttack = new Vector2(60f, 0f);
    [Space]
    public float DelayOnAlert = 1f;
    public float DelayAfterAttack = 2f;
    public float DelayAfterHit = 1f;

    private TargetDetector playerDetector;
    private DamageDetector damageDetector;
    private Animator animator;

    private bool attackDisabled;

    private PatrolState state = PatrolState.Normal;
    private PatrolState nextState = PatrolState.Normal;

    protected override void Awake()
    {
        base.Awake();
        playerDetector = GetComponentInChildren<TargetDetector>();
        damageDetector = GetComponentInChildren<DamageDetector>();
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

        nextState = PatrolState.None;
    }

    private PatrolState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PatrolState.Normal:
                if (controller.Collisions.front || CliffOnFront())
                    return PatrolState.Turning;
                else if (playerDetector.TargetFound)
                    return PatrolState.AlertDelay;
                else
                    return PatrolState.Normal;
            case PatrolState.Turning:
                return PatrolState.Normal;
            case PatrolState.AlertDelay:
                if (stateEndTime > Time.time)
                    return PatrolState.AlertDelay;
                else
                    return PatrolState.Alert;
            case PatrolState.Alert:
                if (playerDetector.TargetFound)
                    return PatrolState.Alert;
                else
                    return PatrolState.SearchingLeft;
            case PatrolState.SearchingLeft:
                if (playerDetector.TargetFound)
                    return PatrolState.Alert;
                else if (stateEndTime > Time.time)
                    return PatrolState.SearchingLeft;
                else
                    return PatrolState.SearchingRight;
            case PatrolState.SearchingRight:
                if (playerDetector.TargetFound)
                    return PatrolState.Alert;
                else if (stateEndTime > Time.time)
                    return PatrolState.SearchingRight;
                else
                    return PatrolState.Normal;
            case PatrolState.PostAttackDelay:
                if (stateEndTime > Time.time)
                    return PatrolState.PostAttackDelay;
                else
                    return PatrolState.Alert;
            case PatrolState.Hit:
                if (stateEndTime > Time.time)
                    return PatrolState.Hit;
                else
                    return PatrolState.Alert;
            case PatrolState.Dead:
                return PatrolState.Dead;
            default:
                throw new Exception("Forbidden state for Cleaner : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(PatrolState oldState, PatrolState newState)
    {
        switch (oldState)
        {
            case PatrolState.Alert:
            case PatrolState.SearchingLeft:
            case PatrolState.SearchingRight:
                maxSpeed /= AlertModeSpeedMultiplier;
                break;
            case PatrolState.Hit:
                attackDisabled = false;
                break;
        }

        switch (newState)
        {
            case PatrolState.Turning:
                HeadingRight = !HeadingRight;
                velocity.x = 0f;
                break;
            case PatrolState.AlertDelay:
                stateEndTime = Time.time + DelayOnAlert;
                break;
            case PatrolState.Alert:
                maxSpeed *= AlertModeSpeedMultiplier;
                break;
            case PatrolState.SearchingLeft:
                maxSpeed *= AlertModeSpeedMultiplier;
                HeadingRight = false;
                stateEndTime = Time.time + 1f;
                break;
            case PatrolState.SearchingRight:
                maxSpeed *= AlertModeSpeedMultiplier;
                HeadingRight = true;
                stateEndTime = Time.time + 1f;
                break;
            case PatrolState.PostAttackDelay:
                stateEndTime = Time.time + DelayAfterAttack;
                break;
            case PatrolState.Hit:
                attackDisabled = true;
                stateEndTime = Time.time + DelayAfterHit;
                break;
            case PatrolState.Dead:
                attackDisabled = true;
                damageDetector.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateAnimationState(PatrolState state)
    {
        switch (state)
        {
            case PatrolState.Normal:
            case PatrolState.Turning:
                animator.Play("PatrolNormal");
                break;
            case PatrolState.AlertDelay:
            case PatrolState.Alert:
            case PatrolState.SearchingLeft:
            case PatrolState.SearchingRight:
            case PatrolState.PostAttackDelay:
            case PatrolState.Hit:
                animator.Play("PatrolAlert");
                break;
            case PatrolState.Dead:
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
            nextState = PatrolState.PostAttackDelay;
        }
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);
        nextState = PatrolState.Hit;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        float targetVelocityX;

        switch (state)
        {
            case PatrolState.Normal:
            case PatrolState.Alert:
            case PatrolState.SearchingLeft:
            case PatrolState.SearchingRight:
                targetVelocityX = maxSpeed;
                break;
            default:
                targetVelocityX = 0f;
                break;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        velocity.y += Gravity * Time.deltaTime;
    }

    public override void Die()
    {
        nextState = PatrolState.Dead;
        UpdateState();
        base.Die();
    }
}
