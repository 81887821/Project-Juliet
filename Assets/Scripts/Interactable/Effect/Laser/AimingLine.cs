using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LaserController))]
public class AimingLine : MonoBehaviour
{
    public float SpeedRelativeToJulia = 10f;

    private LaserController controller;
    private float speed;
    private bool growing = true;
    private List<IInteractable> attackedTargets = new List<IInteractable>();

    private void Awake()
    {
        controller = GetComponent<LaserController>();
    }

    private void Start()
    {
        speed = PlayerData.Instance.JuliaMoveSpeed * SpeedRelativeToJulia;
    }

    private void Update()
    {
        if (growing)
            controller.Grow(speed * Time.deltaTime);
        else
        {
            controller.Shrink(speed * Time.deltaTime);
            if (transform.localScale.x <= 0f)
                Destroy(gameObject);
        }
    }

    public void Stop()
    {
        growing = false;
    }
}
