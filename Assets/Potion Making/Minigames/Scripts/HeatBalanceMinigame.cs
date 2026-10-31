using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Concept: cauldron heat balance. Hold to stir (cools), let go to heat. Keep the needle in the drifting green band.
    public class HeatBalanceMinigame : Minigame
    {
        [SerializeField] private Color coldColor = new Color(0.35f, 0.5f, 0.9f);
        [SerializeField] private Color rightColor = new Color(0.45f, 0.75f, 0.55f);
        [SerializeField] private Color hotColor = new Color(0.9f, 0.3f, 0.2f);

        public override string Title => "Mind the Heat";
        public override string Instructions => "Hold E or Space (A on a pad) to stir and cool the brew, let go and it heats up. Keep the needle in the green.";

        public HeatBalanceLogic Logic => logic;

        private HeatBalanceLogic logic;
        private RectTransform gauge, band, needle, pot, spoon, flame, progress, timer;
        private Image liquid;
        private TMP_Text status;
        private float spoonAngle;
        private readonly List<(RectTransform rt, float life)> bubbles = new List<(RectTransform, float)>();
        private const float GaugeHeight = 420f;

        protected override void Build()
        {
            logic = new HeatBalanceLogic();
            bubbles.Clear();

            gauge = MinigameUI.Rect(Root, "Gauge", new Vector2(-330, 0), new Vector2(70, GaugeHeight), MinigameUI.Dark);
            band = MinigameUI.Rect(gauge, "Band", Vector2.zero, new Vector2(70, logic.bandHalf * 2f * GaugeHeight), new Color(0.55f, 0.85f, 0.45f, 0.85f));
            needle = MinigameUI.Rect(gauge, "Needle", Vector2.zero, new Vector2(120, 10), MinigameUI.Gold);
            MinigameUI.Label(gauge, "hot", new Vector2(0, GaugeHeight / 2 + 24), 20, MinigameUI.Bad, 100);
            MinigameUI.Label(gauge, "cold", new Vector2(0, -GaugeHeight / 2 - 24), 20, coldColor, 100);

            flame = MinigameUI.Rect(Root, "Flame", new Vector2(130, -190), new Vector2(240, 90), new Color(1f, 0.55f, 0.15f), MinigameUI.Circle());
            pot = MinigameUI.Rect(Root, "Pot", new Vector2(130, 0), new Vector2(380, 380), MinigameUI.Dark, MinigameUI.Circle());
            liquid = MinigameUI.Rect(pot, "Liquid", Vector2.zero, new Vector2(290, 290), rightColor, MinigameUI.Circle()).GetComponent<Image>();
            spoon = MinigameUI.Rect(pot, "Spoon", new Vector2(80, 0), new Vector2(34, 180), MinigameUI.Wood, MinigameUI.Circle());

            status = MinigameUI.Label(Root, "", new Vector2(0, -228), 28, MinigameUI.Ink);
            MinigameUI.Label(Root, "Brewed", new Vector2(-440, -262), 20, MinigameUI.Ink, 200);
            progress = MinigameUI.Bar(Root, "Progress", new Vector2(40, -262), new Vector2(760, 20), MinigameUI.Dark, MinigameUI.Gold);
            MinigameUI.Label(Root, "Time", new Vector2(-440, -289), 20, MinigameUI.Ink, 200);
            timer = MinigameUI.Bar(Root, "Timer", new Vector2(40, -289), new Vector2(760, 12), MinigameUI.Dark, MinigameUI.Good);
            timer.localScale = Vector3.one;
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt, MinigameInput.ConfirmHeld);

            band.anchoredPosition = new Vector2(0, (logic.Band - 0.5f) * GaugeHeight);
            needle.anchoredPosition = new Vector2(0, (logic.Heat - 0.5f) * GaugeHeight);
            needle.GetComponent<Image>().color = logic.InBand ? MinigameUI.Good : MinigameUI.Gold;

            float off = Mathf.Clamp((logic.Heat - logic.Band) / 0.4f, -1f, 1f);
            liquid.color = off < 0f ? Color.Lerp(rightColor, coldColor, -off) : Color.Lerp(rightColor, hotColor, off);
            flame.localScale = new Vector3(0.5f + logic.Heat, 0.4f + logic.Heat * 1.2f, 1f);

            if (logic.Stirring) spoonAngle += 420f * dt;
            float a = spoonAngle * Mathf.Deg2Rad;
            spoon.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 85f;
            spoon.localRotation = Quaternion.Euler(0, 0, spoonAngle + 90f);

            if (off > 0.3f && Random.value < dt * 10f * off)
            {
                var bubble = MinigameUI.Rect(pot, "Bubble", new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 60f)), new Vector2(18, 18), new Color(1f, 1f, 1f, 0.5f), MinigameUI.Circle());
                bubbles.Add((bubble, 0.6f));
            }
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                var b = bubbles[i];
                b.life -= dt;
                if (b.life <= 0f) { Destroy(b.rt.gameObject); bubbles.RemoveAt(i); continue; }
                b.rt.anchoredPosition += new Vector2(0, 120f * dt);
                bubbles[i] = b;
            }

            status.text = logic.InBand ? "<color=#8cd973>Just right, keep it there</color>"
                : logic.Heat > logic.Band ? "<color=#f2665c>Too hot! Stir it</color>" : "<color=#7fa6ff>Too cold, let it heat</color>";
            progress.localScale = new Vector3(logic.Progress, 1f, 1f);
            timer.localScale = new Vector3(logic.timeLimit > 0f ? logic.TimeLeft / logic.timeLimit : 0f, 1f, 1f);
            timer.GetComponent<Image>().color = logic.TimeLeft < 4f ? MinigameUI.Bad : MinigameUI.Good;

            if (logic.Done)
                Finish(logic.Score, logic.Progress >= 1f ? "Brewed to perfection!" : logic.Score >= 0.5f ? "A bit scorched, but it'll do" : "Ruined the brew...");
        }
    }
}
