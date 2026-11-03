using System;
using System.Collections;
using UnityEngine;

public enum DayPhase { Day, Night, GameOver }

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Length of the game")]
    public int totalDays = 14;

    [Header("NPCs to help each day (Element 0 = Day 1)")]
    public int[] npcsPerDay = { 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5, 6, 6, 6 };
    public int npcsFallback = 6;

    [Header("Timing")]
    [Tooltip("Seconds to wait after the last NPC before night starts")]
    public float endOfDayDelay = 0f;

    [Tooltip("Tick this to skip the Start Next Day button and go on automatically")]
    public bool autoStartNextDay = true;

    [Tooltip("Seconds the night lasts before the next day starts (0 = instant)")]
    public float nightDuration = 3f;

    public int CurrentDay { get; private set; } = 1;
    public int NpcsHelpedToday { get; private set; }
    public DayPhase Phase { get; private set; } = DayPhase.Day;

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

    private bool endingDay;
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
        StartDay();
    }

    public void StartDay()
    {
        NpcsHelpedToday = 0;
        endingDay = false;
        Phase = DayPhase.Day;
        OnDayStarted?.Invoke(CurrentDay);
    }

    // Call this every time an NPC has been fully helped
    public void NpcHelped()
    {
        if (Phase != DayPhase.Day || endingDay) return;

        NpcsHelpedToday++;
        OnNpcHelped?.Invoke(NpcsHelpedToday, NpcsRequiredToday);

        if (NpcsHelpedToday >= NpcsRequiredToday)
        {
            StartCoroutine(EndDayRoutine());
        }
    }

    private IEnumerator EndDayRoutine()
    {
        endingDay = true;

        if (endOfDayDelay > 0f)
        {
            yield return new WaitForSeconds(endOfDayDelay);
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
            yield return new WaitForSeconds(nightDuration);
        }

        autoRoutine = null;
        BeginNextDay();
    }

    // The Start Next Day button (and the M test key) call this
    public void BeginNextDay()
    {
        // Only works at night, so it can't skip days by accident
        if (Phase != DayPhase.Night) return;

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