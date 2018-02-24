using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Report : Item
{
    [Range(1, 3)]
    public int ReportNumberInScene = 1;

    private string stageName;

    protected override void Awake()
    {
        base.Awake();
        OnItemGet.AddListener(CollectReport);
    }

    protected void Start()
    {
        stageName = StageManager.Instance.StageName;

        if (ReportManager.IsCollected(stageName, ReportNumberInScene))
            Destroy(gameObject);
    }

    public void CollectReport()
    {
        ReportManager.MarkCollected(stageName, ReportNumberInScene);
        Destroy(gameObject);
    }
}
