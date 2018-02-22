using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SlideDoor : MonoBehaviour
{
    public Vector2 LocalOpenPosition;
    public float Duration;

    private Vector3 globalClosedPosition;
    private Vector3 globalOpenPosition;

    private void Awake()
    {
        globalClosedPosition = transform.position;
        globalOpenPosition = transform.position + (Vector3)LocalOpenPosition;
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(globalClosedPosition, globalOpenPosition));
    }
    
    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(globalOpenPosition, globalClosedPosition));
    }

    private IEnumerator MoveDoor(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 totalMovement = endPosition - startPosition;
        float progress = (transform.position - startPosition).sqrMagnitude / totalMovement.sqrMagnitude;

        while (progress < 1f)
        {
            transform.position = startPosition + totalMovement * progress;
            yield return new WaitForEndOfFrame();
            progress += Time.deltaTime / Duration;
        }

        transform.position = endPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            float size = .3f;
            Vector3 globalOpenPosition = (Vector3)LocalOpenPosition + transform.position;

            Gizmos.DrawLine(globalOpenPosition - Vector3.up * size, globalOpenPosition + Vector3.up * size);
            Gizmos.DrawLine(globalOpenPosition - Vector3.left * size, globalOpenPosition + Vector3.left * size);
            Handles.Label(globalOpenPosition, "Opened");
        }
    }
#endif
}
