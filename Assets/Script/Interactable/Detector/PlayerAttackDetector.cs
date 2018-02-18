using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAttackDetector : MonoBehaviour
{
    public LayerMask TargetMask;
    public bool ActiveOnStart = false;

    private IInteractable interactable;
    private BoxCollider2D boxCollider;
    private List<DamageDetector> hitList = new List<DamageDetector>();
    
    private void OnEnable()
    {
        hitList.Clear();
    }

    private void Start()
    {
        interactable = GetComponentInParent<IInteractable>();
        if (interactable == null)
            Debug.LogError("Cannot find interactive object.");
        boxCollider = GetComponent<BoxCollider2D>();
        gameObject.SetActive(ActiveOnStart);
    }

    private void Update()
    {
        foreach (Collider2D collision in Physics2D.OverlapBoxAll(boxCollider.bounds.center, boxCollider.bounds.size, 0f, TargetMask))
        {
            DamageDetector target = collision.GetComponent<DamageDetector>();

            if (target != null && TargetMask.Contains(target.gameObject.layer))
            {
                if (!hitList.Contains(target))
                {
                    hitList.Add(target);
                    interactable.OnAttack(target.Interactable);
                }
            }
        }
    }
}
