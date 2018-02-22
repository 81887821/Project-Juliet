using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class ButtonTrigger : MonoBehaviour
{
    public LayerMask InteractableMask;
    public UnityEvent OnButtonPressed;
    public UnityEvent OnButtonReleased;

    private bool Pressing
    {
        get
        {
            return pressing;
        }
        set
        {
            if (pressing != value)
            {
                pressing = value;
                if (value == true)
                    OnButtonPressed.Invoke();
                else
                    OnButtonReleased.Invoke();
            }
        }
    }

    private BoxCollider2D boxCollider;
    private bool pressing = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (Physics2D.OverlapBox(boxCollider.bounds.center, boxCollider.bounds.size, 0f, InteractableMask))
            Pressing = true;
        else
            Pressing = false;
    }
}
