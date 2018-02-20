using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LaserController : MonoBehaviour
{
    public LayerMask CollisionMask = 2560;

    private const float SKIN_WIDTH = .015f;

    private SpriteRenderer spriteRenderer;
    private float length;
    private Vector2 localOrigin;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Sprite sprite = spriteRenderer.sprite;
        Bounds bounds = sprite.bounds;

        length = bounds.size.x;
        localOrigin = new Vector2(length, bounds.size.y / 2f) - sprite.pivot / sprite.pixelsPerUnit;
    }

    public RaycastHit2D Grow(float growAmount)
    {
        if (growAmount <= 0)
        {
            transform.localScale = GetNewScale(growAmount);
            return new RaycastHit2D();
        }
        else
        {
            float rayLength = growAmount + SKIN_WIDTH;
            Vector2 rayOrigin = localOrigin.ToGlobalPosition(this);

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.right.ToGlobalSpace(this), rayLength, CollisionMask);
            RaycastHit2D nearestHit = new RaycastHit2D();
            float minDistance = float.MaxValue;

            foreach (RaycastHit2D hit in hits)
            {
#if DEBUG
                Debug.DrawRay(rayOrigin, Vector2.right.ToGlobalSpace(this) * rayLength, Color.red);
#endif
                if (hit.distance < minDistance)
                {
                    nearestHit = hit;
                    minDistance = hit.distance;

                    if (hit.distance <= SKIN_WIDTH)
                    {
                        growAmount = 0f;
                        break;
                    }
                    else
                    {
                        growAmount = hit.distance - SKIN_WIDTH;
                        rayLength = hit.distance;
                    }
                }
            }
#if DEBUG
            if (hits.Length == 0)
                Debug.DrawRay(rayOrigin, Vector2.right.ToGlobalSpace(this) * rayLength, Color.white);
#endif

            transform.localScale = GetNewScale(growAmount);
            return nearestHit;
        }
    }

    private Vector3 GetNewScale(float growAmount)
    {
        Vector3 scale = transform.localScale;
        scale.x += growAmount / length;
        return scale;
    }

    public void Shrink(float shrinkAmount)
    {
        transform.localScale = GetNewScale(-shrinkAmount);
        transform.Translate(Vector3.right * shrinkAmount);
    }
}
