using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class DialogueManager : MonoBehaviour
{
    public event Action SentenceFinished;

    public static DialogueManager Instance
    {
        get;
        private set;
    }

    [Header("Text")]
    public Text NameText;
    public Text DialogueText;
    public float TextInterval = 0.1f;
    [Header("Image")]
    public Image CharacterImage;

    public bool IsOpen
    {
        get
        {
            return isOpen;
        }
        set
        {
            if (isOpen != value)
            {
                isOpen = value;
                animator.SetBool("isOpen", value);
            }
        }
    }

    private Animator animator;
    private bool isOpen = false;
    private WaitForSeconds textShowInterval;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
        animator = GetComponent<Animator>();
    }

    public void ShowSentence(ScenarioScript.Character character, string sentence)
    {
        if (!IsOpen)
            IsOpen = true;

        // TODO : Show character.
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        DialogueText.text = string.Empty;
        textShowInterval = new WaitForSeconds(TextInterval);

        foreach (char letter in sentence)
        {
            DialogueText.text += letter;
            yield return textShowInterval;
        }

        SentenceFinished();
    }
}
