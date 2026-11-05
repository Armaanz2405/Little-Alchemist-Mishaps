using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Concept: rhythm chopping. The knife sweeps across the ingredient one cut line per beat, press as it reaches each line.
    public class RhythmChopMinigame : Minigame
    {
        [SerializeField] private Color pieceColor = new Color(0.85f, 0.45f, 0.55f);
        [SerializeField] private float bpm = 100f;
        [SerializeField] private bool metronome = true;

        public override string Title => "Chop to the Beat";
        public override string Instructions => "Press E or Space (A on a pad) as the knife reaches each cut line. Stay on the beat!";

        public RhythmChopLogic Logic => logic;
        public void SetIngredientColor(Color color) => pieceColor = color;

        private RhythmChopLogic logic;
        private RectTransform board, knife, pulse;
        private readonly List<RectTransform> lines = new List<RectTransform>();
        private TMP_Text counter, countdown;
        private AudioSource clicks;
        private AudioClip tick, tock, chop;
        private int shown, lastBeat = -1;
        private float pulseScale = 1f;
        private readonly List<(RectTransform rt, float life)> flashes = new List<(RectTransform, float)>();
        private readonly List<(TMP_Text text, float life)> popups = new List<(TMP_Text, float)>();

        private const float LineSpacing = 90f;
        private const float FirstLineX = -315f;

        protected override void Build()
        {
            logic = new RhythmChopLogic { bpm = bpm };
            lines.Clear();
            flashes.Clear();
            popups.Clear();
            shown = 0;
            lastBeat = -1;

            board = MinigameUI.Rect(Root, "Board", new Vector2(0, 0), new Vector2(1000, 440), new Color(0.3f, 0.22f, 0.16f));
            board.gameObject.AddComponent<RectMask2D>();
            MinigameUI.Rect(board, "Ingredient", new Vector2(0, 30), new Vector2(760, 200), pieceColor);
            for (int i = 0; i < logic.cuts; i++)
                lines.Add(MinigameUI.Rect(board, "Cut", new Vector2(LineX(i), 30), new Vector2(6, 236), new Color(0.1f, 0.08f, 0.12f, 0.55f)));
            knife = MinigameUI.Rect(board, "Knife", new Vector2(KnifeX(), 30), new Vector2(14, 290), MinigameUI.Gold);
            MinigameUI.Rect(knife, "Handle", new Vector2(0, 150), new Vector2(26, 50), MinigameUI.Wood);
            pulse = MinigameUI.Rect(board, "Pulse", new Vector2(0, -170), new Vector2(36, 36), MinigameUI.Ink, MinigameUI.Circle());
            countdown = MinigameUI.Label(board, "", new Vector2(0, -170), 40, MinigameUI.Gold, 300);
            counter = MinigameUI.Label(Root, "", new Vector2(0, -262), 24, MinigameUI.Ink);

            if (metronome)
            {
                clicks = GetComponent<AudioSource>();
                if (clicks == null) clicks = gameObject.AddComponent<AudioSource>();
                if (tick == null) tick = Click("tick", 1320f);
                if (tock == null) tock = Click("tock", 880f);
                if (chop == null) chop = Click("chop", 220f, 0.09f);
            }
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt);

            if (MinigameInput.ConfirmPressed && !logic.Done)
            {
                var result = logic.Press();
                if (result == RhythmChopLogic.Judgement.None) Popup("Early!", KnifeX(), MinigameUI.Bad);
                else if (clicks != null) clicks.PlayOneShot(chop);
            }

            // show whatever got judged this frame, by press or by letting the line slide past
            while (shown < logic.NextCut)
            {
                var r = logic.Results[shown];
                Color c = r == RhythmChopLogic.Judgement.Perfect ? MinigameUI.Good : r == RhythmChopLogic.Judgement.Good ? MinigameUI.Gold : MinigameUI.Bad;
                lines[shown].GetComponent<Image>().color = c;
                Popup(r == RhythmChopLogic.Judgement.Perfect ? "Perfect!" : r == RhythmChopLogic.Judgement.Good ? "Good" : "Miss", LineX(shown), c);
                if (r != RhythmChopLogic.Judgement.Miss)
                {
                    var flash = MinigameUI.Rect(board, "Flash", new Vector2(LineX(shown), 30), new Vector2(22, 250), Color.white);
                    flashes.Add((flash, 0.18f));
                }
                shown++;
            }

            if (logic.Beat != lastBeat)
            {
                lastBeat = logic.Beat;
                pulseScale = 1.7f;
                if (clicks != null) clicks.PlayOneShot(lastBeat % 4 == 0 ? tick : tock, 0.7f);
            }
            pulseScale = Mathf.Lerp(pulseScale, 1f, dt * 10f);
            pulse.localScale = Vector3.one * pulseScale;
            knife.anchoredPosition = new Vector2(KnifeX(), 30);

            int beatsLeft = logic.countIn - logic.Beat;
            countdown.text = beatsLeft > 0 && beatsLeft <= logic.countIn ? beatsLeft.ToString() : "";
            pulse.gameObject.SetActive(beatsLeft <= 0);
            counter.text = $"Cut {Mathf.Min(logic.NextCut, logic.cuts)}/{logic.cuts}   " +
                $"<color=#8cd973>Perfect {logic.Perfects}</color>   <color=#e8c46a>Good {logic.Goods}</color>   <color=#f2665c>Miss {logic.Misses}</color>";

            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                var f = flashes[i];
                f.life -= dt;
                if (f.life <= 0f) { Destroy(f.rt.gameObject); flashes.RemoveAt(i); continue; }
                f.rt.GetComponent<Image>().color = new Color(1f, 1f, 1f, f.life / 0.18f);
                flashes[i] = f;
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

            if (logic.Done)
            {
                counter.text = "";
                string result = logic.Perfects == logic.cuts ? "Flawless rhythm!" : logic.Score >= 0.5f ? "Nicely chopped" : "Off the beat...";
                Finish(logic.Score, result);
            }
        }

        private float LineX(int cut) => FirstLineX + cut * LineSpacing;

        // one line's worth of travel per beat, so the knife reaches line i exactly on its beat
        private float KnifeX() => Mathf.Clamp(FirstLineX + (logic.Time - logic.CutTime(0)) / logic.BeatLength * LineSpacing, -520f, 520f);

        private void Popup(string text, float x, Color color)
        {
            var label = MinigameUI.Label(board, $"<b>{text}</b>", new Vector2(x, 175), 26, color, 200);
            popups.Add((label, 0.8f));
        }

        // a short synthesized click so the beat is audible without any audio assets
        private static AudioClip Click(string name, float frequency, float length = 0.05f)
        {
            const int rate = 44100;
            int samples = Mathf.CeilToInt(rate * length);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * Mathf.Exp(-t * 70f) * 0.8f;
            }
            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
