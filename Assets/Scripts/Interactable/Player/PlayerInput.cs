using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerData))]
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
    
    private PlayerData player;
    private InGameUIManager ui;

    private void Awake()
    {
        player = GetComponent<PlayerData>();
    }

    private void Start()
    {
        ui = InGameUIManager.Instance;
        ui.ActionButton.onClick.AddListener(player.OnActionButtonClicked);
        ui.TransformationButton.onClick.AddListener(player.OnTransformationButtonClicked);
    }
    
    private void Update()
    {
        HorizontalInput = ui.MovementScrollbar.value * 2 - 1;

#if DEBUG
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ui.MovementScrollbar.value -= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ui.MovementScrollbar.value += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            ui.MovementScrollbar.value += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            ui.MovementScrollbar.value -= 0.5f;
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
