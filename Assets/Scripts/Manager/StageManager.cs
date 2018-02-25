using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    public static string[] VisitedStages
    {
        get
        {
            return sceneList.ToArray();
        }
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

    private static List<string> sceneList = new List<string>();

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

        if (!sceneList.Contains(StageName))
        {
            sceneList.Add(StageName);
            SaveLoadManager.Save();
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded -= FadeInAfterSceneLoad;
    }

    public static void SavePlayerPrefs()
    {
        StringBuilder scenes = new StringBuilder();
        foreach (string scene in sceneList)
        {
            scenes.Append(scene);
            scenes.Append('\n');
        }
        PlayerPrefs.SetString(SaveLoadManager.SCENE_LIST_KEY, scenes.ToString());
    }

    public static void LoadPlayerPrefs()
    {
        string savedSceneList = PlayerPrefs.GetString(SaveLoadManager.SCENE_LIST_KEY, string.Empty);

        sceneList.Clear();
        foreach (string stageName in savedSceneList.Split('\n'))
        {
            if (stageName != string.Empty)
                sceneList.Add(stageName);
        }
    }

    public static bool Visited(string stageName)
    {
        return sceneList.Contains(stageName);
    }

    public void RestartCurrentStage()
    {
        SceneManager.LoadScene(StageBuildIndex);
    }

    public static void LoadStage(string stageName)
    {
        const float FADE_OUT_DURATION = 1f;
        Fader.Instance.FadeOut(FADE_OUT_DURATION);
        Instance.StartCoroutine(LoadSceneWithDelay(stageName, FADE_OUT_DURATION));
        SceneManager.sceneLoaded += FadeInAfterSceneLoad;
    }

    private static IEnumerator LoadSceneWithDelay(string stageName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(stageName);
    }

    private static void FadeInAfterSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Fader.Instance.FadeIn();
    }

    /// <summary>
    /// Stop current stage and go back to main menu.
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Scenes/Main");
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
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
