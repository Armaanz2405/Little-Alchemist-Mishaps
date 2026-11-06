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
