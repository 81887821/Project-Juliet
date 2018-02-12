using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class PlayerBase : MonoBehaviour, IInteractable
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
        ROLLING, SUPER_JUMP, JUMPING_UP, // For Julia
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

    #region Children objects
    protected GameObject damageDetector;
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
    private bool headingRight = true;
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
            damageDetector.SetActive(isActive);
        }
    }

    public bool HeadingRight
    {
        get
        {
            return headingRight;
        }
        set
        {
            if (horizontalMovementEnabled && headingRight != value)
            {
                headingRight = value;
                playerTransform.rotation = (headingRight ? new Quaternion(0f, 0f, 0f, 1f) : new Quaternion(0f, 1f, 0f, 0f));
                velocity.x = -velocity.x;
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
        playerTransform = transform.parent;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageDetector = transform.Find("DamageDetector").gameObject;

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
            controller.Move(velocity * Time.deltaTime);
        }

        if (movementEnable)
        {
            if (controller.collisions.above || controller.collisions.below)
            {
                velocity.y = 0;
            }

            UpdateDirection();
        }

        UpdateState();
    }

    protected void UpdateState()
    {
        PlayerState nextStateByDirectionalInput = HandleDirectionalInput();
        nextState = (nextState > nextStateByDirectionalInput ? nextState : nextStateByDirectionalInput);
        PlayerState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
            UpdateAnimationState(state);
        }

        nextState = PlayerState.NONE;
    }

    public abstract void OnActionButtonClicked();
    
    protected abstract float GetMoveSpeed();

    /// <summary>
    /// Update animator state using PlayerState and change Player sprite immediately.
    /// </summary>
    /// <param name="state"></param>
    protected abstract void UpdateAnimationState(PlayerState state);

    private void UpdateDirection()
    {
        if (horizontalMovementEnabled && input.HorizontalInput != 0f)
            HeadingRight = input.HorizontalInput > 0;
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
                if (newState != PlayerState.CANCELABLE_SPECIAL_ACTION_READY)
                    playerCore.OnSpecialActionDisabled();
                break;
            case PlayerState.CANCELABLE_SPECIAL_ACTION_READY:
                playerCore.OnSpecialActionDisabled();
                break;
            case PlayerState.POST_TRANSFORMATION_DELAY:
                movementEnable = true;
                transformationEnabled = true;
                break;
            case PlayerState.HIT:
                ignoreDamage = false;
                horizontalMovementEnabled = true;
                transformationEnabled = true;
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
        float direction = headingRight ? 1f : -1f;

        if (horizontalMovementEnabled)
            targetVelocityX = direction * input.HorizontalInput * moveSpeed;
        else
            targetVelocityX = 0f;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? playerCore.accelerationTimeGrounded : playerCore.accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
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
    ///   1. Transit prior character to NONE state to clean-up state flags,
    ///   2. Copy state flags,
    ///   3. Copy physical variables,
    ///   4. Transit current character to appropriate state,
    ///   5. Play animation and set character direction.
    /// </summary>
    /// <param name="priorCharacter">Player character before transformation.</param>
    public void OnTransformation(PlayerBase priorCharacter)
    {
        horizontalMovementEnabled = true;
        HeadingRight = priorCharacter.headingRight;

        priorCharacter.HandleStateTransitionSideEffect(priorCharacter.state, PlayerState.NONE);
        horizontalMovementEnabled = priorCharacter.horizontalMovementEnabled;
        ignoreDamage = priorCharacter.ignoreDamage;

        // Velocity must be copied after copy HeadingRight flag to keep direction of velocity.
        velocity = priorCharacter.velocity;
        velocityXSmoothing = priorCharacter.velocityXSmoothing;
        wallDirX = priorCharacter.wallDirX;
        timeToWallUnstick = priorCharacter.timeToWallUnstick;

        state = PlayerState.NONE;
        nextState = PlayerState.POST_TRANSFORMATION_DELAY;

        UpdateDirection();

        HandleStateTransitionSideEffect(state, nextState);
        state = nextState;
        UpdateAnimationState(state);

        nextState = PlayerState.NONE;
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        OnDamaged(attacker, damage, playerCore.Knockback);
    }
    
    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        if (!ignoreDamage)
        {
            bool attackerOnRight = attacker.transform.position.x > playerTransform.position.x;

            playerCore.CurrentHealth -= damage;

            HeadingRight = attackerOnRight;
            velocity.x -= knockback.x;
            velocity.y += knockback.y;

            nextState = PlayerState.HIT;
            UpdateState();
        }
    }

    public abstract void OnAttack(IInteractable target);

    public virtual void Die()
    {
        Destroy(gameObject);
    }
}
