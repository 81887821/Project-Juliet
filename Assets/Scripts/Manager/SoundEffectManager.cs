using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance
    {
        get;
        private set;
    }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Double instantiation : " + this);
            Destroy(this);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}
