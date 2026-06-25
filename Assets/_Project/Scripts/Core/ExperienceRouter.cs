using OystermouthCastle.Analytics;
using OystermouthCastle.QR;
using UnityEngine;

namespace OystermouthCastle.Core
{
    public sealed class ExperienceRouter : MonoBehaviour
    {
        [SerializeField] private ExperienceRegistry registry;
        [SerializeField] private AppLanguage defaultLanguage = AppLanguage.English;

        public ExperienceDefinition CurrentExperience { get; private set; }
        public AppLanguage CurrentLanguage { get; private set; }

        private void Awake()
        {
            CurrentLanguage = defaultLanguage;
        }

        public void OpenFromQr(string rawPayload)
        {
            if (!QRCodePayload.TryParse(rawPayload, out var payload))
            {
                AnalyticsManager.Instance.TrackQrScanFailed("payload_parse_failed");
                Debug.LogWarning($"Could not parse QR payload: {rawPayload}");
                return;
            }

            CurrentLanguage = payload.Language ?? defaultLanguage;
            OpenExperience(payload.ExperienceId, "qr");
        }

        public void OpenExperience(string experienceIdOrAlias, string source = "manual")
        {
            if (registry == null)
            {
                Debug.LogError("ExperienceRouter needs an ExperienceRegistry.");
                return;
            }

            var experience = registry.ResolveOrFallback(experienceIdOrAlias);
            if (experience == null)
            {
                AnalyticsManager.Instance.TrackQrScanFailed($"experience_not_found:{experienceIdOrAlias}");
                Debug.LogWarning($"No experience found for '{experienceIdOrAlias}'.");
                return;
            }

            CurrentExperience = experience;
            AnalyticsManager.Instance.TrackQrScanSuccess(experience.ExperienceId, source);
            SceneLoader.LoadScene(experience.SceneName);
        }
    }
}
