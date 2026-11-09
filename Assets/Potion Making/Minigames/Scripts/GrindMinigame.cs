using System.Collections.Generic;
using System.Linq;
using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Mortar and pestle. First a look at what's in the mortar and what grinding it will make, with a chance to take
    // something back out. Then alternate left and right to grind it down before the timer runs out.
    public class GrindMinigame : Minigame
    {
        private enum State { Review, Edit, Grind }

        [SerializeField] private Color pasteColor = new Color(0.6f, 0.5f, 0.35f);
        [SerializeField] private Sprite mortarSprite;
        [SerializeField] private Sprite apprentice;

        public override string Title => "Grind It Down";
        public override string Instructions =>
            state == State.Edit ? "A / D to pick an ingredient, E to take it out of the mortar."
            : state == State.Review ? "Check what's going in. Grinding packs it all into one paste."
            : "Alternate A and D (or left and right) to grind before the time runs out.";

        public void SetPasteColor(Color color) => pasteColor = color;
        // the mortar hands itself over so the screen can show and edit what's in it
        public void SetSource(IGrindSource mortar) => source = mortar;

        private IGrindSource source;
        private State state = State.Grind;
        private GrindLogic logic;
        private RectTransform left, drawer, content, bowl, paste, guy, progress, timer;
        private TMP_Text status;
        private Vector2 guyHome, pasteSmall, pasteBig;
        private float flash;
        private int selected;
        private readonly List<(RectTransform rt, Vector2 velocity, float life)> splashes = new List<(RectTransform, Vector2, float)>();

        private int Count => source != null ? source.Contents.Count : 0;
        private int Capacity => source != null ? source.Capacity : 3;
        private static Color Muted => UIStyle.TextColor(TextKind.Muted);

        protected override void Build()
        {
            logic = null;
            selected = 0;
            flash = 0f;
            content = null;
            splashes.Clear();
            state = source != null ? State.Review : State.Grind;

            left = MinigameUI.Card(Root, "Mortar", new Vector2(-270, -22), new Vector2(480, 490));
            if (mortarSprite != null)
            {
                bowl = MinigameUI.Picture(left, "Bowl", new Vector2(20, 22), new Vector2(420, 420), mortarSprite).rectTransform;
                paste = MinigameUI.Rect(bowl, "Paste", new Vector2(47, 44), Vector2.zero, pasteColor, MinigameUI.Circle());
                pasteSmall = new Vector2(28, 12);
                pasteBig = new Vector2(105, 35);
            }
            else
            {
                bowl = MinigameUI.Rect(left, "Bowl", new Vector2(0, 28), new Vector2(300, 300), MinigameUI.Dark, MinigameUI.Circle());
                paste = MinigameUI.Rect(bowl, "Paste", new Vector2(0, -30), Vector2.zero, pasteColor, MinigameUI.Circle());
                pasteSmall = new Vector2(60, 34);
                pasteBig = new Vector2(250, 140);
            }
            guy = null;
            if (apprentice != null)
            {
                guy = MinigameUI.Picture(left, "Apprentice", Vector2.zero, new Vector2(130, 177), apprentice).rectTransform;
                guy.pivot = new Vector2(0.5f, 0f);
                guyHome = new Vector2(-172, -80);
                guy.anchoredPosition = guyHome;
            }
            status = MinigameUI.Label(left, "", new Vector2(0, 218), 24, MinigameUI.Ink, 450);
            var keys = MinigameUI.Row(left, "Grind", new Vector2(0, -206));
            MinigameUI.RowText(keys, "<b>GRIND</b>", 26, MinigameUI.Gold);
            UIStyle.Key(keys, "A", 36f);
            MinigameUI.RowText(keys, "and", 20, MinigameUI.Ink);
            UIStyle.Key(keys, "D", 36f);

            drawer = MinigameUI.Card(Root, "Drawer", new Vector2(270, -22), new Vector2(480, 490));
            SetState(state);
        }

        private void SetState(State next)
        {
            state = next;
            selected = Mathf.Clamp(selected, 0, Mathf.Max(0, Count - 1));
            if (state == State.Grind) logic = new GrindLogic();
            if (InstructionLabel != null) InstructionLabel.text = Instructions;
            Refresh();
        }

        // rebuilds the drawer: what's in each slot, what grinding makes of it, and the buttons for this state
        private void Refresh()
        {
            if (content != null) Destroy(content.gameObject);
            content = MinigameUI.Rect(drawer, "Content", Vector2.zero, drawer.sizeDelta, Color.clear);
            var contents = source != null ? source.Contents : null;
            var preview = MortarPreview.Of(contents);
            if (preview.outcome.Count > 0) pasteColor = preview.color;
            paste.GetComponent<Image>().color = pasteColor;
            if (state != State.Grind)
                status.text = Count == 0 ? "The mortar is empty" : $"Makes <b>{preview.name}</b>";

            MinigameUI.Label(content, state == State.Edit ? "Take something out" : "In the mortar", new Vector2(0, 222), 20, Muted, 440);
            for (int i = 0; i < Capacity; i++)
            {
                float x = (i - (Capacity - 1) / 2f) * 150f;
                if (state == State.Edit && i == selected && i < Count)
                    MinigameUI.Rect(content, "Selected", new Vector2(x, 152), new Vector2(116, 116), MinigameUI.Gold);
                var slot = MinigameUI.Card(content, "Slot", new Vector2(x, 152), new Vector2(104, 104), 2f);
                if (i >= Count)
                {
                    MinigameUI.Label(slot, "empty", Vector2.zero, 16, Muted, 100);
                    continue;
                }
                var item = contents[i];
                if (item.sprite != null) MinigameUI.Picture(slot, "Icon", new Vector2(0, 6), new Vector2(76, 76), item.sprite).color = item.color;
                else MinigameUI.Rect(slot, "Icon", new Vector2(0, 6), new Vector2(60, 60), item.color, MinigameUI.Circle());
                MinigameUI.Label(content, item.displayName, new Vector2(x, 84), 17, MinigameUI.Ink, 148);
                var tags = item.ingredientEffects.Where(t => t != null && t.tag_value > 0).ToList();
                for (int t = 0; t < tags.Count; t++)
                    MinigameUI.Rect(content, "Tag", new Vector2(x + (t - (tags.Count - 1) / 2f) * 18f, 111), new Vector2(13, 13),
                        IngredientTags.ColorOf(tags[t].tag_effect), MinigameUI.Circle());
                var shared = MortarPreview.SharedTags(contents, i);
                MinigameUI.Label(content, shared.Count > 0 ? "boosts " + string.Join(", ", shared.Select(IngredientTags.Name)) : "no boost",
                    new Vector2(x, 54), 15, shared.Count > 0 ? MinigameUI.Good : Muted, 150);
            }

            float y = 14f;
            y = Section("Enhancing", y, preview.enhanced.Select(e => (IngredientTags.Name(e), IngredientTags.ColorOf(e))));
            y = Section("Removing", y, preview.removed.Select(e => (IngredientTags.Name(e), IngredientTags.ColorOf(e))));
            Section("Outcome", y, preview.outcome.Select(t => ($"{IngredientTags.Name(t.tag_effect)} {t.tag_value}", IngredientTags.ColorOf(t.tag_effect))));

            if (state == State.Grind)
            {
                MinigameUI.Label(content, "Ground", new Vector2(-180, -165), 18, MinigameUI.Ink, 100);
                progress = MinigameUI.Bar(content, "Progress", new Vector2(40, -165), new Vector2(320, 18), MinigameUI.Dark, MinigameUI.Gold);
                MinigameUI.Label(content, "Time", new Vector2(-180, -200), 18, MinigameUI.Ink, 100);
                timer = MinigameUI.Bar(content, "Timer", new Vector2(40, -200), new Vector2(320, 12), MinigameUI.Dark, MinigameUI.Good);
                timer.localScale = Vector3.one;
            }
            else if (state == State.Edit)
            {
                Choice(MinigameUI.Icon.Cross, -75, MinigameUI.Bad, "E: take out");
                Choice(MinigameUI.Icon.Check, 75, MinigameUI.Good, "Q: done");
            }
            else
            {
                bool full = Count >= Capacity;
                Choice(MinigameUI.Icon.Check, -150, full ? MinigameUI.Good : Muted, full ? "Space: grind" : Count == 0 ? "add ingredients" : $"add {Capacity - Count} more");
                Choice(MinigameUI.Icon.Pencil, 0, Count > 0 ? MinigameUI.Gold : Muted, "Q: edit");
                Choice(MinigameUI.Icon.Cross, 150, MinigameUI.Bad, "Esc: leave");
            }
        }

        // a heading with its tags as chips, wrapping onto more lines when they don't fit. Returns where the next section starts.
        private float Section(string heading, float y, IEnumerable<(string text, Color color)> chips)
        {
            var label = MinigameUI.Label(content, heading + ":", new Vector2(-166, y), 19, MinigameUI.Ink, 124);
            label.horizontalAlignment = HorizontalAlignmentOptions.Left;
            const float start = -102f, width = 330f;
            RectTransform row = null;
            float used = 0f;
            bool any = false;
            foreach (var (text, color) in chips)
            {
                float w = text.Length * 9.4f + 26f;
                if (row == null || used + w > width)
                {
                    if (row != null) y -= 30f;
                    row = MinigameUI.Row(content, heading + " Tags", new Vector2(start, y), 6f, 0f);
                    used = 0f;
                }
                MinigameUI.Chip(row, text, color, 16);
                used += w;
                any = true;
            }
            if (!any)
            {
                var none = MinigameUI.Label(content, "none", new Vector2(start + 60f, y), 16, Muted, 120);
                none.horizontalAlignment = HorizontalAlignmentOptions.Left;
            }
            return y - 30f;
        }

        private void Choice(MinigameUI.Icon icon, float x, Color color, string label)
        {
            MinigameUI.IconBox(content, icon, new Vector2(x, -165), 44, color);
            MinigameUI.Label(content, label, new Vector2(x, -212), 16, MinigameUI.Ink, 150);
        }

        protected override bool HandleCancel()
        {
            if (state != State.Edit) return false;
            SetState(State.Review);
            return true;
        }

        protected override void Step(float dt)
        {
            flash = Mathf.Max(0f, flash - dt);
            Animate(dt);

            if (state == State.Review)
            {
                if (MinigameInput.EditPressed && Count > 0) SetState(State.Edit);
                else if (MinigameInput.ConfirmPressed && Count >= Capacity) SetState(State.Grind);
                return;
            }
            if (state == State.Edit)
            {
                if (MinigameInput.LeftPressed) { selected = (selected + Count - 1) % Count; Refresh(); }
                if (MinigameInput.RightPressed) { selected = (selected + 1) % Count; Refresh(); }
                if (MinigameInput.EditPressed) SetState(State.Review);
                else if (MinigameInput.ConfirmPressed)
                {
                    source.Eject(selected);
                    SetState(Count == 0 ? State.Review : State.Edit);
                }
                return;
            }

            logic.Tick(dt);
            if (!logic.Done)
            {
                if (MinigameInput.LeftPressed) Press(GrindLogic.Side.Left);
                if (MinigameInput.RightPressed) Press(GrindLogic.Side.Right);
            }
            progress.localScale = new Vector3(logic.Progress, 1f, 1f);
            timer.localScale = new Vector3(logic.timeLimit > 0f ? logic.TimeLeft / logic.timeLimit : 0f, 1f, 1f);
            timer.GetComponent<Image>().color = logic.TimeLeft < 2f ? MinigameUI.Bad : MinigameUI.Good;

            if (logic.Done)
                Finish(logic.Score, logic.Succeeded ? "Finely ground!" : logic.Score >= 0.5f ? "Coarse, but it'll do" : "Barely ground...");
        }

        private void Animate(float dt)
        {
            var side = logic != null ? logic.LastSide : GrindLogic.Side.None;
            float tilt = side == GrindLogic.Side.Left ? 4f : side == GrindLogic.Side.Right ? -4f : 0f;
            bowl.localRotation = Quaternion.Lerp(bowl.localRotation, Quaternion.Euler(0, 0, tilt), dt * 18f);
            paste.sizeDelta = Vector2.Lerp(pasteSmall, pasteBig, logic != null ? logic.Progress : 0f) * (1f + flash * 0.4f);
            if (guy != null) guy.anchoredPosition = guyHome + new Vector2(0f, flash * 70f);

            for (int i = splashes.Count - 1; i >= 0; i--)
            {
                var s = splashes[i];
                s.life -= dt;
                if (s.life <= 0f) { Destroy(s.rt.gameObject); splashes.RemoveAt(i); continue; }
                s.velocity += new Vector2(0f, -900f) * dt;
                s.rt.anchoredPosition += s.velocity * dt;
                var c = pasteColor;
                s.rt.GetComponent<Image>().color = new Color(c.r, c.g, c.b, Mathf.Clamp01(s.life / 0.3f));
                splashes[i] = s;
            }
        }

        private void Press(GrindLogic.Side side)
        {
            if (logic.Press(side))
            {
                status.text = "<color=#8cd973>Grind!</color>";
                flash = 0.2f;
                for (int i = 0; i < 3; i++)
                {
                    var drop = MinigameUI.Rect(left, "Splash", bowl.anchoredPosition + paste.anchoredPosition, Vector2.one * Random.Range(8f, 16f), pasteColor, MinigameUI.Circle());
                    splashes.Add((drop, new Vector2(Random.Range(-170f, 170f), Random.Range(220f, 380f)), 0.55f));
                }
            }
            else status.text = "<color=#f2665c>Alternate sides!</color>";
        }
    }
}
