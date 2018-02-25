using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ApplyGravity : MonoBehaviour
{
    private const int RAY_COUNT = 3;

    public LayerMask CollisionMask;
    public float Gravity = -150f;

    private float speed = 0f;
    private Vector2 localRaycastOrigin;
    private float distanceBetweenRays;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        localRaycastOrigin = new Vector2(boxCollider.offset.x - boxCollider.size.x / 2f, boxCollider.offset.y - boxCollider.size.y / 2f);
        distanceBetweenRays = boxCollider.size.x / 2f;
#if DEBUG
        if (Gravity > 0)
            Debug.LogErrorFormat("[{0}] To collision check work properly, Gravity must be negative.", name);
#endif
    }

    private void Update()
    {
        float moveAmount = speed * Time.deltaTime;
        bool collided = CollisionCheck(ref moveAmount);

        if (collided)
            speed = 0f;
        else
        {
            transform.Translate(0f, moveAmount, 0f);
            speed += Gravity * Time.deltaTime;
        }
    }

    private Vector2 GetRaycastOrigin(int rayNumber)
    {
        return (localRaycastOrigin + new Vector2(distanceBetweenRays * rayNumber, 0f)).ToGlobalPosition(this);
    }

    private bool CollisionCheck(ref float moveAmount)
    {
        float absoluteMoveAmount = Mathf.Abs(moveAmount);

        for (int i = 0; i < RAY_COUNT; i++)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(GetRaycastOrigin(i), Vector2.down, absoluteMoveAmount, CollisionMask);
#if DEBUG
            Debug.DrawRay(GetRaycastOrigin(i), Vector2.down * absoluteMoveAmount, hits.Length != 0 ? Color.red : Color.white);
#endif
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform == transform) // To prevent self collision.
                    continue;
                else if (hit.distance == 0f)
                    return true;
                else if (hit.distance < absoluteMoveAmount)
                    absoluteMoveAmount = hit.distance;
            }
        }

        moveAmount = Mathf.Sign(moveAmount) * absoluteMoveAmount;
        return false;
    }
}
