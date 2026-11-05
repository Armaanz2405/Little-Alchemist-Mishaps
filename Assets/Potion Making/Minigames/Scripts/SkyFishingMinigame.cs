using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Concept: sky fishing. Move the lantern, hold the light to draw bugs in, catch them for the Moth shopkeeper.
    public class SkyFishingMinigame : Minigame
    {
        [SerializeField] private Color bugColor = new Color(0.78f, 0.95f, 0.45f);

        public override string Title => "Lantern Fishing";
        public override string Instructions => "A/D (or the stick) move the lantern. Hold E or Space (A on a pad) to shine the light and draw the bugs in.";

        public SkyFishingLogic Logic => logic;

        private SkyFishingLogic logic;
        private RectTransform sky, lantern, glow;
        private TMP_Text counter;
        private readonly Dictionary<SkyFishingLogic.Bug, RectTransform> views = new Dictionary<SkyFishingLogic.Bug, RectTransform>();
        private readonly List<(TMP_Text text, float life)> popups = new List<(TMP_Text, float)>();

        protected override void Build()
        {
            logic = new SkyFishingLogic();
            views.Clear();
            popups.Clear();

            sky = MinigameUI.Rect(Root, "Sky", new Vector2(0, 0), new Vector2(1000, 480), new Color(0.08f, 0.07f, 0.18f));
            sky.gameObject.AddComponent<RectMask2D>();
            for (int i = 0; i < 28; i++)
                MinigameUI.Rect(sky, "Star", new Vector2(Random.Range(-480f, 480f), Random.Range(-60f, 230f)), Vector2.one * Random.Range(3f, 7f), new Color(1f, 1f, 1f, 0.55f), MinigameUI.Circle());
            MinigameUI.Rect(sky, "Moon", new Vector2(380, 160), new Vector2(90, 90), new Color(0.95f, 0.93f, 0.8f), MinigameUI.Circle());
            MinigameUI.Rect(sky, "Ground", new Vector2(0, -215), new Vector2(1000, 50), new Color(0.12f, 0.1f, 0.14f));

            glow = MinigameUI.Rect(sky, "Glow", Vector2.zero, Vector2.one * logic.lightRadius * 2f, new Color(1f, 0.85f, 0.4f, 0.16f), MinigameUI.Circle());
            lantern = MinigameUI.Rect(sky, "Lantern", Vector2.zero, new Vector2(36, 54), MinigameUI.Wood);
            MinigameUI.Rect(lantern, "Glass", new Vector2(0, 2), new Vector2(24, 30), MinigameUI.Gold);
            MinigameUI.Rect(lantern, "Jar", new Vector2(0, 46), new Vector2(44, 26), new Color(0.8f, 0.9f, 1f, 0.35f));

            counter = MinigameUI.Label(Root, "", new Vector2(0, -262), 24, MinigameUI.Ink);
        }

        protected override void Step(float dt)
        {
            int caught = logic.Tick(dt, MinigameInput.Horizontal, MinigameInput.ConfirmHeld);

            lantern.anchoredPosition = logic.LanternPos;
            glow.anchoredPosition = logic.LanternPos + new Vector2(0, 30f);
            glow.gameObject.SetActive(logic.Lit);
            glow.GetComponent<Image>().color = new Color(1f, 0.85f, 0.4f, 0.14f + Mathf.Sin(logic.Elapsed * 6f) * 0.03f);

            foreach (var bug in logic.bugs)
            {
                if (!views.TryGetValue(bug, out var rt))
                {
                    rt = MinigameUI.Rect(sky, "Bug", bug.pos, new Vector2(22, 22), bugColor, MinigameUI.Circle());
                    MinigameUI.Rect(rt, "Wing", new Vector2(-10, 6), new Vector2(14, 10), new Color(1f, 1f, 1f, 0.5f), MinigameUI.Circle());
                    MinigameUI.Rect(rt, "Wing", new Vector2(10, 6), new Vector2(14, 10), new Color(1f, 1f, 1f, 0.5f), MinigameUI.Circle());
                    views[bug] = rt;
                }
                if (bug.caught && rt.gameObject.activeSelf) rt.gameObject.SetActive(false);
                rt.anchoredPosition = bug.pos;
                rt.localScale = Vector3.one * (1f + Mathf.Sin(logic.Elapsed * 9f + bug.phase) * 0.12f);
            }
            if (caught > 0)
            {
                var label = MinigameUI.Label(sky, $"<b>+{caught}</b>", logic.LanternPos + new Vector2(0, 90), 28, MinigameUI.Good, 100);
                popups.Add((label, 0.8f));
            }
            for (int i = popups.Count - 1; i >= 0; i--)
            {
                var p = popups[i];
                p.life -= dt;
                if (p.life <= 0f) { Destroy(p.text.gameObject); popups.RemoveAt(i); continue; }
                p.text.rectTransform.anchoredPosition += new Vector2(0, 50f * dt);
                p.text.alpha = Mathf.Clamp01(p.life / 0.4f);
                popups[i] = p;
            }

            counter.text = $"Caught {logic.Caught}/{logic.total}      {Mathf.CeilToInt(logic.TimeLeft)}s";

            if (logic.Done)
            {
                counter.text = "";
                Finish(logic.Score, logic.Caught >= logic.total ? "Jar's full!" : logic.Score >= 0.5f ? "Good haul" : "They got away...");
            }
        }
    }
}
