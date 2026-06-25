using UnityEngine;

namespace OystermouthCastle.Localization
{
    public sealed class ExperienceVisibilityController : MonoBehaviour
    {
        [SerializeField] private AreaTargetLocalizationManager localizationManager;
        [SerializeField] private GameObject[] reconstructionRoots;
        [SerializeField] private bool showOnlyWhenLocalized = true;

        private void Reset()
        {
            localizationManager = FindObjectOfType<AreaTargetLocalizationManager>();
        }

        private void OnEnable()
        {
            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged += HandleLocalizationStateChanged;
                HandleLocalizationStateChanged(localizationManager.State);
            }
        }

        private void OnDisable()
        {
            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged -= HandleLocalizationStateChanged;
            }
        }

        private void HandleLocalizationStateChanged(LocalizationState state)
        {
            var visible = !showOnlyWhenLocalized || state == LocalizationState.Localized;
            for (var i = 0; i < reconstructionRoots.Length; i++)
            {
                if (reconstructionRoots[i] != null)
                {
                    reconstructionRoots[i].SetActive(visible);
                }
            }
        }
    }
}
