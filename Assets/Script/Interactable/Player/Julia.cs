using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Julia : PlayerBase
{
    private GameObject jumpDownAttackDetector;
    private GameObject rollingAttackDetector;

    protected override void Awake()
    {
        base.Awake();
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
            case PlayerState.Idle:
            case PlayerState.Walking:
            case PlayerState.WallStick:
                nextState = PlayerState.JumpingUp;
                break;
            case PlayerState.SpecialActionReady:
            case PlayerState.CancelableSpecialActionReady:
                nextState = PlayerState.SuperJump;
                break;
        }
    }

    protected override float GetMoveSpeed()
    {
        return playerCore.JuliaMoveSpeed;
    }
    
    protected override void UpdateAnimationState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.PostTransformationDelay:
                animator.Play("JuliaIdle");
                break;
            case PlayerState.JumpingUp:
                animator.Play("JuliaJumpUp");
                break;
            case PlayerState.JumpingDown:
            case PlayerState.SpecialJumpingDown:
                animator.Play("JuliaJumpDownPrepare");
                break;
            case PlayerState.Walking:
                animator.Play("JuliaWalk");
                break;
            case PlayerState.SpecialActionReady:
            case PlayerState.CancelableSpecialActionReady:
                animator.Play("JuliaSuperJumpPrepare");
                break;
            case PlayerState.SuperJump:
                animator.Play("JuliaSuperJump");
                break;
            case PlayerState.Rolling:
                animator.Play("JuliaRolling");
                break;
            case PlayerState.WallStick:
                animator.Play("JuliaWallStick");
                break;
            case PlayerState.Hit:
                animator.Play("JuliaHit");
                break;
            case PlayerState.GameOver:
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
            case PlayerState.JumpingDown:
            case PlayerState.SpecialJumpingDown:
                if (!controller.Collisions.below && controller.Collisions.front.Contains("Obstacle"))
                    return PlayerState.WallStick;
                else
                    return base.GetNextStateByEnvironment();
            case PlayerState.WallStick:
                if (controller.Collisions.below)
                    return PlayerState.Idle;
                else
                    return PlayerState.WallStick;
            case PlayerState.JumpingUp:
                if (velocity.y <= 0f)
                    return PlayerState.JumpingDown;
                else
                    return PlayerState.JumpingUp;
            case PlayerState.SuperJump:
                if (velocity.y <= 0f)
                    return PlayerState.Rolling;
                else
                    return PlayerState.SuperJump;
            case PlayerState.Rolling:
                if (controller.Collisions.below)
                    return PlayerState.Idle;
                else if (controller.Collisions.front.Contains("Obstacle"))
                    return PlayerState.WallStick;
                else
                    return PlayerState.Rolling;
            default:
                return base.GetNextStateByEnvironment();
        }
    }

    protected override void HandleStateTransitionSideEffect(PlayerState oldState, PlayerState newState)
    {
        switch (oldState)
        {
            case PlayerState.JumpingDown:
                jumpDownAttackDetector.SetActive(false);
                break;
            case PlayerState.WallStick:
                horizontalMovementEnabled = true;
                gravity /= playerCore.WallGravityRatio;
                break;
            case PlayerState.Rolling:
                rollingAttackDetector.SetActive(false);
                break;
        }

        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (newState)
        {
            case PlayerState.JumpingDown:
                jumpDownAttackDetector.SetActive(true);
                break;
            case PlayerState.WallStick:
                horizontalMovementEnabled = false;
                velocity.y = 0f;
                gravity *= playerCore.WallGravityRatio;
                break;
            case PlayerState.JumpingUp:
                if (oldState == PlayerState.WallStick)
                    WallJump();
                else
                    Jump();
                break;
            case PlayerState.Rolling:
                rollingAttackDetector.SetActive(true);
                break;
            case PlayerState.SuperJump:
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
            velocity.x = -playerCore.WallJumpOff.x;
            velocity.y = playerCore.WallJumpOff.y;
        }
        else if (wallDirX == Mathf.Sign(input.HorizontalInput))
        {
            velocity.x = -playerCore.WallJumpClimb.x;
            velocity.y = playerCore.WallJumpClimb.y;
        }
        else
        {
            velocity.x = -playerCore.WallLeap.x;
            velocity.y = playerCore.WallLeap.y;
        }
    }

    private void SuperJump()
    {
        velocity.y = maxJumpVelocity * playerCore.SupperJumpMultiplier;
    }

    public override void OnAttack(IInteractable target)
    {
        switch (state)
        {
            case PlayerState.JumpingDown:
                target.OnDamaged(this, 1, Vector2.zero);
                nextState = PlayerState.JumpingUp;
                break;
            case PlayerState.Rolling:
                target.OnDamaged(this, 1, Vector2.zero);
                nextState = PlayerState.JumpingUp;
                break;
            default:
                Debug.LogWarning("Attack detected on non-attacking state : " + state);
                break;
        }
    }
}
