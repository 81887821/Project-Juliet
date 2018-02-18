using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DamageDetector : MonoBehaviour
{
    private IInteractable interactable;

    public IInteractable Interactable
    {
        get
        {
            return interactable;
        }
    }

    private void Awake()
    {
        interactable = GetComponent<IInteractable>();
        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();
        if (interactable == null)
            Debug.LogError("Cannot find interactive object.");
    }
}
