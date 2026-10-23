using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nocturne.UI
{
    // how to play and controls screen shown at the start, the game stays paused until Start is pressed
    public class ControlsScreen : MonoBehaviour
    {
        [Serializable]
        public class Control
        {
            public string action;
            public string keyboard;
            public string controller;

            public Control(string action, string keyboard, string controller)
            {
                this.action = action;
                this.keyboard = keyboard;
                this.controller = controller;
            }
        }

        [SerializeField] private bool showOnStart = true;
        [SerializeField] private Key openKey = Key.C;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private List<Control> controls = new List<Control>
        {
            new Control("Move", "WASD/Arrows", "Left Stick/D-pad"),
            new Control("Jump", "Space", "A"),
            new Control("Interact", "E", "B"),
            new Control("Throw", "Q", "Y"),
            new Control("Dash", "Shift", "X"),
            new Control("Inspect", "R", ""),
            new Control("Book", "Tab", ""),
            new Control("Shop", "B", "Select"),
        };
        [SerializeField, TextArea(8, 16)] private string howToPlay =
            "Start by talking to your customer to hear what they need.\n\n" +
            "Ingredients are scattered across the shelves of your apothecary, so climb the ladders and pick up what you can find. Inspect anything new to learn its tags, then check your book whenever you forget.\n\n" +
            "Most ingredients have a primary tag and a secondary tag, so you'll often need to refine them first. Chopping an ingredient on the cutting board cuts out its primary tag, and the mortar packs three ingredients together into one that keeps all of their tags.\n\n" +
            "When you're ready, drop two ingredients into the cauldron and stir to brew. Hand the potion to your customer, who wants a primary and secondary tag.\n\n" +
            "Inspecting, refining, brewing and asking questions all use energy. Running short on something? The shop sells quality ingredients that only carry their primary tag.";

        private GameObject canvasObject;
        private TextMeshProUGUI title;
        private GameObject howToPage;
        private Button nextButton;
        private GameObject choosePage;
        private GameObject listPage;
        private RectTransform list;
        private TextMeshProUGUI listTitle;
        private Button keyboardButton;
        private Button startButton;
        private float previousTimeScale = 1f;

        public bool IsOpen => canvasObject != null && canvasObject.activeSelf;

        private void Update()
        {
            var kb = Keyboard.current;
            // not while talking or in another menu
            bool busy = playerInput != null && !playerInput.inputIsActive;
            if (kb != null && kb[openKey].wasPressedThisFrame && !IsOpen && !busy) Open();
        }

        private void Start()
        {
            Build();
            if (showOnStart) Open();
            else canvasObject.SetActive(false);
        }

        public void Open()
        {
            if (IsOpen && Time.timeScale == 0f) return;
            canvasObject.SetActive(true);
            previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            if (playerInput != null) playerInput.DeactivateInput();
            if (!string.IsNullOrWhiteSpace(howToPlay)) ShowHowTo();
            else ShowChoose();
        }

        public void Close()
        {
            canvasObject.SetActive(false);
            Time.timeScale = previousTimeScale;
            if (playerInput != null) playerInput.ActivateInput();
        }

        private void ShowHowTo()
        {
            title.text = "How to Play";
            howToPage.SetActive(true);
            choosePage.SetActive(false);
            listPage.SetActive(false);
            Select(nextButton);
        }

        private void ShowChoose()
        {
            title.text = "Controls";
            howToPage.SetActive(false);
            choosePage.SetActive(true);
            listPage.SetActive(false);
            Select(keyboardButton);
        }

        private void ShowList(bool controller)
        {
            choosePage.SetActive(false);
            listPage.SetActive(true);
            listTitle.text = controller ? "Controller" : "Keyboard";
            for (int i = list.childCount - 1; i >= 0; i--) Destroy(list.GetChild(i).gameObject);
            foreach (var control in controls)
            {
                string keys = controller ? control.controller : control.keyboard;
                if (!string.IsNullOrWhiteSpace(keys)) MakeRow(control.action, keys);
            }
            Select(startButton);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private void Build()
        {
            var canvas = UIStyle.MakeCanvas(transform, "Controls Canvas", 90);
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            canvasObject = canvas.gameObject;

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(canvas.transform, false);
            var drt = (RectTransform)dim.transform;
            drt.anchorMin = Vector2.zero;
            drt.anchorMax = Vector2.one;
            drt.offsetMin = drt.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(canvas.transform, false);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            UIStyle.Box(panel.GetComponent<Image>(), BoxKind.Panel);
            var v = panel.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(56, 56, 40, 44);
            v.spacing = 18;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = v.childForceExpandHeight = false;
            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            title = UIStyle.MakeText(panel.transform, "Controls", 54, TextKind.Title, "Title");
            title.horizontalAlignment = HorizontalAlignmentOptions.Center;
            UIStyle.Divider(panel.transform, 14f).GetComponent<LayoutElement>().minWidth = 520f;

            howToPage = Page(panel.transform, "How to Play");
            var description = UIStyle.MakeText(howToPage.transform, howToPlay, 25, TextKind.Body, "Description");
            description.horizontalAlignment = HorizontalAlignmentOptions.Center;
            description.gameObject.AddComponent<LayoutElement>().preferredWidth = 900f;
            nextButton = UIStyle.MakeButton(howToPage.transform, "Next");
            nextButton.onClick.AddListener(ShowChoose);

            choosePage = Page(panel.transform, "Choose");
            var question = UIStyle.MakeText(choosePage.transform, "Which are you using?", 30, TextKind.Muted, "Question");
            question.horizontalAlignment = HorizontalAlignmentOptions.Center;
            var choices = Row(choosePage.transform, "Choices", 28);
            keyboardButton = UIStyle.MakeButton(choices, "Keyboard");
            keyboardButton.onClick.AddListener(() => ShowList(false));
            var controllerButton = UIStyle.MakeButton(choices, "Controller");
            controllerButton.onClick.AddListener(() => ShowList(true));

            listPage = Page(panel.transform, "Controls");
            listTitle = UIStyle.MakeText(listPage.transform, "", 30, TextKind.Accent, "Device");
            listTitle.horizontalAlignment = HorizontalAlignmentOptions.Center;
            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listGo.transform.SetParent(listPage.transform, false);
            var lv = listGo.GetComponent<VerticalLayoutGroup>();
            lv.spacing = 10;
            lv.childControlWidth = lv.childControlHeight = true;
            lv.childForceExpandWidth = lv.childForceExpandHeight = false;
            list = (RectTransform)listGo.transform;
            var buttons = Row(listPage.transform, "Buttons", 24);
            var back = UIStyle.MakeButton(buttons, "Back", 160f);
            back.onClick.AddListener(ShowChoose);
            startButton = UIStyle.MakeButton(buttons, "Start");
            startButton.onClick.AddListener(Close);
        }

        private void MakeRow(string action, string keys)
        {
            var row = Row(list, action, 14);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            var keyColumn = Row(row, "Keys", 8);
            var keyLayout = keyColumn.GetComponent<HorizontalLayoutGroup>();
            keyLayout.childAlignment = TextAnchor.MiddleRight;
            keyColumn.gameObject.AddComponent<LayoutElement>().minWidth = 280f;
            string[] options = keys.Split('/');
            for (int i = 0; i < options.Length; i++)
            {
                if (i > 0) UIStyle.MakeText(keyColumn, "or", 22, TextKind.Muted, "Or").verticalAlignment = VerticalAlignmentOptions.Geometry;
                UIStyle.Key(keyColumn, options[i].Trim(), 42f);
            }
            var label = UIStyle.MakeText(row, action, 28, TextKind.Body, "Action");
            label.verticalAlignment = VerticalAlignmentOptions.Geometry;
            label.gameObject.AddComponent<LayoutElement>().minHeight = 42f;
        }

        private static GameObject Page(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var v = go.GetComponent<VerticalLayoutGroup>();
            v.spacing = 22;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = v.childForceExpandHeight = false;
            return go;
        }

        private static RectTransform Row(Transform parent, string name, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var h = go.GetComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            return (RectTransform)go.transform;
        }
    }
}
