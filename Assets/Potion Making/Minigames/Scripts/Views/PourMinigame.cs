using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    public class PourMinigame : Minigame
    {
        [SerializeField] private Color potionColor = new Color(0.4f, 0.85f, 0.95f);

        public override string Title => "Bottle It";
        public override string Instructions => "Hold Space (or the mouse) to pour. Let go in the gold band!";

        private PourLogic logic;
        private RectTransform fill;
        private const float BottleHeight = 380f;

        protected override void Build()
        {
            logic = new PourLogic();
            var bottle = MinigameUI.Rect(Root, "Bottle", new Vector2(0, -10), new Vector2(230, BottleHeight), new Color(1f, 1f, 1f, 0.18f));
            MinigameUI.Rect(Root, "Neck", new Vector2(0, -10 + BottleHeight / 2 + 35), new Vector2(90, 70), new Color(1f, 1f, 1f, 0.18f));
            fill = MinigameUI.Rect(bottle, "Potion", new Vector2(0, -BottleHeight / 2), new Vector2(230, BottleHeight), potionColor);
            fill.pivot = new Vector2(0.5f, 0f);
            fill.anchoredPosition = new Vector2(0, -BottleHeight / 2);
            fill.localScale = new Vector3(1f, 0f, 1f);
            float bandMid = (logic.targetMin + logic.targetMax) / 2f;
            MinigameUI.Rect(bottle, "Target", new Vector2(0, (bandMid - 0.5f) * BottleHeight),
                new Vector2(230, (logic.targetMax - logic.targetMin) * BottleHeight), new Color(0.98f, 0.82f, 0.35f, 0.35f));
        }

        protected override void Step(float dt)
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            bool holding = (kb != null && kb.spaceKey.isPressed) || (mouse != null && mouse.leftButton.isPressed);
            logic.Tick(dt, holding);
            fill.localScale = new Vector3(1f, logic.Fill, 1f);

            if (logic.Done)
            {
                string result = logic.Spilled ? "Spilled everywhere!" : logic.InTarget ? "Perfect pour! *pop*" : "Corked.";
                Finish(logic.Score, result);
            }
        }
    }
}
