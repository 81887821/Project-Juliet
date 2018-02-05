using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
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
        IDLE, SPECIAL_ACTION_READY, CANCELABLE_SPECIAL_ACTION_READY, WALKING, // For both
        ATTACK1, ATTACK2, ATTACK3, ATTACK4, // For Juliett
        JUMPING_DOWN, // For both
        /// <summary>
        /// Jumping down state right after transformation.
        /// Next action can be special action.
        /// State for both Julia and Juliett.
        /// </summary>
        SPECIAL_JUMPING_DOWN,
        UPPERCUT, // For Juliett
        JUMPING_UP, ROLLING, SUPER_JUMP, // For Julia
        POST_TRANSFORMATION_DELAY, HIT, GAME_OVER // For both
    }
    protected const float EPSILON = 0.1f;

    #region Variables shown in Unity Editor
    [Tooltip("Physical bounds of character, relative to parent transform.")]
    public Bounds PhysicalBounds;
    #endregion

    #region Unity components in parent
    protected PlayerCore playerCore;
    protected PlayerInput input;
    protected Controller2D controller;
    protected Transform playerTransform;
    #endregion

    #region Unity components
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    #endregion

    protected PlayerState state = PlayerState.IDLE;
    protected PlayerState nextState = PlayerState.IDLE;

    #region State flags
    /// <summary>
    /// If movementEnable is set to false, player character will not move even though horizontalMovementEnabled is true.
    /// </summary>
    protected bool movementEnable = true;
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
    protected float gravity;
    protected float maxJumpVelocity;
    protected float minJumpVelocity;
    protected Vector3 velocity;
    protected float velocityXSmoothing;
    protected int wallDirX;
    protected bool wallSliding = false;

    private float moveSpeed;
    #endregion

    #region Timer variables
    private float timeToWallUnstick;
    /// <summary>
    /// Time variable for states which last fixed amount of time.
    /// </summary>
    protected float stateEndTime;
    protected float specialActionAvailableTime;
    #endregion

    protected virtual void Start()
    {
        playerCore = GetComponentInParent<PlayerCore>();
        input = GetComponentInParent<PlayerInput>();
        controller = GetComponentInParent<Controller2D>();
        playerTransform = GetComponentInParent<Transform>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        gravity = -(8 * playerCore.maxJumpHeight) / Mathf.Pow(playerCore.floatingTime, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * (playerCore.floatingTime / 2);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * playerCore.minJumpHeight);

        moveSpeed = GetMoveSpeed();
    }

    protected virtual void Update()
    {
        if (movementEnable)
        {
            CalculateVelocity();

            if (playerCore.wallJumpEnabled)
                HandleWallSliding();

            controller.Move(velocity * Time.deltaTime);
        }

        PlayerState nextStateByDirectionalInput = HandleDirectionalInput();
        nextState = (nextState > nextStateByDirectionalInput ? nextState : nextStateByDirectionalInput);
        PlayerState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (movementEnable)
        {
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
        }

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
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
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
            case PlayerState.SPECIAL_JUMPING_DOWN:
                if (!controller.collisions.below)
                    return PlayerState.SPECIAL_JUMPING_DOWN;
                else
                    return PlayerState.SPECIAL_ACTION_READY;
            case PlayerState.SPECIAL_ACTION_READY:
                if (stateEndTime > Time.time)
                    return PlayerState.SPECIAL_ACTION_READY;
                else
                    return PlayerState.CANCELABLE_SPECIAL_ACTION_READY;
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                if (stateEndTime > Time.time)
                    return PlayerState.CANCELABLE_SPECIAL_ACTION_READY;
                else
                    return PlayerState.IDLE;
            case PlayerState.POST_TRANSFORMATION_DELAY:
                if (stateEndTime > Time.time)
                    return PlayerState.POST_TRANSFORMATION_DELAY;
                else if (controller.collisions.below)
                    return PlayerState.SPECIAL_ACTION_READY;
                else
                    return PlayerState.SPECIAL_JUMPING_DOWN;
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
            case PlayerState.SPECIAL_ACTION_READY:
                horizontalMovementEnabled = true;
                break;
            case PlayerState.POST_TRANSFORMATION_DELAY:
                movementEnable = true;
                transformationEnabled = true;
                break;
            case PlayerState.HIT:
                ignoreDamage = false;
                horizontalMovementEnabled = true;
                transformationEnabled = true;
                HeadingLeft = !HeadingLeft;
                break;
        }

        switch (newState)
        {
            case PlayerState.SPECIAL_ACTION_READY:
                stateEndTime = Time.time + playerCore.totalSpecialActionAvailableTime - playerCore.cancelableSpecialActionAvailableTime;
                horizontalMovementEnabled = false;
                break;
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                stateEndTime = Time.time + playerCore.totalSpecialActionAvailableTime;
                break;
            case PlayerState.POST_TRANSFORMATION_DELAY:
                stateEndTime = Time.time + playerCore.transformationDelayTime;
                transformationEnabled = false;
                movementEnable = false;
                break;
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
        
        state = PlayerState.NONE;
        nextState = PlayerState.POST_TRANSFORMATION_DELAY;

        UpdateDirection();

        HandleStateTransitionSideEffect(state, nextState);
        state = nextState;
        UpdateAnimationState(state);

        nextState = PlayerState.NONE;
    }
}
