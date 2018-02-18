using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileHead : Enemy
{
    private enum HeadState { NONE, VERTICAL_LOADING, HORIZONTAL_LOADING, WAITING, LAUNCHED, OBSTACLE_HIT }

    public GameObject ExplosionEffect;
    public float WaitingTimeBeforeLaunch = 1f;
    public float LoadingAbsoluteSpeed = 3f;
    public int ExplosionDamageToBody = 3;

    private HeadState state = HeadState.VERTICAL_LOADING;
    private HeadState nextState = HeadState.VERTICAL_LOADING;
    private Vector2 launchVelocity;
    private int[] obstacles;

    protected override void Start()
    {
        base.Start();
        launchVelocity = new Vector2(maxSpeed, 0f);
        obstacles = new int[2];
        obstacles[0] = LayerMask.NameToLayer("Obstacle");
        obstacles[1] = LayerMask.NameToLayer("Obstacle (Unstickable)");
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

        nextState = HeadState.NONE;
    }

    private HeadState GetNextStateByEnvironment()
    {
        switch (state)
        {
            case HeadState.VERTICAL_LOADING:
                if (transform.localPosition.y < 5.4f)
                    return HeadState.VERTICAL_LOADING;
                else
                    return HeadState.HORIZONTAL_LOADING;
            case HeadState.HORIZONTAL_LOADING:
                if (transform.localPosition.x < 0.3f)
                    return HeadState.HORIZONTAL_LOADING;
                else
                    return HeadState.WAITING;
            case HeadState.WAITING:
                if (Time.time >= stateEndTime)
                    return HeadState.LAUNCHED;
                else
                    return HeadState.WAITING;
            case HeadState.LAUNCHED:
                foreach (int layer in obstacles)
                {
                    Controller2D.CollisionInfo collision = controller.collisions;
                    if (collision.above.Contains(layer) || collision.below.Contains(layer) || collision.front.Contains(layer) || collision.back.Contains(layer))
                        return HeadState.OBSTACLE_HIT;
                }
                return HeadState.LAUNCHED;
            case HeadState.OBSTACLE_HIT:
                return HeadState.OBSTACLE_HIT;
            default:
                throw new Exception("Forbidden state for MissileHead : " + state);
        }
    }

    private void HandleStateTransitionSideEffect(HeadState oldState, HeadState newState)
    {
        switch (newState)
        {
            case HeadState.WAITING:
                stateEndTime = Time.time + WaitingTimeBeforeLaunch;
                break;
            case HeadState.LAUNCHED:
                transform.parent = transform.parent.parent;
                break;
            case HeadState.OBSTACLE_HIT:
                Die();
                break;
        }
    }

    public override void Die()
    {
        ExplosionEffect.transform.parent = transform.parent.parent;
        ExplosionEffect.SetActive(true);

        if (state <= HeadState.WAITING)
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
            case HeadState.VERTICAL_LOADING:
                velocity = new Vector2(0f, LoadingAbsoluteSpeed);
                break;
            case HeadState.HORIZONTAL_LOADING:
                velocity = new Vector2(LoadingAbsoluteSpeed, 0f);
                break;
            case HeadState.WAITING:
                velocity = Vector2.zero;
                break;
            case HeadState.LAUNCHED:
                velocity = launchVelocity;
                break;
            default:
                throw new Exception("Forbidden state for MissileHead : " + state);
        }
    }
}
