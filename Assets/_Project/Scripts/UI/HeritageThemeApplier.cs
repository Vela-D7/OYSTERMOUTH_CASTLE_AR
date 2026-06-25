using UnityEngine;

namespace OystermouthCastle.UI
{
    public sealed class HeritageThemeApplier : MonoBehaviour
    {
        [SerializeField] private HeritageUITheme theme;

        private void Awake()
        {
            ApplyTheme();
        }

        private void OnValidate()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            var graphics = GetComponentsInChildren<HeritageThemedGraphic>(true);
            for (var i = 0; i < graphics.Length; i++)
            {
                graphics[i].SetTheme(theme);
            }

            var labels = GetComponentsInChildren<HeritageThemedText>(true);
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i].SetTheme(theme);
            }
        }
    }
}
