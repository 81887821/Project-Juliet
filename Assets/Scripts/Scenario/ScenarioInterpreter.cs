using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioInterpreter : MonoBehaviour
{
    public event Action ScriptFinished;

    public static ScenarioInterpreter Instance
    {
        get;
        private set;
    }

    public bool ExecutingScript
    {
        get
        {
            return currentScript != null;
        }
    }

    private DialogueManager dialogueManager;
    private Queue<ScenarioScript> scripts = new Queue<ScenarioScript>();
    private IEnumerator currentScript = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
        ScriptFinished += ExecuteNextScript;
    }

    private void Start()
    {
        dialogueManager = DialogueManager.Instance;
        dialogueManager.SentenceFinished += ExecuteNextLine;
    }

    public void EnqueueScript(ScenarioScript script)
    {
        scripts.Enqueue(script);
        if (!ExecutingScript)
        {
            ExecuteNextScript();
        }
    }

    private void ExecuteNextScript()
    {
        if (scripts.Count > 0)
        {
            currentScript = scripts.Dequeue().Dialogues.GetEnumerator();
            ExecuteNextLine();
        }
        else
        {
            currentScript = null;
            dialogueManager.IsOpen = false;
        }
    }

    private void ExecuteNextLine()
    {
        if (currentScript.MoveNext())
        {
            var script = (ScenarioScript.Dialogue) currentScript.Current;
            if (script.Speaker == ScenarioScript.Character.ScenarioDirective)
                RunScenarioDirective(script.Sentence);
            else
                dialogueManager.ShowSentence(script.Speaker, script.Sentence);
        }
        else
            ScriptFinished();
    }

    #region Scenario directives
    private void RunScenarioDirective(string directive)
    {
        try
        {
            string[] parsedDirective = ParseScenarioDirective(directive);

            switch (parsedDirective[0])
            {
                case "scene":
                    if (HandleSceneDirective(parsedDirective))
                        return;
                    break;
            }
            Debug.LogWarningFormat("\"{0}\" is not a valid scenario directive.", directive);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Exception on processing directive \"{0}\" : {1}", directive, e.Message);
        }
    }

    private Regex parser = new Regex("(?<match>[^\\s\"]+)|\"(?<match>[^\"]*)\"");

    private string[] ParseScenarioDirective(string directive)
    {
        return parser.Matches(directive).Cast<Match>().Select(m => m.Groups["match"].Value).ToArray();
    }

    private bool HandleSceneDirective(string[] parsedDirective)
    {
        switch (parsedDirective[1])
        {
            case "load":
                SceneManager.LoadScene(parsedDirective[2]);
                return true;
            default:
                return false;
        }
    }
    #endregion
}
