﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This Script controls in-game UI like
//Specifically Movement, Action, Transformation, Health Icons, Diary Icons
//Setting Buttons should be controlled by Global UI Manager. Keep in Mind.

//Singleton Class
public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance
    {
        get;
        private set;
    }

    //Movement Variables
    [Header("Movement")]
    public Scrollbar MovementScrollbar; //joystick for movement

    [Header("Buttons")]
    public Button ActionButton;
    public Button TransformationButton;

    [Header("Transformation Button Images")]
    public Sprite JuliaToJuliettButtonSprite;
    public Sprite JuliettToJuliaButtonSprite;

    [Header("Action Button Images")]
    public Sprite JumpButtonSprite;
    public Sprite AttackButtonSprite;
    public Sprite SuperJumpButtonSprite;
    public Sprite UppercutButtonSprite;

    //Health & Diary Variables
    [Header("Health & Diary Sprite")]
    public Sprite BlankHeartImage;
    public Sprite HalfHeaertImage;
    public Sprite FullHeartImage;
    public Sprite EmptyReportImage;
    public Sprite CollectedReportImage;

    [Header("Health & Diary Set")]
    public GameObject HeartParentObject; //Object Which Contains heart Objects
    public GameObject DiaryParentObject; //Object Which Contains diary Objects

    private List<Image> heartImageList = new List<Image>();
    private List<Image> reportImageList = new List<Image>();

    private bool playerIsSmallForm = true;
    private bool playerCanDoSpecialAction = false;

    private bool PlayerIsSmallForm
    {
        get
        {
            return playerIsSmallForm;
        }
        set
        {
            playerIsSmallForm = value;
            UpdateButtonImages();
        }
    }
    private bool PlayerCanDoSpecialAction
    {
        get
        {
            return playerCanDoSpecialAction;
        }
        set
        {
            playerCanDoSpecialAction = value;
            UpdateButtonImages();
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;

        foreach (var itr in HeartParentObject.GetComponentsInChildren<Image>())
        {
            if (itr != null)
            {
                heartImageList.Add(itr);
            }
        }

        foreach (var itr in DiaryParentObject.GetComponentsInChildren<Image>())
        {
            if (itr != null)
            {
                reportImageList.Add(itr);
            }
        }
    }

    private void Start()
    {
        InitUIElements();

        PlayerData player = PlayerData.Instance;
        player.AvailableActionChanged += (canDoSpecialAction) => PlayerCanDoSpecialAction = canDoSpecialAction;
        player.PlayerTransformed += (isSmallForm) => PlayerIsSmallForm = isSmallForm;
        player.PlayerHPChanged += SetCurrentHeartNum;

        ReportManager.ReportCollected += SetReportSprites;
    }

    public void InitUIElements()
    {
        //0.5f = Center, 0 = Left, 1 = Right
        MovementScrollbar.value = 0.5f;
        foreach (var itr in heartImageList)
        {
            itr.sprite = FullHeartImage;
        }

        SetReportSprites();
    }

    //amount: current number of heart
    public void SetCurrentHeartNum(int amount)
    {
        for (int i = 0; i < heartImageList.Count; ++i)
        {
            if (i < (amount / 2))
            {
                heartImageList[i].sprite = FullHeartImage;
            }
            else
            {
                if ((amount - i * 2) == 1)
                {
                    heartImageList[i].sprite = HalfHeaertImage;
                }
                else
                {
                    heartImageList[i].sprite = BlankHeartImage;
                }
            }

        }
    }

    public void SetReportSprites()
    {
        string stageName = StageManager.Instance.StageName;

        for (int i = 0; i < reportImageList.Count; ++i)
        {
            if (ReportManager.IsCollected(stageName, i + 1))
                reportImageList[i].sprite = CollectedReportImage;
            else
                reportImageList[i].sprite = EmptyReportImage;
        }
    }

    private void UpdateButtonImages()
    {
        if (PlayerIsSmallForm)
        {
            TransformationButton.image.sprite = JuliaToJuliettButtonSprite;
            if (PlayerCanDoSpecialAction)
                ActionButton.image.sprite = SuperJumpButtonSprite;
            else
                ActionButton.image.sprite = JumpButtonSprite;
        }
        else
        {
            TransformationButton.image.sprite = JuliettToJuliaButtonSprite;
            if (PlayerCanDoSpecialAction)
                ActionButton.image.sprite = UppercutButtonSprite;
            else
                ActionButton.image.sprite = AttackButtonSprite;
        }
    }
}