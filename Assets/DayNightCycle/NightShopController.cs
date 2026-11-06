using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NightShopController : MonoBehaviour
{
    [Header("The big day card (drag DayTitleText here)")]
    public DayTitleCard titleCard;

    [Header("Show 'Day 1' when Day 1 begins")]
    public bool showDayOneOnStart = true;
    public float dayOneDelay = 0.5f;

    [Header("The Start Next Day button (drag the button object here)")]
    public GameObject nextDayButton;

    [Header("Optional: a shop canvas to switch on/off (leave empty if the shop opens itself)")]
    public GameObject shopRoot;

    [Header("Optional: freeze these at night (e.g. the player's movement script)")]
    public Behaviour[] disableAtNight;

    [Header("Optional: things to hide at night")]
    public GameObject[] hideAtNight;

    [Header("Call the shop's own open/close functions here")]
    public UnityEvent onNightStarted;
    public UnityEvent onDayStarted;
    public UnityEvent onGameCompleted;

    void Start()
    {
        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }

        if (nextDayButton != null)
        {
            Button b = nextDayButton.GetComponentInChildren<Button>(true);
            if (b != null)
            {
                b.onClick.AddListener(OnStartNextDayPressed);
            }
            else
            {
                Debug.LogWarning("NightShopController: the Next Day Button object has no Button component.");
            }
            nextDayButton.SetActive(false);
        }

        var dm = DayManager.Instance;
        if (dm != null)
        {
            dm.OnNightStarted += HandleNight;
            dm.OnDayStarted += HandleDay;
            dm.OnGameCompleted += HandleGameCompleted;

            // If Day 1 already began before this script started, show its card now
            if (showDayOneOnStart && dm.Phase == DayPhase.Day && dm.CurrentDay == 1 && dm.NpcsHelpedToday == 0)
            {
                StartCoroutine(DayOneRoutine());
            }
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

    // Kept so older button entries don't break. Day 1 is now automatic.
    public void ShowDayOneCard()
    {
    }

    IEnumerator DayOneRoutine()
    {
        if (titleCard == null)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(dayOneDelay);
        yield return StartCoroutine(titleCard.ShowRoutine("Day 1"));
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

        // 2. After the last day the game ends instead of opening the shop
        if (lastDay)
        {
            dm.BeginNextDay();
            yield break;
        }

        // 3. Open the shop, then show the Start Next Day button
        if (shopRoot != null)
        {
            shopRoot.SetActive(true);
        }
        onNightStarted.Invoke();

        if (nextDayButton != null)
        {
            nextDayButton.SetActive(true);
        }
    }

    void HandleDay(int day)
    {
        if (day > 1)
        {
            onDayStarted.Invoke();
        }

        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
        if (nextDayButton != null)
        {
            nextDayButton.SetActive(false);
        }

        foreach (var g in hideAtNight)
        {
            if (g != null) g.SetActive(true);
        }
        foreach (var b in disableAtNight)
        {
            if (b != null) b.enabled = true;
        }

        if (day == 1 && showDayOneOnStart)
        {
            StartCoroutine(DayOneRoutine());
        }
    }

    void HandleGameCompleted()
    {
        if (shopRoot != null)
        {
            shopRoot.SetActive(false);
        }
        if (nextDayButton != null)
        {
            nextDayButton.SetActive(false);
        }
        onGameCompleted.Invoke();
    }

    // The Start Next Day button calls this
    public void OnStartNextDayPressed()
    {
        var dm = DayManager.Instance;
        Debug.Log("Start Next Day pressed. Phase = " + (dm != null ? dm.Phase.ToString() : "no DayManager"));

        if (dm != null)
        {
            dm.BeginNextDay();
        }
    }
}