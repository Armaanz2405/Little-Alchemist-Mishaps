using UnityEngine;
using UnityEngine.InputSystem;

public class DebugDayTester : MonoBehaviour
{
    private GUIStyle style;

    void Start()
    {
        Debug.Log("DebugDayTester started. DayManager found: " + (DayManager.Instance != null));
    }

    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        var dm = DayManager.Instance;
        if (dm == null)
        {
            return;
        }

        // N = pretend an NPC was helped
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            dm.NpcHelped();
            Debug.Log("NPC helped! " + dm.NpcsHelpedToday + " / " + dm.NpcsRequiredToday);
        }
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 28;
            style.normal.textColor = Color.yellow;
        }

        var dm = DayManager.Instance;
        string text;

        if (dm == null)
        {
            text = "DEBUG: no DayManager found in the scene!";
        }
        else
        {
            text = "DEBUG  Day " + dm.CurrentDay + "/" + dm.totalDays
                 + "   NPCs " + dm.NpcsHelpedToday + "/" + dm.NpcsRequiredToday
                 + "   " + dm.Phase
                 + "   timeScale " + Time.timeScale;
        }

        GUI.Label(new Rect(20, Screen.height - 60, 1600, 50), text, style);
    }
}