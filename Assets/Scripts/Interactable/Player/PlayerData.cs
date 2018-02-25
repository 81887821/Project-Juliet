using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerSoundEffects))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Controller2D))]
public class PlayerData : MonoBehaviour
{
    public delegate void PlayerTransformationHandler(bool isSmallForm);
    public delegate void AvailabilityChangeHandler(bool available);
    public delegate void PlayerHPChangeHandler(int currentHealth);

    public event PlayerTransformationHandler PlayerTransformed = delegate {};
    public event AvailabilityChangeHandler SpecialActionAvailbilityChanged = delegate {};
    public event AvailabilityChangeHandler TransformationAvailbilityChanged = delegate {};
    public event PlayerHPChangeHandler PlayerHPChanged = delegate {};
    public event Action PlayerDead = delegate {};

    public static PlayerData Instance
    {
        get;
        private set;
    }

    [Header("Player Condition")]
    public bool IsSmallForm = true;
    public int MaxHealth = 6;

    [Header("Common Actions")]
    public Vector2 Knockback = new Vector2(50f, 15f);
    public float KnockbackTime = .3f;
    public GameObject TransformationEffects;

    [Space]
    public float SpecialActionAvailableTime = .5f;
    public float DamageIgnoreDurationAfterHit = 1f;
    public float TransformationCoolTime = .5f;

    [Header("Julia Actions")]
    public float MaxJumpHeight = 15;
    public float MinJumpHeight = 4;
    public float FloatingTime = .8f;
    public float AccelerationTimeAirborne = .2f;
    public float AccelerationTimeGrounded = .1f;
    public float JuliaMoveSpeed = 25;
    public float SupperJumpMultiplier = 1.5f;

    [Header("Julia Advanced Movement")]
    public Vector2 WallJump = new Vector2(70f, 70f);
    public float WallGravityRatio = .3f;

    [Header("Juliett Actions")]
    public float JuliettMoveSpeed = 33;
    public float UppercutDuration = 0.5f; // Match this value with JuliettUppercut animation.
    public float[] AttackInterval = { 0.3f, 0.4f, 0.6f, 0.6f };
    public Vector3[] AccelerationOnAttack = { Vector3.zero, Vector3.zero, new Vector3(60f, 0f), new Vector3(100f, 0f) };
    public Vector2 EnemyKnockbackOnUppercut = new Vector2(20f, 100f);
    public Vector2[] EnemyKnockbackOnAttack = { new Vector2(15f, 12f), new Vector2(20f, 12f), new Vector2(60f, 12f), new Vector2(110f, 15f) };

    public PlayerBase CurrentPlayerCharacter
    {
        get
        {
            if (IsSmallForm)
                return julia;
            else
                return juliett;
        }
    }
    public PlayerBase WaitingPlayerCharacter
    {
        get
        {
            if (IsSmallForm)
                return juliett;
            else
                return julia;
        }
    }
    public int CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;
            PlayerHPChanged(value);
            if (currentHealth <= 0)
                PlayerDead();
        }
    }
    public bool CanDoSpecialAction
    {
        get
        {
            return specialActionAvailable;
        }

        set
        {
            specialActionAvailable = value;
            SpecialActionAvailbilityChanged(value);

            if (specialActionAvailable)
                specialActionTimer = StartCoroutine(SpecialActionTimer());
            else if (specialActionTimer != null)
                StopCoroutine(specialActionTimer);
        }
    }
    public bool IsTransformationCoolTime
    {
        get
        {
            return isTransformationCoolTime;
        }

        set
        {
            isTransformationCoolTime = value;
            TransformationAvailbilityChanged(!value);

            if (transformationCoolTimeTimer != null)
                StopCoroutine(transformationCoolTimeTimer);

            if (value)
                StartCoroutine(TransformationCoolTimeTimer());
        }
    }

    private int currentHealth;

    private Julia julia;
    private Juliett juliett;
    private BoxCollider2D physicalCollider;
    private Controller2D controller;
    private PlayerSoundEffects soundEffects;
    private bool specialActionAvailable = false;
    private Coroutine specialActionTimer = null;
    private bool isTransformationCoolTime = false;
    private Coroutine transformationCoolTimeTimer = null;

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

        julia = GetComponentInChildren<Julia>();
        juliett = GetComponentInChildren<Juliett>();
        physicalCollider = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();
        soundEffects = GetComponent<PlayerSoundEffects>();

        currentHealth = MaxHealth;
    }

    private void Start()
    {
        WaitingPlayerCharacter.IsActive = false;
    }

    public void OnActionButtonClicked()
    {
        CurrentPlayerCharacter.OnActionButtonClicked();
    }

    public void OnTransformationButtonClicked()
    {
        if (julia.CanTransform && juliett.CanTransform)
        {
            IsSmallForm = !IsSmallForm;
            physicalCollider.size = CurrentPlayerCharacter.PhysicalBounds.size;
            physicalCollider.offset = CurrentPlayerCharacter.PhysicalBounds.center;
            controller.CalculateRaySpacing();

            CurrentPlayerCharacter.IsActive = true;
            WaitingPlayerCharacter.IsActive = false;
            CurrentPlayerCharacter.OnTransformation(WaitingPlayerCharacter);

            PlayerTransformed(IsSmallForm);
            CanDoSpecialAction = true;
            IsTransformationCoolTime = true;
            PlayTransformationEffects();
            soundEffects.PlayTransformationSound();
        }
        else
            soundEffects.PlayTransformationDeniedSound();
    }

    private IEnumerator SpecialActionTimer()
    {
        yield return new WaitForSeconds(SpecialActionAvailableTime);
        specialActionTimer = null;
        CanDoSpecialAction = false;
    }

    private IEnumerator TransformationCoolTimeTimer()
    {
        yield return new WaitForSeconds(TransformationCoolTime);
        transformationCoolTimeTimer = null;
        IsTransformationCoolTime = false;
    }

    private void PlayTransformationEffects()
    {
        GameObject effect = Instantiate(TransformationEffects, transform);
        effect.SetActive(true);
    }
}
