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
    private ResourceRequest resourceLoading;

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
            if (instance.resourceLoading == null)
            {
                if (restartCurrentClip)
                    instance.audioSource.Play();
                else if (!instance.audioSource.isPlaying)
                    instance.audioSource.Play();
            }
        }
        else
        {
            if (instance.resourceLoading != null)
            {
                instance.resourceLoading.completed -= OnAudioClipLoaded;
                instance.resourceLoading.completed += UnloadImmediately;
                Debug.LogWarning("Requested to play another audio clip before previous one is loaded.");
            }

            if (instance.currentAudioClip != null)
                Resources.UnloadAsset(instance.currentAudioClip);
            instance.currentClipName = audioClipName;
            instance.resourceLoading = Resources.LoadAsync<AudioClip>(audioClipName);
            instance.resourceLoading.completed += OnAudioClipLoaded;
        }
    }

    private static void OnAudioClipLoaded(AsyncOperation resourceRequest)
    {
        MusicManager instance = Instance;
        instance.currentAudioClip = (resourceRequest as ResourceRequest).asset as AudioClip;
        if (instance.currentAudioClip == null)
            throw new Exception("Cannot load audio clip : " + instance.currentClipName);
        instance.audioSource.clip = instance.currentAudioClip;
        instance.audioSource.Play();
        instance.resourceLoading = null;
    }

    private static void UnloadImmediately(AsyncOperation resourceRequest)
    {
        Resources.UnloadAsset((resourceRequest as ResourceRequest).asset);
    }
}
