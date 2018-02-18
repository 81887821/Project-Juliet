using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

    private void RunScenarioDirective(string directive)
    {
        Debug.LogWarningFormat("\"{0}\" is not a valid scenario directive.", directive);
    }
}
