using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
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
    
    private Player player;
    private InGameUIManager ui;

    void Start()
    {
        player = GetComponent<Player>();
        ui = InGameUIManager.Instance;
        ui.ActionButton.onClick.AddListener(player.OnActionButtonClicked);
        ui.TransformationButton.onClick.AddListener(player.OnTransformationButtonClicked);
    }
    
    void Update()
    {
        HorizontalInput = ui.movementScrollbar.value * 2 - 1;

#if DEBUG
        if (Input.GetKeyDown(KeyCode.Q))
        {
            player.OnPlayerDamaged(10, 1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            player.OnPlayerDamaged(10, -1);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            ui.movementScrollbar.value = 0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            ui.movementScrollbar.value = 1f;
        }
        else
        {
            ui.movementScrollbar.value = 0.5f;
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
