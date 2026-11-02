using UnityEngine;
using UnityEngine.InputSystem;

public class DebugDayTester : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return;

        var dm = DayManager.Instance;
        if (dm == null) return;

        // N = pretend an NPC was helped
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            dm.NpcHelped();
            Debug.Log("NPC helped! " + dm.NpcsHelpedToday + " / " + dm.NpcsRequiredToday);
        }

        // M = start the next day (only works at night)
        if (Keyboard.current.mKey.wasPressedThisFrame && dm.Phase == DayPhase.Night)
        {
            dm.BeginNextDay();
            Debug.Log("Now on day " + dm.CurrentDay);
        }
    }
}