using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Julia : PlayerBase
{
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
                nextState = PlayerState.JUMPING_UP;
                break;
        }
    }

    protected override float GetMoveSpeed()
    {
        return playerCore.juliaMoveSpeed;
    }
    
    protected override void UpdateAnimationState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                animator.Play("JuliaIdle");
                break;
            case PlayerState.JUMPING_UP:
                animator.Play("JuliaJumpUp");
                break;
            case PlayerState.JUMPING_DOWN:
                animator.Play("JuliaJumpDownPrepare");
                break;
            case PlayerState.WALKING:
                animator.Play("JuliaWalk");
                break;
            case PlayerState.SUPER_JUMP:
                animator.Play("JuliaSuperJumpPrepare");
                break;
            case PlayerState.ROLLING:
                animator.Play("JuliaRolling");
                break;
            case PlayerState.HIT:
                animator.Play("JuliaHit");
                break;
            case PlayerState.GAME_OVER:
                animator.Play("JuliaGameOver");
                break;
            default:
                throw new Exception("Forbidden state for Julia : " + state);
        }

        animator.Update(0f);
    }

    protected override PlayerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PlayerState.JUMPING_UP:
                if (velocity.y < 0f)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.JUMPING_UP;
            case PlayerState.SUPER_JUMP:
                if (velocity.y < 0f)
                    return PlayerState.ROLLING;
                else
                    return PlayerState.SUPER_JUMP;
            case PlayerState.ROLLING:
                if (!controller.collisions.below)
                    return PlayerState.ROLLING;
                else
                    return PlayerState.IDLE;
            default:
                return base.GetNextStateByEnvironment();
        }
    }

    protected override void HandleStateTransitionSideEffect(PlayerState oldState, PlayerState newState)
    {
        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (newState)
        {
            case PlayerState.JUMPING_UP:
                Jump();
                break;
        }
    }
}
