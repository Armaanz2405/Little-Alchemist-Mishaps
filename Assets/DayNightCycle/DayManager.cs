using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DayPhase { NotStarted, Day, Night, GameOver }

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Length of the game")]
    public int totalDays = 7;

    [Header("NPCs to help each day (Element 0 = Day 1)")]
    public int[] npcsPerDay = { 2, 2, 3, 3, 3, 4, 4, };
    public int npcsFallback = 6;

    [Header("Start of the game")]
    [Tooltip("Day 1 waits until the Controls popup's Start button is pressed")]
    public bool waitForStartButton = true;

    [Tooltip("If the game never gets paused, start anyway after this many seconds")]
    public float startFallbackSeconds = 3f;

    [Header("Timing")]
    [Tooltip("Seconds to wait after the last NPC before night starts")]
    public float endOfDayDelay = 1f;

    [Tooltip("Tick this to skip the Start Next Day button and go on automatically")]
    public bool autoStartNextDay = false;

    [Tooltip("Only used when Auto Start Next Day is ticked")]
    public float nightDuration = 3f;

    public int CurrentDay { get; private set; } = 1;
    public int NpcsHelpedToday { get; private set; }
    public DayPhase Phase { get; private set; } = DayPhase.NotStarted;

    public int NpcsRequiredToday
    {
        get
        {
            int i = CurrentDay - 1;
            return i < npcsPerDay.Length ? npcsPerDay[i] : npcsFallback;
        }
    }

    public float DayProgress01
    {
        get
        {
            if (NpcsRequiredToday <= 0) return 0f;
            return Mathf.Clamp01((float)NpcsHelpedToday / NpcsRequiredToday);
        }
    }

    public event Action<int> OnDayStarted;
    public event Action<int> OnNightStarted;
    public event Action<int, int> OnNpcHelped;
    public event Action OnGameCompleted;

    private bool gameBegun;
    private bool sawPause;
    private float launchTime;
    private bool endingDay;
    private bool noMoreCustomers = false;
    private Coroutine autoRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        launchTime = Time.unscaledTime;

        if (!waitForStartButton)
        {
            BeginGame();
        }
    }

    private void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame && noMoreCustomers)
        {
            StartCoroutine(EndDayRoutine());
            Debug.Log("Ending Day " + CurrentDay);
        }

        if (gameBegun || !waitForStartButton)
        {
            return;
        }

        if (Time.timeScale <= 0f)
        {
            // The Controls popup has paused the game
            sawPause = true;
        }
        else if (sawPause)
        {
            // The game was paused and now it isn't: Start was pressed
            BeginGame();
        }
        else if (Time.unscaledTime - launchTime > startFallbackSeconds)
        {
            // The game was never paused, so don't wait forever
            BeginGame();
        }
    }

    // Day 1 starts here. A button can also call this directly.
    public void BeginGame()
    {
        if (gameBegun)
        {
            return;
        }

        gameBegun = true;
        StartDay();
    }

    public void StartDay()
    {
        NpcsHelpedToday = 0;
        endingDay = false;
        Phase = DayPhase.Day;
        noMoreCustomers = false;
        OnDayStarted?.Invoke(CurrentDay);
    }

    // Call this every time an NPC has been fully helped
    public void NpcHelped()
    {
        if (Phase != DayPhase.Day || endingDay)
        {
            return;
        }

        NpcsHelpedToday++;
        OnNpcHelped?.Invoke(NpcsHelpedToday, NpcsRequiredToday);

        if (NpcsHelpedToday >= NpcsRequiredToday)
        {
            noMoreCustomers = true;
            StartCoroutine(EndDayRoutine());
        }
    }

    private IEnumerator EndDayRoutine()
    {
        endingDay = true;

        if (endOfDayDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(endOfDayDelay);
        }

        StartNight();
    }

    public void StartNight()
    {
        Phase = DayPhase.Night;
        OnNightStarted?.Invoke(CurrentDay);

        if (autoStartNextDay)
        {
            autoRoutine = StartCoroutine(AutoNextDayRoutine());
        }
    }

    private IEnumerator AutoNextDayRoutine()
    {
        if (nightDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(nightDuration);
        }

        autoRoutine = null;
        BeginNextDay();
    }

    // The Start Next Day button calls this (it only works at night)
    public void BeginNextDay()
    {
        if (Phase != DayPhase.Night)
        {
            return;
        }

        if (autoRoutine != null)
        {
            StopCoroutine(autoRoutine);
            autoRoutine = null;
        }

        if (CurrentDay >= totalDays)
        {
            Phase = DayPhase.GameOver;
            OnGameCompleted?.Invoke();
            return;
        }

        CurrentDay++;
        StartDay();
    }
}