using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nocturne.PotionMaking.Minigames
{
    public class MinigameMenu : MonoBehaviour
    {
        [SerializeField] private StirMinigame stir;
        [SerializeField] private ChopMinigame chop;
        [SerializeField] private GrindMinigame grind;
        [SerializeField] private PourMinigame pour;

        private TextMeshProUGUI text;
        private readonly List<string> history = new List<string>();
        private Minigame current;

        private void Awake()
        {
            if (stir == null) stir = GetComponent<StirMinigame>();
            if (chop == null) chop = GetComponent<ChopMinigame>();
            if (grind == null) grind = GetComponent<GrindMinigame>();
            if (pour == null) pour = GetComponent<PourMinigame>();
            var canvas = MinigameUI.MakeCanvas(transform, "MenuCanvas", 10);
            text = MinigameUI.Text(canvas.transform, "", Vector2.zero, 30, MinigameUI.Ink, 1400);
            var rt = (RectTransform)text.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -30);
            rt.sizeDelta = new Vector2(1400, 400);
            text.alignment = TextAlignmentOptions.Top;
            Redraw();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (current != null && current.Running)
            {
                if (kb.escapeKey.wasPressedThisFrame) { current.Cancel(); current = null; Redraw(); }
                return;
            }
            if (kb.digit1Key.wasPressedThisFrame) Launch(stir);
            else if (kb.digit2Key.wasPressedThisFrame) Launch(chop);
            else if (kb.digit3Key.wasPressedThisFrame) Launch(grind);
            else if (kb.digit4Key.wasPressedThisFrame) Launch(pour);
        }

        private void Launch(Minigame game)
        {
            if (game == null) return;
            current = game;
            text.text = "";
            game.Begin(score =>
            {
                history.Insert(0, $"{game.Title}: {Mathf.RoundToInt(score * 100)}%");
                if (history.Count > 5) history.RemoveAt(history.Count - 1);
                current = null;
                Redraw();
            });
        }

        private void Redraw()
        {
            text.text = "<b>Potion Station Minigames</b> <size=70%>(placeholder art)</size>\n" +
                        "<size=80%>1 Stir   ·   2 Chop   ·   3 Grind   ·   4 Pour   ·   Esc: quit a minigame</size>\n\n" +
                        (history.Count > 0 ? "<size=70%>" + string.Join("\n", history) + "</size>" : "");
        }
    }
}
