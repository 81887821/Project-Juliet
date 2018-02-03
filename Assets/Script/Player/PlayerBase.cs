using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class PlayerBase : MonoBehaviour
{
    /// <summary>
    /// State of player character.
    /// State with higher value has higher priority.
    /// </summary>
    protected enum PlayerState
    {
        /// <summary>
        /// Special state to represent a method doesn't want to change state.
        /// </summary>
        NONE,
        IDLE, WALKING, // For both
        ATTACK1, ATTACK2, ATTACK3, ATTACK4, // For Juliett
        JUMPING_DOWN, // For both
        JUMPING_UP, ROLLING, SUPER_JUMP, // For Julia
        HIT, GAME_OVER // For both
    }
    protected const float EPSILON = 0.1f;

    #region Unity components in parent
    protected PlayerCore playerCore;
    protected PlayerInput input;
    protected Controller2D controller;
    protected Transform playerTransform;
    #endregion

    #region Unity components
    protected Animator animator;
    protected BoxCollider2D physicalCollider;
    protected SpriteRenderer spriteRenderer;

    public BoxCollider2D PhysicalCollider
    {
        get
        {
            return physicalCollider;
        }
    }
    #endregion

    protected PlayerState state = PlayerState.IDLE;
    protected PlayerState nextState = PlayerState.IDLE;

    #region State flags
    protected bool horizontalMovementEnabled = true;
    protected bool ignoreDamage = false;
    protected bool transformationEnabled = true;

    private bool isActive = true;
    private bool headingLeft = true;
    #endregion

    #region State flag properties
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            enabled = isActive;
            spriteRenderer.enabled = isActive;
            animator.enabled = isActive;
        }
    }

    public bool HeadingLeft
    {
        get
        {
            return headingLeft;
        }
        set
        {
            if (horizontalMovementEnabled && headingLeft != value)
            {
                headingLeft = value;
                playerTransform.rotation = (headingLeft ? new Quaternion(0f, 0f, 0f, 1f) : new Quaternion(0f, 1f, 0f, 0f));
            }
        }
    }
    #endregion

    #region Physics variables
    private float gravity;
    private float maxJumpVelocity;
    private float minJumpVelocity;
    protected Vector3 velocity;
    private float velocityXSmoothing;
    private int wallDirX;
    private bool wallSliding = false;

    private float moveSpeed;
    #endregion

    #region Timer variables
    private float timeToWallUnstick;
    /// <summary>
    /// Time variable for states which last fixed amount of time.
    /// </summary>
    protected float stateEndTime;
    #endregion

    protected virtual void Start()
    {
        playerCore = GetComponentInParent<PlayerCore>();
        input = GetComponentInParent<PlayerInput>();
        controller = GetComponentInParent<Controller2D>();
        playerTransform = GetComponentInParent<Transform>();
        animator = GetComponent<Animator>();
        physicalCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gravity = -(8 * playerCore.maxJumpHeight) / Mathf.Pow(playerCore.floatingTime, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * (playerCore.floatingTime / 2);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * playerCore.minJumpHeight);

        moveSpeed = GetMoveSpeed();
    }

    protected virtual void Update()
    {
        CalculateVelocity();

        if (playerCore.wallJumpEnabled)
            HandleWallSliding();

        controller.Move(velocity * Time.deltaTime);

        PlayerState nextStateByDirectionalInput = HandleDirectionalInput();
        nextState = (nextState > nextStateByDirectionalInput ? nextState : nextStateByDirectionalInput);
        PlayerState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        UpdateDirection();

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
            UpdateAnimationState(state);
        }

        nextState = PlayerState.NONE;
    }

    public abstract void OnActionButtonClicked();

    public void OnPlayerDamaged(int dmg, int directionX)
    {
        if (!ignoreDamage)
        {
            playerCore.currentHealth -= dmg;

            velocity.x += -directionX * playerCore.Knockback.x;
            velocity.y += playerCore.Knockback.y;
            HeadingLeft = directionX > 0;

            nextState = PlayerState.HIT;
        }
    }

    protected abstract float GetMoveSpeed();

    /// <summary>
    /// Update animator state using PlayerState and change Player sprite immediately.
    /// </summary>
    /// <param name="state"></param>
    protected abstract void UpdateAnimationState(PlayerState state);

    private void UpdateDirection()
    {
        if (input.HorizontalInput != 0f)
            HeadingLeft = input.HorizontalInput < 0;
    }

    /// <summary>
    /// Handle directional input.
    /// </summary>
    /// <returns>Next state by directional input.</returns>
    private PlayerState HandleDirectionalInput()
    {
        switch (state)
        {
            case PlayerState.IDLE:
                if (input.HorizontalInput != 0.0f)
                    return PlayerState.WALKING;
                else
                    return PlayerState.NONE;
            default:
                return PlayerState.NONE;
        }
    }

    protected virtual PlayerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PlayerState.IDLE:
                if (!controller.collisions.below)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.IDLE;
            case PlayerState.WALKING:
                if (!controller.collisions.below)
                    return PlayerState.JUMPING_DOWN;
                else if (Math.Abs(velocity.x) > EPSILON)
                    return PlayerState.WALKING;
                else
                    return PlayerState.IDLE;
            case PlayerState.JUMPING_DOWN:
                if (!controller.collisions.below)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.IDLE;
            case PlayerState.HIT:
                if (stateEndTime > Time.time)
                    return PlayerState.HIT;
                else
                    return PlayerState.IDLE;
            case PlayerState.GAME_OVER:
                return PlayerState.GAME_OVER;
            default:
                throw new Exception("Undefined state " + state);
        }
    }

    /// <summary>
    /// Handle side effect caused by state transition.
    /// </summary>
    /// <param name="oldState">Player state before the transition.</param>
    /// <param name="newState">Player state after the transition.</param>
    protected virtual void HandleStateTransitionSideEffect(PlayerState oldState, PlayerState newState)
    {
        switch (oldState)
        {
            case PlayerState.HIT:
                ignoreDamage = false;
                horizontalMovementEnabled = true;
                transformationEnabled = true;
                HeadingLeft = !HeadingLeft;
                break;
        }

        switch (newState)
        {
            case PlayerState.HIT:
                ignoreDamage = true;
                horizontalMovementEnabled = false;
                transformationEnabled = false;
                stateEndTime = Time.time + playerCore.KnockbackTime;
                break;
            case PlayerState.GAME_OVER:
                ignoreDamage = true;
                horizontalMovementEnabled = false;
                transformationEnabled = false;
                break;
        }
    }

    private void CalculateVelocity()
    {
        float targetVelocityX;

        if (horizontalMovementEnabled)
            targetVelocityX = input.HorizontalInput * moveSpeed;
        else
            targetVelocityX = 0f;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? playerCore.accelerationTimeGrounded : playerCore.accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -playerCore.wallSlideSpeedMax)
            {
                velocity.y = -playerCore.wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.HorizontalInput != wallDirX && input.HorizontalInput != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = playerCore.wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = playerCore.wallStickTime;
            }
        }
    }

    protected void Jump()
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

    public virtual bool CanTransform
    {
        get
        {
            return transformationEnabled;
        }
    }

    /// <summary>
    /// Callback method to be called on transformation.
    /// This method should be called only once, on newly active player character.
    /// 
    /// This method does
    ///   1. Copy physical variables,
    ///   2. Transit prior character to NONE state to clean-up state flags,
    ///   3. Copy state flags,
    ///   4. Transit current character to appropriate state,
    ///   5. Play animation and set character direction.
    /// </summary>
    /// <param name="priorCharacter">Player character before transformation.</param>
    public void OnTransformation(PlayerBase priorCharacter)
    {
        velocity = priorCharacter.velocity;
        velocityXSmoothing = priorCharacter.velocityXSmoothing;
        wallDirX = priorCharacter.wallDirX;
        timeToWallUnstick = priorCharacter.timeToWallUnstick;

        horizontalMovementEnabled = true;
        HeadingLeft = priorCharacter.headingLeft;

        priorCharacter.HandleStateTransitionSideEffect(priorCharacter.state, PlayerState.NONE);
        horizontalMovementEnabled = priorCharacter.horizontalMovementEnabled;
        ignoreDamage = priorCharacter.ignoreDamage;
        
        state = PlayerState.IDLE;
        nextState = PlayerState.NONE;
        
        PlayerState nextStateByDirectionalInput = HandleDirectionalInput();
        nextState = (nextState > nextStateByDirectionalInput ? nextState : nextStateByDirectionalInput);
        PlayerState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        UpdateDirection();

        HandleStateTransitionSideEffect(PlayerState.NONE, nextState);
        state = nextState;
        UpdateAnimationState(state);

        nextState = PlayerState.NONE;
    }
}
