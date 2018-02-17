using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MissileBody : Enemy
{
    [Space]
    public MissileHead SpareHead;
    [Space]
    public Sprite NormalBodySprite;
    public Sprite HitBodySprite;
    [Space]
    public float HeadLaunchInterval = 3f;

    private MissileHead currentHead;
    private SpriteRenderer spriteRenderer;
    private float hitStateEndTime;
    private bool attackEnabled = true;
    private float nextHeadLaunch;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SpareHead.gameObject.SetActive(false);
        LaunchHead();
    }

    protected override void Update()
    {
        base.Update();

        float now = Time.time;
        if (now >= hitStateEndTime)
        {
            attackEnabled = true;
            spriteRenderer.sprite = NormalBodySprite;
        }
        if (now >= nextHeadLaunch)
            LaunchHead();
    }

    public override void Die()
    {
        Destroy(gameObject);
    }

    public override void OnAttack(IInteractable target)
    {
        if (attackEnabled)
            target.OnDamaged(this, 1);
    }

    public override void OnDamaged(IInteractable attacker, int damage, Vector2 knockback)
    {
        base.OnDamaged(attacker, damage, knockback);
        hitStateEndTime = Time.time + 1f;
        attackEnabled = false;
        spriteRenderer.sprite = HitBodySprite;
    }

    protected override void UpdateVelocity()
    {
        const float targetVelocityX = 0f;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }

    private void LaunchHead()
    {
        currentHead = Instantiate(SpareHead, transform);
        currentHead.gameObject.SetActive(true);
        nextHeadLaunch = Time.time + HeadLaunchInterval;
    }
}
