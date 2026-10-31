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

        public static bool ConfirmHeld
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.spaceKey.isPressed || kb.eKey.isPressed || kb.enterKey.isPressed))
                    || (pad != null && pad.buttonSouth.isPressed);
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

        public static bool LeftHeld
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed))
                    || (pad != null && (pad.dpad.left.isPressed || pad.leftStick.x.ReadValue() < -0.3f));
            }
        }

        public static bool RightHeld
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed))
                    || (pad != null && (pad.dpad.right.isPressed || pad.leftStick.x.ReadValue() > 0.3f));
            }
        }

        // -1, 0 or 1 from whichever of left/right is held
        public static float Horizontal => (LeftHeld ? -1f : 0f) + (RightHeld ? 1f : 0f);
    }
}
