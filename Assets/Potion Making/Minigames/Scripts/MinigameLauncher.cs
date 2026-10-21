using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Testing shortcut: 1 = knife, 2 = stir. Esc quits a minigame.
    public class MinigameLauncher : MonoBehaviour
    {
        [SerializeField] private KnifeSliceMinigame knife;
        [SerializeField] private StirTimingMinigame stir;

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (knife != null) knife.Cancel();
                if (stir != null) stir.Cancel();
            }
            bool busy = (knife != null && knife.Running) || (stir != null && stir.Running);
            if (busy) return;
            if (kb.digit1Key.wasPressedThisFrame && knife != null) knife.Begin();
            if (kb.digit2Key.wasPressedThisFrame && stir != null) stir.Begin();
        }
    }
}
