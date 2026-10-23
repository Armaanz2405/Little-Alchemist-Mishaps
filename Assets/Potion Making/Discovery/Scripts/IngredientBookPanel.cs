using System.Collections;
using System.Collections.Generic;
using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Discovery
{
    public class IngredientBookPanel : MonoBehaviour
    {
        [SerializeField] private DiscoveryLog log;
        [SerializeField] private IngredientCatalog catalog;
        [SerializeField] private Key toggleKey = Key.Tab;
        [SerializeField] private bool startOpen = false;

        private GameObject page;
        private RectTransform list;
        private TextMeshProUGUI toast;
        private readonly List<GameObject> rows = new List<GameObject>();
        private Coroutine toastRoutine;

        private static readonly Color Silhouette = new Color(0.12f, 0.1f, 0.14f);
        private readonly Dictionary<Sprite, Sprite> greyedCache = new Dictionary<Sprite, Sprite>();

        private void Awake()
        {
            if (log == null) log = FindFirstObjectByType<DiscoveryLog>();
            Build();
            page.SetActive(startOpen);
        }

        private void OnEnable() { if (log != null) log.onDiscovered.AddListener(OnDiscovered); }
        private void OnDisable() { if (log != null) log.onDiscovered.RemoveListener(OnDiscovered); }
        private void Start() => Refresh();

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb[toggleKey].wasPressedThisFrame)
            {
                page.SetActive(!page.activeSelf);
                if (page.activeSelf) Refresh();
            }
        }

        private void OnDiscovered(IngredientData ingredient)
        {
            Refresh();
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ShowToast($"New book entry: {ingredient.displayName}"));
        }

        private IEnumerator ShowToast(string message)
        {
            toast.text = message;
            toast.transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            toast.transform.parent.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            foreach (var row in rows) Destroy(row);
            rows.Clear();
            if (catalog == null) return;
            foreach (var entry in catalog.entries)
                if (entry != null && entry.ingredient != null) rows.Add(MakeRow(entry));
        }

        private GameObject MakeRow(IngredientCatalog.Entry entry)
        {
            bool known = log != null && log.IsDiscovered(entry.ingredient);

            var row = new GameObject(known ? entry.ingredient.displayName : "Unknown", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(list, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 16;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            var swatch = new GameObject("Swatch", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            swatch.transform.SetParent(row.transform, false);
            var image = swatch.GetComponent<Image>();
            Sprite art = entry.ingredient.sprite;
            if (art != null)
            {
                Sprite greyed = known ? null : Greyed(art);
                image.sprite = known ? art : greyed != null ? greyed : art;
                image.color = known || greyed != null ? Color.white : Silhouette;
                image.preserveAspect = true;
            }
            else
            {
                image.color = known ? entry.ingredient.color : Silhouette;
            }
            var le = swatch.GetComponent<LayoutElement>();
            le.preferredWidth = le.preferredHeight = art != null ? 72 : 56;

            string title = known ? $"<b>{entry.ingredient.displayName}</b>" : "<b>???</b>";
            string detail = known ? $"<color={UITheme.Hex(UITheme.Current.accentText)}>{IngredientCatalog.DescribeTags(entry.ingredient)}</color>" : $"<i>{entry.hint}</i>";
            var text = MakeText(row.transform, $"{title}\n<size=70%>{detail}</size>", 30, known ? UITheme.Current.text : UITheme.Current.mutedText);
            text.GetComponent<LayoutElement>().preferredWidth = 520;
            return row;
        }

        // flat grey silhouette of the art for undiscovered entries
        private Sprite Greyed(Sprite art)
        {
            if (greyedCache.TryGetValue(art, out var cached)) return cached;
            Sprite greyed = null;
            if (art.texture.isReadable)
            {
                int x = Mathf.FloorToInt(art.rect.x), y = Mathf.FloorToInt(art.rect.y);
                int w = Mathf.FloorToInt(art.rect.width), h = Mathf.FloorToInt(art.rect.height);
                var pixels = art.texture.GetPixels(x, y, w, h);
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color(UITheme.Current.silhouette.r, UITheme.Current.silhouette.g, UITheme.Current.silhouette.b, pixels[i].a);
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.SetPixels(pixels);
                tex.Apply();
                greyed = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), art.pixelsPerUnit);
            }
            greyedCache[art] = greyed;
            return greyed;
        }

        private void Build()
        {
            var canvasGo = UIStyle.MakeCanvas(transform, "IngredientBookCanvas", 60).gameObject;

            page = new GameObject("Page", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            page.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)page.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-40, 0);
            UIStyle.Box(page.GetComponent<Image>(), BoxKind.Panel);
            var v = page.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(36, 36, 28, 32);
            v.spacing = 14;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = v.childForceExpandHeight = false;
            var fit = page.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText(page.transform, "Ingredients", 42, UITheme.Current.text);
            UIStyle.Divider(page.transform, 14f);
            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            listGo.transform.SetParent(page.transform, false);
            UIStyle.Box(listGo.GetComponent<Image>(), BoxKind.Card);
            var lv = listGo.GetComponent<VerticalLayoutGroup>();
            lv.padding = new RectOffset(16, 16, 16, 16);
            lv.spacing = 12;
            lv.childControlWidth = lv.childControlHeight = true;
            lv.childForceExpandWidth = lv.childForceExpandHeight = false;
            list = (RectTransform)listGo.transform;
            MakeText(page.transform, "R: inspect an ingredient    Tab: open/close book", 22, UITheme.Current.mutedText);

            var toastBox = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            toastBox.transform.SetParent(canvasGo.transform, false);
            var trt = (RectTransform)toastBox.transform;
            trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = new Vector2(0, -50);
            UIStyle.Box(toastBox.GetComponent<Image>(), BoxKind.Panel, 1.6f);
            var th = toastBox.GetComponent<HorizontalLayoutGroup>();
            th.padding = new RectOffset(30, 30, 14, 16);
            th.childControlWidth = th.childControlHeight = true;
            var tfit = toastBox.GetComponent<ContentSizeFitter>();
            tfit.horizontalFit = tfit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            toast = MakeText(toastBox.transform, "", 32, UITheme.Current.text);
            toast.horizontalAlignment = HorizontalAlignmentOptions.Center;
            toast.verticalAlignment = VerticalAlignmentOptions.Geometry;
            toastBox.SetActive(false);
        }

        private static TextMeshProUGUI MakeText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = UITheme.Current.TextFont;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            return tmp;
        }
    }
}
