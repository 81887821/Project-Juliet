using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance
    {
        get;
        private set;
    }
    public string StageName
    {
        get;
        private set;
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
        StageName = SceneManager.GetActiveScene().name;
    }
}
