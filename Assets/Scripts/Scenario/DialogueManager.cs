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
    public float DelayAfterSentence = 1f;
    [Header("Image")]
    public GameObject Profile;
    public Image CharacterImage;
    [Space]
    public Sprite JuliaSprite;
    public Sprite JuliettSprite;
    public Sprite RomeoSprite;
    public Sprite ResearcherSprite;
    public Sprite GuardSprite;

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

        UpdateCharacterName(character);
        UpdateProfileImage(character);
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    private void UpdateCharacterName(ScenarioScript.Character character)
    {
        string name;

        switch (character)
        {
            case ScenarioScript.Character.Julia:
            case ScenarioScript.Character.Juliett:
            case ScenarioScript.Character.JuliettCurrentForm:
                name = "Juliett";
                break;
            case ScenarioScript.Character.Romeo:
                name = "Romeo";
                break;
            case ScenarioScript.Character.Researcher:
                name = "Researcher";
                break;
            case ScenarioScript.Character.Guard:
                name = "Guard";
                break;
            default:
                Debug.LogError("Unknown character : " + character);
                return;
        }

        NameText.text = name;
    }

    private void UpdateProfileImage(ScenarioScript.Character character)
    {
        bool showProfile = true;
        Sprite characterSprite = null;

        switch (character)
        {
            case ScenarioScript.Character.None:
                showProfile = false;
                break;
            case ScenarioScript.Character.Julia:
                characterSprite = JuliaSprite;
                break;
            case ScenarioScript.Character.Juliett:
                characterSprite = JuliettSprite;
                break;
            case ScenarioScript.Character.JuliettCurrentForm:
                try
                {
                    if (PlayerData.Instance.IsSmallForm)
                        characterSprite = JuliaSprite;
                    else
                        characterSprite = JuliettSprite;
                }
                catch (Exception)
                {
                    characterSprite = JuliaSprite;
                }
                break;
            case ScenarioScript.Character.Romeo:
                characterSprite = RomeoSprite;
                break;
            case ScenarioScript.Character.Researcher:
                characterSprite = ResearcherSprite;
                break;
            case ScenarioScript.Character.Guard:
                characterSprite = GuardSprite;
                break;
            default:
                Debug.LogError("Unknown character : " + character);
                return;
        }

        Profile.SetActive(showProfile);
        if (showProfile)
            CharacterImage.sprite = characterSprite;
    }

    IEnumerator TypeSentence(string sentence)
    {
        DialogueText.text = string.Empty;

        foreach (char letter in sentence)
        {
            DialogueText.text += letter;
            yield return new WaitForSecondsRealtime(TextInterval);
        }

        yield return new WaitForSecondsRealtime(DelayAfterSentence);
        SentenceFinished();
    }
}
