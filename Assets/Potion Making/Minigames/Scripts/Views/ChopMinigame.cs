using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    public class ChopMinigame : Minigame
    {
        [SerializeField] private Color ingredientColor = new Color(0.85f, 0.45f, 0.55f);

        public override string Title => "Chop It Up";
        public override string Instructions => "Press Space (or click) when the knife lines up with the gold mark. 3 cuts!";

        private ChopLogic logic;
        private RectTransform ingredient, knife, mark;
        private TextMeshProUGUI counter;
        private const float HalfWidth = 340f;

        protected override void Build()
        {
            logic = new ChopLogic();
            MinigameUI.Rect(Root, "Board", new Vector2(0, 10), new Vector2(860, 300), MinigameUI.Wood);
            ingredient = MinigameUI.Rect(Root, "Ingredient", new Vector2(0, 10), new Vector2(HalfWidth * 2, 130), ingredientColor);
            mark = MinigameUI.Rect(Root, "Mark", new Vector2(0, 10), new Vector2(10, 190), new Color(0.98f, 0.82f, 0.35f, 0.9f));
            knife = MinigameUI.Rect(Root, "Knife", new Vector2(0, 40), new Vector2(14, 230), new Color(0.82f, 0.85f, 0.9f));
            MinigameUI.Rect(knife, "Handle", new Vector2(0, 150), new Vector2(30, 80), MinigameUI.Dark);
            counter = MinigameUI.Text(Root, "", new Vector2(0, -200), 30, MinigameUI.Ink);
        }

        private float X(float t) => Mathf.Lerp(-HalfWidth, HalfWidth, t);

        protected override void Step(float dt)
        {
            logic.Tick(dt);
            if (!logic.Done) mark.anchoredPosition = new Vector2(X(logic.CurrentTarget), 10);
            knife.anchoredPosition = new Vector2(X(logic.KnifePosition), 40);

            var kb = Keyboard.current;
            var mouse = Mouse.current;
            bool pressed = (kb != null && kb.spaceKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (pressed && !logic.Done)
            {
                bool clean = logic.Chop();
                MinigameUI.Rect(ingredient, "Cut", new Vector2(X(logic.KnifePosition), 0), new Vector2(6, 150), clean ? Color.white : MinigameUI.Bad);
            }

            counter.text = logic.Done ? "" : $"Cut {logic.CutIndex + 1}/{logic.CutsTotal}";
            if (logic.Done)
            {
                mark.gameObject.SetActive(false);
                Finish(logic.Score, logic.Hits == logic.CutsTotal ? "Perfect chop!" : $"{logic.Hits}/{logic.CutsTotal} clean cuts");
            }
        }
    }
}
