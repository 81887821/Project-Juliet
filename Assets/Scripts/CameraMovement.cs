using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform TraceTarget;

    private void LateUpdate()
    {
        transform.position = new Vector3(TraceTarget.position.x, TraceTarget.position.y + 9f, -10f);
    }
}
