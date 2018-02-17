using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
    public CollisionInfo collisions;

    public void Move(Vector2 moveAmount, bool standingOnPlatform = false, Space moveSpace = Space.Self)
    {
        UpdateRaycastOrigins();

        collisions.Reset();
        collisions.moveAmountOld = moveAmount;

        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        transform.Translate(moveAmount, moveSpace);
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = transform.right.x * Mathf.Sign(moveAmount.x);
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
#if DEBUG
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
#endif
                if (hit.distance <= skinWidth)
                {
                    moveAmount.x = 0f;
                    i = horizontalRayCount; // Break after setting collision flags.
                }
                else
                {
                    moveAmount.x = (hit.distance - skinWidth) * Mathf.Sign(moveAmount.x);
                    rayLength = hit.distance;
                }

                if (collisions.moveAmountOld.x >= 0f)
                    collisions.front += hit.transform.gameObject.layer;
                else
                    collisions.back += hit.transform.gameObject.layer;
            }
#if DEBUG
            else
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.white);
#endif
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + transform.right.x * moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            if (hit)
            {
#if DEBUG
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
#endif

                if (hit.distance <= skinWidth)
                {
                    moveAmount.y = 0f;
                    i = verticalRayCount; // Break after setting collision flags.
                }
                else
                {
                    moveAmount.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;
                }

                if (directionY == -1)
                    collisions.below += hit.transform.gameObject.layer;
                else if (directionY == 1)
                    collisions.above += hit.transform.gameObject.layer;
                else
                    Debug.LogError("Error : Wrong vertical collisions direction Y : " + directionY);
            }
#if DEBUG
            else
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.white);
#endif
        }
    }
    
    public struct CollisionInfo
    {
        public Contacts above;
        public Contacts below;
        public Contacts front;
        public Contacts back;

        public Vector2 moveAmountOld;

        public void Reset()
        {
            above = below = front = back = new Contacts(0);
        }
    }

    public struct Contacts
    {
        public LayerMask value;

        public Contacts(LayerMask initialValue)
        {
            value = initialValue;
        }

        public bool Contains(int layer)
        {
            return value.Contains(layer);
        }

        public bool Contains(string layerName)
        {
            return value.Contains(LayerMask.NameToLayer(layerName));
        }

        public static Contacts operator +(Contacts contacts, int layer)
        {
            if (!contacts.value.Contains(layer))
                contacts.value += (1 << layer);
            return contacts;
        }

        public static implicit operator bool(Contacts contacts)
        {
            return !contacts.value.Empty();
        }
    }
}
