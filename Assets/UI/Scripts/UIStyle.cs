using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.UI
{
    public enum BoxKind { Panel, Card }
    public enum TextKind { Body, Title, Muted, Accent, Gold }

    public static class UIStyle
    {
        private const string LayerPrefix = "Theme ";

        public static bool IsLayer(Component c) => c.name.StartsWith(LayerPrefix);

        public static void Box(Image image, BoxKind kind, float scale = 1f)
        {
            if (image == null) return;
            var t = UITheme.Current;
            bool panel = kind == BoxKind.Panel;
            Sliced(image, panel ? t.panelFillSprite : t.cardFillSprite, panel ? t.panelFill : t.cardFill, scale);
            Layer(image.transform, "Frame", panel ? t.panelFrameSprite : t.cardFrameSprite, panel ? t.panelFrame : t.cardFrame, 0f, scale);
            if (!panel) return;
            Layer(image.transform, "Flourish", t.panelFlourishSprite, t.panelFlourish, 0f, scale);
            Layer(image.transform, "Glow", t.panelGlowSprite, t.panelGlow, t.glowSize / scale, scale);
        }

        public static void Button(Button button, bool tab = false)
        {
            if (button == null || !(button.targetGraphic is Image image)) return;
            var t = UITheme.Current;
            Sliced(image, tab ? t.tabFillSprite : t.buttonFillSprite, Color.white, 1f);
            Layer(image.transform, "Frame", tab ? t.tabFrameSprite : t.buttonFrameSprite, t.buttonFrame, 0f, 1f);
            var colors = button.colors;
            colors.normalColor = t.buttonFill;
            colors.highlightedColor = t.buttonHover;
            colors.selectedColor = t.buttonHover;
            colors.pressedColor = t.buttonPressed;
            colors.disabledColor = t.buttonDisabled;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
            foreach (var label in button.GetComponentsInChildren<TMP_Text>(true)) Text(label, TextKind.Body);
        }

        public static void Text(TMP_Text text, TextKind kind, bool recolor = true)
        {
            if (text == null) return;
            var font = UITheme.Current.TextFont;
            if (font != null) text.font = font;
            if (text.verticalAlignment == VerticalAlignmentOptions.Middle) text.verticalAlignment = VerticalAlignmentOptions.Geometry;
            if (recolor) text.color = TextColor(kind);
        }

        public static Color TextColor(TextKind kind)
        {
            var t = UITheme.Current;
            switch (kind)
            {
                case TextKind.Title: return t.text;
                case TextKind.Muted: return t.mutedText;
                case TextKind.Accent: return t.accentText;
                case TextKind.Gold: return t.goldText;
                default: return t.text;
            }
        }

        public static TextMeshProUGUI MakeText(Transform parent, string text, float size, TextKind kind, string name = "Text")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.richText = true;
            tmp.raycastTarget = false;
            Text(tmp, kind);
            return tmp;
        }

        // a key cap with a letter on it
        public static TextMeshProUGUI Key(Transform parent, string label, float size = 36f)
        {
            var t = UITheme.Current;
            var go = new GameObject("Key", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            Sliced(image, t.keyFillSprite, t.keyFill, 1.6f);
            image.raycastTarget = false;
            Layer(go.transform, "Frame", t.keyFrameSprite, t.keyFrame, 0f, 1.6f);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = size;
            var letter = MakeText(go.transform, "", size * 0.6f, TextKind.Body, "Letter");
            letter.color = t.keyText;
            letter.horizontalAlignment = HorizontalAlignmentOptions.Center;
            letter.verticalAlignment = VerticalAlignmentOptions.Geometry;
            var rt = letter.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0f, size * 0.08f);
            rt.offsetMax = Vector2.zero;
            SetKey(letter, label);
            return letter;
        }

        // changes a key's label and widens the key for words like "Space"
        public static void SetKey(TMP_Text letter, string label)
        {
            letter.text = label;
            var le = letter.transform.parent.GetComponent<LayoutElement>();
            if (le == null) return;
            float size = le.preferredHeight;
            float width = string.IsNullOrEmpty(label) || label.Length <= 1 ? size : letter.GetPreferredValues(label).x + size * 0.7f;
            le.minWidth = le.preferredWidth = Mathf.Max(size, width);
        }

        // whole-number slider
        public static Slider Slider(Transform parent, float min, float max, float value, float width = 260f, float height = 26f)
        {
            var t = UITheme.Current;
            var go = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = width;
            le.minHeight = le.preferredHeight = height;

            var track = Child(go.transform, "Track", new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), Vector2.zero, Vector2.zero);
            var trackImage = track.gameObject.AddComponent<Image>();
            Box(trackImage, BoxKind.Card, 3f);
            trackImage.raycastTarget = true;

            var fillArea = Child(go.transform, "Fill Area", new Vector2(0f, 0.3f), new Vector2(1f, 0.7f), new Vector2(3f, 0f), new Vector2(-3f, 0f));
            var fill = Child(fillArea, "Fill", Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            var fillImage = fill.gameObject.AddComponent<Image>();
            Sliced(fillImage, t.cardFillSprite, t.pipFull, 3f);
            fillImage.raycastTarget = false;

            var handleArea = Child(go.transform, "Handle Area", Vector2.zero, Vector2.one, new Vector2(height * 0.4f, 0f), new Vector2(-height * 0.4f, 0f));
            var handle = Child(handleArea, "Handle", Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            handle.sizeDelta = new Vector2(height * 0.8f, 0f);
            var handleImage = handle.gameObject.AddComponent<Image>();
            Sliced(handleImage, t.keyFillSprite, Color.white, 2.4f);
            Layer(handle, "Frame", t.keyFrameSprite, t.keyFrame, 0f, 2.4f);

            var slider = go.GetComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.wholeNumbers = true;
            slider.minValue = min;
            slider.maxValue = max;
            slider.SetValueWithoutNotify(value);
            var colors = slider.colors;
            colors.normalColor = t.keyFill;
            colors.highlightedColor = t.keyFrame;
            colors.selectedColor = t.keyFrame;
            colors.pressedColor = t.buttonPressed;
            colors.colorMultiplier = 1f;
            slider.colors = colors;
            return slider;
        }

        private static RectTransform Child(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        // a themed button with a text label, for UI built in code
        public static Button MakeButton(Transform parent, string label, float width = 220f, float height = 60f, float textSize = 28f)
        {
            var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = width;
            le.minHeight = le.preferredHeight = height;
            var text = MakeText(go.transform, label, textSize, TextKind.Body, "Label");
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Geometry;
            var rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var button = go.GetComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            Button(button);
            return button;
        }

        // thin line that fades at the ends with a star in the middle
        public static RectTransform Divider(Transform parent, float height = 12f)
        {
            var t = UITheme.Current;
            var go = new GameObject("Divider", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var line = go.GetComponent<Image>();
            Sliced(line, t.dividerSprite, t.panelFrame, 1f);
            line.raycastTarget = false;
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = height;
            le.flexibleWidth = 1f;
            if (t.starSprite != null)
            {
                var star = new GameObject("Star", typeof(RectTransform), typeof(Image));
                star.transform.SetParent(go.transform, false);
                var srt = (RectTransform)star.transform;
                srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 0.5f);
                srt.sizeDelta = new Vector2(height * 1.3f, height * 1.3f);
                var img = star.GetComponent<Image>();
                img.sprite = t.starSprite;
                img.color = t.panelFlourish;
                img.raycastTarget = false;
            }
            return (RectTransform)go.transform;
        }

        public static Canvas MakeCanvas(Transform parent, string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(ThemeIgnore));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void Sliced(Image image, Sprite sprite, Color color, float scale)
        {
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = scale;
            }
            image.color = color;
        }

        private static void Layer(Transform parent, string name, Sprite sprite, Color color, float outset, float scale)
        {
            if (sprite == null) return;
            string full = LayerPrefix + name;
            var existing = parent.Find(full);
            var go = existing != null ? existing.gameObject : new GameObject(full, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.transform.SetAsFirstSibling();
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = -Vector2.one * outset;
            rt.offsetMax = Vector2.one * outset;
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var image = go.GetComponent<Image>();
            Sliced(image, sprite, color, scale);
            image.raycastTarget = false;
        }
    }
}
