/*
================================================================================
TEMP / BACKUP COPY (Commented out to prevent duplicate class conflicts in Unity)
================================================================================

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Nocturne.PotionMaking.Minigames
{
    // Timing stir: the spoon goes round the pot, press when it's in the green zone. Each good stir speeds it up.
    public class StirTimingMinigame_Backup : Minigame
    {
        [SerializeField] private Color liquidColor = new Color(0.45f, 0.75f, 0.55f);

        public override string Title => "Stir the Cauldron";
        public override string Instructions => "Press Space (or click) when the spoon is in the green zone.";

        private StirTimingLogic logic;
        private RectTransform pot, zone, spoon, quality;
        private Image liquid;
        private TMP_Text status;
        private TMP_Text inventoryLabel;
        private float flash;
        private const float Radius = 170f;

        private GameObject popupObj;

        protected override void Build()
        {
            logic = new StirTimingLogic();

            pot = MinigameUI.Rect(Root, "Pot", new Vector2(0, 10), new Vector2(440, 440), MinigameUI.Dark, MinigameUI.Circle());
            liquid = MinigameUI.Rect(pot, "Liquid", Vector2.zero, new Vector2(300, 300), liquidColor, MinigameUI.Circle()).GetComponent<Image>();
            MinigameUI.Rect(pot, "Track", Vector2.zero, new Vector2(Radius * 2 + 50, Radius * 2 + 50), new Color(1f, 1f, 1f, 0.12f), MinigameUI.Ring());

            zone = MinigameUI.Rect(pot, "Zone", Vector2.zero, new Vector2(Radius * 2 + 50, Radius * 2 + 50), MinigameUI.Good, MinigameUI.Ring());
            var zi = zone.GetComponent<Image>();
            zi.type = Image.Type.Filled;
            zi.fillMethod = Image.FillMethod.Radial360;
            zi.fillOrigin = (int)Image.Origin360.Right;
            zi.fillClockwise = false;

            spoon = MinigameUI.Rect(pot, "Spoon", new Vector2(Radius, 0), new Vector2(46, 46), MinigameUI.Wood, MinigameUI.Circle());
            status = MinigameUI.Label(Root, "", new Vector2(0, -235), 28, MinigameUI.Ink);
            MinigameUI.Label(Root, "Stir quality", new Vector2(-440, -275), 20, MinigameUI.Ink, 200);
            quality = MinigameUI.Bar(Root, "Quality", new Vector2(40, -275), new Vector2(760, 20), MinigameUI.Dark, MinigameUI.Gold);

            var inventoryBox = MinigameUI.Rect(Root, "CauldronContentsBox", new Vector2(-340, 80), new Vector2(200, 220), new Color(0.15f, 0.15f, 0.18f, 0.95f), null);
            var invTitle = MinigameUI.Label(inventoryBox, "Cauldron Mix", new Vector2(0, 80), 16, MinigameUI.Gold, 180);
            invTitle.alignment = TextAlignmentOptions.Center;
            
            inventoryLabel = MinigameUI.Label(inventoryBox, "• Moonpetal\n• Nightshade", new Vector2(0, -10), 14, MinigameUI.Ink, 180);
            inventoryLabel.alignment = TextAlignmentOptions.TopLeft;

            CreateHelpButton();
        }

        private void CreateHelpButton()
        {
            var btnObj = MinigameUI.Rect(Root, "HelpButton", new Vector2(380, 240), new Vector2(40, 40), MinigameUI.Dark, MinigameUI.Circle());
            
            var btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null) btnImage.raycastTarget = true;

            var button = btnObj.gameObject.AddComponent<Button>();
            
            var btnLabel = MinigameUI.Label(btnObj, "?", Vector2.zero, 24, MinigameUI.Gold);
            btnLabel.alignment = TextAlignmentOptions.Center;
            var textComp = btnLabel.GetComponent<TMP_Text>();
            if (textComp != null) textComp.raycastTarget = false;

            popupObj = MinigameUI.Rect(Root, "TutorialPopup", Vector2.zero, new Vector2(460, 220), new Color(0.12f, 0.12f, 0.15f, 0.98f), null).gameObject;
            popupObj.SetActive(false);

            var titleLabel = MinigameUI.Label(popupObj.transform, "How to Play", new Vector2(0, 75), 20, MinigameUI.Gold, 420);
            titleLabel.alignment = TextAlignmentOptions.Center;

            var contentLabel = MinigameUI.Label(popupObj.transform, "To play the cauldron game you need to click the spacebar in the green bar.", new Vector2(0, 5), 15, MinigameUI.Ink, 400);
            contentLabel.alignment = TextAlignmentOptions.Center;

            var footerLabel = MinigameUI.Label(popupObj.transform, "Press the <b>?</b> button or <b>/</b> key to go back.", new Vector2(0, -75), 13, new Color(0.7f, 0.7f, 0.7f), 400);
            footerLabel.alignment = TextAlignmentOptions.Center;

            button.onClick.AddListener(ToggleTutorialPopup);
        }

        private void ToggleTutorialPopup()
        {
            if (popupObj != null)
            {
                popupObj.SetActive(!popupObj.activeSelf);
            }
        }

        protected override void Step(float dt)
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (kb != null && kb.slashKey.wasPressedThisFrame)
            {
                ToggleTutorialPopup();
            }

            logic.Tick(dt);

            bool pressed = (kb != null && kb.spaceKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (pressed && !logic.Done && (popupObj == null || !popupObj.activeSelf))
            {
                bool hit = logic.Stir();
                status.text = hit ? "<color=#8cd973>Nice stir!</color>" : "<color=#f2665c>Splash!</color>";
                flash = 0.25f;
            }

            float a = logic.Angle * Mathf.Deg2Rad;
            spoon.anchoredPosition = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Radius;
            zone.localRotation = Quaternion.Euler(0, 0, logic.ZoneStart);
            zone.GetComponent<Image>().fillAmount = logic.ZoneSize / 360f;
            spoon.GetComponent<Image>().color = logic.InZone ? MinigameUI.Gold : MinigameUI.Wood;

            flash = Mathf.Max(0f, flash - dt);
            float s = 1f + flash * 0.3f;
            liquid.rectTransform.localScale = new Vector3(s, s, 1f);
            liquid.color = Color.Lerp(liquidColor, MinigameUI.Gold, logic.Score * 0.5f);
            quality.localScale = new Vector3(logic.Score, 1f, 1f);

            if (logic.Done)
                Finish(logic.Score, logic.Hits == logic.stirsNeeded ? "Perfectly stirred!" : $"{logic.Hits}/{logic.stirsNeeded} good stirs");
        }

        public void UpdateCauldronInventory(List<string> ingredientNames)
        {
            if (inventoryLabel != null)
            {
                inventoryLabel.text = ingredientNames.Count == 0 
                    ? "<color=#888>Empty</color>" 
                    : "• " + string.Join("\n• ", ingredientNames);
            }
        }
    }
}
================================================================================
*/