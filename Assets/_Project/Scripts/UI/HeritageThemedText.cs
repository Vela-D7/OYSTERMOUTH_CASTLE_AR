using TMPro;
using UnityEngine;

namespace OystermouthCastle.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class HeritageThemedText : MonoBehaviour
    {
        [SerializeField] private HeritageUITheme theme;
        [SerializeField] private bool useHeadingFont;
        [SerializeField] private bool useAccessibilityFont;
        [SerializeField] private HeritageThemeColor color = HeritageThemeColor.DeepSlateText;
        [SerializeField] private float normalSize = 18f;
        [SerializeField] private float largeTextSize = 22f;
        [SerializeField] private bool largeTextMode;

        private TMP_Text label;

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        public void SetTheme(HeritageUITheme value)
        {
            theme = value;
            Apply();
        }

        public void SetLargeTextMode(bool value)
        {
            largeTextMode = value;
            Apply();
        }

        public void Apply()
        {
            if (theme == null)
            {
                return;
            }

            if (label == null)
            {
                label = GetComponent<TMP_Text>();
            }

            var font = useAccessibilityFont ? theme.AccessibilityFont : useHeadingFont ? theme.HeadingFont : theme.BodyFont;
            if (font != null)
            {
                label.font = font;
            }

            label.color = theme.GetColor(color);
            label.fontSize = largeTextMode ? largeTextSize : normalSize;
        }
    }
}
