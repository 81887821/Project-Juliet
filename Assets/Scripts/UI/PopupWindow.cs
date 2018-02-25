using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupWindow : MonoBehaviour
{
    public AudioClip OpeningSound;
    public AudioClip ClosingSound;

    protected bool showing = false;

    protected virtual void Start()
    {
        gameObject.SetActive(false);
    }
    
    public virtual void Open()
    {
        if (!showing)
        {
            gameObject.SetActive(true);
            StageManager.Instance.Pause();
            showing = true;
            if (OpeningSound != null)
                SoundEffectManager.Instance.Play(OpeningSound);
        }
    }

    public virtual void Close()
    {
        if (showing)
        {
            gameObject.SetActive(false);
            StageManager.Instance.Resume();
            showing = false;
            if (ClosingSound != null)
                SoundEffectManager.Instance.Play(ClosingSound);
        }
    }
}
