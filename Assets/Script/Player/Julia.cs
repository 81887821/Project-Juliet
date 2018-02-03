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
            case PlayerState.SPECIAL_ACTION_READY:
                nextState = PlayerState.SUPER_JUMP;
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
            case PlayerState.POST_TRANSFORMATION_DELAY:
                animator.Play("JuliaIdle");
                break;
            case PlayerState.JUMPING_UP:
                animator.Play("JuliaJumpUp");
                break;
            case PlayerState.JUMPING_DOWN:
            case PlayerState.SPECIAL_JUMPING_DOWN:
                animator.Play("JuliaJumpDownPrepare");
                break;
            case PlayerState.WALKING:
                animator.Play("JuliaWalk");
                break;
            case PlayerState.SPECIAL_ACTION_READY:
                animator.Play("JuliaSuperJumpPrepare");
                break;
            case PlayerState.SUPER_JUMP:
                animator.Play("JuliaSuperJump");
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
            case PlayerState.SUPER_JUMP:
                SuperJump();
                break;
        }
    }

    private void Jump()
    {
        if (playerCore.wallJumpEnabled)
        {
            if (wallSliding)
            {
                if (wallDirX == input.HorizontalInput)
                {
                    velocity.x = -wallDirX * playerCore.wallJumpClimb.x;
                    velocity.y = playerCore.wallJumpClimb.y;
                }
                else if (input.HorizontalInput == 0)
                {
                    velocity.x = -wallDirX * playerCore.wallJumpOff.x;
                    velocity.y = playerCore.wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * playerCore.wallLeap.x;
                    velocity.y = playerCore.wallLeap.y;
                }
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (input.HorizontalInput != -Mathf.Sign(controller.collisions.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    private void SuperJump()
    {
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (input.HorizontalInput != -Mathf.Sign(controller.collisions.slopeNormal.x))
                {
                    velocity.y = maxJumpVelocity * playerCore.supperJumpMultiplier * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * playerCore.supperJumpMultiplier * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity * playerCore.supperJumpMultiplier;
            }
        }
    }
}
