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

        // Stirring: positive turns the spoon counter-clockwise (A / left), negative clockwise (D / right).
        // Keys give a full push; the stick gives how far it's pushed, so a pad can stir slowly on purpose.
        public static float StirAxis
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                float v = 0f;
                if (kb != null)
                {
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v += 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v -= 1f;
                }
                if (pad != null)
                {
                    float stick = pad.leftStick.x.ReadValue();
                    if (System.Math.Abs(stick) > 0.12f) v -= stick;
                    if (pad.dpad.left.isPressed) v += 1f;
                    if (pad.dpad.right.isPressed) v -= 1f;
                }
                return v < -1f ? -1f : v > 1f ? 1f : v;
            }
        }

        // the bellows: X on the keyboard, the west face button (X / Square) on a pad
        public static bool StokePressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && kb.xKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame);
            }
        }

        // starting a station's minigame: its key, or the pad's east button, which is Interact in the player's controls
        public static bool StationPressed(Key key)
        {
            var kb = Keyboard.current;
            var pad = Gamepad.current;
            return (kb != null && kb[key].wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame);
        }
    }
}
