﻿using System.Collections;
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
    private ButtonPresseHandler leftButton;
    private ButtonPresseHandler rightButton;

    private void Awake()
    {
        player = GetComponent<PlayerData>();
    }

    private void Start()
    {
        ui = InGameUIManager.Instance;
        ui.ActionButton.onClick.AddListener(player.OnActionButtonClicked);
        ui.TransformationButton.onClick.AddListener(player.OnTransformationButtonClicked);
        leftButton = ui.LeftButton.GetComponent<ButtonPresseHandler>();
        rightButton = ui.RightButton.GetComponent<ButtonPresseHandler>();
    }
    
    private void Update()
    {
        if (SettingManager.UsingScrollbar)
            HorizontalInput = ui.MovementScrollbar.value * 2 - 1;
        else
        {
            float horizontalInput = 0.5f;
            if (leftButton.Pressing)
                horizontalInput -= 0.5f;
            if (rightButton.Pressing)
                horizontalInput += 0.5f;
            HorizontalInput = horizontalInput;
        }

#if DEBUG
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HorizontalInput -= 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            HorizontalInput += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            HorizontalInput += 0.5f;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            HorizontalInput -= 0.5f;
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
