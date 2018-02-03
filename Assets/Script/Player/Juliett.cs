using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Juliett : PlayerBase
{
    private static readonly float[] ATTACK_INVERVAL = { 0.4f, 0.4f, 0.4f, 0.4f };

    #region State flags
    private bool attackContinue = false;
    #endregion

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnActionButtonClicked()
    {
        switch (state)
        {
            case PlayerState.IDLE:
            case PlayerState.WALKING:
                nextState = PlayerState.ATTACK1;
                break;
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
                attackContinue = true;
                break;
        }
    }

    protected override float GetMoveSpeed()
    {
        return playerCore.juliettMoveSpeed;
    }
    
    protected override void UpdateAnimationState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                animator.Play("JuliettIdle");
                break;
            case PlayerState.JUMPING_DOWN:
                animator.Play("JuliettJumpDown");
                break;
            case PlayerState.WALKING:
                animator.Play("JuliettWalk");
                break;
            case PlayerState.ATTACK1:
                animator.Play("JuliettAttack1");
                break;
            case PlayerState.ATTACK2:
                animator.Play("JuliettAttack2");
                break;
            case PlayerState.ATTACK3:
                animator.Play("JuliettAttack3");
                break;
            case PlayerState.ATTACK4:
                animator.Play("JuliettAttack4");
                break;
            case PlayerState.HIT:
                animator.Play("JuliettHit");
                break;
            case PlayerState.GAME_OVER:
                animator.Play("JuliettGameOver");
                break;
            default:
                throw new Exception("Forbidden state for Juliett : " + state);
        }

        animator.Update(0f);
    }

    protected override PlayerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
                if (stateEndTime > Time.time)
                    return state;
                else if (attackContinue)
                    return state + 1;
                else
                    return PlayerState.IDLE;
            case PlayerState.ATTACK4:
                if (stateEndTime > Time.time)
                    return state;
                else
                    return PlayerState.IDLE;
            default:
                return base.GetNextStateByEnvironment();
        }
    }

    protected override void HandleStateTransitionSideEffect(PlayerState oldState, PlayerState newState)
    {
        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (oldState)
        {
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
            case PlayerState.ATTACK4:
                attackContinue = false;
                horizontalMovementEnabled = true;
                break;
        }

        switch (newState)
        {
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
            case PlayerState.ATTACK4:
                stateEndTime = Time.time + ATTACK_INVERVAL[newState - PlayerState.ATTACK1];
                horizontalMovementEnabled = false;
                break;
        }
    }
}
