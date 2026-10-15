using System.Collections;
using System.Collections.Generic;
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
            toast.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            toast.gameObject.SetActive(false);
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
            swatch.GetComponent<Image>().color = known ? entry.ingredient.color : Silhouette;
            var le = swatch.GetComponent<LayoutElement>();
            le.preferredWidth = le.preferredHeight = 56;

            string title = known ? $"<b>{entry.ingredient.displayName}</b>" : "<b>???</b>";
            string detail = known ? IngredientCatalog.DescribeTags(entry.ingredient) : $"<i>{entry.hint}</i>";
            var text = MakeText(row.transform, $"{title}\n<size=70%>{detail}</size>", 30, known ? Color.white : new Color(1f, 1f, 1f, 0.55f));
            text.GetComponent<LayoutElement>().preferredWidth = 520;
            return row;
        }

        private void Build()
        {
            var canvasGo = new GameObject("IngredientBookCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            page = new GameObject("Page", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            page.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)page.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-40, 0);
            page.GetComponent<Image>().color = new Color(0.93f, 0.87f, 0.74f, 0.97f);
            var v = page.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(28, 28, 24, 28);
            v.spacing = 14;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = v.childForceExpandHeight = false;
            var fit = page.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MakeText(page.transform, "<b>Ingredients</b>", 40, new Color(0.25f, 0.15f, 0.1f));
            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            listGo.transform.SetParent(page.transform, false);
            listGo.GetComponent<Image>().color = new Color(0.2f, 0.16f, 0.22f, 0.9f);
            var lv = listGo.GetComponent<VerticalLayoutGroup>();
            lv.padding = new RectOffset(16, 16, 16, 16);
            lv.spacing = 12;
            lv.childControlWidth = lv.childControlHeight = true;
            lv.childForceExpandWidth = lv.childForceExpandHeight = false;
            list = (RectTransform)listGo.transform;
            MakeText(page.transform, "R: inspect an ingredient    Tab: open/close book", 22, new Color(0.25f, 0.15f, 0.1f, 0.8f));

            toast = MakeText(canvasGo.transform, "", 34, Color.white);
            var trt = (RectTransform)toast.transform;
            trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 1f);
            trt.anchoredPosition = new Vector2(0, -60);
            trt.sizeDelta = new Vector2(900, 60);
            toast.alignment = TextAlignmentOptions.Center;
            toast.gameObject.SetActive(false);
        }

        private static TextMeshProUGUI MakeText(Transform parent, string text, float size, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.richText = true;
            return tmp;
        }
    }
}
