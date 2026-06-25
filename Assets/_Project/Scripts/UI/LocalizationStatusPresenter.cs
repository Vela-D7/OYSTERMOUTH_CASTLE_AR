using OystermouthCastle.Core;
using OystermouthCastle.Localization;
using TMPro;
using UnityEngine;

namespace OystermouthCastle.UI
{
    public sealed class LocalizationStatusPresenter : MonoBehaviour
    {
        [SerializeField] private AreaTargetLocalizationManager localizationManager;
        [SerializeField] private TrackingQualityManager trackingQualityManager;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AppLanguage language = AppLanguage.English;
        [SerializeField] private float localizedHoldSeconds = 1.5f;
        [SerializeField] private float fadeSeconds = 0.35f;

        private float localizedAt = -1f;
        private float targetAlpha = 1f;
        private LocalizationState currentState = LocalizationState.Searching;
        private int guidanceLevel;
        private float nextGuidanceRefreshTime;

        private void Reset()
        {
            localizationManager = FindObjectOfType<AreaTargetLocalizationManager>();
            trackingQualityManager = FindObjectOfType<TrackingQualityManager>();
            canvasGroup = GetComponent<CanvasGroup>();
            statusLabel = GetComponentInChildren<TMP_Text>();
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

            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged += HandleLocalizationChanged;
                HandleLocalizationChanged(localizationManager.State);
            }

            if (trackingQualityManager != null)
            {
                trackingQualityManager.TrackingQualityChanged += HandleTrackingQualityChanged;
            }
        }

        private void OnDisable()
        {
            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged -= HandleLocalizationChanged;
            }

            if (trackingQualityManager != null)
            {
                trackingQualityManager.TrackingQualityChanged -= HandleTrackingQualityChanged;
            }
        }

        private void Update()
        {
            if ((currentState == LocalizationState.Searching ||
                 currentState == LocalizationState.Limited ||
                 currentState == LocalizationState.Lost) &&
                Time.unscaledTime >= nextGuidanceRefreshTime)
            {
                nextGuidanceRefreshTime = Time.unscaledTime + 1f;
                UpdateStatusLabel();
            }

            if (localizedAt > 0f && Time.time - localizedAt > localizedHoldSeconds)
            {
                targetAlpha = 0f;
            }

            if (canvasGroup != null)
            {
                var fadeStep = fadeSeconds > 0f ? Time.deltaTime / fadeSeconds : 1f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeStep);
                canvasGroup.interactable = canvasGroup.alpha > 0.95f;
                canvasGroup.blocksRaycasts = canvasGroup.alpha > 0.05f;
            }
        }

        public void SetLanguage(AppLanguage value)
        {
            language = value;
            if (localizationManager != null)
            {
                HandleLocalizationChanged(localizationManager.State);
            }
            else
            {
                UpdateStatusLabel();
            }
        }

        private void HandleLocalizationChanged(LocalizationState state)
        {
            currentState = state;
            guidanceLevel = 0;
            localizedAt = state == LocalizationState.Localized ? Time.time : -1f;
            targetAlpha = 1f;

            if (canvasGroup != null && canvasGroup.alpha < 0.05f)
            {
                canvasGroup.alpha = 0f;
            }

            UpdateStatusLabel();
        }

        private void HandleTrackingQualityChanged()
        {
            if (currentState != LocalizationState.Localized)
            {
                UpdateStatusLabel();
            }
        }

        private void UpdateStatusLabel()
        {
            if (statusLabel != null)
            {
                statusLabel.text = GetMessage(currentState);
            }
        }

        private string GetMessage(LocalizationState state)
        {
            switch (state)
            {
                case LocalizationState.Localized:
                    return language == AppLanguage.Welsh ? "Wedi canfod y Capel" : "Chapel Found";
                case LocalizationState.Limited:
                    if (trackingQualityManager != null && trackingQualityManager.IsRecovering)
                    {
                        return trackingQualityManager.GetVisitorGuidanceMessage(language == AppLanguage.Welsh);
                    }

                    return language == AppLanguage.Welsh ? "Cadwch y capel yn y golwg" : "Keep the chapel in view";
                case LocalizationState.Lost:
                    if (trackingQualityManager != null)
                    {
                        return trackingQualityManager.GetVisitorGuidanceMessage(language == AppLanguage.Welsh);
                    }

                    return language == AppLanguage.Welsh
                        ? "Collwyd tracio. Edrychwch yn ol tuag at y capel."
                        : "Tracking lost. Please look back toward the chapel.";
                default:
                    if (trackingQualityManager != null)
                    {
                        return trackingQualityManager.GetVisitorGuidanceMessage(language == AppLanguage.Welsh);
                    }

                    var searchElapsed = localizationManager != null ? localizationManager.SearchElapsedSeconds : 0f;
                    guidanceLevel = searchElapsed >= 30f ? 2 : searchElapsed >= 15f ? 1 : 0;
                    if (guidanceLevel >= 2)
                    {
                        return language == AppLanguage.Welsh
                            ? "Ceisiwch sefyll ger y bwrdd gwybodaeth a sganio eto."
                            : "Try standing near the information board and scanning again.";
                    }

                    if (guidanceLevel >= 1)
                    {
                        return language == AppLanguage.Welsh
                            ? "Symudwch yn araf a phwyntiwch eich camera tuag at wal y capel."
                            : "Move slowly and point your camera toward the chapel wall.";
                    }

                    return language == AppLanguage.Welsh ? "Chwilio am y Capel..." : "Searching for Chapel...";
            }
        }
    }
}
