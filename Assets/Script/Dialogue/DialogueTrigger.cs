using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
	public Dialogue dialogue;
	public TriggerType triggerType;
	public DialogueTrigger prevTrigger;
	public bool disposable = true;
	bool isActivated;

	void Start()
	{
		isActivated = false;
		if (triggerType == TriggerType.GameStart)
			Invoke("TriggerDialogue", .3f);
    }

	void Update()
	{
		if (triggerType == TriggerType.Continued && !isActivated)
		{
			ContinuedDialogue();
			//gameObject.SetActive(false);
		}
	}

	public void ContinuedDialogue()
	{
		if(prevTrigger == null)
		{
			Debug.LogError("No Prev Dialogue : " + gameObject.name);
			return;
        }

		if(prevTrigger.isActivated && DialogueManager.Instance.dialogueStatus == DialogueManager.DialogueStatus.Close)
			Invoke("TriggerDialogue", .2f);
	}

	public void TriggerDialogue()
	{
		if (!isActivated || !disposable)
		{
			isActivated = true;
			DialogueManager.Instance.StartDialogue(dialogue);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (triggerType == TriggerType.Located)
		{
			TriggerDialogue();
		}
	}
}

public enum TriggerType
{
	Located,
	Destroyed,
	GameStart,
	GameEnd,
	Continued,
	Custom
}