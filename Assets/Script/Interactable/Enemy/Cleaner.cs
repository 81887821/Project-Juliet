using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleaner : Enemy
{
    private enum CleanerState { NONE, CLEANING, TURNING, HIT, DEAD }

    private bool attackDisabled;

    private CleanerState state = CleanerState.CLEANING;
    private CleanerState nextState = CleanerState.CLEANING;

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

        nextState = CleanerState.NONE;
    }

    private CleanerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case CleanerState.CLEANING:
                if (controller.collisions.front || CliffOnFront())
                    return CleanerState.TURNING;
                else
                    return CleanerState.CLEANING;
            case CleanerState.TURNING:
                if (stateEndTime > Time.time)
                    return CleanerState.TURNING;
                else
                    return CleanerState.CLEANING;
            case CleanerState.HIT:
                if (stateEndTime > Time.time)
                    return CleanerState.HIT;
                else
                    return CleanerState.CLEANING;
            case CleanerState.DEAD:
                return CleanerState.DEAD;
            default:
                throw new Exception("Forbidden state for Cleaner : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(CleanerState oldState, CleanerState newState)
    {
        switch (oldState)
        {
            case CleanerState.HIT:
                attackDisabled = false;
                break;
        }

        switch (newState)
        {
            case CleanerState.TURNING:
                HeadingRight = !HeadingRight;
                velocity.x = 0f;
                stateEndTime = Time.time + 0.5f;
                break;
            case CleanerState.HIT:
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
        nextState = CleanerState.HIT;
        UpdateState();
    }

    protected override void UpdateVelocity()
    {
        float targetVelocityX = (state == CleanerState.CLEANING ? maxSpeed : 0f);

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    public override void Die()
    {
        Destroy(gameObject);
    }
}
