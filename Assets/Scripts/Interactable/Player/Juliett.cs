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
    private JuliettEffects effects;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < 4; i++)
            normalAttackDetectors[i] = transform.Find(string.Format("Attack{0}Detector", i + 1)).gameObject;
        uppercutDetector = transform.Find("UppercutDetector").gameObject;
        effects = GetComponentInChildren<JuliettEffects>();
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
                nextState = PlayerState.Attack1;
                break;
            case PlayerState.Attack1:
            case PlayerState.Attack2:
            case PlayerState.Attack3:
                attackContinue = true;
                break;
            case PlayerState.SpecialActionReady:
            case PlayerState.CancelableSpecialActionReady:
                nextState = PlayerState.Uppercut;
                break;
        }
    }

    protected override float GetMoveSpeed()
    {
        return playerData.JuliettMoveSpeed;
    }
    
    protected override void UpdateAnimationState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.PostTransformationDelay:
                animator.Play("JuliettIdle");
                break;
            case PlayerState.JumpingDown:
            case PlayerState.SpecialJumpingDown:
                animator.Play("JuliettJumpDown");
                break;
            case PlayerState.Walking:
                animator.Play("JuliettWalk");
                break;
            case PlayerState.Attack1:
                animator.Play("JuliettAttack1");
                break;
            case PlayerState.Attack2:
                animator.Play("JuliettAttack2");
                break;
            case PlayerState.Attack3:
                animator.Play("JuliettAttack3");
                break;
            case PlayerState.Attack4:
                animator.Play("JuliettAttack4");
                break;
            case PlayerState.SpecialActionReady:
            case PlayerState.CancelableSpecialActionReady:
                animator.Play("JuliettUppercutPrepare");
                break;
            case PlayerState.Uppercut:
                animator.Play("JuliettUppercut");
                break;
            case PlayerState.Hit:
                animator.Play("JuliettHit");
                break;
            case PlayerState.GameOver:
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
            case PlayerState.Attack1:
            case PlayerState.Attack2:
            case PlayerState.Attack3:
                if (stateEndTime > Time.time)
                    return state;
                else if (attackContinue)
                    return state + 1;
                else
                    return PlayerState.Idle;
            case PlayerState.Attack4:
                if (stateEndTime > Time.time)
                    return state;
                else
                    return PlayerState.Idle;
            case PlayerState.Uppercut:
                if (stateEndTime > Time.time)
                    return PlayerState.Uppercut;
                else
                    return PlayerState.Idle;
            default:
                return base.GetNextStateByEnvironment();
        }
    }

    protected override void HandleStateTransitionSideEffect(PlayerState oldState, PlayerState newState)
    {
        switch (oldState)
        {
            case PlayerState.Attack1:
            case PlayerState.Attack2:
            case PlayerState.Attack3:
            case PlayerState.Attack4:
                attackContinue = false;
                horizontalMovementEnabled = true;
                normalAttackDetectors[oldState - PlayerState.Attack1].SetActive(false);
                break;
            case PlayerState.Uppercut:
                horizontalMovementEnabled = true;
                uppercutDetector.SetActive(false);
                break;
        }

        base.HandleStateTransitionSideEffect(oldState, newState);

        switch (newState)
        {
            case PlayerState.Attack1:
            case PlayerState.Attack2:
            case PlayerState.Attack3:
            case PlayerState.Attack4:
                stateEndTime = Time.time + playerData.AttackInterval[newState - PlayerState.Attack1];
                velocity += playerData.AccelerationOnAttack[newState - PlayerState.Attack1];
                horizontalMovementEnabled = false;
                normalAttackDetectors[newState - PlayerState.Attack1].SetActive(true);
                effects.PlayAttackEffects(newState - PlayerState.Attack1, HeadingRight);
                break;
            case PlayerState.Uppercut:
                stateEndTime = Time.time + playerData.UppercutDuration;
                horizontalMovementEnabled = false;
                uppercutDetector.SetActive(true);
                effects.PlayUppercutEffect(HeadingRight);
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

            Collider2D collision = Physics2D.OverlapBox(transform.position + center, size - COLLISION_BOX_SHRINK, 0f, controller.CollisionMask);
            return base.CanTransform && collision == null;
        }
    }
    
    public override void OnAttack(IInteractable target)
    {
        switch (state)
        {
            case PlayerState.Attack1:
            case PlayerState.Attack2:
            case PlayerState.Attack3:
            case PlayerState.Attack4:
                target.OnDamaged(this, 1, playerData.EnemyKnockbackOnAttack[state - PlayerState.Attack1]);
                break;
            case PlayerState.Uppercut:
                target.OnDamaged(this, 1, playerData.EnemyKnockbackOnUppercut);
                break;
            default:
                Debug.LogWarning("Attack detected on non-attacking state : " + state);
                break;
        }
    }
}
