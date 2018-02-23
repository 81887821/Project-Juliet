using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileHead : Enemy
{
    private enum HeadState { None, VerticalLoading, HorizontalLoading, Waiting, Launched, ObstacleHit }

    public GameObject ExplosionEffect;
    public float WaitingTimeBeforeLaunch = 1f;
    public float LoadingAbsoluteSpeed = 3f;
    public int ExplosionDamageToBody = 3;

    private HeadState state = HeadState.VerticalLoading;
    private HeadState nextState = HeadState.VerticalLoading;
    private Vector2 launchVelocity;
    private int[] obstacles;
    private bool bodyDestroyed = false;

    protected override void Awake()
    {
        base.Awake();
        obstacles = new int[2];
        obstacles[0] = LayerMask.NameToLayer("Obstacle");
        obstacles[1] = LayerMask.NameToLayer("Obstacle (Unstickable)");
    }

    protected override void Start()
    {
        base.Start();
        launchVelocity = new Vector2(maxSpeed, 0f);
    }

    protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    private void UpdateState()
    {
        HeadState nextStateByEnvironment = GetNextStateByEnvironment();
        nextState = (nextState > nextStateByEnvironment ? nextState : nextStateByEnvironment);

        if (state != nextState)
        {
            HandleStateTransitionSideEffect(state, nextState);
            state = nextState;
        }

        nextState = HeadState.None;
    }

    private HeadState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case HeadState.VerticalLoading:
                if (transform.localPosition.y < 5.4f)
                    return HeadState.VerticalLoading;
                else
                    return HeadState.HorizontalLoading;
            case HeadState.HorizontalLoading:
                if (transform.localPosition.x < 0.3f)
                    return HeadState.HorizontalLoading;
                else
                    return HeadState.Waiting;
            case HeadState.Waiting:
                if (Time.time >= stateEndTime)
                    return HeadState.Launched;
                else
                    return HeadState.Waiting;
            case HeadState.Launched:
                foreach (int layer in obstacles)
                {
                    Controller2D.CollisionInfo collision = controller.Collisions;
                    if (collision.above.Contains(layer) || collision.below.Contains(layer) || collision.front.Contains(layer) || collision.back.Contains(layer))
                        return HeadState.ObstacleHit;
                }
                return HeadState.Launched;
            case HeadState.ObstacleHit:
                return HeadState.ObstacleHit;
            default:
                throw new Exception("Forbidden state for MissileHead : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(HeadState oldState, HeadState newState)
    {
        switch (newState)
        {
            case HeadState.Waiting:
                stateEndTime = Time.time + WaitingTimeBeforeLaunch;
                break;
            case HeadState.Launched:
                transform.parent = transform.parent.parent;
                break;
            case HeadState.ObstacleHit:
                Die();
                break;
        }
    }

    public override void Die()
    {
        ExplosionEffect.transform.parent = transform.parent.parent;
        ExplosionEffect.SetActive(true);

        if (!bodyDestroyed && state <= HeadState.Waiting)
            GetComponentInParent<MissileBody>().OnDamaged(this, ExplosionDamageToBody);

        Destroy(gameObject);
    }

    public override void OnAttack(IInteractable target)
    {
        target.OnDamaged(this, 1);
        Die();
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        OnAttack(attacker);
    }

    protected override void UpdateVelocity()
    {
        switch (state)
        {
            case HeadState.VerticalLoading:
                velocity = new Vector2(0f, LoadingAbsoluteSpeed);
                break;
            case HeadState.HorizontalLoading:
                velocity = new Vector2(LoadingAbsoluteSpeed, 0f);
                break;
            case HeadState.Waiting:
                velocity = Vector2.zero;
                break;
            case HeadState.Launched:
                velocity = launchVelocity;
                break;
            default:
                throw new Exception("Forbidden state for MissileHead : " + state);
        }
    }

    public void OnBodyDestroyed()
    {
        bodyDestroyed = true;
        if (state <= HeadState.Waiting)
            Die();
    }
}
