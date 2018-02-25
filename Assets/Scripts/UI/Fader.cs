using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public static Fader Instance
    {
        get;
        private set;
    }

    private bool fadeOuted = false;
    private Coroutine fading;
    private Image image;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        foreach (Image image in GetComponentsInChildren<Image>())
        {
            if (image.name == "Fader image")
                this.image = image;
        }

        DontDestroyOnLoad(gameObject);

#if DEBUG
        if (image == null)
            Debug.LogError("Fade image not found.");
#endif
    }

    public void FadeOut(float duration = 1f)
    {
        if (!fadeOuted)
        {
            if (fading != null)
                StopCoroutine(fading);

            fading = StartCoroutine(Fade(duration, true));
            fadeOuted = true;
        }
    }

    public void FadeIn(float duration = 1f)
    {
        if (fadeOuted)
        {
            if (fading != null)
                StopCoroutine(fading);

            fading = StartCoroutine(Fade(duration, false));
            fadeOuted = false;
        }
    }

    private IEnumerator Fade(float duration, bool fadeOut)
    {
        float alpha = image.color.a;

        if (fadeOut)
        {
            while (alpha < 1f)
            {
                image.color = new Color(0f, 0f, 0f, alpha);
                yield return new WaitForEndOfFrame();
                alpha += Time.deltaTime / duration;
            }
            alpha = 1f;
        }
        else
        {
            while (alpha > 0f)
            {
                image.color = new Color(0f, 0f, 0f, alpha);
                yield return new WaitForEndOfFrame();
                alpha -= Time.deltaTime / duration;
            }
            alpha = 0f;
        }

        image.color = new Color(0f, 0f, 0f, alpha);
        fading = null;
    }
}