using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Report : Item
{
    [Range(1, 3)]
    public int ReportNumberInScene;

    private string stageName;

    protected override void Awake()
    {
        base.Awake();
    }

    protected void Start()
    {
        stageName = StageManager.Instance.StageName;

        if (ReportManager.IsCollected(stageName, ReportNumberInScene))
            Destroy(gameObject);
    }

    public override void OnItemGet()
    {
        ReportManager.MarkCollected(stageName, ReportNumberInScene);
        Destroy(gameObject);
    }
}
