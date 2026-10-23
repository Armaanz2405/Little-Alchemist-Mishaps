using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.UI
{
    public struct PromptLine
    {
        public string key;
        public string text;
        public string cost;

        public PromptLine(string key, string text, string cost = null)
        {
            this.key = key;
            this.text = text;
            this.cost = cost;
        }
    }

    // what a prompt box shows aka optional header and counter plus key lines
    public class PromptContent
    {
        public string header;
        public string counter;
        public readonly List<PromptLine> lines = new List<PromptLine>();

        public bool IsEmpty => lines.Count == 0 && string.IsNullOrEmpty(header) && string.IsNullOrEmpty(counter);

        public void Clear()
        {
            header = null;
            counter = null;
            lines.Clear();
        }

        public void CopyFrom(PromptContent other)
        {
            header = other.header;
            counter = other.counter;
            lines.Clear();
            lines.AddRange(other.lines);
        }

        public string Signature()
        {
            var sb = new StringBuilder();
            sb.Append(header).Append('|').Append(counter);
            foreach (var line in lines) sb.Append('|').Append(line.key).Append(':').Append(line.text).Append(':').Append(line.cost);
            return sb.ToString();
        }
    }

    [DefaultExecutionOrder(1000)]
    public class PromptBox : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private float riseDistance = 14f;
        [SerializeField] private float bobHeight = 3f;
        [SerializeField] private float bobSpeed = 2.2f;

        private RectTransform canvasRect;
        private RectTransform box;
        private CanvasGroup group;
        private GameObject headerRow;
        private TextMeshProUGUI headerText;
        private TextMeshProUGUI counterText;
        private GameObject divider;
        private readonly List<(GameObject row, TextMeshProUGUI key, TextMeshProUGUI label, TextMeshProUGUI cost)> rows = new List<(GameObject, TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI)>();
        private string shown;
        private bool visible;
        private float alpha;
        private Vector3 anchor;

        public static PromptBox Create(string name = "Prompt Box")
        {
            var box = new GameObject(name).AddComponent<PromptBox>();
            box.Build();
            return box;
        }

        public void Show(PromptContent content, Vector3 worldAnchor)
        {
            if (content == null || content.IsEmpty)
            {
                Hide();
                return;
            }
            anchor = worldAnchor;
            string signature = content.Signature();
            if (signature != shown)
            {
                Fill(content);
                shown = signature;
            }
            if (!visible && alpha <= 0f) Place(worldAnchor, 0f);
            visible = true;
        }

        public void Hide() => visible = false;

        private void LateUpdate()
        {
            alpha = Mathf.MoveTowards(alpha, visible ? 1f : 0f, Time.unscaledDeltaTime * fadeSpeed);
            group.alpha = alpha;
            if (alpha > 0f) Place(anchor, (1f - alpha) * -riseDistance + Mathf.Sin(Time.unscaledTime * bobSpeed) * bobHeight);
        }

        private void Place(Vector3 worldPoint, float yOffset)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector3 screen = cam.WorldToScreenPoint(worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local);
            box.anchoredPosition = local + new Vector2(0f, yOffset);
        }

        private void Fill(PromptContent content)
        {
            bool hasHeader = !string.IsNullOrEmpty(content.header) || !string.IsNullOrEmpty(content.counter);
            headerRow.SetActive(hasHeader);
            headerText.text = content.header ?? "";
            counterText.text = content.counter ?? "";
            counterText.gameObject.SetActive(!string.IsNullOrEmpty(content.counter));
            divider.SetActive(hasHeader && content.lines.Count > 0);

            while (rows.Count < content.lines.Count) rows.Add(MakeRow());
            for (int i = 0; i < rows.Count; i++)
            {
                bool used = i < content.lines.Count;
                rows[i].row.SetActive(used);
                if (!used) continue;
                UIStyle.SetKey(rows[i].key, content.lines[i].key);
                rows[i].label.text = content.lines[i].text;
                rows[i].cost.text = content.lines[i].cost ?? "";
                rows[i].cost.gameObject.SetActive(!string.IsNullOrEmpty(content.lines[i].cost));
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(box);
        }

        private void Build()
        {
            var canvas = UIStyle.MakeCanvas(transform, "Prompt Canvas", 30);
            canvasRect = (RectTransform)canvas.transform;

            var go = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
            go.transform.SetParent(canvas.transform, false);
            box = (RectTransform)go.transform;
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
            box.pivot = new Vector2(0.5f, 0f);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            UIStyle.Box(image, BoxKind.Panel, 1.6f);
            group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 14, 16);
            layout.spacing = 6;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fit = go.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            go.GetComponent<LayoutElement>().minWidth = 150f;

            headerRow = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            headerRow.transform.SetParent(box, false);
            var h = headerRow.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 24;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            headerText = UIStyle.MakeText(headerRow.transform, "", 24, TextKind.Accent, "Title");
            headerText.verticalAlignment = VerticalAlignmentOptions.Geometry;
            var titleLayout = headerText.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1f;
            titleLayout.minHeight = 30f;
            counterText = UIStyle.MakeText(headerRow.transform, "", 24, TextKind.Gold, "Counter");
            counterText.horizontalAlignment = HorizontalAlignmentOptions.Right;
            counterText.verticalAlignment = VerticalAlignmentOptions.Geometry;
            counterText.gameObject.AddComponent<LayoutElement>().minHeight = 30f;

            divider = UIStyle.Divider(box, 10f).gameObject;

            MakeNub();
        }

        private (GameObject, TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI) MakeRow()
        {
            var row = new GameObject("Line", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(box, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 12;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            var key = UIStyle.Key(row.transform, "", 38f);
            var label = UIStyle.MakeText(row.transform, "", 27, TextKind.Body, "Label");
            // same height as the key so the text lines up with it
            label.verticalAlignment = VerticalAlignmentOptions.Geometry;
            label.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            var cost = UIStyle.MakeText(row.transform, "", 24, TextKind.Gold, "Cost");
            cost.verticalAlignment = VerticalAlignmentOptions.Geometry;
            cost.gameObject.AddComponent<LayoutElement>().minHeight = 38f;
            return (row, key, label, cost);
        }

        private void MakeNub()
        {
            var t = UITheme.Current;
            if (t.nubFillSprite == null) return;
            var nub = new GameObject("Pointer", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            nub.transform.SetParent(box, false);
            nub.GetComponent<LayoutElement>().ignoreLayout = true;
            var rt = (RectTransform)nub.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, 3f);
            rt.sizeDelta = new Vector2(26f, 14f);
            var fill = nub.GetComponent<Image>();
            fill.sprite = t.nubFillSprite;
            fill.color = t.panelFill;
            fill.raycastTarget = false;
            if (t.nubFrameSprite == null) return;
            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(nub.transform, false);
            var frt = (RectTransform)frame.transform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = frt.offsetMax = Vector2.zero;
            var img = frame.GetComponent<Image>();
            img.sprite = t.nubFrameSprite;
            img.color = t.panelFrame;
            img.raycastTarget = false;
        }
    }
}
