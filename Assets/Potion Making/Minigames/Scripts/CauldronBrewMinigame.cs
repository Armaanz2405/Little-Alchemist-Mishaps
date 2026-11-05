using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Aliyah's cauldron game. Every ingredient in the pot wants its own stir (direction and speed, for a few turns) and
    // the fire underneath has to stay in range: it burns down on its own, X pumps the bellows.
    public class CauldronBrewMinigame : Minigame
    {
        [SerializeField] private Color coldColor = new Color(0.3f, 0.42f, 0.6f);
        [SerializeField] private Color scorchColor = new Color(0.55f, 0.18f, 0.12f);

        public override string Title => "Brew the Potion";
        public override string Instructions => "A / D (or the stick) stir the way each ingredient asks. X pumps the bellows, keep the fire in the green.";

        public CauldronBrewLogic Logic => logic;

        private class Row
        {
            public CauldronBrewLogic.Need need;
            public RectTransform frame;
            public TMP_Text ask;
            public readonly List<RectTransform> pips = new List<RectTransform>();
        }

        private readonly List<(string name, Color color, float quality)> ingredients = new List<(string, Color, float)>();
        private CauldronBrewLogic logic;
        private readonly List<Row> rows = new List<Row>();
        private RectTransform pot, spoon, hint, speedBand, speedNeedle, flame, flameCore, heatBand, heatNeedle, bellows, timer;
        private Image liquid;
        private TMP_Text status, askLabel, fuelLabel;
        private Color baseLiquid;
        private float bellowsSquash;
        private readonly List<(RectTransform rt, float life)> bubbles = new List<(RectTransform, float)>();

        private const float PotX = 70f;
        private const float GaugeWidth = 400f;

        // the cauldron calls this with what's in the pot before the game can start
        public void SetIngredients(IEnumerable<IngredientData> items)
        {
            ingredients.Clear();
            foreach (var i in items)
                if (i != null) ingredients.Add((i.displayName, i.color, i.refineQuality));
        }

        protected override void Build()
        {
            rows.Clear();
            bubbles.Clear();
            var needs = ingredients.Count > 0
                ? ingredients.Select(i => CauldronBrewLogic.NeedFor(i.name, i.color, i.quality)).ToList()
                : new List<CauldronBrewLogic.Need>
                {
                    CauldronBrewLogic.NeedFor("Heartroot", new Color(0.85f, 0.45f, 0.55f), 1f),
                    CauldronBrewLogic.NeedFor("Siren Tear", new Color(0.45f, 0.7f, 0.9f), 0.5f),
                };
            logic = new CauldronBrewLogic(needs, MinigameVariants.Flameflies);

            // ingredient list, like the sketch: one slot per ingredient with its stir written under it
            var list = MinigameUI.Rect(Root, "Ingredients", new Vector2(-390, -30), new Vector2(260, 500), MinigameUI.Board);
            MinigameUI.Label(list, "<b>Ingredients</b>", new Vector2(0, 225), 24, MinigameUI.Ink, 250);
            float rowHeight = Mathf.Min(150f, 440f / Mathf.Max(1, needs.Count));
            for (int i = 0; i < needs.Count; i++)
            {
                var need = needs[i];
                float y = 150f - i * rowHeight;
                var row = new Row { need = need };
                row.frame = MinigameUI.Rect(list, "Current", new Vector2(0, y - 24), new Vector2(244, rowHeight - 6), new Color(1f, 0.85f, 0.45f, 0.14f));
                MinigameUI.Rect(list, "Slot", new Vector2(0, y + 18), new Vector2(60, 60), need.color, MinigameUI.Circle());
                MinigameUI.Label(list, need.name, new Vector2(0, y - 26), 17, MinigameUI.Ink, 250);
                row.ask = MinigameUI.Label(list, AskText(need), new Vector2(0, y - 56), 15, MinigameUI.Gold, 250);
                for (int p = 0; p < need.turns; p++)
                    row.pips.Add(Cross(list, new Vector2((p - (need.turns - 1) / 2f) * 26f, y - 88), 14f));
                rows.Add(row);
            }

            // the pot, with the stir hint ring turning the way the current ingredient wants
            hint = MinigameUI.Rect(Root, "Hint", new Vector2(PotX, 10), new Vector2(400, 400), new Color(1f, 0.85f, 0.45f, 0.22f), MinigameUI.Ring());
            var hintImg = hint.GetComponent<Image>();
            hintImg.type = Image.Type.Filled;
            hintImg.fillMethod = Image.FillMethod.Radial360;
            hintImg.fillAmount = 0.82f;
            pot = MinigameUI.Rect(Root, "Pot", new Vector2(PotX, 10), new Vector2(340, 340), MinigameUI.Dark, MinigameUI.Circle());
            baseLiquid = needs[0].color;
            liquid = MinigameUI.Rect(pot, "Liquid", Vector2.zero, new Vector2(262, 262), baseLiquid, MinigameUI.Circle()).GetComponent<Image>();
            spoon = MinigameUI.Rect(pot, "Spoon", new Vector2(80, 0), new Vector2(30, 170), MinigameUI.Wood, MinigameUI.Circle());
            askLabel = MinigameUI.Label(Root, "", new Vector2(PotX, 228), 23, MinigameUI.Gold, 760);

            // stir speed under the pot
            MinigameUI.Label(Root, "stir speed", new Vector2(PotX - GaugeWidth / 2 - 70, -180), 17, MinigameUI.Ink, 120);
            var speedBack = MinigameUI.Rect(Root, "Speed", new Vector2(PotX, -180), new Vector2(GaugeWidth, 16), MinigameUI.Dark);
            speedBand = MinigameUI.Rect(speedBack, "Band", Vector2.zero, new Vector2(10, 16), new Color(0.55f, 0.85f, 0.45f, 0.75f));
            speedNeedle = MinigameUI.Rect(speedBack, "Needle", Vector2.zero, new Vector2(6, 30), MinigameUI.Gold);

            // fire: flame, heat gauge and the bellows
            flame = MinigameUI.Rect(Root, "Flame", new Vector2(PotX, -250), new Vector2(170, 80), MinigameUI.Ember, MinigameUI.Circle());
            flameCore = MinigameUI.Rect(flame, "Core", new Vector2(0, -8), new Vector2(90, 44), new Color(1f, 0.9f, 0.55f), MinigameUI.Circle());
            MinigameUI.Label(Root, "fire", new Vector2(PotX - GaugeWidth / 2 - 70, -300), 17, MinigameUI.Ink, 120);
            var heatBack = MinigameUI.Rect(Root, "Heat", new Vector2(PotX, -300), new Vector2(GaugeWidth, 16), MinigameUI.Dark);
            heatBand = MinigameUI.Rect(heatBack, "Band", Vector2.zero, new Vector2((logic.heatMax - logic.heatMin) * GaugeWidth, 16), new Color(0.55f, 0.85f, 0.45f, 0.75f));
            heatBand.anchoredPosition = new Vector2(((logic.heatMin + logic.heatMax) / 2f - 0.5f) * GaugeWidth, 0);
            heatNeedle = MinigameUI.Rect(heatBack, "Needle", Vector2.zero, new Vector2(6, 30), MinigameUI.Gold);
            bellows = MinigameUI.Rect(Root, "Bellows", new Vector2(PotX + 300, -255), new Vector2(54, 92), MinigameUI.Wood);
            bellows.pivot = new Vector2(0.5f, 0f);
            bellows.anchoredPosition = new Vector2(PotX + 300, -300);
            MinigameUI.Rect(bellows, "Nozzle", new Vector2(0, 0), new Vector2(18, 26), MinigameUI.Stone).anchoredPosition = new Vector2(0, -6);
            MinigameUI.Label(Root, "X: stoke", new Vector2(PotX + 300, -325), 18, MinigameUI.Ink, 160);
            fuelLabel = MinigameUI.Label(Root, logic.Fuel > 0 ? $"<color=#e8c46a>{logic.Fuel} flameflies</color> in the fire" : "", new Vector2(PotX + 300, -190), 17, MinigameUI.Ink, 200);

            status = MinigameUI.Label(Root, "", new Vector2(PotX, -345), 22, MinigameUI.Ink, 600);
            timer = MinigameUI.Bar(Root, "Timer", new Vector2(PotX, 254), new Vector2(460, 6), MinigameUI.Dark, MinigameUI.Good);
            timer.localScale = Vector3.one;
            Refresh();
        }

        protected override void Step(float dt)
        {
            bool stoke = MinigameInput.StokePressed;
            logic.Tick(dt, MinigameInput.StirAxis, stoke);
            if (stoke) bellowsSquash = 1f;
            Refresh();

            float a = logic.Angle * Mathf.Deg2Rad;
            spoon.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 78f;
            spoon.localRotation = Quaternion.Euler(0, 0, logic.Angle + 90f);

            var need = logic.CurrentNeed;
            if (need != null)
            {
                float mid = (logic.BandMin(need) + logic.BandMax(need)) / 2f * logic.DirectionSign(need.direction);
                hint.localRotation = Quaternion.Euler(0, 0, hint.localEulerAngles.z + mid * dt);
                hint.GetComponent<Image>().fillClockwise = need.direction == CauldronBrewLogic.Direction.Clockwise;
            }
            hint.gameObject.SetActive(need != null);

            // liquid: the current ingredient's colour, blue when the fire's too low, reddening as it scorches
            Color target = need != null ? need.color : baseLiquid;
            if (logic.FireTooLow) target = Color.Lerp(target, coldColor, 0.6f);
            target = Color.Lerp(target, scorchColor, logic.Scorch);
            liquid.color = Color.Lerp(liquid.color, target, dt * 4f);

            flame.localScale = new Vector3(0.45f + logic.Heat * 0.9f, 0.3f + logic.Heat * 1.4f, 1f);
            flame.GetComponent<Image>().color = logic.FireTooHot ? Color.Lerp(MinigameUI.Ember, MinigameUI.Bad, 0.6f) : MinigameUI.Ember;
            flameCore.localScale = Vector3.one * (0.8f + Mathf.Sin(logic.Elapsed * 14f) * 0.08f);
            heatNeedle.anchoredPosition = new Vector2((logic.Heat - 0.5f) * GaugeWidth, 0);
            heatNeedle.GetComponent<Image>().color = logic.FireOk ? MinigameUI.Good : MinigameUI.Bad;
            bellowsSquash = Mathf.Max(0f, bellowsSquash - dt * 6f);
            bellows.localScale = new Vector3(1f + bellowsSquash * 0.25f, 1f - bellowsSquash * 0.45f, 1f);

            if (logic.FireTooHot && Random.value < dt * 8f)
                bubbles.Add((MinigameUI.Rect(pot, "Bubble", new Vector2(Random.Range(-90f, 90f), Random.Range(-90f, 50f)), new Vector2(16, 16), new Color(1f, 1f, 1f, 0.5f), MinigameUI.Circle()), 0.6f));
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                var b = bubbles[i];
                b.life -= dt;
                if (b.life <= 0f) { Destroy(b.rt.gameObject); bubbles.RemoveAt(i); continue; }
                b.rt.anchoredPosition += new Vector2(0, 110f * dt);
                bubbles[i] = b;
            }

            timer.localScale = new Vector3(logic.timeLimit > 0f ? logic.TimeLeft / logic.timeLimit : 0f, 1f, 1f);
            timer.GetComponent<Image>().color = logic.TimeLeft < 6f ? MinigameUI.Bad : MinigameUI.Good;

            if (logic.Done)
            {
                status.text = "";
                MinigameVariants.Flameflies = 0;
                string result = logic.Current >= logic.Needs.Count
                    ? (logic.Scorch > 0.2f ? "Brewed, but a little scorched" : "Brewed to perfection!")
                    : logic.Score >= 0.5f ? "Half-brewed, it'll have to do" : "The brew never came together...";
                Finish(logic.Score, result);
            }
        }

        private void Refresh()
        {
            var need = logic.CurrentNeed;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                row.frame.gameObject.SetActive(row.need == need);
                for (int p = 0; p < row.pips.Count; p++)
                {
                    bool lit = row.need.Done >= p + 1f - 1e-3f;
                    float partial = Mathf.Clamp01(row.need.Done - p);
                    foreach (var img in row.pips[p].GetComponentsInChildren<Image>())
                        img.color = lit ? MinigameUI.Gold : Color.Lerp(new Color(1f, 1f, 1f, 0.18f), MinigameUI.Gold, partial * 0.7f);
                }
                row.ask.color = row.need.Complete ? MinigameUI.Good : row.need == need ? MinigameUI.Gold : MinigameUI.Ink;
            }

            if (need != null)
            {
                askLabel.text = $"<b>{need.name}</b>: {DirectionWord(need)}, {(need.fast ? "fast" : "slowly")}";
                float lo = logic.BandMin(need) / logic.maxSpeed, hi = logic.BandMax(need) / logic.maxSpeed;
                speedBand.sizeDelta = new Vector2((hi - lo) * GaugeWidth, 16);
                speedBand.anchoredPosition = new Vector2(((lo + hi) / 2f - 0.5f) * GaugeWidth, 0);
            }
            else askLabel.text = "";
            speedNeedle.anchoredPosition = new Vector2((Mathf.Abs(logic.Speed) / logic.maxSpeed - 0.5f) * GaugeWidth, 0);
            speedNeedle.GetComponent<Image>().color = logic.StirringRight ? MinigameUI.Good : MinigameUI.Gold;

            if (need == null) status.text = "";
            else if (logic.FireTooLow) status.text = "<color=#7fa6ff>The fire's dying, pump the bellows!</color>";
            else if (logic.FireTooHot) status.text = "<color=#f2665c>Too hot! Let it settle</color>";
            else if (Mathf.Abs(logic.Speed) < 20f) status.text = "Start stirring";
            else if (!logic.DirectionRight(need)) status.text = "<color=#f2665c>Other way!</color>";
            else if (Mathf.Abs(logic.Speed) < logic.BandMin(need)) status.text = "<color=#e8c46a>Faster</color>";
            else if (Mathf.Abs(logic.Speed) > logic.BandMax(need)) status.text = "<color=#e8c46a>Slower</color>";
            else status.text = "<color=#8cd973>That's it, keep going</color>";
        }

        private string AskText(CauldronBrewLogic.Need need) => $"{DirectionWord(need)}\n{(need.fast ? "fast" : "slow")}, {need.turns} turns";

        private static string DirectionWord(CauldronBrewLogic.Need need) =>
            need.direction == CauldronBrewLogic.Direction.Clockwise ? "clockwise (D)" : "counter-clockwise (A)";

        // the x marks from the sketch, one per turn
        private static RectTransform Cross(Transform parent, Vector2 pos, float size)
        {
            var holder = MinigameUI.Rect(parent, "Turn", pos, new Vector2(size, size), Color.clear);
            MinigameUI.Rect(holder, "a", Vector2.zero, new Vector2(size, 3), Color.white).localRotation = Quaternion.Euler(0, 0, 45f);
            MinigameUI.Rect(holder, "b", Vector2.zero, new Vector2(size, 3), Color.white).localRotation = Quaternion.Euler(0, 0, -45f);
            return holder;
        }
    }
}
