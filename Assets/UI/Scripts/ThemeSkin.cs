using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nocturne.UI
{
    // styles UI built in the scene with the theme
    public class ThemeSkin : MonoBehaviour
    {
        [SerializeField] private bool allCanvases = true;
        [SerializeField] private float rescanInterval = 0.2f;

        private static readonly HashSet<string> BuiltInSprites = new HashSet<string>
            { "UISprite", "Background", "InputFieldBackground", "Knob", "Checkmark", "DropdownArrow", "UIMask" };

        private readonly HashSet<Object> done = new HashSet<Object>();
        private readonly HashSet<Image> themed = new HashSet<Image>();
        private float timer;

        private void Start() => Scan();

        private void Update()
        {
            if (rescanInterval <= 0f) return;
            timer += Time.unscaledDeltaTime;
            if (timer < rescanInterval) return;
            timer = 0f;
            Scan();
        }

        public void Scan()
        {
            if (!allCanvases)
            {
                Apply(transform);
                return;
            }
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
                Apply(canvas.transform);
        }

        public void Apply(Transform root)
        {
            foreach (var image in root.GetComponentsInChildren<Image>(true))
                if (done.Add(image) && !UIStyle.IsLayer(image) && !Ignored(image)) StyleImage(image);
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                if (done.Add(text) && !Ignored(text)) StyleText(text);
        }

        private static bool Ignored(Component c) => c.GetComponentInParent<ThemeIgnore>(true) != null || RoleOf(c) == ThemeRole.Role.Ignore;

        private static ThemeRole.Role RoleOf(Component c)
        {
            var role = c.GetComponent<ThemeRole>();
            return role != null ? role.role : ThemeRole.Role.Auto;
        }

        private void StyleImage(Image image)
        {
            var role = RoleOf(image);
            var button = image.GetComponent<Button>();
            if (role == ThemeRole.Role.Auto)
            {
                if (image.type == Image.Type.Filled || image.GetComponent<Mask>() != null || Has(image, "Icon")) return;
                if (!IsPlain(image)) return;
                // background of a pickable row (like a dialogue option) becomes a box that lights up
                var row = image.transform.parent != null ? image.transform.parent.GetComponent<Selectable>() : null;
                if (row != null && !(row is Button) && Has(image, "Background"))
                {
                    if (row.GetComponent<ThemeSelectableCard>() == null) row.gameObject.AddComponent<ThemeSelectableCard>().Setup(row);
                    themed.Add(image);
                    return;
                }
                if (button != null && button.targetGraphic == image) role = Has(image, "Tab") ? ThemeRole.Role.Tab : ThemeRole.Role.Button;
                else role = IsTopLevel(image) || Has(image, "Window") || Has(image, "Panel") || Has(image, "Background") ? ThemeRole.Role.Panel : ThemeRole.Role.Card;
            }

            switch (role)
            {
                case ThemeRole.Role.Panel: UIStyle.Box(image, BoxKind.Panel); break;
                case ThemeRole.Role.Card: UIStyle.Box(image, BoxKind.Card); break;
                case ThemeRole.Role.Button:
                case ThemeRole.Role.Tab:
                    if (button != null) UIStyle.Button(button, role == ThemeRole.Role.Tab);
                    else UIStyle.Box(image, BoxKind.Card);
                    break;
                default: return;
            }
            themed.Add(image);
        }

        private void StyleText(TMP_Text text)
        {
            var role = RoleOf(text);
            TextKind kind;
            switch (role)
            {
                case ThemeRole.Role.Title: kind = TextKind.Title; break;
                case ThemeRole.Role.Muted: kind = TextKind.Muted; break;
                case ThemeRole.Role.Accent: kind = TextKind.Accent; break;
                case ThemeRole.Role.Gold: kind = TextKind.Gold; break;
                case ThemeRole.Role.Body: kind = TextKind.Body; break;
                default:
                    kind = Has(text, "Title") || Has(text, "Header") ? TextKind.Title
                        : Has(text, "Name") ? TextKind.Accent
                        : Has(text, "Gold") || Has(text, "Price") ? TextKind.Gold
                        : TextKind.Body;
                    break;
            }
            UIStyle.Text(text, kind, !OnArt(text));
        }

        private bool OnArt(Component c)
        {
            for (var t = c.transform.parent; t != null; t = t.parent)
            {
                var image = t.GetComponent<Image>();
                if (image != null && !themed.Contains(image) && image.sprite != null && !IsPlain(image)) return true;
                if (t.GetComponent<Canvas>() != null) break;
            }
            return false;
        }

        private static bool IsPlain(Image image) =>
            image.sprite == null || BuiltInSprites.Contains(image.sprite.name) || Has(image, "Background");

        private static bool IsTopLevel(Image image) =>
            image.transform.parent != null && image.transform.parent.GetComponent<Canvas>() != null;

        private static bool Has(Component c, string word) => c.name.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
