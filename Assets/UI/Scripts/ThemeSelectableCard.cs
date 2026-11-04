using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nocturne.UI
{
    // bopx behind a selectable row that lights up when hovered or selected
    [DefaultExecutionOrder(1000)]
    public class ThemeSelectableCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Selectable selectable;
        private RectTransform row;
        private RectTransform cardRect;
        private Image card;
        private Image frame;
        private TMP_Text text;
        private Image indicator;
        private bool hovered;

        public void Setup(Selectable target)
        {
            selectable = target;
            row = (RectTransform)target.transform;
            text = target.GetComponent<TMP_Text>();
            if (text != null) text.margin += new Vector4(0f, 8f, 16f, 8f);
            foreach (var image in target.GetComponentsInChildren<Image>(true))
                if (image.name.IndexOf("Indicator", System.StringComparison.OrdinalIgnoreCase) >= 0) indicator = image;

            var go = new GameObject("Theme Card", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            cardRect = (RectTransform)go.transform;
            cardRect.SetParent(row.parent, false);
            card = go.GetComponent<Image>();
            card.raycastTarget = false;
            UIStyle.Box(card, BoxKind.Card, 1.8f);
            var frameLayer = cardRect.Find("Theme Frame");
            frame = frameLayer != null ? frameLayer.GetComponent<Image>() : null;
            Follow();
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;
        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        private void OnEnable()
        {
            if (cardRect != null) cardRect.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            hovered = false;
            if (cardRect != null) cardRect.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (cardRect != null) Destroy(cardRect.gameObject);
        }

        private void LateUpdate()
        {
            if (selectable == null || card == null) return;
            Follow();
            var t = UITheme.Current;
            bool usable = selectable.IsInteractable();
            bool lit = usable && (hovered || (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == selectable.gameObject));
            card.color = lit ? t.buttonHover : t.cardFill;
            if (frame != null) frame.color = lit ? t.panelFlourish : t.cardFrame;
            if (text != null) text.color = !usable ? t.mutedText : lit ? t.text : t.accentText;
            if (indicator != null) indicator.color = t.panelFlourish;
        }

        // same rect as the row, one place before it so it draws underneath
        private void Follow()
        {
            if (cardRect.parent != row.parent) cardRect.SetParent(row.parent, false);
            int index = row.GetSiblingIndex();
            if (cardRect.GetSiblingIndex() != index - 1) cardRect.SetSiblingIndex(cardRect.GetSiblingIndex() < index ? index - 1 : index);
            cardRect.anchorMin = row.anchorMin;
            cardRect.anchorMax = row.anchorMax;
            cardRect.pivot = row.pivot;
            cardRect.anchoredPosition = row.anchoredPosition;
            cardRect.sizeDelta = row.sizeDelta;
            cardRect.localScale = row.localScale;
        }
    }
}
