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
    
    /// <summary>
    /// Scale and rotate a local space vector to global space.
    /// </summary>
    /// <param name="localVector">A local space vector.</param>
    /// <param name="component">Component in which localVector exists.</param>
    /// <returns>Global space vector.</returns>
    internal static Vector3 ToGlobalSpace(this Vector3 localVector, Component component)
    {
        Transform transform = component.transform;
        return transform.rotation * Vector3.Scale(localVector, transform.lossyScale);
    }

    /// <summary>
    /// Scale and rotate a local space vector to global space.
    /// </summary>
    /// <param name="localVector">A local space vector.</param>
    /// <param name="component">Component in which localVector exists.</param>
    /// <returns>Global space vector.</returns>
    internal static Vector2 ToGlobalSpace(this Vector2 localVector, Component component)
    {
        Transform transform = component.transform;
        return transform.rotation * Vector2.Scale(localVector, transform.lossyScale);
    }

    /// <summary>
    /// Convert local coordinate position to global coordinates.
    /// </summary>
    /// <param name="localVector">A local coordinate vector.</param>
    /// <param name="component">Component in which localVector exists.</param>
    /// <returns>Global coordinates.</returns>
    internal static Vector3 ToGlobalPosition(this Vector3 localVector, Component component)
    {
        Transform transform = component.transform;
        return transform.position + transform.rotation * Vector3.Scale(localVector, transform.lossyScale);
    }

    /// <summary>
    /// Convert local coordinate position to global coordinates.
    /// </summary>
    /// <param name="localVector">A local coordinate vector.</param>
    /// <param name="component">Component in which localVector exists.</param>
    /// <returns>Global coordinates.</returns>
    internal static Vector2 ToGlobalPosition(this Vector2 localVector, Component component)
    {
        Transform transform = component.transform;
        return transform.position + transform.rotation * Vector3.Scale(localVector, transform.lossyScale);
    }
}
