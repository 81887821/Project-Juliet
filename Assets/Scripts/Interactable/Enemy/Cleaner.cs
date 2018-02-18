using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : Enemy
{
    private enum CleanerState { None, Cleaning, Turning, Hit, Dead }

    private bool attackDisabled;

    private CleanerState state = CleanerState.Cleaning;
    private CleanerState nextState = CleanerState.Cleaning;

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    private void UpdateState()
    {
        CleanerState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
        }

        nextState = CleanerState.None;
    }

    private CleanerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case CleanerState.Cleaning:
                if (controller.Collisions.front || CliffOnFront())
                    return CleanerState.Turning;
                else
                    return CleanerState.Cleaning;
            case CleanerState.Turning:
                if (stateEndTime > Time.time)
                    return CleanerState.Turning;
                else
                    return CleanerState.Cleaning;
            case CleanerState.Hit:
                if (stateEndTime > Time.time)
                    return CleanerState.Hit;
                else
                    return CleanerState.Cleaning;
            case CleanerState.Dead:
                return CleanerState.Dead;
            default:
                throw new Exception("Forbidden state for Cleaner : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(CleanerState oldState, CleanerState newState)
    {
        switch (oldState)
        {
            case CleanerState.Hit:
                attackDisabled = false;
                break;
        }

        switch (newState)
        {
            case CleanerState.Turning:
                HeadingRight = !HeadingRight;
                velocity.x = 0f;
                stateEndTime = Time.time + 0.5f;
                break;
            case CleanerState.Hit:
                attackDisabled = true;
                stateEndTime = Time.time + 0.5f;
                break;
        }
    }

    public override void OnAttack(IInteractable target)
    {
        if (!attackDisabled)
            target.OnDamaged(this, 1);
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);
        nextState = CleanerState.Hit;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        float targetVelocityX = (state == CleanerState.Cleaning ? maxSpeed : 0f);

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        velocity.y += Gravity * Time.deltaTime;
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
