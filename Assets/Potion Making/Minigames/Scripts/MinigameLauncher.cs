using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Testing shortcut: 1 = knife, 2 = stir, 3 = grind. Esc quits a minigame.
    public class MinigameLauncher : MonoBehaviour
    {
        [SerializeField] private KnifeSliceMinigame knife;
        [SerializeField] private StirTimingMinigame stir;
        [SerializeField] private GrindMinigame grind;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (knife != null) knife.Cancel();
                if (stir != null) stir.Cancel();
                if (grind != null) grind.Cancel();
            }
            bool busy = (knife != null && knife.Running) || (stir != null && stir.Running) || (grind != null && grind.Running);
            if (busy) return;
            if (kb.digit1Key.wasPressedThisFrame && knife != null) knife.Begin();
            if (kb.digit2Key.wasPressedThisFrame && stir != null) stir.Begin();
            if (kb.digit3Key.wasPressedThisFrame && grind != null) grind.Begin();
        }
    }
}
