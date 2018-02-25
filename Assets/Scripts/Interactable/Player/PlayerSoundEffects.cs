using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
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

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAttackSound()
    {
        audioSource.PlayOneShot(AttackSound);
    }

    public void PlayUppercutSound()
    {
        audioSource.PlayOneShot(UppercutSound);
    }

    public void PlayPressSound()
    {
        audioSource.PlayOneShot(PressSound);
    }

    public void PlayTransformationSound()
    {
        audioSource.PlayOneShot(TransformationSound);
    }

    public void PlayTransformationDeniedSound()
    {
        audioSource.PlayOneShot(TransformationDeniedSound);
    }
}