using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nocturne.UI
{
    [CreateAssetMenu(fileName = "UITheme", menuName = "UI/Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Font")]
        public Font font;
        public TMP_FontAsset fontAsset;

        [Header("Text")]
        public Color text = new Color(0.902f, 0.878f, 0.933f);
        public Color mutedText = new Color(0.541f, 0.502f, 0.596f);
        public Color accentText = new Color(0.647f, 0.58f, 0.741f);
        public Color goldText = new Color(0.831f, 0.69f, 0.416f);
        public Color keyText = Color.white;
        public Color goodText = new Color(0.55f, 0.85f, 0.45f);
        public Color badText = new Color(0.95f, 0.4f, 0.36f);

        [Header("Boxes")]
        public Color panelFill = new Color(0.133f, 0.098f, 0.196f, 0.96f);
        public Color panelFrame = new Color(0.29f, 0.235f, 0.369f);
        public Color panelFlourish = new Color(0.49f, 0.424f, 0.58f);
        public Color panelGlow = new Color(0.235f, 0.188f, 0.282f, 0.5f);
        public Color cardFill = new Color(0.11f, 0.082f, 0.157f, 0.95f);
        public Color cardFrame = new Color(0.212f, 0.169f, 0.275f);

        [Header("Buttons")]
        public Color buttonFill = new Color(0.173f, 0.133f, 0.251f);
        public Color buttonHover = new Color(0.239f, 0.188f, 0.329f);
        public Color buttonPressed = new Color(0.114f, 0.086f, 0.161f);
        public Color buttonDisabled = new Color(0.129f, 0.11f, 0.161f, 0.7f);
        public Color buttonFrame = new Color(0.29f, 0.235f, 0.369f);
        public Color tabIdle = new Color(0.6f, 0.6f, 0.6f);

        [Header("Keys, pips and silhouettes")]
        public Color keyFill = new Color(0.294f, 0.227f, 0.388f);
        public Color keyFrame = new Color(0.541f, 0.471f, 0.639f);
        public Color pipFull = new Color(0.541f, 0.416f, 0.659f);
        public Color pipEmpty = new Color(1f, 1f, 1f, 0.12f);
        public Color silhouette = new Color(0.29f, 0.259f, 0.337f);

        [Header("Sprites")]
        public Sprite panelFillSprite;
        public Sprite panelFrameSprite;
        public Sprite panelFlourishSprite;
        public Sprite panelGlowSprite;
        public float glowSize = 24f;
        public Sprite cardFillSprite;
        public Sprite cardFrameSprite;
        public Sprite buttonFillSprite;
        public Sprite buttonFrameSprite;
        public Sprite tabFillSprite;
        public Sprite tabFrameSprite;
        public Sprite keyFillSprite;
        public Sprite keyFrameSprite;
        public Sprite pipSprite;
        public Sprite dividerSprite;
        public Sprite starSprite;
        public Sprite nubFillSprite;
        public Sprite nubFrameSprite;

        private static UITheme current;
        [System.NonSerialized] private TMP_FontAsset runtimeFont;

        public static UITheme Current
        {
            get
            {
                if (current == null) current = Resources.Load<UITheme>("UITheme");
                if (current == null) current = CreateInstance<UITheme>();
                return current;
            }
        }

        // the font set here
        public TMP_FontAsset TextFont
        {
            get
            {
                if (fontAsset != null) return fontAsset;
                if (runtimeFont == null && font != null)
                {
                    runtimeFont = TMP_FontAsset.CreateFontAsset(font);
                    if (runtimeFont != null && TMP_Settings.defaultFontAsset != null)
                        runtimeFont.fallbackFontAssetTable = new List<TMP_FontAsset> { TMP_Settings.defaultFontAsset };
                }
                return runtimeFont != null ? runtimeFont : TMP_Settings.defaultFontAsset;
            }
        }

        public static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
    }
}
