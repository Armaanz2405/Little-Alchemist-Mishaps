using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    // Keyboard and gamepad polling shared by the minigames so none of them need the mouse.
    public static class MinigameInput
    {
        public static bool ConfirmPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                var mouse = Mouse.current;
                return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                    || (pad != null && pad.buttonSouth.wasPressedThisFrame)
                    || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            }
        }

        public static bool LeftPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame))
                    || (pad != null && (pad.dpad.left.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame));
            }
        }

        public static bool RightPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame))
                    || (pad != null && (pad.dpad.right.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame));
            }
        }
    }
}
