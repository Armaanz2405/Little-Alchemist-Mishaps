using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NightShopController : MonoBehaviour
{
    [Header("Required")]
    public GameObject shopRoot;          // drag Shop Canvas here

    [Header("Optional: a 'Night Falls' banner")]
    public CanvasGroup bannerGroup;
    public TMP_Text bannerText;
    public float bannerHold = 2f;
    public float bannerFade = 1f;

    [Header("Optional: things to hide while the shop is open")]
    public GameObject[] hideAtNight;

    [Header("Optional: call the shop's own open/close functions")]
    public UnityEvent onNightStarted;
    public UnityEvent onDayStarted;
    public UnityEvent onGameCompleted;

    void Start()
    {
        shopRoot.SetActive(false);

        var dm = DayManager.Instance;
        if (dm != null)
        {
            dm.OnNightStarted += HandleNight;
            dm.OnDayStarted += HandleDay;
            dm.OnGameCompleted += HandleGameCompleted;
        }
    }

    void OnDestroy()
    {
        var dm = DayManager.Instance;
        if (dm != null)
        {
            dm.OnNightStarted -= HandleNight;
            dm.OnDayStarted -= HandleDay;
            dm.OnGameCompleted -= HandleGameCompleted;
        }
    }

    void HandleNight(int day)
    {
        foreach (var g in hideAtNight) g.SetActive(false);
        shopRoot.SetActive(true);
        onNightStarted.Invoke();

        if (bannerGroup != null)
        {
            if (bannerText != null)
                bannerText.text = "Night Falls\nDay " + day + " complete";
            StartCoroutine(ShowBanner());
        }
    }

    void HandleDay(int day)
    {
        onDayStarted.Invoke();
        shopRoot.SetActive(false);
        foreach (var g in hideAtNight) g.SetActive(true);
    }

    void HandleGameCompleted()
    {
        shopRoot.SetActive(false);
        onGameCompleted.Invoke();
    }

    IEnumerator ShowBanner()
    {
        bannerGroup.gameObject.SetActive(true);
        bannerGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(bannerHold);

        float t = 0f;
        while (t < bannerFade)
        {
            t += Time.unscaledDeltaTime;
            bannerGroup.alpha = 1f - t / bannerFade;
            yield return null;
        }
        bannerGroup.gameObject.SetActive(false);
    }

    // The "Start Next Day" button calls this
    public void OnStartNextDayPressed()
    {
        DayManager.Instance.BeginNextDay();
    }
}