using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    public class StirMinigame : Minigame
    {
        [SerializeField] private Color liquidColor = new Color(0.45f, 0.75f, 0.55f);

        public override string Title => "Stir the Cauldron";
        public override string Instructions => "Hold the mouse button and stir in circles. Slow and steady... until it bubbles!";

        private StirLogic logic;
        private RectTransform pot, liquid, spoon, progressFill, overflowFill;
        private TextMeshProUGUI status;
        private float lastAngle;
        private bool hadAngle;
        private const float SpoonRadius = 140f;

        protected override void Build()
        {
            logic = new StirLogic();
            hadAngle = false;
            pot = MinigameUI.Rect(Root, "Pot", new Vector2(0, 20), new Vector2(400, 400), MinigameUI.Dark, round: true);
            liquid = MinigameUI.Rect(pot, "Liquid", Vector2.zero, new Vector2(350, 350), liquidColor, round: true);
            spoon = MinigameUI.Rect(pot, "Spoon", new Vector2(SpoonRadius, 0), new Vector2(56, 56), MinigameUI.Wood, round: true);
            status = MinigameUI.Text(Root, "", new Vector2(0, -200), 30, MinigameUI.Ink);
            MinigameUI.Text(Root, "Stirred", new Vector2(-470, -240), 22, MinigameUI.Ink, 160);
            progressFill = MinigameUI.Bar(Root, "Progress", new Vector2(40, -240), new Vector2(760, 22), MinigameUI.Dark, MinigameUI.Good);
            MinigameUI.Text(Root, "Overflow", new Vector2(-470, -276), 22, MinigameUI.Ink, 160);
            overflowFill = MinigameUI.Bar(Root, "Overflow", new Vector2(40, -276), new Vector2(760, 22), MinigameUI.Dark, MinigameUI.Bad);
        }

        protected override void Step(float dt)
        {
            float delta = 0f;
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                Vector2 center = RectTransformUtility.WorldToScreenPoint(null, pot.position);
                Vector2 dir = mouse.position.ReadValue() - center;
                if (dir.sqrMagnitude > 25f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    if (hadAngle) delta = Mathf.DeltaAngle(lastAngle, angle) * Mathf.Deg2Rad;
                    lastAngle = angle;
                    hadAngle = true;
                    spoon.anchoredPosition = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * SpoonRadius;
                }
            }
            else hadAngle = false;

            logic.Tick(dt, delta);

            bool bubbling = logic.Bubbling;
            float pulse = bubbling ? 1f + 0.04f * Mathf.Sin(Time.time * 18f) : 1f;
            liquid.localScale = new Vector3(pulse, pulse, 1f);
            liquid.GetComponent<UnityEngine.UI.Image>().color = bubbling ? Color.Lerp(liquidColor, Color.white, 0.35f) : liquidColor;
            progressFill.localScale = new Vector3(logic.Progress, 1f, 1f);
            overflowFill.localScale = new Vector3(logic.Overflow, 1f, 1f);
            status.text = bubbling ? "<color=#ffb15c><b>It's bubbling! Stir FAST!</b></color>"
                : logic.Speed > logic.calmMaxSpeed ? "Too fast, it's splashing!" : "Nice and slow...";

            if (logic.Done) Finish(logic.Score, logic.Succeeded ? "Nicely stirred!" : "It boiled over!");
        }
    }
}
