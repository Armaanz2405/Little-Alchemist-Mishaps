using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Minigames
{
    public static class MinigameUI
    {
        public static readonly Color Panel = new Color(0.16f, 0.12f, 0.2f, 0.96f);
        public static readonly Color Wood = new Color(0.55f, 0.38f, 0.24f);
        public static readonly Color Dark = new Color(0.1f, 0.08f, 0.12f);
        public static readonly Color Good = new Color(0.55f, 0.85f, 0.45f);
        public static readonly Color Bad = new Color(0.95f, 0.4f, 0.3f);
        public static readonly Color Gold = new Color(0.98f, 0.82f, 0.35f);
        public static readonly Color Ink = new Color(0.95f, 0.92f, 0.85f);

        private static Sprite circle, ring;
        private static Font font;

        public static Canvas MakeCanvas(Transform parent, string name, int order)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler));
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

        public static Text Label(Transform parent, string text, Vector2 pos, int size, Color color, float width = 1000f)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, size * 2.5f);
            var t = go.GetComponent<Text>();
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.supportRichText = true;
            t.raycastTarget = false;
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
