using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalExpander : Expander
{
    public SpriteRenderer LeftBlock;
    public SpriteRenderer RightBlock;

    protected override void Awake()
    {
        base.Awake();
        endBlock1 = LeftBlock;
        endBlock2 = RightBlock;

        endBlock1Length = LeftBlock.sprite.bounds.size.x;
        endBlock2Length = RightBlock.sprite.bounds.size.x;
        totalLength = boxCollider.bounds.size.x - endBlock1Length - endBlock2Length;
        if (MiddleBlocks.Length > 0)
            middleBlockLength = MiddleBlocks[0].sprite.bounds.size.x;
        else
            middleBlockLength = 0f;

        expandDirection = Vector2.right;
        initialLocation = new Vector2(boxCollider.bounds.min.x + endBlock1Length / 2f, boxCollider.bounds.center.y);
    }
}
