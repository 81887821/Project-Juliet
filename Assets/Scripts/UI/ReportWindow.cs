using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReportWindow : PopupWindow
{
    public static ReportWindow Instance
    {
        get;
        private set;
    }

    public RectTransform ReportListViewContent;
    public RectTransform ReportListStageNameTextPrefab;
    public RectTransform ReportListReportButtonPrefab;
    public Text ReportContentViewContent;
    
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
        ListInitialize();
        CloseContent();

        ReportManager.ReportCollected += ListInitialize;
    }

    private void OnDestroy()
    {
        ReportManager.ReportCollected -= ListInitialize;
        Close();
    }

    private void ListInitialize()
    {
        Dictionary<string, int> reportCollection = ReportManager.GetCollection();
        float totalListHeight = 0f;

        foreach (var stage in reportCollection)
        {
            RectTransform stageNameText = Instantiate(ReportListStageNameTextPrefab, ReportListViewContent);
            stageNameText.GetComponent<Text>().text = stage.Key;
            stageNameText.anchoredPosition = new Vector3(0f, -totalListHeight);
            totalListHeight += stageNameText.sizeDelta.y;

            for (int i = 0; i < sizeof(int) * 8; i++)
            {
                if (((stage.Value >> i) & 1) == 1)
                {
                    int reportNumber = i;
                    RectTransform reportButton = Instantiate(ReportListReportButtonPrefab, ReportListViewContent);
                    reportButton.GetComponentInChildren<Text>().text = i.ToString();
                    // Since i changes, do not use i directly on lambda function.
                    reportButton.GetComponent<Button>().onClick.AddListener(() => OpenContent(stage.Key, reportNumber));
                    reportButton.anchoredPosition = new Vector3(0f, -totalListHeight);
                    totalListHeight += reportButton.sizeDelta.y;
                }
            }
        }

        ReportListViewContent.sizeDelta = new Vector2(ReportListViewContent.sizeDelta.x, totalListHeight);
    }

    private void OpenContent(string stage, int reportNumber)
    {
        ReportContentViewContent.text = string.Format("Stage : {0}\nReport number : {1}\n\n{2}", stage, reportNumber, ReportManager.GetReportContent(stage, reportNumber));
    }

    private void CloseContent()
    {
        ReportContentViewContent.text = "Closed.";
    }
}
