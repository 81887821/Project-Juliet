using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text nameText;
    public Text dialogueText;

    public Animator animator;
    public float autoPagingTime;

    public static DialogueManager Instance;
    public Queue<string> sentences;
    public DialogueStatus dialogueStatus;

    float pagingTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Unused Dialogue Manager is attached : " + gameObject.name);
            GetComponent<DialogueManager>().enabled = false;
        }
    }

    private void Start()
    {
        sentences = new Queue<string>();
    }

    private void Update()
    {
        if (dialogueStatus != DialogueStatus.Close && Time.time > pagingTime)
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueStatus = DialogueStatus.Start;
        animator.SetBool("isOpen", true);

        nameText.text = dialogue.speaker;

        sentences.Clear();
        
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        pagingTime = Time.time + autoPagingTime;

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueStatus = DialogueStatus.Typing;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }

        if (sentences.Count != 0)
            dialogueStatus = DialogueStatus.Done;
        else
            dialogueStatus = DialogueStatus.End;
    }

    void EndDialogue()
    {
        animator.SetBool("isOpen", false);
        dialogueStatus = DialogueStatus.Close; 
    }

    public enum DialogueStatus
    {
        Close,
        Start,
        Typing,
        Done,
        End
    }
}
