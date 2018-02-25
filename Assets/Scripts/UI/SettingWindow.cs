using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingWindow : PopupWindow
{
    public static SettingWindow Instance
    {
        get;
        private set;
    }

    public YesNoMessageBox MessageBoxPrefab;
    public Image UseJoystickYesButtonImage;
    public Image UseJoystickNoButtonImage;

    public Sprite YesSelectedSprite;
    public Sprite YesDeselectedSprite;
    public Sprite NoSelectedSprite;
    public Sprite NoDeselectedSprite;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    protected override void Start()
    {
        base.Start();
        OnUseJoystickChanged(SettingManager.UsingScrollbar);
    }

    private void OnDestroy()
    {
        Close();
    }

    public void ExitGame()
    {
        YesNoMessageBox messageBox = Instantiate(MessageBoxPrefab, transform);
        messageBox.Title = "Exit";
        messageBox.Content = "정말로 게임을 종료하시겠습니까?";
        messageBox.OnNoButtonClick.AddListener(messageBox.Close);
        messageBox.OnYesButtonClick.AddListener(StageManager.Instance.ExitGame);
    }

    public void LoadMainMenu()
    {
        StageManager.Instance.LoadMainMenu();
    }

    public void DeleteSaveData()
    {
        YesNoMessageBox messageBox = Instantiate(MessageBoxPrefab, transform);
        messageBox.Title = "Delete Save Data";
        messageBox.Content = "모든 세이브 데이터를 삭제하고 게임을 종료합니다.\n이 작업은 취소할 수 없습니다.\n실행하겠습니까?";
        messageBox.OnNoButtonClick.AddListener(messageBox.Close);
        messageBox.OnYesButtonClick.AddListener(SaveLoadManager.DeleteSaveData);
    }

    public void OnUseJoystickChanged(bool useJoystick)
    {
        if (useJoystick)
        {
            UseJoystickYesButtonImage.sprite = YesSelectedSprite;
            UseJoystickNoButtonImage.sprite = NoDeselectedSprite;
        }
        else
        {
            UseJoystickYesButtonImage.sprite = YesDeselectedSprite;
            UseJoystickNoButtonImage.sprite = NoSelectedSprite;
        }
        SettingManager.UsingScrollbar = useJoystick;
    }
}
