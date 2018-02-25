using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoundEffects : MonoBehaviour
{
    public static PlayerSoundEffects Instance
    {
        get;
        private set;
    }

    public AudioClip AttackSound;
    public AudioClip UppercutSound;
    public AudioClip PressSound;
    public AudioClip TransformationDeniedSound;
    public AudioClip TransformationSound;

    private SoundEffectManager soundEffectManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        soundEffectManager = SoundEffectManager.Instance;
    }

    public void PlayAttackSound()
    {
        soundEffectManager.Play(AttackSound);
    }

    public void PlayUppercutSound()
    {
        soundEffectManager.Play(UppercutSound);
    }

    public void PlayPressSound()
    {
        soundEffectManager.Play(PressSound);
    }

    public void PlayTransformationSound()
    {
        soundEffectManager.Play(TransformationSound);
    }

    public void PlayTransformationDeniedSound()
    {
        soundEffectManager.Play(TransformationDeniedSound);
    }
}