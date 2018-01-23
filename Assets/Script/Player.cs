using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
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
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;
	Vector2 directionalInput;
	bool wallSliding;
	int wallDirX;

	float endKnockbackTime;

	void Start()
	{
		controller = GetComponent<Controller2D>();

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
	}

	public void SetDirectionalInput(Vector2 input)
	{
		directionalInput = input;
	}

	public void OnJumpInputDown()
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