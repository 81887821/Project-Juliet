using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput
    {
        get;
        private set;
    }
    public Vector2 DirectionalInput
    {
        get
        {
            return new Vector2(HorizontalInput, 0f);
        }
    }
    
    private PlayerCore player;
    private InGameUIManager ui;

    private void Awake()
    {
        player = GetComponent<PlayerCore>();
    }

    private void Start()
    {
        ui = InGameUIManager.Instance;
        ui.ActionButton.onClick.AddListener(player.OnActionButtonClicked);
        ui.TransformationButton.onClick.AddListener(player.OnTransformationButtonClicked);
    }
    
    private void Update()
    {
        HorizontalInput = ui.movementScrollbar.value * 2 - 1;

#if DEBUG
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ui.movementScrollbar.value -= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ui.movementScrollbar.value += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            ui.movementScrollbar.value += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            ui.movementScrollbar.value -= 0.5f;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ui.ActionButton.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ui.TransformationButton.onClick.Invoke();
        }
#endif
    }
}
