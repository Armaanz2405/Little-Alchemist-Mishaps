using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NightShopController : MonoBehaviour
{
    [Header("Required")]
    public GameObject shopRoot;          // drag Shop Canvas here

    [Header("The big day card (drag DayTitleText here)")]
    public DayTitleCard titleCard;

    [Header("Optional: freeze these at night (e.g. the player's movement script)")]
    public Behaviour[] disableAtNight;

    [Header("Optional: things to hide at night")]
    public GameObject[] hideAtNight;

    [Header("Optional: the shop's own open/close functions")]
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
        StartCoroutine(NightSequence(day));
    }

    IEnumerator NightSequence(int day)
    {
        var dm = DayManager.Instance;

        foreach (var g in hideAtNight)
        {
            if (g != null) g.SetActive(false);
        }
        foreach (var b in disableAtNight)
        {
            if (b != null) b.enabled = false;
        }

        bool lastDay = day >= dm.totalDays;

        // 1. Show the card for the next day
        if (titleCard != null)
        {
            string message = lastDay ? "Two weeks complete!" : "Day " + (day + 1);
            yield return StartCoroutine(titleCard.ShowRoutine(message));
        }

        // 2. After day 14 the game ends instead of opening the shop
        if (lastDay)
        {
            dm.BeginNextDay();
            yield break;
        }

        // 3. Open the shop
        shopRoot.SetActive(true);
        onNightStarted.Invoke();
    }

    void HandleDay(int day)
    {
        onDayStarted.Invoke();
        shopRoot.SetActive(false);

        foreach (var g in hideAtNight)
        {
            if (g != null) g.SetActive(true);
        }
        foreach (var b in disableAtNight)
        {
            if (b != null) b.enabled = true;
        }
    }

    void HandleGameCompleted()
    {
        shopRoot.SetActive(false);
        onGameCompleted.Invoke();
    }

    // The "Start Next Day" button calls this
    public void OnStartNextDayPressed()
    {
        DayManager.Instance.BeginNextDay();
    }
}