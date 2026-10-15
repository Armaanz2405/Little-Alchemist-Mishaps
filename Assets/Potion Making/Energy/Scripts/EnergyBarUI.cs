using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Energy
{
    public class EnergyBarUI : MonoBehaviour
    {
        [SerializeField] private EnergyMeter meter;
        [SerializeField] private Color fullColor = new Color(0.98f, 0.82f, 0.35f);
        [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Vector2 anchoredPosition = new Vector2(30f, -30f);

        private RectTransform pipRow;
        private TextMeshProUGUI label;
        private readonly List<Image> pips = new List<Image>();

        private void Awake()
        {
            if (meter == null) meter = FindFirstObjectByType<EnergyMeter>();
            Build();
        }

        private void OnEnable() { if (meter != null) meter.onChanged.AddListener(Show); }
        private void OnDisable() { if (meter != null) meter.onChanged.RemoveListener(Show); }

        private void Build()
        {
            var canvasGo = new GameObject("EnergyCanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var panel = new GameObject("EnergyPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(canvasGo.transform, false);
            var rt = (RectTransform)panel.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPosition;
            panel.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.14f, 0.75f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 12);
            layout.spacing = 6;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(panel.transform, false);
            label = labelGo.GetComponent<TextMeshProUGUI>();
            label.fontSize = 28;
            label.color = Color.white;

            var row = new GameObject("Pips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(panel.transform, false);
            pipRow = (RectTransform)row.transform;
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 6;
            h.childControlWidth = h.childControlHeight = false;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
        }

        private void Show(int current, int max)
        {
            while (pips.Count < max)
            {
                var pip = new GameObject("Pip", typeof(RectTransform), typeof(Image));
                pip.transform.SetParent(pipRow, false);
                ((RectTransform)pip.transform).sizeDelta = new Vector2(26, 26);
                pips.Add(pip.GetComponent<Image>());
            }
            for (int i = 0; i < pips.Count; i++)
            {
                pips[i].gameObject.SetActive(i < max);
                pips[i].color = i < current ? fullColor : emptyColor;
            }
            var pipLayout = pipRow.GetComponent<HorizontalLayoutGroup>();
            pipRow.sizeDelta = new Vector2(max * 26 + Mathf.Max(0, max - 1) * pipLayout.spacing, 26);
            label.text = $"Energy  {current}/{max}";
        }
    }
}
