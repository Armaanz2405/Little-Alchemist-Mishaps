using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Timing stir: the spoon goes round the pot, press when it's in the green zone and never in the red one.
    public class StirTimingMinigame : Minigame
    {
        [SerializeField] private Color liquidColor = new Color(0.45f, 0.75f, 0.55f);

        public override string Title => "Stir the Cauldron";
        public override string Instructions => "Press E or Space when the spoon is in the green zone. Don't stir in the red!";

        private StirTimingLogic logic;
        private RectTransform pot, zone, burn, spoon, quality;
        private Image liquid;
        private TMP_Text status;
        private float flash;
        private const float Radius = 170f;

        protected override void Build()
        {
            logic = new StirTimingLogic();
            pot = MinigameUI.Rect(Root, "Pot", new Vector2(0, 10), new Vector2(440, 440), MinigameUI.Dark, MinigameUI.Circle());
            liquid = MinigameUI.Rect(pot, "Liquid", Vector2.zero, new Vector2(300, 300), liquidColor, MinigameUI.Circle()).GetComponent<Image>();
            MinigameUI.Rect(pot, "Track", Vector2.zero, new Vector2(Radius * 2 + 50, Radius * 2 + 50), new Color(1f, 1f, 1f, 0.12f), MinigameUI.Ring());
            zone = RadialRing("Zone", MinigameUI.Good);
            burn = RadialRing("Burn", MinigameUI.Bad);
            spoon = MinigameUI.Rect(pot, "Spoon", new Vector2(Radius, 0), new Vector2(46, 46), MinigameUI.Wood, MinigameUI.Circle());
            status = MinigameUI.Label(Root, "", new Vector2(0, -235), 28, MinigameUI.Ink);
            MinigameUI.Label(Root, "Stir quality", new Vector2(-440, -275), 20, MinigameUI.Ink, 200);
            quality = MinigameUI.Bar(Root, "Quality", new Vector2(40, -275), new Vector2(760, 20), MinigameUI.Dark, MinigameUI.Gold);
        }

        private RectTransform RadialRing(string name, Color color)
        {
            var rt = MinigameUI.Rect(pot, name, Vector2.zero, new Vector2(Radius * 2 + 50, Radius * 2 + 50), color, MinigameUI.Ring());
            var img = rt.GetComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Right;
            img.fillClockwise = false;
            return rt;
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt);

            if (MinigameInput.ConfirmPressed && !logic.Done)
            {
                logic.Stir();
                status.text = logic.Last == StirTimingLogic.Result.Hit ? "<color=#8cd973>Nice stir!</color>"
                    : logic.Last == StirTimingLogic.Result.Burn ? "<color=#f2665c>Scorched! That undoes a good stir</color>"
                    : "<color=#f2665c>Splash!</color>";
                flash = 0.25f;
            }

            float a = logic.Angle * Mathf.Deg2Rad;
            spoon.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Radius;
            zone.localRotation = Quaternion.Euler(0, 0, logic.ZoneStart);
            zone.GetComponent<Image>().fillAmount = logic.ZoneSize / 360f;
            burn.localRotation = Quaternion.Euler(0, 0, logic.BurnStart);
            burn.GetComponent<Image>().fillAmount = logic.BurnSize / 360f;
            spoon.GetComponent<Image>().color = logic.InZone ? MinigameUI.Gold : logic.InBurnZone ? MinigameUI.Bad : MinigameUI.Wood;

            flash = Mathf.Max(0f, flash - dt);
            float s = 1f + flash * 0.3f;
            liquid.rectTransform.localScale = new Vector3(s, s, 1f);
            liquid.color = Color.Lerp(liquidColor, MinigameUI.Gold, logic.Score * 0.5f);
            quality.localScale = new Vector3(logic.Score, 1f, 1f);

            if (logic.Done)
            {
                string result = logic.Hits == logic.stirsNeeded && logic.Burns == 0 ? "Perfectly stirred!"
                    : logic.Burns > 0 ? $"{logic.Hits}/{logic.stirsNeeded} good stirs, {logic.Burns} scorched"
                    : $"{logic.Hits}/{logic.stirsNeeded} good stirs";
                Finish(logic.Score, result);
            }
        }
    }
}
