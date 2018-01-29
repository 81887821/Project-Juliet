﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    /// <summary>
    /// State of player character.
    /// State with higher value has higher priority.
    /// </summary>
	private enum PlayerState { NONE, IDLE, WALKING, ATTACK, JUMPING_DOWN, JUMPING_UP, ROLLING, SUPER_JUMP, HIT, GAME_OVER }
	private const float EPSILON = 0.1f;

	[Header("Player Condition")]
	public bool isSmallForm;
	public int currentHealth;

	[Header("Common Movement")]
	public float maxJumpHeight = 3;
	public float floatingTime = .8f;
	public float moveSpeed = 6;
	public float bigFormSpeedMul = 1.5f;

	public Vector2 Knockback;
	public float KnockbackTime = .3f;

	[Header("Advanced Movement")]
	public bool wallJumpEnabled;
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	[Space]
	public bool downSideJumpEnabled;           // for obstacle with "Through" Tag 
	public bool variableHeightJumpEnabled;
	public float minJumpHeight = 1;

	[Space]
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	public Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;
	bool wallSliding;
	public int wallDirX;

	float endKnockbackTime;

	private PlayerState state = PlayerState.IDLE;
    private PlayerState nextState = PlayerState.IDLE;
	private Animator animator;

	private SpriteRenderer spriteRenderer;
	private bool headingLeft = true;
	private bool HeadingLeft
	{
		get
		{
			return headingLeft;
		}
		set
		{
			if (headingLeft != value)
			{
				headingLeft = value;
				spriteRenderer.flipX = value == false;
			}
		}
	}

    PlayerInput input;

	private void UpdateAnimationState(PlayerState state)
	{
		int layer = isSmallForm ? 0 : 1;

		switch (state)
		{
			case PlayerState.IDLE:
				animator.Play("Idle", layer);
				break;
			case PlayerState.JUMPING_UP:
				animator.Play("JumpUp", layer);
				break;
			case PlayerState.JUMPING_DOWN:
				animator.Play("JumpDownPrepare", layer);
				break;
			case PlayerState.WALKING:
				animator.Play("Walk", layer);
				break;
			case PlayerState.SUPER_JUMP:
				animator.Play("SuperJumpPrepare", layer);
				break;
			case PlayerState.ROLLING:
				animator.Play("Rolling", layer);
				break;
			case PlayerState.HIT:
				animator.Play("Hit", layer);
				break;
			case PlayerState.GAME_OVER:
				animator.Play("GameOver", layer);
				break;
			default:
				throw new Exception("Undefined state " + state);
		}
	}

    /// <summary>
    /// Handle user input.
    /// </summary>
    /// <returns>Next state by user input.</returns>
    private PlayerState HandleInput()
    {
        PlayerState nextState = PlayerState.NONE;

        #region Action
        switch (state)
        {
            case PlayerState.IDLE:
            case PlayerState.WALKING:
                if (input.ActionInputDown)
                {
                    if (isSmallForm)
                    {
                        Jump();
                        nextState = PlayerState.JUMPING_UP;
                    }
                    else
                    {
                        Attack();
                        nextState = PlayerState.ATTACK;
                    }
                }
                break;
        }
        #endregion

        #region Directional input
        switch (state)
        {
            case PlayerState.IDLE:
                if (input.HorizontalInput != 0.0f)
                    nextState = PlayerState.WALKING;
                controller.Move(velocity * Time.deltaTime, input.DirectionalInput);
                break;
            case PlayerState.WALKING:
            case PlayerState.JUMPING_UP:
            case PlayerState.JUMPING_DOWN:
            case PlayerState.SUPER_JUMP:
            case PlayerState.ROLLING:
                controller.Move(velocity * Time.deltaTime, input.DirectionalInput);
                break;
            default:
                controller.Move(velocity * Time.deltaTime, Vector2.zero);
                break;
        }
        #endregion

        return nextState;
    }

	private PlayerState GetNextStateByEnvironment()
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
            case PlayerState.JUMPING_UP:
                if (velocity.y < 0f)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.JUMPING_UP;
            case PlayerState.JUMPING_DOWN:
				if (!controller.collisions.below)
                    return PlayerState.JUMPING_DOWN;
                else
                    return PlayerState.IDLE;
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
			case PlayerState.HIT:
				if (endKnockbackTime > Time.time)
					return PlayerState.HIT;
				else
					return PlayerState.IDLE;
			case PlayerState.GAME_OVER:
				return PlayerState.GAME_OVER;
			default:
				throw new Exception("Undefined state " + state);
		}
	}

	void Start()
	{
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
        input = GetComponent<PlayerInput>();

		gravity = -(8 * maxJumpHeight) / Mathf.Pow(floatingTime, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * (floatingTime / 2);
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update()
	{
		CalculateVelocity();
		
		if (wallJumpEnabled)
			HandleWallSliding();
        
        PlayerState nextStateByInput = HandleInput();
        PlayerState nextStateByEnvironment = GetNextStateByEnvironment();
        PlayerState nextState = (nextStateByInput > nextStateByEnvironment ? nextStateByInput : nextStateByEnvironment);
        
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

        HeadingLeft = velocity.x < 0f;

        if (state != nextState)
        {
            state = nextState;
            UpdateAnimationState(state);
        }
	}

	private void Jump()
	{
		if (wallJumpEnabled)
		{
			if (wallSliding)
			{
				if (wallDirX == input.HorizontalInput)
				{
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (input.HorizontalInput == 0)
				{
					velocity.x = -wallDirX * wallJumpOff.x;
					velocity.y = wallJumpOff.y;
				}
				else
				{
					velocity.x = -wallDirX * wallLeap.x;
					velocity.y = wallLeap.y;
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

	public void OnJumpInputUp()
	{
		if (variableHeightJumpEnabled && velocity.y > minJumpVelocity)
		{
			velocity.y = minJumpVelocity;
		}
	}

	void HandleWallSliding()
	{
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
		{
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax)
			{
				velocity.y = -wallSlideSpeedMax;
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
					timeToWallUnstick = wallStickTime;
				}
			}
			else
			{
				timeToWallUnstick = wallStickTime;
			}
		}
	}

	public void Attack()
	{

	}

	public void OnPlayerDamaged(int dmg, int directionX)
	{
		if (endKnockbackTime > Time.time)
			return;

		endKnockbackTime = Time.time + KnockbackTime;
		currentHealth -= dmg;

		velocity.x += -directionX * Knockback.x;
		velocity.y += Knockback.y;
	}

	void CalculateVelocity()
	{
		float targetVelocityX = input.HorizontalInput * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}
}