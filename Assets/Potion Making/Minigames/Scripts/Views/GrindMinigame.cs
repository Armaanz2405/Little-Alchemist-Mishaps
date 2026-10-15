using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    public class GrindMinigame : Minigame
    {
        [SerializeField] private Color powderColor = new Color(0.75f, 0.68f, 0.9f);

        public override string Title => "Grind It Down";
        public override string Instructions => "Alternate A and D (or Left / Right) to rock the pestle.";

        private GrindLogic logic;
        private RectTransform pestle, powder, timerFill, progressFill;

        protected override void Build()
        {
            logic = new GrindLogic();
            var bowl = MinigameUI.Rect(Root, "Mortar", new Vector2(0, -20), new Vector2(380, 380), new Color(0.55f, 0.55f, 0.6f), round: true);
            MinigameUI.Rect(bowl, "Inside", new Vector2(0, 20), new Vector2(320, 320), new Color(0.35f, 0.35f, 0.4f), round: true);
            powder = MinigameUI.Rect(bowl, "Powder", new Vector2(0, 20), new Vector2(300, 300), powderColor, round: true);
            powder.localScale = new Vector3(0.05f, 0.05f, 1f);
            pestle = MinigameUI.Rect(Root, "Pestle", new Vector2(0, -40), new Vector2(64, 240), MinigameUI.Wood);
            pestle.pivot = new Vector2(0.5f, 0f);
            MinigameUI.Text(Root, "Time", new Vector2(-470, -240), 22, MinigameUI.Ink, 160);
            timerFill = MinigameUI.Bar(Root, "Timer", new Vector2(40, -240), new Vector2(760, 22), MinigameUI.Dark, MinigameUI.Gold);
            MinigameUI.Text(Root, "Ground", new Vector2(-470, -276), 22, MinigameUI.Ink, 160);
            progressFill = MinigameUI.Bar(Root, "Progress", new Vector2(40, -276), new Vector2(760, 22), MinigameUI.Dark, MinigameUI.Good);
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt);
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) logic.Press(GrindLogic.Side.Left);
                if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) logic.Press(GrindLogic.Side.Right);
            }

            float tilt = logic.LastSide == GrindLogic.Side.Left ? 22f : logic.LastSide == GrindLogic.Side.Right ? -22f : 0f;
            pestle.localRotation = Quaternion.Lerp(pestle.localRotation, Quaternion.Euler(0, 0, tilt), Mathf.Clamp01(dt * 20f));
            float s = Mathf.Lerp(0.05f, 1f, logic.Progress);
            powder.localScale = new Vector3(s, s, 1f);
            timerFill.localScale = new Vector3(logic.TimeLeft / logic.timeLimit, 1f, 1f);
            progressFill.localScale = new Vector3(logic.Progress, 1f, 1f);

            if (logic.Done) Finish(logic.Score, logic.Succeeded ? "Ground to powder!" : "Time's up. A bit chunky");
        }
    }
}
