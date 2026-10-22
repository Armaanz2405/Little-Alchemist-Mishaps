using UnityEngine;
using UnityEngine.InputSystem;
using Nocturne.PotionMaking.Energy;
using Unity.VisualScripting;

public class DebugMenu : MonoBehaviour
{
    [Header("Settings Asset")]
    [SerializeField]private EnergyCosts energyCosts;
    [SerializeField]private CustomerEnergy customerEnergy;

    private bool showDebugMenu = false;
    private Rect windowRect = new Rect(20, 20, 300, 280);

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            showDebugMenu = !showDebugMenu;
        }
    }

    void OnGUI()
    {
        if (!showDebugMenu || energyCosts == null) return;

        windowRect = GUI.Window(1, windowRect, DrawDebugWindow, "Debug Menu (~ to close)");
    }

    void DrawDebugWindow(int windowID)
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
            GUILayout.Label($"<b>Energy Costs</b>");

            GUILayout.Label($"Inspect: {energyCosts.inspect}");
            energyCosts.inspect = Mathf.RoundToInt(GUILayout.HorizontalSlider(energyCosts.inspect, 0f, 3f));

            GUILayout.Label($"Refine: {energyCosts.refine}");
            energyCosts.refine = Mathf.RoundToInt(GUILayout.HorizontalSlider(energyCosts.refine, 0f, 3f));

            GUILayout.Label($"Bottle: {energyCosts.bottle}");
            energyCosts.bottle = Mathf.RoundToInt(GUILayout.HorizontalSlider(energyCosts.bottle, 0f, 3f));

            GUILayout.Label($"Talk: {energyCosts.talk}");
            energyCosts.talk = Mathf.RoundToInt(GUILayout.HorizontalSlider(energyCosts.talk, 0f, 3f));

            if (GUILayout.Button("Reset to Defaults"))
            {
                energyCosts.inspect = 1;
                energyCosts.refine = 1;
                energyCosts.bottle = 1;
                energyCosts.talk = 1;
            }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
            GUILayout.Label("<b>Customer Energy</b>");

            GUILayout.Label($"Standard Energy: {customerEnergy.standardEnergy}");
            customerEnergy.standardEnergy = Mathf.RoundToInt(GUILayout.HorizontalSlider(customerEnergy.standardEnergy, 0f, 6f));

            foreach (var npc in customerEnergy.keyNpcs)
            {
                GUILayout.Label($"{npc.npcName}: {npc.energy}");
                npc.energy = Mathf.RoundToInt(GUILayout.HorizontalSlider(npc.energy, 0f, 10f));
            }

            if (GUILayout.Button("Reset to Defaults"))
            {
                customerEnergy.standardEnergy = 5;
                foreach(var npc in customerEnergy.keyNpcs)
                    npc.energy = 6;
            }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }
}
