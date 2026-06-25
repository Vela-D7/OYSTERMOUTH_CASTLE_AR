using System;
using TMPro;
using UnityEngine;

namespace OystermouthCastle.UI
{
    [CreateAssetMenu(menuName = "Oystermouth/UI/Heritage UI Theme", fileName = "HeritageUITheme")]
    public sealed class HeritageUITheme : ScriptableObject
    {
        [Header("Palette")]
        [SerializeField] private Color medievalStone = new Color32(110, 106, 95, 255);
        [SerializeField] private Color warmLimestone = new Color32(184, 173, 147, 255);
        [SerializeField] private Color parchment = new Color32(242, 231, 201, 255);
        [SerializeField] private Color agedParchment = new Color32(216, 199, 157, 255);
        [SerializeField] private Color heritageGold = new Color32(181, 138, 58, 255);
        [SerializeField] private Color deepSlateText = new Color32(37, 42, 44, 255);
        [SerializeField] private Color mossAccent = new Color32(93, 111, 79, 255);
        [SerializeField] private Color chapelBlue = new Color32(77, 111, 130, 255);
        [SerializeField] private Color warning = new Color32(138, 74, 59, 255);

        [Header("Typography")]
        [SerializeField] private TMP_FontAsset headingFont;
        [SerializeField] private TMP_FontAsset bodyFont;
        [SerializeField] private TMP_FontAsset accessibilityFont;

        [Header("Sprites")]
        [SerializeField] private Sprite buttonBackground;
        [SerializeField] private Sprite iconButtonBackground;
        [SerializeField] private Sprite panelBackground;
        [SerializeField] private Sprite sliderBackground;
        [SerializeField] private Sprite sliderFill;
        [SerializeField] private Sprite sliderHandle;

        [Header("Layout")]
        [SerializeField] private float minimumTouchSize = 56f;
        [SerializeField] private float phoneSafePadding = 24f;
        [SerializeField] private float tabletSafePadding = 40f;

        public Color MedievalStone => medievalStone;
        public Color WarmLimestone => warmLimestone;
        public Color Parchment => parchment;
        public Color AgedParchment => agedParchment;
        public Color HeritageGold => heritageGold;
        public Color DeepSlateText => deepSlateText;
        public Color MossAccent => mossAccent;
        public Color ChapelBlue => chapelBlue;
        public Color Warning => warning;

        public TMP_FontAsset HeadingFont => headingFont;
        public TMP_FontAsset BodyFont => bodyFont;
        public TMP_FontAsset AccessibilityFont => accessibilityFont != null ? accessibilityFont : bodyFont;

        public Sprite ButtonBackground => buttonBackground;
        public Sprite IconButtonBackground => iconButtonBackground;
        public Sprite PanelBackground => panelBackground;
        public Sprite SliderBackground => sliderBackground;
        public Sprite SliderFill => sliderFill;
        public Sprite SliderHandle => sliderHandle;

        public float MinimumTouchSize => minimumTouchSize;
        public float PhoneSafePadding => phoneSafePadding;
        public float TabletSafePadding => tabletSafePadding;

        public Color GetColor(HeritageThemeColor color)
        {
            switch (color)
            {
                case HeritageThemeColor.MedievalStone:
                    return medievalStone;
                case HeritageThemeColor.WarmLimestone:
                    return warmLimestone;
                case HeritageThemeColor.Parchment:
                    return parchment;
                case HeritageThemeColor.AgedParchment:
                    return agedParchment;
                case HeritageThemeColor.HeritageGold:
                    return heritageGold;
                case HeritageThemeColor.DeepSlateText:
                    return deepSlateText;
                case HeritageThemeColor.MossAccent:
                    return mossAccent;
                case HeritageThemeColor.ChapelBlue:
                    return chapelBlue;
                case HeritageThemeColor.Warning:
                    return warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }
    }

    public enum HeritageThemeColor
    {
        MedievalStone,
        WarmLimestone,
        Parchment,
        AgedParchment,
        HeritageGold,
        DeepSlateText,
        MossAccent,
        ChapelBlue,
        Warning
    }
}
