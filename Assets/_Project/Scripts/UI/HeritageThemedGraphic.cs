using UnityEngine;
using UnityEngine.UI;

namespace OystermouthCastle.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public sealed class HeritageThemedGraphic : MonoBehaviour
    {
        [SerializeField] private HeritageUITheme theme;
        [SerializeField] private HeritageThemeColor color = HeritageThemeColor.Parchment;
        [Range(0f, 1f)]
        [SerializeField] private float alpha = 1f;

        private Graphic graphic;

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

        public void Apply()
        {
            if (theme == null)
            {
                return;
            }

            if (graphic == null)
            {
                graphic = GetComponent<Graphic>();
            }

            var themedColor = theme.GetColor(color);
            themedColor.a = alpha;
            graphic.color = themedColor;
        }
    }
}
