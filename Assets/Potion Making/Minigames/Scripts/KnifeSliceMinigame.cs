using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    // Fruit Ninja style knife game: hold the mouse and swipe through the pieces, skip the rotten ones.
    public class KnifeSliceMinigame : Minigame
    {
        [SerializeField] private Color pieceColor = new Color(0.85f, 0.45f, 0.55f);
        [SerializeField] private Color rottenColor = new Color(0.25f, 0.3f, 0.2f);

        public override string Title => "Chop It!";
        public override string Instructions => "Hold the mouse button and swipe through the pieces. Don't cut the rotten ones!";

        public void SetIngredientColor(Color color) => pieceColor = color;

        private SliceLogic logic;
        private RectTransform board;
        private Text counter;
        private readonly Dictionary<SliceLogic.Piece, RectTransform> views = new Dictionary<SliceLogic.Piece, RectTransform>();
        private readonly List<(RectTransform rt, Vector2 vel, float life)> halves = new List<(RectTransform, Vector2, float)>();
        private readonly List<(Image img, float life)> trail = new List<(Image, float)>();
        private Vector2? lastMouse;

        protected override void Build()
        {
            logic = new SliceLogic();
            views.Clear();
            halves.Clear();
            trail.Clear();
            lastMouse = null;
            board = MinigameUI.Rect(Root, "Board", new Vector2(0, -20), new Vector2(1000, 560), new Color(0.3f, 0.22f, 0.16f));
            board.gameObject.AddComponent<RectMask2D>();
            counter = MinigameUI.Label(Root, "", new Vector2(0, -330), 26, MinigameUI.Ink);
        }

        protected override void Step(float dt)
        {
            logic.Tick(dt);
            HandleMouse();
            Draw(dt);
            counter.text = $"Cut: {logic.GoodSliced}/{logic.goodPieces}" + (logic.RottenSliced > 0 ? $"   <color=#f2665c>Rotten: {logic.RottenSliced}</color>" : "");

            if (logic.Done)
            {
                counter.text = "";
                string result = logic.RottenSliced == 0 && logic.GoodSliced == logic.goodPieces ? "Perfect chop!" : logic.Score >= 0.5f ? "Nicely chopped" : "A bit messy...";
                Finish(logic.Score, result);
            }
        }

        private void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed) { lastMouse = null; return; }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, mouse.position.ReadValue(), null, out Vector2 local);
            local += board.anchoredPosition;
            if (lastMouse.HasValue && (local - lastMouse.Value).sqrMagnitude > 4f)
            {
                logic.Swipe(lastMouse.Value, local);
                AddTrail(lastMouse.Value, local);
            }
            lastMouse = local;
        }

        private void Draw(float dt)
        {
            foreach (var p in logic.pieces)
            {
                if (!views.TryGetValue(p, out var rt))
                {
                    rt = MinigameUI.Rect(board, p.rotten ? "Rotten" : "Piece", Vector2.zero, new Vector2(100, 100), p.rotten ? rottenColor : pieceColor, MinigameUI.Circle());
                    if (p.rotten) MinigameUI.Label(rt, "x", Vector2.zero, 48, new Color(0.6f, 0.7f, 0.4f), 100);
                    views[p] = rt;
                }
                if (p.sliced && rt.gameObject.activeSelf)
                {
                    rt.gameObject.SetActive(false);
                    SpawnHalves(p);
                }
                rt.anchoredPosition = p.pos - board.anchoredPosition;
                if (p.gone) rt.gameObject.SetActive(false);
            }

            for (int i = halves.Count - 1; i >= 0; i--)
            {
                var h = halves[i];
                h.vel.y -= 900f * dt;
                h.rt.anchoredPosition += h.vel * dt;
                h.life -= dt;
                if (h.life <= 0f) { Destroy(h.rt.gameObject); halves.RemoveAt(i); }
                else halves[i] = h;
            }

            for (int i = trail.Count - 1; i >= 0; i--)
            {
                var t = trail[i];
                t.life -= dt;
                if (t.life <= 0f) { Destroy(t.img.gameObject); trail.RemoveAt(i); continue; }
                t.img.color = new Color(1f, 1f, 1f, t.life / 0.15f * 0.8f);
                trail[i] = t;
            }
        }

        private void SpawnHalves(SliceLogic.Piece p)
        {
            Color c = p.rotten ? rottenColor : pieceColor;
            for (int side = -1; side <= 1; side += 2)
            {
                var half = MinigameUI.Rect(board, "Half", p.pos - board.anchoredPosition + new Vector2(side * 20, 0), new Vector2(50, 95), c, MinigameUI.Circle());
                halves.Add((half, new Vector2(side * 220f, 150f), 0.8f));
            }
        }

        private void AddTrail(Vector2 a, Vector2 b)
        {
            Vector2 mid = (a + b) / 2f - board.anchoredPosition;
            var rt = MinigameUI.Rect(board, "Trail", mid, new Vector2((b - a).magnitude, 8f), Color.white);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            trail.Add((rt.GetComponent<Image>(), 0.15f));
        }
    }
}
