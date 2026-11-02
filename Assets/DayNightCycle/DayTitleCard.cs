using System.Collections;
using TMPro;
using UnityEngine;

public class DayTitleCard : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text label;

    public bool showOnFirstDay = false;

    public float fadeIn = 0.6f;
    public float hold = 1.5f;
    public float fadeOut = 0.8f;

    private Coroutine running;

    void Awake()
    {
        if (group == null)
        {
            group = GetComponent<CanvasGroup>();
        }
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>(true);
        }

        if (label == null)
        {
            Debug.LogError("DayTitleCard: put this script on the DayTitleText object.");
            enabled = false;
            return;
        }

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    void Start()
    {
        var dm = DayManager.Instance;
        if (dm == null)
        {
            return;
        }

        dm.OnDayStarted += ShowDay;
        dm.OnGameCompleted += ShowComplete;
    }

    void OnDestroy()
    {
        var dm = DayManager.Instance;
        if (dm == null)
        {
            return;
        }

        dm.OnDayStarted -= ShowDay;
        dm.OnGameCompleted -= ShowComplete;
    }

    void ShowDay(int day)
    {
        if (day == 1)
        {
            if (!showOnFirstDay)
            {
                return;
            }
        }
        Play("Day " + day);
    }

    void ShowComplete()
    {
        Play("Two weeks complete!");
    }

    void Play(string message)
    {
        if (running != null)
        {
            StopCoroutine(running);
        }
        label.text = message;
        running = StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        Vector3 big = Vector3.one;
        Vector3 small = Vector3.one * 0.85f;

        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeIn);
            group.alpha = p;
            label.rectTransform.localScale = Vector3.Lerp(small, big, p);
            yield return null;
        }
        group.alpha = 1f;
        label.rectTransform.localScale = big;

        yield return new WaitForSecondsRealtime(hold);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }
        group.alpha = 0f;
    }
}