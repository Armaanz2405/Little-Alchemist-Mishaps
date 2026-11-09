using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    public static class MinigameUI
    {
        // props and boards keep to the lab's palette: dark stone, old wood, ember light
        public static readonly Color Wood = new Color(0.45f, 0.31f, 0.2f);
        public static readonly Color Dark = new Color(0.1f, 0.08f, 0.12f);
        public static readonly Color Board = new Color(0.17f, 0.14f, 0.15f);
        public static readonly Color Stone = new Color(0.24f, 0.23f, 0.26f);
        public static readonly Color Ember = new Color(0.98f, 0.6f, 0.22f);
        public static Color Panel => UITheme.Current.panelFill;
        public static Color Good => UITheme.Current.goodText;
        public static Color Bad => UITheme.Current.badText;
        public static Color Gold => UITheme.Current.goldText;
        public static Color Ink => UITheme.Current.text;

        private static Sprite circle, ring;

        public static Canvas MakeCanvas(Transform parent, string name, int order) => UIStyle.MakeCanvas(parent, name, order);

        public static RectTransform Rect(Transform parent, string name, Vector2 pos, Vector2 size, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            return rt;
        }

        public static RectTransform Bar(Transform parent, string name, Vector2 pos, Vector2 size, Color back, Color fill)
        {
            var bg = Rect(parent, name, pos, size, back);
            var f = Rect(bg, "Fill", Vector2.zero, size, fill);
            f.anchorMin = f.anchorMax = f.pivot = new Vector2(0f, 0.5f);
            f.anchoredPosition = Vector2.zero;
            f.localScale = new Vector3(0f, 1f, 1f);
            return f;
        }

        public static TMP_Text Label(Transform parent, string text, Vector2 pos, int size, Color color, float width = 1000f)
        {
            var t = UIStyle.MakeText(parent, text, size, TextKind.Body, "Label");
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, size * 2.5f);
            t.color = color;
            t.horizontalAlignment = HorizontalAlignmentOptions.Center;
            t.verticalAlignment = VerticalAlignmentOptions.Geometry;
            return t;
        }

        public static Image Picture(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite)
        {
            var img = Rect(parent, name, pos, size, Color.white, sprite).GetComponent<Image>();
            img.preserveAspect = true;
            return img;
        }

        // a themed box that isn't the whole panel: the mortar's drawer, an ingredient slot
        public static RectTransform Card(Transform parent, string name, Vector2 pos, Vector2 size, float scale = 1f)
        {
            var rt = Rect(parent, name, pos, size, Color.white);
            UIStyle.Box(rt.GetComponent<Image>(), BoxKind.Card, scale);
            return rt;
        }

        // children line up left to right and the row sizes itself around them
        public static RectTransform Row(Transform parent, string name, Vector2 pos, float spacing = 10f, float pivotX = 0.5f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(pivotX, 0.5f);
            rt.anchoredPosition = pos;
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fit = go.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        public static TMP_Text RowText(Transform row, string text, int size, Color color)
        {
            var t = UIStyle.MakeText(row, text, size, TextKind.Body, "Text");
            t.color = color;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.verticalAlignment = VerticalAlignmentOptions.Geometry;
            return t;
        }

        // a tag as a little coloured label, for rows
        public static RectTransform Chip(Transform row, string text, Color fill, int size = 16)
        {
            var go = new GameObject("Chip", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(row, false);
            var img = go.GetComponent<Image>();
            img.color = fill;
            img.raycastTarget = false;
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(9, 9, 3, 3);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            RowText(go.transform, text, size, Dark);
            return (RectTransform)go.transform;
        }

        public enum Icon { Check, Cross, Pencil }

        // the confirm / edit / exit boxes from the UI sketches, drawn from bars so they don't depend on a font having the glyphs
        public static RectTransform IconBox(Transform parent, Icon icon, Vector2 pos, float size, Color color, bool inRow = false)
        {
            var box = Card(parent, icon + " Box", pos, new Vector2(size, size), 2f);
            if (inRow)
            {
                var le = box.gameObject.AddComponent<LayoutElement>();
                le.minWidth = le.preferredWidth = le.minHeight = le.preferredHeight = size;
            }
            float t = size * 0.11f;
            switch (icon)
            {
                case Icon.Check:
                    Stroke(box, new Vector2(-size * 0.17f, -size * 0.08f), new Vector2(size * 0.28f, t), -45f, color);
                    Stroke(box, new Vector2(size * 0.09f, size * 0.02f), new Vector2(size * 0.52f, t), 50f, color);
                    break;
                case Icon.Cross:
                    Stroke(box, Vector2.zero, new Vector2(size * 0.58f, t), 45f, color);
                    Stroke(box, Vector2.zero, new Vector2(size * 0.58f, t), -45f, color);
                    break;
                default:
                    Stroke(box, new Vector2(size * 0.04f, size * 0.04f), new Vector2(size * 0.46f, t * 1.3f), 45f, color);
                    Stroke(box, new Vector2(-size * 0.18f, -size * 0.18f), new Vector2(t * 1.1f, t * 1.1f), 45f, Dark);
                    break;
            }
            return box;
        }

        // is a click at this screen position on the box? The minigame canvases are screen space overlay.
        public static bool Contains(RectTransform rt, Vector2 screen) =>
            rt != null && rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, screen, null);

        // rows size themselves a frame late unless asked, which shows as a flicker when a screen is rebuilt
        public static void Settle(RectTransform rt) => LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        private static void Stroke(Transform parent, Vector2 pos, Vector2 size, float angle, Color color) =>
            Rect(parent, "Stroke", pos, size, color).localRotation = Quaternion.Euler(0f, 0f, angle);

        public static Sprite Circle() => circle != null ? circle : circle = MakeDisc(0f);
        public static Sprite Ring() => ring != null ? ring : ring = MakeDisc(0.78f);

        private static Sprite MakeDisc(float hole)
        {
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            float r = n / 2f - 1f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                    float a = Mathf.Clamp01(r - d) * Mathf.Clamp01(d - r * hole);
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
