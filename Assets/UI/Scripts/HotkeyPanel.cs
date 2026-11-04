using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.UI
{
    // menu keys under the energy bar
    public class HotkeyPanel : MonoBehaviour
    {
        [Serializable]
        public class Hotkey
        {
            public string key;
            public string label;

            public Hotkey(string key, string label)
            {
                this.key = key;
                this.label = label;
            }
        }

        [SerializeField] private Vector2 anchoredPosition = new Vector2(30f, -150f);
        [SerializeField] private string aboveName = "EnergyPanel";
        [SerializeField] private float gap = 28f;
        [SerializeField] private List<Hotkey> hotkeys = new List<Hotkey>
        {
            new Hotkey("Tab", "Book"),
            new Hotkey("B", "Shop"),
            new Hotkey("C", "Controls"),
        };

        private RectTransform panelRect;
        private RectTransform above;

        private void Awake() => Build();

        private void Start()
        {
            var found = GameObject.Find(aboveName);
            if (found != null) above = found.transform as RectTransform;
        }

        private void LateUpdate()
        {
            if (above == null) return;
            float bottom = above.anchoredPosition.y - above.rect.height;
            panelRect.anchoredPosition = new Vector2(above.anchoredPosition.x, bottom - gap);
        }

        private void Build()
        {
            var canvas = UIStyle.MakeCanvas(transform, "Hotkey Canvas", 50);

            var panel = new GameObject("Hotkey Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(canvas.transform, false);
            panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = anchoredPosition;
            UIStyle.Box(panel.GetComponent<Image>(), BoxKind.Panel, 1.6f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 14, 16);
            layout.spacing = 6;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var hotkey in hotkeys)
            {
                var row = new GameObject(hotkey.label, typeof(RectTransform), typeof(HorizontalLayoutGroup));
                row.transform.SetParent(panel.transform, false);
                var h = row.GetComponent<HorizontalLayoutGroup>();
                h.spacing = 10;
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = h.childControlHeight = true;
                h.childForceExpandWidth = h.childForceExpandHeight = false;
                UIStyle.Key(row.transform, hotkey.key, 32f);
                var label = UIStyle.MakeText(row.transform, hotkey.label, 24, TextKind.Body, "Label");
                label.verticalAlignment = TMPro.VerticalAlignmentOptions.Geometry;
                label.gameObject.AddComponent<LayoutElement>().minHeight = 32f;
            }
        }
    }
}
