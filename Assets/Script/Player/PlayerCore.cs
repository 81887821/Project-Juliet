using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Controller2D))]
public class PlayerCore : MonoBehaviour
{
    [Header("Player Condition")]
    public bool isSmallForm;
    public int currentHealth;

    [Header("Common Movement")]
    public float maxJumpHeight = 3;
    public float floatingTime = .8f;
    public float juliaMoveSpeed = 6;
    public float juliettMoveSpeed = 9;
    public float bigFormSpeedMul = 1.5f;

    public Vector2 Knockback;
    public float KnockbackTime = .3f;
    public float AttackMotionInverval = .4f;

    [Header("Advanced Movement")]
    public bool wallJumpEnabled;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;

    [Space]
    public float minJumpHeight = 1;

    [Space]
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;

    public PlayerBase CurrentPlayerCharacter
    {
        get
        {
            if (isSmallForm)
                return julia;
            else
                return juliett;
        }
    }
    public PlayerBase WaitingPlayerCharacter
    {
        get
        {
            if (isSmallForm)
                return juliett;
            else
                return julia;
        }
    }

    private Julia julia;
    private Juliett juliett;
    private BoxCollider2D physicalCollider;
    private Controller2D controller;

    void Start()
    {
        julia = GetComponentInChildren<Julia>();
        juliett = GetComponentInChildren<Juliett>();
        physicalCollider = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();

        WaitingPlayerCharacter.IsActive = false;
    }

    void Update()
    {

    }

    public void OnActionButtonClicked()
    {
        CurrentPlayerCharacter.OnActionButtonClicked();
    }

    public void OnTransformationButtonClicked()
    {
        if (julia.CanTransform && juliett.CanTransform)
        {
            isSmallForm = !isSmallForm;
            physicalCollider.size = CurrentPlayerCharacter.PhysicalCollider.size;
            physicalCollider.offset = CurrentPlayerCharacter.PhysicalCollider.offset;
            controller.CalculateRaySpacing();

            CurrentPlayerCharacter.IsActive = true;
            WaitingPlayerCharacter.IsActive = false;
            CurrentPlayerCharacter.OnTransformation(WaitingPlayerCharacter);
        }
    }
}