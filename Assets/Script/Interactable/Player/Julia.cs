using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Julia : PlayerBase
{
    private GameObject jumpDownAttackDetector;
    private GameObject rollingAttackDetector;

    protected override void Start()
    {
        base.Start();
        jumpDownAttackDetector = transform.Find("JumpDownAttackDetector").gameObject;
        rollingAttackDetector = transform.Find("RollingAttackDetector").gameObject;
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
            case PlayerState.WALL_STICK:
                nextState = PlayerState.JUMPING_UP;
                break;
            case PlayerState.SPECIAL_ACTION_READY:
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
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
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                animator.Play("JuliaSuperJumpPrepare");
                break;
            case PlayerState.SUPER_JUMP:
                animator.Play("JuliaSuperJump");
                break;
            case PlayerState.ROLLING:
                animator.Play("JuliaRolling");
                break;
            case PlayerState.WALL_STICK:
                animator.Play("JuliaWallStick");
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
            case PlayerState.JUMPING_DOWN:
            case PlayerState.SPECIAL_JUMPING_DOWN:
                if (!controller.collisions.below && controller.collisions.front)
                    return PlayerState.WALL_STICK;
                else
                    return base.GetNextStateByEnvironment();
            case PlayerState.WALL_STICK:
                if (controller.collisions.below)
                    return PlayerState.IDLE;
                else
                    return PlayerState.WALL_STICK;
            case PlayerState.JUMPING_UP:
                if (velocity.y <= 0f)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.JUMPING_UP;
            case PlayerState.SUPER_JUMP:
                if (velocity.y <= 0f)
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
        switch (oldState)
        {
            case PlayerState.JUMPING_DOWN:
                jumpDownAttackDetector.SetActive(false);
                break;
            case PlayerState.WALL_STICK:
                horizontalMovementEnabled = true;
                gravity /= playerCore.wallGravityRatio;
                break;
            case PlayerState.ROLLING:
                rollingAttackDetector.SetActive(false);
                break;
        }

        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (newState)
        {
            case PlayerState.JUMPING_DOWN:
                jumpDownAttackDetector.SetActive(true);
                break;
            case PlayerState.WALL_STICK:
                horizontalMovementEnabled = false;
                velocity.y = 0f;
                gravity *= playerCore.wallGravityRatio;
                break;
            case PlayerState.JUMPING_UP:
                if (oldState == PlayerState.WALL_STICK)
                    WallJump();
                else
                    Jump();
                break;
            case PlayerState.ROLLING:
                rollingAttackDetector.SetActive(true);
                break;
            case PlayerState.SUPER_JUMP:
                SuperJump();
                break;
        }
    }

    private void Jump()
    {
        velocity.y = maxJumpVelocity;
    }

    private void WallJump()
    {
        float wallDirX = HeadingRight ? 1f : -1f;
        
        if (input.HorizontalInput == 0)
        {
            velocity.x = -playerCore.wallJumpOff.x;
            velocity.y = playerCore.wallJumpOff.y;
        }
        else if (wallDirX == Mathf.Sign(input.HorizontalInput))
        {
            velocity.x = -playerCore.wallJumpClimb.x;
            velocity.y = playerCore.wallJumpClimb.y;
        }
        else
        {
            velocity.x = -playerCore.wallLeap.x;
            velocity.y = playerCore.wallLeap.y;
        }
    }

    private void SuperJump()
    {
        velocity.y = maxJumpVelocity * playerCore.supperJumpMultiplier;
    }

    public override void OnAttack(IInteractable target)
    {
        switch (state)
        {
            case PlayerState.JUMPING_DOWN:
                target.OnDamaged(this, 1, Vector2.zero);
                nextState = PlayerState.JUMPING_UP;
                break;
            case PlayerState.ROLLING:
                target.OnDamaged(this, 1, Vector2.zero);
                nextState = PlayerState.JUMPING_UP;
                break;
            default:
                Debug.LogWarning("Attack detected on non-attacking state : " + state);
                break;
        }
    }
}
