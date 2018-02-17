using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class Item : MonoBehaviour
{
    public float MovingPeriod = 1f;
    public float MovingLength = 1f;

    private Vector3 originalPosition;
    private float movingProgress = 0f;
    private bool movingUpward = true;

    protected virtual void Start()
    {
        originalPosition = transform.position;
    }

    public void Update()
    {
        if (movingUpward)
        {
            movingProgress += Time.deltaTime / MovingPeriod;
            transform.position = originalPosition + new Vector3(0f, (1f - (movingProgress - 1f) * (movingProgress - 1f)) * MovingLength);
            if (movingProgress >= 1f)
            {
                movingProgress = 0f;
                movingUpward = !movingUpward;
            }
        }
        else
        {
            movingProgress += Time.deltaTime / MovingPeriod;
            transform.position = originalPosition + new Vector3(0f, (1f - movingProgress) * (1f - movingProgress) * MovingLength);
            if (movingProgress >= 1f)
            {
                movingProgress = 0f;
                movingUpward = !movingUpward;
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            OnItemGet();
        }
    }

    public abstract void OnItemGet();
}
