using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	private enum PlayerState { IDLE, WALKING, JUMPING_UP, JUMPING_DOWN, SUPER_JUMP, ROLLING, HIT, GAME_OVER }
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
	public bool wallJump;
	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;
	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	[Space]
	public bool downSideJump;           // for obstacle with "Through" Tag 
	public bool variableHeightJump;
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
	public Vector2 directionalInput;
	bool wallSliding;
	public int wallDirX;

	float endKnockbackTime;

	private PlayerState state = PlayerState.IDLE; // Do not assign to state variable directly. Use State property instead.
	private Animator animator;
	private PlayerState State
	{
		get
		{
			return state;
		}

		set
		{
			if (state != value)
			{
				state = value;
				UpdateAnimationState(value);
			}
		}
	}
	private bool jumpButtonPressed = false;

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

	private PlayerState GetNextState()
	{
		switch (State)
		{
			case PlayerState.IDLE:
				if (jumpButtonPressed)
				{
					Jump();
					return PlayerState.JUMPING_UP;
				}
				else if (directionalInput.x != 0.0f)
					return PlayerState.WALKING;
				else
					return PlayerState.IDLE;
			case PlayerState.WALKING:
				if (jumpButtonPressed)
				{
					Jump();
					return PlayerState.JUMPING_UP;
				}
				else if (Math.Abs(velocity.x) < EPSILON)
					return PlayerState.IDLE;
				else
					return PlayerState.WALKING;
			case PlayerState.JUMPING_UP:
				if (velocity.y > 0)
					return PlayerState.JUMPING_UP;
				else
					return PlayerState.JUMPING_DOWN;
			case PlayerState.JUMPING_DOWN:
				if (Math.Abs(velocity.y) < EPSILON)
					return PlayerState.IDLE;
				else
					return PlayerState.JUMPING_DOWN;
			case PlayerState.SUPER_JUMP:
				if (velocity.y > 0)
					return PlayerState.SUPER_JUMP;
				else
					return PlayerState.ROLLING;
			case PlayerState.ROLLING:
				if (Math.Abs(velocity.y) < EPSILON)
					return PlayerState.IDLE;
				else
					return PlayerState.ROLLING;
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

		gravity = -(8 * maxJumpHeight) / Mathf.Pow(floatingTime, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * (floatingTime / 2);
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update()
	{
		CalculateVelocity();
		
		if (wallJump)
			HandleWallSliding();

		controller.Move(velocity * Time.deltaTime, directionalInput);

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

		State = GetNextState();
		jumpButtonPressed = false;
	}

	public void SetDirectionalInput(Vector2 input)
	{
		directionalInput = input;
		if (Math.Abs(input.x) > EPSILON)
		{
			HeadingLeft = input.x < 0.0f;
		}
	}

	public void OnJumpInputDown()
	{
		jumpButtonPressed = true;
	}

	private void Jump()
	{
		if (wallJump)
		{
			if (wallSliding)
			{
				if (wallDirX == directionalInput.x)
				{
					velocity.x = -wallDirX * wallJumpClimb.x;
					velocity.y = wallJumpClimb.y;
				}
				else if (directionalInput.x == 0)
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
				if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
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
		if (variableHeightJump && velocity.y > minJumpVelocity)
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

				if (directionalInput.x != wallDirX && directionalInput.x != 0)
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

	public void OnAttackInputDown()
	{

	}

	public void OnAttackInputUp()
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
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}
}