using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Juliett : PlayerBase
{
    private static readonly Vector3 COLLISION_BOX_SHRINK = new Vector3(0.1f, 0.1f);

    #region State flags
    private bool attackContinue = false;
    #endregion

    private GameObject[] normalAttackDetectors = new GameObject[4];
    private GameObject uppercutDetector;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < 4; i++)
            normalAttackDetectors[i] = transform.Find(string.Format("Attack{0}Detector", i + 1)).gameObject;
        uppercutDetector = transform.Find("UppercutDetector").gameObject;
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
            case PlayerState.SPECIAL_ACTION_READY:
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                nextState = PlayerState.UPPERCUT;
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
            case PlayerState.POST_TRANSFORMATION_DELAY:
                animator.Play("JuliettIdle");
                break;
            case PlayerState.JUMPING_DOWN:
            case PlayerState.SPECIAL_JUMPING_DOWN:
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
            case PlayerState.SPECIAL_ACTION_READY:
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                animator.Play("JuliettUppercutPrepare");
                break;
            case PlayerState.UPPERCUT:
                animator.Play("JuliettUppercut");
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
            case PlayerState.UPPERCUT:
                if (stateEndTime > Time.time)
                    return PlayerState.UPPERCUT;
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
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
            case PlayerState.ATTACK4:
                attackContinue = false;
                horizontalMovementEnabled = true;
                normalAttackDetectors[oldState - PlayerState.ATTACK1].SetActive(false);
                break;
            case PlayerState.UPPERCUT:
                horizontalMovementEnabled = true;
                uppercutDetector.SetActive(false);
                break;
        }

        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (newState)
        {
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
            case PlayerState.ATTACK4:
                stateEndTime = Time.time + playerCore.attackInterval[newState - PlayerState.ATTACK1];
                velocity += playerCore.accelerationOnAttack[newState - PlayerState.ATTACK1];
                horizontalMovementEnabled = false;
                normalAttackDetectors[newState - PlayerState.ATTACK1].SetActive(true);
                break;
            case PlayerState.UPPERCUT:
                stateEndTime = Time.time + playerCore.uppercutDuration;
                horizontalMovementEnabled = false;
                uppercutDetector.SetActive(true);
                break;
        }
    }

    public override bool CanTransform
    {
        get
        {
            Vector3 center = PhysicalBounds.center;
            center.Scale(transform.lossyScale);

            Vector3 size = PhysicalBounds.size;
            size.Scale(transform.lossyScale);

            Collider2D collision = Physics2D.OverlapBox(transform.position + center, size - COLLISION_BOX_SHRINK, 0f, controller.collisionMask);
            return base.CanTransform && collision == null;
        }
    }
    
    public override void OnAttack(IInteractable target)
    {
        switch (state)
        {
            case PlayerState.ATTACK1:
            case PlayerState.ATTACK2:
            case PlayerState.ATTACK3:
            case PlayerState.ATTACK4:
                target.OnDamaged(this, 1, playerCore.enemyKnockbackOnAttack[state - PlayerState.ATTACK1]);
                break;
            case PlayerState.UPPERCUT:
                target.OnDamaged(this, 1, playerCore.enemyKnockbackOnUppercut);
                break;
            default:
                Debug.LogWarning("Attack detected on non-attacking state : " + state);
                break;
        }
    }
}
