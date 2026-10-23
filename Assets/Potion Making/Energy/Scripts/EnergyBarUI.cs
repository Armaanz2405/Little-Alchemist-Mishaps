using System.Collections.Generic;
using Nocturne.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.PotionMaking.Energy
{
    public class EnergyBarUI : MonoBehaviour
    {
        [SerializeField] private EnergyMeter meter;
        [SerializeField] private Vector2 anchoredPosition = new Vector2(30f, -30f);
        [SerializeField] private int minPips = 5;

        private RectTransform panelRect;
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
            var canvasGo = UIStyle.MakeCanvas(transform, "EnergyCanvas", 50).gameObject;

            var panel = new GameObject("EnergyPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panel.transform.SetParent(canvasGo.transform, false);
            panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = anchoredPosition;
            UIStyle.Box(panel.GetComponent<Image>(), BoxKind.Panel, 1.6f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 12, 16);
            layout.spacing = 6;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            label = UIStyle.MakeText(panel.transform, "", 28, TextKind.Title, "Label");
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var row = new GameObject("Pips", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(panel.transform, false);
            pipRow = (RectTransform)row.transform;
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 6;
            row.GetComponent<LayoutElement>().minWidth = minPips * 26 + Mathf.Max(0, minPips - 1) * h.spacing;
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
                var image = pip.GetComponent<Image>();
                image.sprite = UITheme.Current.pipSprite;
                image.raycastTarget = false;
                pips.Add(image);
            }
            for (int i = 0; i < pips.Count; i++)
            {
                pips[i].gameObject.SetActive(i < max);
                pips[i].color = i < current ? UITheme.Current.pipFull : UITheme.Current.pipEmpty;
            }
            var pipLayout = pipRow.GetComponent<HorizontalLayoutGroup>();
            pipRow.sizeDelta = new Vector2(max * 26 + Mathf.Max(0, max - 1) * pipLayout.spacing, 26);
            label.text = $"Energy  <color={UITheme.Hex(UITheme.Current.accentText)}>{current}/{max}</color>";
            // resize now so the text never spills out of the box
            label.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(pipRow);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }
    }
}
