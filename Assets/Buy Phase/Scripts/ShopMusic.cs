using System.Collections;
using UnityEngine;

// play from start
[RequireComponent(typeof(AudioSource))]
public class ShopMusic : MonoBehaviour
{
    [SerializeField] private ShopUI shop;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float fadeTime = 0.3f;

    private AudioSource source;
    private Coroutine fade;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.Stop();
        source.loop = true;
        source.volume = 0f;
        if (shop == null)
        {
            shop = GetComponent<ShopUI>();
        }
    }

    private void OnEnable()
    {
        if (shop != null)
        {
            shop.OpenChanged += OnShopOpenChanged;
        }
    }

    private void OnDisable()
    {
        if (shop != null)
        {
            shop.OpenChanged -= OnShopOpenChanged;
        }
    }

    private void OnShopOpenChanged(bool open)
    {
        if (fade != null)
        {
            StopCoroutine(fade);
        }

        if (open)
        {
            source.Stop();
            source.Play();
            fade = StartCoroutine(FadeTo(volume, false));
        }
        else
        {
            fade = StartCoroutine(FadeTo(0f, true));
        }
    }

    private IEnumerator FadeTo(float target, bool stopWhenDone)
    {
        float start = source.volume;
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        source.volume = target;
        if (stopWhenDone)
        {
            source.Stop();
        }
        fade = null;
    }
}
