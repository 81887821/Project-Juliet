using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This Script controls in-game UI like
//Specifically Movement, Action, Transformation, Health Icons, Diary Icons
//Setting Buttons should be controlled by Global UI Manager. Keep in Mind.

//Singleton Class
public class InGameUIManager : MonoBehaviour
{
    public enum Mode { None, Normal, Hidden, GameOver }

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
    public Color TransformationUnavailableColor;

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

    [Header("Child objects")]
    public GameObject HeartParentObject; //Object Which Contains heart Objects
    public GameObject ReportParentObject; //Object Which Contains diary Objects
    public GameObject GameOverMenu;
    public GameObject OptionMenu;

    public Mode UIMode
    {
        get
        {
            return mode;
        }

        set
        {
            if (mode != value)
            {
                mode = value;

                switch (mode)
                {
                    case Mode.Normal:
                        SetUIVisibility(true);
                        break;
                    case Mode.Hidden:
                        SetUIVisibility(false);
                        break;
                    case Mode.GameOver:
                        GameOverMenu.SetActive(true);
                        SetUIVisibility(false);
                        break;
                }
            }
        }
    }
    private Mode mode = Mode.Normal;

    private List<Image> heartImageList = new List<Image>();
    private List<Image> reportImageList = new List<Image>();

    private bool playerIsSmallForm = true;
    private bool playerCanDoSpecialAction = false;
    private bool playerCanTransform = true;
    private bool optionOpened = false;

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
    private bool PlayerCanTransform
    {
        get
        {
            return playerCanTransform;
        }

        set
        {
            playerCanTransform = value;
            UpdateButtonImages();
        }
    }
    public bool OptionOpened
    {
        get
        {
            return optionOpened;
        }

        set
        {
            if (optionOpened != value)
            {
                optionOpened = value;
                OptionMenu.SetActive(value);
                if (optionOpened)
                    StageManager.Instance.Pause();
                else
                    StageManager.Instance.Resume();
            }
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

        foreach (var itr in ReportParentObject.GetComponentsInChildren<Image>())
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
        player.SpecialActionAvailbilityChanged += (canDoSpecialAction) => PlayerCanDoSpecialAction = canDoSpecialAction;
        player.TransformationAvailbilityChanged += (canTransform) => PlayerCanTransform = canTransform;
        player.PlayerTransformed += (isSmallForm) => PlayerIsSmallForm = isSmallForm;
        player.PlayerHPChanged += SetCurrentHeartNum;
        player.PlayerDead += () => UIMode = Mode.GameOver;

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

        if (PlayerCanTransform)
            TransformationButton.image.color = Color.white;
        else
            TransformationButton.image.color = TransformationUnavailableColor;
    }

    private void SetUIVisibility(bool visible)
    {
        MovementScrollbar.gameObject.SetActive(visible);
        ActionButton.gameObject.SetActive(visible);
        TransformationButton.gameObject.SetActive(visible);
        HeartParentObject.SetActive(visible);
        ReportParentObject.SetActive(visible);
    }
}
