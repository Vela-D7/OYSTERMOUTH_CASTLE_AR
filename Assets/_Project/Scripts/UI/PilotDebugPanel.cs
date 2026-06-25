using OystermouthCastle.Core;
using OystermouthCastle.Localization;
using TMPro;
using UnityEngine;

namespace OystermouthCastle.UI
{
    public sealed class PilotDebugPanel : MonoBehaviour
    {
        [SerializeField] private AreaTargetLocalizationManager localizationManager;
        [SerializeField] private TrackingQualityManager trackingQualityManager;
        [SerializeField] private Accessibility.AccessibilityManager accessibilityManager;
        [SerializeField] private ExperienceManager experienceManager;
        [SerializeField] private TMP_Text debugLabel;
        [SerializeField] private float refreshSeconds = 0.25f;

        private float nextRefreshTime;
        private float smoothedDeltaTime;

        private void Awake()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            gameObject.SetActive(false);
#endif
        }

        private void Reset()
        {
            localizationManager = FindObjectOfType<AreaTargetLocalizationManager>();
            trackingQualityManager = FindObjectOfType<TrackingQualityManager>();
            debugLabel = GetComponentInChildren<TMP_Text>();
        }

        private void OnEnable()
        {
            if (localizationManager == null)
            {
                localizationManager = FindObjectOfType<AreaTargetLocalizationManager>();
            }

            if (trackingQualityManager == null)
            {
                trackingQualityManager = FindObjectOfType<TrackingQualityManager>();
            }
        }

        private void Update()
        {
            smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + refreshSeconds;
            Refresh();
        }

        private void Refresh()
        {
            if (debugLabel == null)
            {
                return;
            }

            var state = localizationManager != null ? localizationManager.State.ToString() : "Unknown";
            var localized = localizationManager != null && localizationManager.IsLocalized ? "found" : "not found";
            var timeToLocalization = localizationManager != null && localizationManager.TimeToLocalizationSeconds >= 0f
                ? localizationManager.TimeToLocalizationSeconds.ToString("0.0") + "s"
                : "-";
            var confidence = trackingQualityManager != null
                ? $"{trackingQualityManager.ConfidenceLevel} ({trackingQualityManager.TrackingConfidence:0%})"
                : "-";
            var vuforiaStatus = trackingQualityManager != null
                ? $"{trackingQualityManager.LastVuforiaStatus} / {trackingQualityManager.LastVuforiaStatusInfo}"
                : "-";
            var averageLocalization = trackingQualityManager != null
                ? trackingQualityManager.AverageLocalizationTimeSeconds.ToString("0.0") + "s"
                : "-";
            var successfulLocalizations = trackingQualityManager != null
                ? trackingQualityManager.SuccessfulLocalizations.ToString()
                : "-";
            var failedLocalizations = trackingQualityManager != null
                ? trackingQualityManager.FailedLocalizations.ToString()
                : "-";
            var lostRecovered = trackingQualityManager != null
                ? $"{trackingQualityManager.TrackingLostEvents}/{trackingQualityManager.TrackingRecoveredEvents}"
                : "-";
            var relocalization = trackingQualityManager != null
                ? trackingQualityManager.RelocalizationState.ToString()
                : "-";
            var recoveryAttempts = trackingQualityManager != null
                ? trackingQualityManager.RecoveryAttempts.ToString()
                : "-";
            var fps = smoothedDeltaTime > 0f ? Mathf.RoundToInt(1f / smoothedDeltaTime).ToString() : "-";
            var language = accessibilityManager != null ? accessibilityManager.Language.ToString() : "-";
            var experience = experienceManager != null ? experienceManager.CurrentExperienceId : "chapel";

            debugLabel.text =
                $"Pilot Debug\n" +
                $"Area Target: {state}\n" +
                $"Tracking: {localized}\n" +
                $"Confidence: {confidence}\n" +
                $"Vuforia: {vuforiaStatus}\n" +
                $"Time to localization: {timeToLocalization}\n" +
                $"Average localization: {averageLocalization}\n" +
                $"Success / failed: {successfulLocalizations}/{failedLocalizations}\n" +
                $"Lost / recovered: {lostRecovered}\n" +
                $"Relocalization: {relocalization}\n" +
                $"Recovery attempts: {recoveryAttempts}\n" +
                $"FPS: {fps}\n" +
                $"Language: {language}\n" +
                $"Experience: {experience}";
        }
    }
}
