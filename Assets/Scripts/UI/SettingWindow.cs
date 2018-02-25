using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingWindow : PopupWindow
{
    public static SettingWindow Instance
    {
        get;
        private set;
    }

    public MessageBox MessageBoxPrefab;

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

    private void OnDestroy()
    {
        Close();
    }

    public void ExitGame()
    {
        MessageBox messageBox = Instantiate(MessageBoxPrefab, transform);
        messageBox.Title = "Exit";
        messageBox.Content = "정말로 게임을 종료하시겠습니까?";
        messageBox.LeftButton = "No";
        messageBox.OnLeftButtonClick.AddListener(messageBox.Close);
        messageBox.RightButton = "Yes";
        messageBox.OnRightButtonClick.AddListener(StageManager.Instance.ExitGame);
    }

    public void LoadMainMenu()
    {
        StageManager.Instance.LoadMainMenu();
    }

    public void DeleteSaveData()
    {
        MessageBox messageBox = Instantiate(MessageBoxPrefab, transform);
        messageBox.Title = "Delete Save Data";
        messageBox.Content = "모든 세이브 데이터를 삭제하고 게임을 종료합니다.\n이 작업은 취소할 수 없습니다.\n실행하겠습니까?";
        messageBox.LeftButton = "No";
        messageBox.OnLeftButtonClick.AddListener(messageBox.Close);
        messageBox.RightButton = "Yes";
        messageBox.OnRightButtonClick.AddListener(SaveLoadManager.DeleteSaveData);
    }
}
