using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Concept: fish the bad bits out of the cauldron. Hold to lift the ladle, let go to sink it, keep it on the gunk.
    public class CauldronFishingMinigame : Minigame
    {
        [SerializeField] private Color liquidColor = new Color(0.45f, 0.75f, 0.55f);
        [SerializeField] private Color gunkColor = new Color(0.32f, 0.14f, 0.38f);

        public override string Title => "Skim the Brew";
        public override string Instructions => "Hold E or Space (A on a pad) to lift the ladle, let go to sink it. Keep it on the gunk to scoop it out.";

        public CauldronFishingLogic Logic => logic;

        private CauldronFishingLogic logic;
        private RectTransform liquid, ladle, gunk, meterFill, timer, tray;
        private TMP_Text status;
        private float flash;
        private readonly List<(TMP_Text text, float life)> popups = new List<(TMP_Text, float)>();
        private const float TrackHeight = 440f;

        protected override void Build()
        {
            logic = new CauldronFishingLogic();
            popups.Clear();

            var pot = MinigameUI.Rect(Root, "Pot", new Vector2(-230, -10), new Vector2(320, 500), MinigameUI.Dark);
            liquid = MinigameUI.Rect(pot, "Liquid", new Vector2(0, 0), new Vector2(250, TrackHeight), liquidColor);
            liquid.gameObject.AddComponent<RectMask2D>();
            gunk = MinigameUI.Rect(liquid, "Gunk", Vector2.zero, new Vector2(64, 64), gunkColor, MinigameUI.Circle());
            MinigameUI.Label(gunk, "x", Vector2.zero, 44, new Color(0.6f, 0.7f, 0.4f), 100);
            ladle = MinigameUI.Rect(liquid, "Ladle", Vector2.zero, new Vector2(210, logic.ladleHalf * 2f * TrackHeight), new Color(0.55f, 0.38f, 0.24f, 0.75f));
            MinigameUI.Rect(ladle, "Handle", new Vector2(130, 0), new Vector2(60, 14), MinigameUI.Wood);

            var meter = MinigameUI.Rect(Root, "Meter", new Vector2(-30, -10), new Vector2(26, TrackHeight), MinigameUI.Dark);
            meterFill = MinigameUI.Rect(meter, "Fill", new Vector2(0, -TrackHeight / 2), new Vector2(26, TrackHeight), MinigameUI.Gold);
            meterFill.pivot = new Vector2(0.5f, 0f);
            MinigameUI.Label(meter, "scoop", new Vector2(0, -TrackHeight / 2 - 24), 18, MinigameUI.Ink, 120);

            tray = MinigameUI.Rect(Root, "Tray", new Vector2(220, 60), new Vector2(360, 200), new Color(0.3f, 0.22f, 0.16f));
            MinigameUI.Label(tray, "Scooped out", new Vector2(0, 72), 22, MinigameUI.Ink, 300);
            for (int i = 0; i < logic.gunkCount; i++)
                MinigameUI.Rect(tray, "Slot", new Vector2((i - (logic.gunkCount - 1) / 2f) * 90f, -10), new Vector2(64, 64), new Color(0.1f, 0.08f, 0.12f, 0.5f), MinigameUI.Circle());
            status = MinigameUI.Label(Root, "", new Vector2(220, -100), 28, MinigameUI.Ink, 400);

            MinigameUI.Label(Root, "Time", new Vector2(-440, -289), 20, MinigameUI.Ink, 200);
            timer = MinigameUI.Bar(Root, "Timer", new Vector2(40, -289), new Vector2(760, 12), MinigameUI.Dark, MinigameUI.Good);
            timer.localScale = Vector3.one;
        }

        protected override void Step(float dt)
        {
            if (logic.Tick(dt, MinigameInput.ConfirmHeld))
            {
                var got = MinigameUI.Rect(tray, "Got", new Vector2((logic.Scooped - 1 - (logic.gunkCount - 1) / 2f) * 90f, -10), new Vector2(64, 64), gunkColor, MinigameUI.Circle());
                MinigameUI.Label(got, "x", Vector2.zero, 44, new Color(0.6f, 0.7f, 0.4f), 100);
                var label = MinigameUI.Label(Root, "<b>Scooped!</b>", new Vector2(-230, 120), 30, MinigameUI.Good, 300);
                popups.Add((label, 0.8f));
                flash = 0.3f;
            }

            ladle.anchoredPosition = new Vector2(0, (logic.Ladle - 0.5f) * TrackHeight);
            gunk.anchoredPosition = new Vector2(Mathf.Sin(logic.Elapsed * 3f) * 40f, (logic.Gunk - 0.5f) * TrackHeight);
            meterFill.localScale = new Vector3(1f, logic.Catch, 1f);
            meterFill.GetComponent<Image>().color = logic.Overlapping ? MinigameUI.Good : MinigameUI.Gold;
            ladle.GetComponent<Image>().color = logic.Overlapping ? new Color(0.75f, 0.55f, 0.3f, 0.85f) : new Color(0.55f, 0.38f, 0.24f, 0.75f);

            flash = Mathf.Max(0f, flash - dt);
            liquid.GetComponent<Image>().color = Color.Lerp(liquidColor, Color.white, flash);
            status.text = logic.Overlapping ? "<color=#8cd973>Got it, hold steady</color>" : "Chase the gunk";
            timer.localScale = new Vector3(logic.timeLimit > 0f ? logic.TimeLeft / logic.timeLimit : 0f, 1f, 1f);
            timer.GetComponent<Image>().color = logic.TimeLeft < 4f ? MinigameUI.Bad : MinigameUI.Good;

            for (int i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.life -= dt;
                if (p.life <= 0f) { Destroy(p.text.gameObject); popups.RemoveAt(i); continue; }
                p.text.rectTransform.anchoredPosition += new Vector2(0, 50f * dt);
                p.text.alpha = Mathf.Clamp01(p.life / 0.4f);
                popups[i] = p;
            }

            if (logic.Done)
            {
                status.text = "";
                Finish(logic.Score, logic.Scooped >= logic.gunkCount ? "Clean brew!" : logic.Score >= 0.5f ? "Mostly clean" : "Still gunky...");
            }
        }
    }
}
