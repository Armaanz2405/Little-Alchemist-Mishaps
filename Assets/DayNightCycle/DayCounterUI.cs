using TMPro;
using UnityEngine;

public class DayCounterUI : MonoBehaviour
{
    public TMP_Text label;

    void Start()
    {
        var dm = DayManager.Instance;
        if (dm == null) return;

        dm.OnDayStarted += OnDay;
        dm.OnNpcHelped += OnHelped;
        dm.OnNightStarted += OnNight;
        dm.OnGameCompleted += OnComplete;

        Refresh();
    }

    void OnDestroy()
    {
        var dm = DayManager.Instance;
        if (dm == null) return;

        dm.OnDayStarted -= OnDay;
        dm.OnNpcHelped -= OnHelped;
        dm.OnNightStarted -= OnNight;
        dm.OnGameCompleted -= OnComplete;
    }

    void OnDay(int day) { Refresh(); }
    void OnHelped(int helped, int required) { Refresh(); }
    void OnNight(int day) { label.text = "Night " + day + " of " + DayManager.Instance.totalDays; }
    void OnComplete() { label.text = "Two weeks complete!"; }

    void Refresh()
    {
        var dm = DayManager.Instance;
        label.text = "Day " + dm.CurrentDay + " of " + dm.totalDays
                   + "   NPCs " + dm.NpcsHelpedToday + "/" + dm.NpcsRequiredToday;
    }
}