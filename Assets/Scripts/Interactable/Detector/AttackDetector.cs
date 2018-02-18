using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackDetector : MonoBehaviour
{
    public LayerMask TargetMask;
    public bool ActiveOnStart = true;

    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<IInteractable>();
        if (interactable == null)
            interactable = GetComponentInParent<IInteractable>();
        if (interactable == null)
            Debug.LogError("Cannot find interactive object.");
    }

    private void Start()
    {
        gameObject.SetActive(ActiveOnStart);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DamageDetector target = collision.GetComponent<DamageDetector>();

        if (target != null && TargetMask.Contains(target.gameObject.layer))
            interactable.OnAttack(target.Interactable);
    }
}
