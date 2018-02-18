using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal static class Utils
{
    internal static bool Contains(this LayerMask mask, int layer)
    {
        return ((mask.value >> layer) & 1) == 1;
    }

    internal static bool Empty(this LayerMask mask)
    {
        return mask.value == 0;
    }
}
