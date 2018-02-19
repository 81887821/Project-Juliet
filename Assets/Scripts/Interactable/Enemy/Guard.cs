using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Guard : Enemy
{
    private enum GuardState { None, Idle, Walking, Turning, FarReady, NearReady, FarShoot, NearShoot, BackJumping, Hit, Dead }

    [Header("Guard")]
    public Vector2 BackJumpAmount = new Vector2(120f, 30f);
    public float NearReadyDelay = 1f;
    public float NearShootDelay = 1f;
    public float FarReadyDelay = 3f;
    public float FarShootDelay = 2f;
    public float HitDelay = .5f;

    #region Unity components
    private Animator animator;
    private TargetDetector nearPlayerDetector;
    private TargetDetector farPlayerDetector;
    #endregion

    private GuardState state = GuardState.Idle;
    private GuardState nextState = GuardState.Idle;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
        foreach (TargetDetector targetDetector in GetComponentsInChildren<TargetDetector>())
        {
            if (targetDetector.name == "Near player detector")
            {
#if DEBUG
                if (nearPlayerDetector != null)
                    Debug.LogError("[Guard] Double allocation to nearPlayerDetector.");
#endif
                nearPlayerDetector = targetDetector;
            }
            else if (targetDetector.name == "Far player detector")
            {
#if DEBUG
                if (farPlayerDetector != null)
                    Debug.LogError("[Guard] Double allocation to farPlayerDetector.");
#endif
                farPlayerDetector = targetDetector;
            }
#if DEBUG
            else
                Debug.LogError("[Guard] Unknown target detector : " + targetDetector.name);
#endif
        }

#if DEBUG
        if (nearPlayerDetector == null)
            Debug.LogError("[Guard] nearPlayerDetector is not allocated.");
        if (farPlayerDetector == null)
            Debug.LogError("[Guard] farPlayerDetector is not allocated.");
#endif
    }

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    private void UpdateState()
    {
        GuardState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
            UpdateAnimationState(state);
        }

        nextState = GuardState.None;
    }

    private GuardState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case GuardState.Idle:
                return GuardState.Walking;
            case GuardState.Walking:
                if (nearPlayerDetector.TargetFound)
                    return GuardState.NearReady;
                else if (farPlayerDetector.TargetFound)
                    return GuardState.FarReady;
                else if (CliffOnFront())
                    return GuardState.Turning;
                else
                    return GuardState.Walking;
            case GuardState.Turning:
                return GuardState.Walking;
            case GuardState.FarReady:
                if (stateEndTime > Time.time)
                    return GuardState.FarReady;
                else
                    return GuardState.FarShoot;
            case GuardState.NearReady:
                if (stateEndTime > Time.time)
                    return GuardState.NearReady;
                else
                    return GuardState.NearShoot;
            case GuardState.FarShoot:
                if (stateEndTime > Time.time)
                    return state;
                else if (nearPlayerDetector.TargetFound)
                    return GuardState.NearReady;
                else if (farPlayerDetector.TargetFound)
                    return GuardState.FarReady;
                else
                    return GuardState.Idle;
            case GuardState.NearShoot:
                if (stateEndTime > Time.time)
                    return state;
                else
                    return GuardState.BackJumping;
            case GuardState.BackJumping:
                if (controller.Collisions.below)
                    return GuardState.Idle;
                else
                    return GuardState.BackJumping;
            case GuardState.Hit:
                if (stateEndTime > Time.time)
                    return GuardState.Hit;
                else
                    return GuardState.Idle;
            case GuardState.Dead:
                return GuardState.Dead;
            default:
                throw new Exception("Forbidden state for Guard : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(GuardState oldState, GuardState newState)
    {
        switch (newState)
        {
            case GuardState.Turning:
                HeadingRight = !HeadingRight;
                velocity.x = 0f;
                break;
            case GuardState.FarReady:
                stateEndTime = Time.time + FarReadyDelay;
                break;
            case GuardState.NearReady:
                stateEndTime = Time.time + NearReadyDelay;
                break;
            case GuardState.FarShoot:
                stateEndTime = Time.time + FarShootDelay;
                break;
            case GuardState.NearShoot:
                stateEndTime = Time.time + NearShootDelay;
                break;
            case GuardState.BackJumping:
                BackJump();
                break;
            case GuardState.Hit:
                stateEndTime = Time.time + HitDelay;
                break;
            case GuardState.Dead:
                Destroy(gameObject);
                break;
        }
    }

    private void UpdateAnimationState(GuardState state)
    {
        switch (state)
        {
            case GuardState.Idle:
            case GuardState.Hit:
            case GuardState.Dead:
                animator.Play("GuardIdle");
                break;
            case GuardState.Walking:
            case GuardState.Turning:
                animator.Play("GuardWalk");
                break;
            case GuardState.FarReady:
                animator.Play("GuardReadyFar");
                break;
            case GuardState.NearReady:
                animator.Play("GuardReadyNear");
                break;
            case GuardState.FarShoot:
                animator.Play("GuardShootFar");
                break;
            case GuardState.NearShoot:
                animator.Play("GuardShootNear");
                break;
            case GuardState.BackJumping:
                animator.Play("GuardBackJump");
                break;
            default:
                throw new Exception("Forbidden state for Guard : " + state);
        }
    }

    public override void Die()
    {
        nextState = GuardState.Dead;
        UpdateState();
    }

    public override void OnAttack(IInteractable target)
    {
        target.OnDamaged(this, 1);
    }

    protected override void UpdateVelocity()
    {
        float targetVelocityX;

        switch (state)
        {
            case GuardState.Walking:
                targetVelocityX = maxSpeed;
                break;
            default:
                targetVelocityX = 0f;
                break;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? AccelerationTimeGrounded : AccelerationTimeAirborne);
        velocity.y += Gravity * Time.deltaTime;
    }

    private void BackJump()
    {
        velocity.x -= BackJumpAmount.x;
        velocity.y += BackJumpAmount.y;
    }
}
