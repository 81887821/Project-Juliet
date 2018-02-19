using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LaserController : MonoBehaviour
{
    public LayerMask CollisionMask = 2560;

    private const float SKIN_WIDTH = .015f;
    private const float DISTANCE_BETWEEN_RAYS = .25f;

    private SpriteRenderer spriteRenderer;
    private float length;
    private Vector2 localOrigin;
    private int rayCount;
    private float raySpacing;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Sprite sprite = spriteRenderer.sprite;
        Bounds bounds = sprite.bounds;

        length = bounds.size.x;
        localOrigin = new Vector2(length, 0f) - sprite.pivot / sprite.pixelsPerUnit;
        rayCount = Mathf.FloorToInt(bounds.size.y / DISTANCE_BETWEEN_RAYS);
        raySpacing = bounds.size.y / (rayCount - 1);
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
            RaycastHit2D nearestHit = new RaycastHit2D();
            float rayLength = growAmount + SKIN_WIDTH;

            for (int i = 0; i < rayCount; i++)
            {
                Vector2 rayOrigin = (localOrigin + Vector2.up * (raySpacing * i)).ToGlobalPosition(this);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right.ToGlobalSpace(this), rayLength, CollisionMask);

                if (hit)
                {
#if DEBUG
                    Debug.DrawRay(rayOrigin, Vector2.right.ToGlobalSpace(this) * rayLength, Color.red);
#endif
                    nearestHit = hit;

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
#if DEBUG
                else
                    Debug.DrawRay(rayOrigin, Vector2.right.ToGlobalSpace(this) * rayLength, Color.white);
#endif
            }

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
