using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Nocturne.PotionMaking.Minigames
{
    // Keyboard and gamepad polling shared by the minigames so none of them need the mouse.
    public static class MinigameInput
    {
        public enum Hint { Confirm, Cancel, Edit, Left, Right }

        private static bool padActive, watching;

        // whichever of keyboard / mouse or gamepad was pressed last, so the key hints can show the right buttons
        public static bool PadActive
        {
            get
            {
                if (!watching)
                {
                    watching = true;
                    InputSystem.onAnyButtonPress.Call(control => padActive = control.device is Gamepad);
                }
                return padActive;
            }
        }

        // what to print on a key cap for an action, on the device the player is using
        public static string Label(Hint hint)
        {
            bool pad = PadActive;
            switch (hint)
            {
                case Hint.Confirm: return pad ? "A" : "Space";
                case Hint.Cancel: return pad ? "B" : "Esc";
                case Hint.Edit: return pad ? "Y" : "Q";
                case Hint.Left: return pad ? "LB" : "A";
                default: return pad ? "RB" : "D";
            }
        }

        // confirm without the mouse, for screens where a click has to land on a button to count
        public static bool ConfirmKeyPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                    || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            }
        }

        public static bool Clicked(out Vector2 position)
        {
            var mouse = Mouse.current;
            position = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

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

        // leaving a minigame without playing it out
        public static bool CancelPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && kb.escapeKey.wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame);
            }
        }

        // the mortar's edit mode
        public static bool EditPressed
        {
            get
            {
                var kb = Keyboard.current;
                var pad = Gamepad.current;
                return (kb != null && kb.qKey.wasPressedThisFrame) || (pad != null && pad.buttonNorth.wasPressedThisFrame);
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
