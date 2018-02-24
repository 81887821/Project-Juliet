using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance
    {
        get;
        private set;
    }

    private AudioSource audioSource;
    private AudioClip currentAudioClip;
    private string currentClipName;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public static bool Loop
    {
        get
        {
            return Instance.audioSource.loop;
        }

        set
        {
            Instance.audioSource.loop = value;
        }
    }

    /// <summary>
    /// Load music from resources and play it.
    /// </summary>
    /// <param name="audioClipName">Name of audio clip resource.</param>
    /// <param name="restartCurrentClip">This flag has no effect if previously playing audio clip is not same with requested audio clip.
    /// If true, restart audio clip from begin when requested audio clip is currently playing one.
    /// If false, do nothing and keep playing previous audio clip.</param>
    public static void Play(string audioClipName, bool restartCurrentClip = false)
    {
        MusicManager instance = Instance;
        if (instance.currentClipName == audioClipName)
        {
            if (restartCurrentClip)
                instance.audioSource.Play();
            else if (!instance.audioSource.isPlaying)
                instance.audioSource.Play();
        }
        else
        {
            if (instance.currentAudioClip != null)
                Resources.UnloadAsset(instance.currentAudioClip);
            instance.currentClipName = audioClipName;
            instance.currentAudioClip = Resources.Load<AudioClip>(audioClipName);
            if (instance.currentAudioClip == null)
                throw new Exception("Cannot load audio clip : " + audioClipName);
            instance.audioSource.clip = instance.currentAudioClip;
            instance.audioSource.Play();
        }
    }
}
