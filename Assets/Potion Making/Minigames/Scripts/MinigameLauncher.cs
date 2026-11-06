using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Testing shortcut on the digit keys. Esc quits a running minigame.
    // 1 knife swipe, 2 stir ring, 3 grind, 4 chop to the beat, 5 brew (Aliyah's cauldron), 6 catch flameflies.
    public class MinigameLauncher : MonoBehaviour
    {
        [SerializeField] private KnifeSliceMinigame knife;
        [SerializeField] private StirTimingMinigame stir;
        [SerializeField] private GrindMinigame grind;
        [SerializeField] private RhythmChopMinigame rhythm;
        [SerializeField] private CauldronBrewMinigame brew;
        [SerializeField] private SkyFishingMinigame flameflies;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            var games = GetComponents<Minigame>();
            if (kb.escapeKey.wasPressedThisFrame) foreach (var game in games) game.Cancel();
            if (games.Any(game => game.Running)) return;
            if (kb.digit1Key.wasPressedThisFrame) Get(ref knife).Begin();
            if (kb.digit2Key.wasPressedThisFrame) Get(ref stir).Begin();
            if (kb.digit3Key.wasPressedThisFrame) Get(ref grind).Begin();
            if (kb.digit4Key.wasPressedThisFrame) Get(ref rhythm).Begin();
            if (kb.digit5Key.wasPressedThisFrame) Get(ref brew).Begin();
            if (kb.digit6Key.wasPressedThisFrame) Get(ref flameflies).Begin();
        }

        // games that aren't on this object yet get added the first time they're asked for
        private T Get<T>(ref T field) where T : Minigame
        {
            if (field != null) return field;
            field = GetComponent<T>();
            if (field == null) field = gameObject.AddComponent<T>();
            return field;
        }
    }
}
