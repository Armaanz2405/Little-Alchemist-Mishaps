using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Testing shortcut: 1 = knife, 2 = stir, 3 = grind. Esc quits a minigame.
    // Concept prototypes (basic shapes, not wired to any station): 4 = rhythm chop, 5 = heat balance, 6 = skim the brew, 7 = lantern fishing.
    public class MinigameLauncher : MonoBehaviour
    {
        [SerializeField] private KnifeSliceMinigame knife;
        [SerializeField] private StirTimingMinigame stir;
        [SerializeField] private GrindMinigame grind;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            var games = GetComponents<Minigame>();
            if (kb.escapeKey.wasPressedThisFrame) foreach (var game in games) game.Cancel();
            if (games.Any(game => game.Running)) return;
            if (kb.digit1Key.wasPressedThisFrame && knife != null) knife.Begin();
            if (kb.digit2Key.wasPressedThisFrame && stir != null) stir.Begin();
            if (kb.digit3Key.wasPressedThisFrame && grind != null) grind.Begin();
            if (kb.digit4Key.wasPressedThisFrame) Concept<RhythmChopMinigame>().Begin();
            if (kb.digit5Key.wasPressedThisFrame) Concept<HeatBalanceMinigame>().Begin();
            if (kb.digit6Key.wasPressedThisFrame) Concept<CauldronFishingMinigame>().Begin();
            if (kb.digit7Key.wasPressedThisFrame) Concept<SkyFishingMinigame>().Begin();
        }

        // the concepts aren't in the scene, so they get added the first time they're asked for
        private T Concept<T>() where T : Minigame
        {
            var game = GetComponent<T>();
            return game != null ? game : gameObject.AddComponent<T>();
        }
    }
}
