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
        None,
        Idle, Walking, // For both
        Attack1, Attack2, Attack3, Attack4, // For Juliett
        JumpingDown, // For both
        Uppercut, // For Juliett
        WallStick, Rolling, SuperJump, JumpingUp, // For Julia
        Hit, GameOver // For both
    }
    protected const float EPSILON = 0.1f;

    #region Variables shown in Unity Editor
    [Tooltip("Physical bounds of character, relative to parent transform.")]
    public Bounds PhysicalBounds;
    #endregion

    #region Unity components in parent
    protected PlayerData playerData;
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

    protected PlayerState state = PlayerState.Idle;
    protected PlayerState nextState = PlayerState.Idle;

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
                Controller2D.Contacts temp = controller.Collisions.back;
                controller.Collisions.back = controller.Collisions.front;
                controller.Collisions.front = temp;
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

    private float moveSpeed;
    #endregion

    #region Timer variables
    private float timeToWallUnstick;
    /// <summary>
    /// Time variable for states which last fixed amount of time.
    /// </summary>
    protected float stateEndTime;
    #endregion

    protected virtual void Awake()
    {
        playerData = GetComponentInParent<PlayerData>();
        input = GetComponentInParent<PlayerInput>();
        controller = GetComponentInParent<Controller2D>();
        playerTransform = transform.parent;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageDetector = transform.Find("DamageDetector").gameObject;
    }

    protected virtual void Start()
    {
        StageManager.Instance.GameStateChanged += OnGameStateChanged;

        gravity = -(8 * playerData.MaxJumpHeight) / Mathf.Pow(playerData.FloatingTime, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * (playerData.FloatingTime / 2);
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * playerData.MinJumpHeight);

        moveSpeed = GetMoveSpeed();
    }

    protected virtual void OnDestroy()
    {
        StageManager.Instance.GameStateChanged -= OnGameStateChanged;
    }

    protected virtual void OnGameStateChanged(bool gamePaused)
    {
        if (isActive)
            enabled = !gamePaused;
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
            if (controller.Collisions.above || controller.Collisions.below)
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

        nextState = PlayerState.None;
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
            case PlayerState.Idle:
                if (input.HorizontalInput != 0.0f)
                    return PlayerState.Walking;
                else
                    return PlayerState.None;
            default:
                return PlayerState.None;
        }
    }

    protected virtual PlayerState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case PlayerState.Idle:
                if (!controller.Collisions.below)
                    return PlayerState.JumpingDown;
                else
                    return PlayerState.Idle;
            case PlayerState.Walking:
                if (!controller.Collisions.below)
                    return PlayerState.JumpingDown;
                else if (Math.Abs(velocity.x) > EPSILON)
                    return PlayerState.Walking;
                else
                    return PlayerState.Idle;
            case PlayerState.JumpingDown:
                if (!controller.Collisions.below)
                    return PlayerState.JumpingDown;
                else
                    return PlayerState.Idle;
            case PlayerState.Hit:
                if (stateEndTime > Time.time)
                    return PlayerState.Hit;
                else
                    return PlayerState.Idle;
            case PlayerState.GameOver:
                return PlayerState.GameOver;
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
            case PlayerState.Hit:
                horizontalMovementEnabled = true;
                break;
        }

        switch (newState)
        {
            case PlayerState.Hit:
                ignoreDamage = true;
                StartCoroutine(DamageIgnoreEffect(playerData.DamageIgnoreDurationAfterHit));
                horizontalMovementEnabled = false;
                transformationEnabled = false;
                stateEndTime = Time.time + playerData.KnockbackTime;
                break;
            case PlayerState.GameOver:
                ignoreDamage = true;
                horizontalMovementEnabled = false;
                transformationEnabled = false;
                break;
        }
    }

    private IEnumerator DamageIgnoreEffect(float duration)
    {
        const float BILINKING_INTERVAL = 1 / 6f;
        WaitForSeconds waitForSeconds = new WaitForSeconds(BILINKING_INTERVAL);
        bool isHalfVisible = false;
        Color halfVisible = spriteRenderer.color;
        Color original = spriteRenderer.color;
        halfVisible.a = 0.5f;

        while (duration >= 0f)
        {
            if (isHalfVisible)
                spriteRenderer.color = original;
            else
                spriteRenderer.color = halfVisible;
            isHalfVisible = !isHalfVisible;
            duration -= BILINKING_INTERVAL;
            yield return waitForSeconds;
        }

        spriteRenderer.color = original;
        ignoreDamage = false;
        transformationEnabled = true; // Remove this code with caution. ignoreDamage flag must be unset before transformation, since this coroutine will not be sent to Player class of the other form.
    }

    private void CalculateVelocity()
    {
        float targetVelocityX;
        float direction = headingRight ? 1f : -1f;

        if (horizontalMovementEnabled)
            targetVelocityX = direction * input.HorizontalInput * moveSpeed;
        else
            targetVelocityX = 0f;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? playerData.AccelerationTimeGrounded : playerData.AccelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
    
    public virtual bool CanTransform
    {
        get
        {
            return transformationEnabled && !playerData.IsTransformationCoolTime;
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
        PlayerState priorState = priorCharacter.state;

        horizontalMovementEnabled = true;
        HeadingRight = priorCharacter.headingRight;

        priorCharacter.HandleStateTransitionSideEffect(priorState, PlayerState.None);
        horizontalMovementEnabled = priorCharacter.horizontalMovementEnabled;
        ignoreDamage = priorCharacter.ignoreDamage;

        // Velocity must be copied after copy HeadingRight flag to keep direction of velocity.
        velocity = priorCharacter.velocity;
        velocityXSmoothing = priorCharacter.velocityXSmoothing;
        timeToWallUnstick = priorCharacter.timeToWallUnstick;

        state = PlayerState.None;
        if (IsAllowedState(priorState))
            nextState = priorState;
        else
            nextState = PlayerState.Idle;

        UpdateDirection();

        HandleStateTransitionSideEffect(state, nextState);
        state = nextState;
        UpdateAnimationState(state);

        nextState = PlayerState.None;
    }

    /// <summary>
    /// Check if given state is allowed to current character form.
    /// </summary>
    /// <param name="state">State to check.</param>
    /// <returns>True if the state is allowed, false otherwise.</returns>
    protected virtual bool IsAllowedState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Walking:
            case PlayerState.JumpingDown:
            case PlayerState.Hit:
            case PlayerState.GameOver:
                return true;
            default:
                return false;
        }
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        OnDamaged(attacker, damage, playerData.Knockback);
    }
    
    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        if (!ignoreDamage)
        {
            bool attackerOnRight = attacker.transform.position.x > playerTransform.position.x;

            playerData.CurrentHealth -= damage;

            if (playerData.CurrentHealth <= 0)
            {
                Die();
                return;
            }

            HeadingRight = attackerOnRight;
            velocity.x -= knockback.x;
            velocity.y += knockback.y;

            nextState = PlayerState.Hit;
            UpdateState();
        }
    }

    public abstract void OnAttack(IInteractable target);

    public virtual void Die()
    {
        playerData.CurrentHealth = 0;
        nextState = PlayerState.GameOver;
        UpdateState();
    }
}
