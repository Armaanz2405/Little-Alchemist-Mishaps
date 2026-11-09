using System.Collections.Generic;
using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Chop to the beat: pick which tag to cut away, then the apprentice walks the knife along the ingredient, one cut line per beat.
    // Press as each line glows. An ingredient with its own track chops to it; with no tracks a synthesized click keeps time.
    public class RhythmChopMinigame : Minigame, ITagChoiceMinigame
    {
        [SerializeField] private Color pieceColor = new Color(0.85f, 0.45f, 0.55f);
        [Tooltip("Tempo of the click when there's no track to play.")]
        [SerializeField] private float bpm = 100f;
        [SerializeField] private List<BeatTrack> tracks = new List<BeatTrack>();
        [SerializeField] private bool metronome = true;
        [Tooltip("The apprentice holding the knife. Without it the knife moves on its own.")]
        [SerializeField] private Sprite apprentice;

        public override string Title => "Chop to the Beat";
        public override string Instructions => choice != null && choice.Open
            ? "Which tag do you want to cut away? A / D to pick, E to start chopping."
            : "Chop as the knife reaches each glowing line (A on a pad). Stay on the beat!";

        public RhythmChopLogic Logic => logic;
        // index into the ingredient's tag list that the player chose to cut off
        public int ChosenTag { get; private set; }

        public void SetIngredientColor(Color color) => pieceColor = color;

        // the cutting board calls this with whatever is on the board before the game can start
        public void SetIngredient(IngredientData data)
        {
            ingredient = data;
            if (data != null) pieceColor = data.color;
            ChosenTag = 0;
            choosing = data != null && data.ingredientEffects != null && data.ingredientEffects.Count > 1;
        }

        private IngredientData ingredient;
        private bool choosing;
        private TagChoiceScreen choice;
        private RhythmChopLogic logic;
        private BeatTrack track;
        private RectTransform board, rig, knife, glow, pulse, check;
        private readonly List<RectTransform> lines = new List<RectTransform>();
        private TMP_Text counter, countdown;
        private AudioSource audio;
        private AudioClip tick, tock, chop;
        private int shown, lastBeat = -1;
        private float pulseScale = 1f, swing, handX, handY, idleAngle, chopAngle;
        private readonly List<(RectTransform rt, float life)> flashes = new List<(RectTransform, float)>();
        private readonly List<(TMP_Text text, float life)> popups = new List<(TMP_Text, float)>();

        private const float LineSpacing = 84f;
        private const float FirstLineX = -280f;
        private const float BodyY = -70f;
        private const float BodyLength = 720f;
        private const float BodyHeight = 150f;
        private const float KnifeLength = 170f;
        private const float SwingTime = 0.18f;
        private const float SwingHold = 0.12f;
        private static readonly Color Steel = new Color(0.8f, 0.82f, 0.86f);
        private static readonly Color CutMark = new Color(0.1f, 0.08f, 0.12f, 0.6f);

        protected override void Build()
        {
            choice = null;
            if (choosing)
            {
                choice = new TagChoiceScreen(ingredient);
                choice.Build(Root);
            }
            else BuildBoard();
        }

        private void BuildBoard()
        {
            int pick = BeatTracks.Pick(tracks, ingredient != null ? ingredient.displayName : null);
            track = pick >= 0 && tracks[pick] != null && tracks[pick].clip != null ? tracks[pick] : null;
            logic = new RhythmChopLogic { bpm = track != null ? track.bpm : bpm };
            lines.Clear();
            flashes.Clear();
            popups.Clear();
            shown = 0;
            lastBeat = -1;
            swing = 0f;

            board = MinigameUI.Rect(Root, "Board", new Vector2(0, 0), new Vector2(1000, 440), MinigameUI.Board);
            board.gameObject.AddComponent<RectMask2D>();
            MinigameUI.Rect(board, "Table", new Vector2(0, -172), new Vector2(1000, 96), MinigameUI.Wood);
            MinigameUI.Rect(board, "Table Edge", new Vector2(0, -125), new Vector2(1000, 5), MinigameUI.Dark);

            // the ingredient lying on its side, long enough to take every cut
            float mid = BodyLength - BodyHeight;
            MinigameUI.Rect(board, "Ingredient", new Vector2(0, BodyY), new Vector2(mid, BodyHeight), pieceColor);
            MinigameUI.Rect(board, "End", new Vector2(-mid / 2f, BodyY), new Vector2(BodyHeight, BodyHeight), pieceColor, MinigameUI.Circle());
            MinigameUI.Rect(board, "End", new Vector2(mid / 2f, BodyY), new Vector2(BodyHeight, BodyHeight), pieceColor, MinigameUI.Circle());
            if (ingredient != null && ingredient.sprite != null)
                MinigameUI.Picture(board, "Top", new Vector2(BodyLength / 2f + 50f, BodyY + 12f), new Vector2(170, 170), ingredient.sprite).color = ingredient.color;

            glow = MinigameUI.Rect(board, "Glow", new Vector2(LineX(0), BodyY), new Vector2(22, BodyHeight + 34), MinigameUI.Gold);
            for (int i = 0; i < logic.cuts; i++)
            {
                var line = MinigameUI.Rect(board, "Cut", new Vector2(LineX(i), BodyY), new Vector2(6, BodyHeight + 16), Color.clear);
                for (int dash = -1; dash <= 1; dash++) MinigameUI.Rect(line, "Dash", new Vector2(0, dash * 54), new Vector2(6, 34), CutMark);
                lines.Add(line);
            }

            rig = MinigameUI.Rect(board, "Rig", new Vector2(KnifeX(), 0), Vector2.zero, Color.clear);
            if (apprentice != null)
            {
                var guy = MinigameUI.Picture(rig, "Apprentice", Vector2.zero, new Vector2(110, 150), apprentice).rectTransform;
                guy.pivot = new Vector2(0.5f, 0f);
                guy.anchoredPosition = new Vector2(-70, BodyY + BodyHeight / 2f - 6f);
                // the raised fist in the hold pose
                handX = -52f;
                handY = guy.anchoredPosition.y + 118f;
                idleAngle = -50f;
                chopAngle = -162f;
            }
            else
            {
                handX = 0f;
                handY = BodyY + BodyHeight / 2f + KnifeLength + 24f;
                idleAngle = chopAngle = 180f;
            }
            knife = MinigameUI.Rect(rig, "Knife", Vector2.zero, new Vector2(26, KnifeLength), Steel);
            knife.pivot = new Vector2(0.5f, 0f);
            knife.anchoredPosition = new Vector2(handX, handY);
            MinigameUI.Rect(knife, "Tip", new Vector2(0, KnifeLength / 2f), new Vector2(18.4f, 18.4f), Steel).localRotation = Quaternion.Euler(0f, 0f, 45f);
            MinigameUI.Rect(knife, "Handle", new Vector2(0, -KnifeLength / 2f + 14f), new Vector2(32, 48), MinigameUI.Wood);

            pulse = MinigameUI.Rect(board, "Pulse", new Vector2(462, 184), new Vector2(26, 26), MinigameUI.Ink, MinigameUI.Circle());
            countdown = MinigameUI.Label(board, "", new Vector2(0, 130), 64, MinigameUI.Gold, 300);
            counter = MinigameUI.Label(board, "", new Vector2(0, -202), 20, MinigameUI.Ink);

            var hint = MinigameUI.Row(Root, "Hint", new Vector2(-60, -256));
            MinigameUI.RowText(hint, "Press", 24, MinigameUI.Ink);
            UIStyle.Key(hint, "Space", 36f);
            MinigameUI.RowText(hint, "to chop", 24, MinigameUI.Ink);
            check = MinigameUI.IconBox(Root, MinigameUI.Icon.Check, new Vector2(372, -256), 44, MinigameUI.Good);
            MinigameUI.IconBox(Root, MinigameUI.Icon.Cross, new Vector2(424, -256), 44, MinigameUI.Bad);
            UIStyle.Key(MinigameUI.Row(Root, "Exit", new Vector2(454, -256), 8f, 0f), "Esc", 30f);

            audio = GetComponent<AudioSource>();
            if (audio == null) audio = gameObject.AddComponent<AudioSource>();
            if (chop == null) chop = Click("chop", 220f, 0.09f);
            if (track != null)
            {
                audio.clip = track.clip;
                audio.time = Mathf.Clamp(track.offset, 0f, Mathf.Max(0f, track.clip.length - 0.01f));
                audio.Play();
            }
            else if (metronome)
            {
                if (tick == null) tick = Click("tick", 1320f);
                if (tock == null) tock = Click("tock", 880f);
            }
        }

        protected override void OnCancelled()
        {
            if (audio != null) audio.Stop();
        }

        protected override void Step(float dt)
        {
            if (choice != null && choice.Open)
            {
                if (!choice.Step()) return;
                ChosenTag = choice.Chosen;
                pieceColor = IngredientTags.ColorOf(ingredient.ingredientEffects[ChosenTag].tag_effect);
                if (InstructionLabel != null) InstructionLabel.text = Instructions;
                BuildBoard();
                return;
            }

            logic.Tick(dt);

            if (MinigameInput.ConfirmPressed && !logic.Done)
            {
                var result = logic.Press();
                if (result == RhythmChopLogic.Judgement.None) Popup("Early!", KnifeX(), MinigameUI.Bad);
                else
                {
                    audio.PlayOneShot(chop);
                    swing = SwingTime;
                    check.localScale = Vector3.one * 1.3f;
                }
            }

            // show whatever got judged this frame, by press or by letting the line slide past
            while (shown < logic.NextCut)
            {
                var r = logic.Results[shown];
                Color c = r == RhythmChopLogic.Judgement.Perfect ? MinigameUI.Good : r == RhythmChopLogic.Judgement.Good ? MinigameUI.Gold : MinigameUI.Bad;
                foreach (var mark in lines[shown].GetComponentsInChildren<Image>()) mark.color = c;
                Popup(r == RhythmChopLogic.Judgement.Perfect ? "Perfect!" : r == RhythmChopLogic.Judgement.Good ? "Good" : "Miss", LineX(shown), c);
                if (r != RhythmChopLogic.Judgement.Miss)
                {
                    var flash = MinigameUI.Rect(board, "Flash", new Vector2(LineX(shown), BodyY), new Vector2(22, BodyHeight + 30), Color.white);
                    flashes.Add((flash, 0.18f));
                }
                shown++;
            }

            if (logic.Beat != lastBeat)
            {
                lastBeat = logic.Beat;
                pulseScale = 1.7f;
                if (track == null && tick != null) audio.PlayOneShot(lastBeat % 4 == 0 ? tick : tock, 0.7f);
            }
            pulseScale = Mathf.Lerp(pulseScale, 1f, dt * 10f);
            pulse.localScale = Vector3.one * pulseScale;
            check.localScale = Vector3.Lerp(check.localScale, Vector3.one, dt * 12f);

            // the apprentice lands on each beat and brings the knife down on a press
            float phase = Mathf.Repeat(logic.Time / logic.BeatLength, 1f);
            swing = Mathf.Max(0f, swing - dt);
            float strike = swing <= 0f ? 0f : swing > SwingHold ? 1f - (swing - SwingHold) / (SwingTime - SwingHold) : swing / SwingHold;
            float hop = apprentice != null ? Mathf.Sin(phase * Mathf.PI) * 12f * (1f - strike) : 0f;
            rig.anchoredPosition = new Vector2(KnifeX(), hop);
            knife.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(idleAngle, chopAngle, strike));
            if (apprentice == null) knife.anchoredPosition = new Vector2(handX, handY - 40f * strike);

            glow.gameObject.SetActive(!logic.Done);
            glow.anchoredPosition = new Vector2(LineX(Mathf.Min(logic.NextCut, logic.cuts - 1)), BodyY);
            var gold = MinigameUI.Gold;
            glow.GetComponent<Image>().color = new Color(gold.r, gold.g, gold.b, 0.25f + 0.4f * (1f - phase));

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
        private float KnifeX() => Mathf.Clamp(FirstLineX + (logic.Time - logic.CutTime(0)) / logic.BeatLength * LineSpacing, -620f, 520f);

        private void Popup(string text, float x, Color color)
        {
            var label = MinigameUI.Label(board, $"<b>{text}</b>", new Vector2(x, -166), 26, color, 200);
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
