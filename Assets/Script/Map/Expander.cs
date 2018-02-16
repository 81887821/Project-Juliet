using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Expander : MonoBehaviour
{
    public SpriteRenderer[] MiddleBlocks;

    protected BoxCollider2D boxCollider;

    protected SpriteRenderer endBlock1;
    protected SpriteRenderer endBlock2;
    protected Vector2 expandDirection;
    protected Vector2 initialLocation;

    protected float totalLength;
    protected float middleBlockLength;
    protected float endBlock1Length;
    protected float endBlock2Length;

    protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
#if DEBUG
        Vector3 size = MiddleBlocks[0].sprite.bounds.size;
        for (int i = 1; i < MiddleBlocks.Length; i++)
        {
            Vector3 nextSize = MiddleBlocks[i].sprite.bounds.size;
            if (size != nextSize)
                Debug.LogWarning("Middle blocks have different sizes.");
            size = nextSize;
        }
#endif
    }

    protected virtual void Start()
    {
#if DEBUG
        if (totalLength < 0f)
            Debug.LogWarning("Length is too short. Minimum length is " + (endBlock1Length + endBlock2Length));
        else if (Mathf.Abs(totalLength % middleBlockLength) > float.Epsilon)
            Debug.LogWarning("Total length - end block lengths is not multiple of middle block length. Shrink length " + totalLength % middleBlockLength);
#endif

        int numMiddleBlocks = Mathf.RoundToInt(totalLength / middleBlockLength);
        Vector2 currentLocation = initialLocation;
        System.Random random = new System.Random();

        Instantiate(endBlock1, transform.parent).transform.position = currentLocation;
        currentLocation += expandDirection * endBlock1Length;

        for (int i = 0; i < numMiddleBlocks; i++)
        {
            int blockIndex = random.Next(MiddleBlocks.Length);
            Instantiate(MiddleBlocks[blockIndex], transform.parent).transform.position = currentLocation;
            currentLocation += expandDirection * middleBlockLength;
        }

        Instantiate(endBlock2, transform.parent).transform.position = currentLocation;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            Destroy(spriteRenderer);
        Destroy(this);
    }
}

