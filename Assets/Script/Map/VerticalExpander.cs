using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalExpander : Expander
{
    public SpriteRenderer UpperBlock;
    public SpriteRenderer LowerBlock;

    protected override void Awake()
    {
        base.Awake();
        endBlock1 = UpperBlock;
        endBlock2 = LowerBlock;

        endBlock1Length = UpperBlock.sprite.bounds.size.y;
        endBlock2Length = LowerBlock.sprite.bounds.size.y;
        totalLength = boxCollider.bounds.size.y - endBlock1Length - endBlock2Length;
        if (MiddleBlocks.Length > 0)
            middleBlockLength = MiddleBlocks[0].sprite.bounds.size.y;
        else
            middleBlockLength = 0f;

        expandDirection = Vector2.down;
        initialLocation = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.max.y - endBlock1Length / 2f);
    }
}
