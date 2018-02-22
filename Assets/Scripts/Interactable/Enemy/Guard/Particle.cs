using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Particle : MonoBehaviour, IInteractable
{
    public float RandomRangeBegin = -90f;
    public float RandomRangeEnd = 0f;
    public float MinInitialSpeedRelativeToJulia = 1f;
    public float MaxInitialSpeedRelativeToJulia = 5f;
    public float RemainingDuration = 2f;
    public float AttackableDuration = 1.5f;
    public GameObject ExplosionEffect;

    private Controller2D controller;
    private Vector2 initialVelocity;
    private float startTime;

    private void Awake()
    {
        controller = GetComponent<Controller2D>();
        transform.parent = transform.parent.parent;
    }

    private void Start()
    {
        float angle = UnityEngine.Random.Range(RandomRangeBegin, RandomRangeEnd);
        float initialSpeed = PlayerData.Instance.JuliaMoveSpeed * UnityEngine.Random.Range(MinInitialSpeedRelativeToJulia, MaxInitialSpeedRelativeToJulia);
        initialVelocity = initialSpeed * new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
        startTime = Time.time;
    }

    private void Update()
    {
        float timeDelta = Time.time - startTime;

        if (timeDelta >= RemainingDuration)
            Destroy(gameObject);

        const float polynomialOrder = 12f;
        Vector2 velocity = Mathf.Pow(RemainingDuration - timeDelta, polynomialOrder) / Mathf.Pow(RemainingDuration, polynomialOrder) * initialVelocity;
        controller.Move(velocity * Time.deltaTime);

        transform.localScale = new Vector3((RemainingDuration - timeDelta) / RemainingDuration, (RemainingDuration - timeDelta) / RemainingDuration);
    }

    public void Die()
    {
        throw new InvalidOperationException("Particle cannot die.");
    }

    public void OnAttack(IInteractable target)
    {
        if (Time.time <= startTime + AttackableDuration)
        {
            target.OnDamaged(this, 1);
            ExplosionEffect.transform.parent = null;
            ExplosionEffect.transform.localScale = new Vector3(2f, 2f, 2f);
            ExplosionEffect.SetActive(true);
            Destroy(gameObject);
        }
    }

    public void OnDamaged(IInteractable attacker, int damage)
    {
        throw new InvalidOperationException("Particle cannot be damaged.");
    }

    public void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        throw new InvalidOperationException("Particle cannot be damaged.");
    }
}
