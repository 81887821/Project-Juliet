using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackDetector : MonoBehaviour
{
    private IInteractable interactable;

    public LayerMask TargetMask;
    public bool ActiveOnStart = false;

    public IInteractable Interactable
    {
        get
        {
            return interactable;
        }
    }

    private void Start()
    {
        interactable = GetComponent<IInteractable>();
        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();
        if (interactable == null)
            Debug.LogError("Cannot find interactive object.");
        gameObject.SetActive(ActiveOnStart);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageDetector target = collision.GetComponent<DamageDetector>();

        if (target != null && TargetMask.Contains(target.gameObject.layer))
            interactable.OnAttack(target.Interactable);
    }
}
