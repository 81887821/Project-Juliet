using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public delegate void GameStateChangeHandler(bool gamePaused);
    public event GameStateChangeHandler GameStateChanged = delegate {};

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
    public int StageBuildIndex
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
        Scene currentScene = SceneManager.GetActiveScene();
        StageName = currentScene.name;
        StageBuildIndex = currentScene.buildIndex;
        SaveLoadManager.Load();
    }

    public void RestartCurrentStage()
    {
        SceneManager.LoadScene(StageBuildIndex);
    }

    /// <summary>
    /// Stop current stage and go back to main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        // TODO : Load main menu
        Application.Quit();
    }

    #region Pause and resume
    private float previousTimeScale = 1f;
    private int pauseRequests = 0;

    public void Pause()
    {
        if (pauseRequests <= 0)
        {
            pauseRequests = 1;
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            GameStateChanged(true);
        }
        else
            pauseRequests++;
    }

    public void Resume()
    {
        if (pauseRequests > 0)
        {
            pauseRequests--;
            if (pauseRequests == 0)
            {
                Time.timeScale = previousTimeScale;
                GameStateChanged(false);
            }
        }
        else
            pauseRequests = 0;
    }
    #endregion
}
