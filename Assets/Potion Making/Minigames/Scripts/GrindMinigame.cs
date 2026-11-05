using TMPro;
using UnityEngine;

namespace Nocturne.PotionMaking.Minigames
{
    // Mortar and pestle: alternate left and right to grind the ingredients down before the timer runs out.
    public class GrindMinigame : Minigame
    {
        [SerializeField] private Color pasteColor = new Color(0.6f, 0.5f, 0.35f);

        public override string Title => "Grind It Down";
        public override string Instructions => "Alternate A and D (or left and right) to grind before the time runs out.";

        public void SetPasteColor(Color color) => pasteColor = color;

        private GrindLogic logic;
        private RectTransform pestle, paste, progress, timer;
        private TMP_Text status;
        private float flash;

        protected override void Build()
        {
            logic = new GrindLogic();
            var bowl = MinigameUI.Rect(Root, "Bowl", new Vector2(0, 20), new Vector2(380, 380), MinigameUI.Dark, MinigameUI.Circle());
            paste = MinigameUI.Rect(bowl, "Paste", new Vector2(0, -40), new Vector2(60, 60), pasteColor, MinigameUI.Circle());
            pestle = MinigameUI.Rect(bowl, "Pestle", new Vector2(0, 50), new Vector2(46, 230), MinigameUI.Wood, MinigameUI.Circle());
            pestle.pivot = new Vector2(0.5f, 0.1f);
            pestle.anchoredPosition = new Vector2(0, -40);
            status = MinigameUI.Label(Root, "", new Vector2(0, -200), 28, MinigameUI.Ink);
            MinigameUI.Label(Root, "Ground", new Vector2(-440, -235), 20, MinigameUI.Ink, 200);
            progress = MinigameUI.Bar(Root, "Progress", new Vector2(40, -235), new Vector2(760, 20), MinigameUI.Dark, MinigameUI.Gold);
            MinigameUI.Label(Root, "Time", new Vector2(-440, -262), 20, MinigameUI.Ink, 200);
            timer = MinigameUI.Bar(Root, "Timer", new Vector2(40, -262), new Vector2(760, 12), MinigameUI.Dark, MinigameUI.Good);
            timer.localScale = Vector3.one;
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt);
            if (!logic.Done)
            {
                if (MinigameInput.LeftPressed) Press(GrindLogic.Side.Left);
                if (MinigameInput.RightPressed) Press(GrindLogic.Side.Right);
            }

            float tilt = logic.LastSide == GrindLogic.Side.Left ? 28f : logic.LastSide == GrindLogic.Side.Right ? -28f : 0f;
            pestle.localRotation = Quaternion.Lerp(pestle.localRotation, Quaternion.Euler(0, 0, tilt), dt * 18f);
            flash = Mathf.Max(0f, flash - dt);
            float size = Mathf.Lerp(60f, 300f, logic.Progress) * (1f + flash * 0.4f);
            paste.sizeDelta = new Vector2(size, size * 0.55f);
            progress.localScale = new Vector3(logic.Progress, 1f, 1f);
            timer.localScale = new Vector3(logic.timeLimit > 0f ? logic.TimeLeft / logic.timeLimit : 0f, 1f, 1f);
            timer.GetComponent<UnityEngine.UI.Image>().color = logic.TimeLeft < 2f ? MinigameUI.Bad : MinigameUI.Good;

            if (logic.Done)
                Finish(logic.Score, logic.Succeeded ? "Finely ground!" : logic.Score >= 0.5f ? "Coarse, but it'll do" : "Barely ground...");
        }

        private void Press(GrindLogic.Side side)
        {
            if (logic.Press(side))
            {
                status.text = "<color=#8cd973>Grind!</color>";
                flash = 0.2f;
            }
            else status.text = "<color=#f2665c>Alternate sides!</color>";
        }
    }
}
