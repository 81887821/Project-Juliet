using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Controller2D))]
public class PlayerCore : MonoBehaviour
{
    public static PlayerCore Instance
    {
        get;
        private set;
    }

    [Header("Player Condition")]
    public bool isSmallForm = true;
    public int currentHealth = 6;

    [Header("Common Actions")]
    public Vector2 Knockback = new Vector2(15f, 4f);
    public float KnockbackTime = .3f;

    [Space]
    public float totalSpecialActionAvailableTime = .5f;
    public float cancelableSpecialActionAvailableTime = .3f;
    public float transformationDelayTime = 1f;

    [Header("Julia Actions")]
    public float maxJumpHeight = 3;
    public float minJumpHeight = 1;
    public float floatingTime = .8f;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float juliaMoveSpeed = 6;
    public float supperJumpMultiplier = 1.5f;

    [Header("Julia Advanced Movement")]
    public bool wallJumpEnabled;
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;

    [Header("Juliett Actions")]
    public float juliettMoveSpeed = 9;
    public float uppercutDuration = 0.75f; // Match this value with JuliettUppercut animation.
    public float[] attackInterval = { 0.4f, 0.4f, 0.4f, 0.4f };
    public Vector3[] accelerationOnAttack = { Vector3.zero, Vector3.zero, new Vector3(20f, 0f), new Vector3(30f, 0f) };
    public Vector2 enemyKnockbackOnUppercut = new Vector2(5f, 20f);
    public Vector2[] enemyKnockbackOnAttack = { new Vector2(5f, 4f), new Vector2(5f, 4f), new Vector2(20f, 4f), new Vector2(30f, 4f) };

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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(gameObject);
            return;
        }

#if DEBUG
        Debug.Assert(cancelableSpecialActionAvailableTime <= totalSpecialActionAvailableTime, "Cancelable special action available time cannot be larger than total special action available time.");
#endif
    }

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
            physicalCollider.size = CurrentPlayerCharacter.PhysicalBounds.size;
            physicalCollider.offset = CurrentPlayerCharacter.PhysicalBounds.center;
            controller.CalculateRaySpacing();

            CurrentPlayerCharacter.IsActive = true;
            WaitingPlayerCharacter.IsActive = false;
            CurrentPlayerCharacter.OnTransformation(WaitingPlayerCharacter);
        }
    }
}