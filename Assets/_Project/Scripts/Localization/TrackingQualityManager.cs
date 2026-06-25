using System;
using OystermouthCastle.Analytics;
using UnityEngine;
using Vuforia;

namespace OystermouthCastle.Localization
{
    public enum TrackingConfidenceLevel
    {
        None,
        Low,
        Medium,
        High
    }

    public enum RelocalizationState
    {
        Idle,
        Searching,
        Recovering,
        Recovered,
        Failed
    }

    public sealed class TrackingQualityManager : MonoBehaviour
    {
        [SerializeField] private AreaTargetLocalizationManager localizationManager;
        [SerializeField] private ObserverBehaviour areaTarget;
        [SerializeField] private float localizationFailureSeconds = 45f;
        [SerializeField] private float recoveryFailureSeconds = 20f;
        [SerializeField] private float guidanceRotationSeconds = 5f;
        [SerializeField] private bool showDebugLogs;

        public event Action TrackingQualityChanged;

        public float CurrentLocalizationTimeSeconds => currentLocalizationStartedAt > 0f && !hasLocalizedInCurrentAttempt
            ? Time.time - currentLocalizationStartedAt
            : 0f;

        public float AverageLocalizationTimeSeconds => successfulLocalizations > 0
            ? totalLocalizationSeconds / successfulLocalizations
            : 0f;

        public int SuccessfulLocalizations => successfulLocalizations;
        public int FailedLocalizations => failedLocalizations;
        public int TrackingLostEvents => trackingLostEvents;
        public int TrackingRecoveredEvents => trackingRecoveredEvents;
        public int RecoveryAttempts => recoveryAttempts;
        public float TrackingConfidence => trackingConfidence;
        public TrackingConfidenceLevel ConfidenceLevel => confidenceLevel;
        public RelocalizationState RelocalizationState => relocalizationState;
        public bool IsRecovering => relocalizationState == RelocalizationState.Recovering;
        public bool ShouldShowRecoveryUI => IsRecovering || relocalizationState == RelocalizationState.Failed;
        public string LastTargetName { get; private set; } = "-";
        public string LastVuforiaStatus { get; private set; } = "Unknown";
        public string LastVuforiaStatusInfo { get; private set; } = "Unknown";
        public float LastRecoveryDurationSeconds { get; private set; }
        public float RecoveryElapsedSeconds => recoveryStartedAt > 0f ? Time.time - recoveryStartedAt : 0f;

        private float currentLocalizationStartedAt = -1f;
        private float recoveryStartedAt = -1f;
        private float totalLocalizationSeconds;
        private int successfulLocalizations;
        private int failedLocalizations;
        private int trackingLostEvents;
        private int trackingRecoveredEvents;
        private int recoveryAttempts;
        private bool hasLocalizedInCurrentAttempt;
        private bool failureCountedForCurrentAttempt;
        private bool hasEverLocalized;
        private float trackingConfidence;
        private TrackingConfidenceLevel confidenceLevel = TrackingConfidenceLevel.None;
        private RelocalizationState relocalizationState = RelocalizationState.Searching;

        private void Reset()
        {
            localizationManager = GetComponent<AreaTargetLocalizationManager>();
            areaTarget = GetComponentInChildren<ObserverBehaviour>();
        }

        private void Awake()
        {
            if (localizationManager == null)
            {
                localizationManager = GetComponent<AreaTargetLocalizationManager>();
            }

            if (localizationManager == null)
            {
                localizationManager = FindObjectOfType<AreaTargetLocalizationManager>();
            }

            if (areaTarget == null)
            {
                areaTarget = GetComponentInChildren<ObserverBehaviour>();
            }

            if (areaTarget == null)
            {
                areaTarget = FindObjectOfType<ObserverBehaviour>();
            }
        }

        private void OnEnable()
        {
            BeginLocalizationAttempt();

            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged += HandleLocalizationStateChanged;
                HandleLocalizationStateChanged(localizationManager.State);
            }

            if (areaTarget != null)
            {
                areaTarget.OnTargetStatusChanged += HandleTargetStatusChanged;
                HandleTargetStatusChanged(areaTarget, areaTarget.TargetStatus);
            }
        }

        private void OnDisable()
        {
            if (localizationManager != null)
            {
                localizationManager.LocalizationStateChanged -= HandleLocalizationStateChanged;
            }

            if (areaTarget != null)
            {
                areaTarget.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }
        }

        private void Update()
        {
            if (!hasLocalizedInCurrentAttempt &&
                !failureCountedForCurrentAttempt &&
                localizationFailureSeconds > 0f &&
                CurrentLocalizationTimeSeconds >= localizationFailureSeconds)
            {
                CountLocalizationFailure("timeout");
            }

            if (IsRecovering &&
                recoveryFailureSeconds > 0f &&
                RecoveryElapsedSeconds >= recoveryFailureSeconds)
            {
                SetRelocalizationState(RelocalizationState.Failed);
                CountLocalizationFailure("recovery_timeout");
            }
        }

        public string GetVisitorGuidanceMessage(bool welsh)
        {
            if (ShouldShowRecoveryUI)
            {
                if (relocalizationState == RelocalizationState.Failed)
                {
                    return welsh
                        ? "Dychwelwch at fan cychwyn y capel a daliwch y ffôn yn llonydd."
                        : "Return to the chapel start point and hold the phone steady.";
                }

                return welsh
                    ? "Adfer tracio. Edrychwch yn ôl tuag at wal y capel."
                    : "Recovering tracking. Look back toward the chapel wall.";
            }

            var elapsed = CurrentLocalizationTimeSeconds;
            var guidanceIndex = guidanceRotationSeconds > 0f
                ? Mathf.FloorToInt(elapsed / guidanceRotationSeconds) % 3
                : 0;

            switch (guidanceIndex)
            {
                case 1:
                    return welsh
                        ? "Pwyntiwch y camera tuag at wal y capel."
                        : "Point your camera toward the chapel wall.";
                case 2:
                    return welsh
                        ? "Daliwch y ffôn yn llonydd am eiliad."
                        : "Hold the phone steady for a moment.";
                default:
                    return welsh
                        ? "Symudwch yn nes at y man cychwyn."
                        : "Move closer to the start point.";
            }
        }

        public string GetDiagnostics()
        {
            return
                $"Confidence: {confidenceLevel} ({trackingConfidence:0%})\n" +
                $"Vuforia: {LastVuforiaStatus} / {LastVuforiaStatusInfo}\n" +
                $"Avg localization: {AverageLocalizationTimeSeconds:0.0}s\n" +
                $"Successful localizations: {successfulLocalizations}\n" +
                $"Failed localizations: {failedLocalizations}\n" +
                $"Lost / recovered: {trackingLostEvents} / {trackingRecoveredEvents}\n" +
                $"Relocalization: {relocalizationState}\n" +
                $"Recovery attempts: {recoveryAttempts}\n" +
                $"Recovery elapsed: {RecoveryElapsedSeconds:0.0}s";
        }

        private void HandleLocalizationStateChanged(LocalizationState state)
        {
            switch (state)
            {
                case LocalizationState.Localized:
                    CountLocalizationSuccess();
                    CompleteRecoveryIfNeeded();
                    break;
                case LocalizationState.Lost:
                    HandleTrackingLost();
                    break;
                case LocalizationState.Searching:
                    if (hasLocalizedInCurrentAttempt)
                    {
                        HandleTrackingLost();
                    }
                    else
                    {
                        SetRelocalizationState(RelocalizationState.Searching);
                    }
                    break;
                case LocalizationState.Limited:
                    if (hasEverLocalized)
                    {
                        SetRelocalizationState(RelocalizationState.Recovering);
                    }
                    break;
            }

            TrackingQualityChanged?.Invoke();
        }

        private void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            LastTargetName = behaviour != null ? behaviour.TargetName : "-";
            LastVuforiaStatus = targetStatus.Status.ToString();
            LastVuforiaStatusInfo = targetStatus.StatusInfo.ToString();
            trackingConfidence = CalculateConfidence(targetStatus);
            confidenceLevel = CalculateConfidenceLevel(trackingConfidence);
            TrackingQualityChanged?.Invoke();
        }

        private void BeginLocalizationAttempt()
        {
            currentLocalizationStartedAt = Time.time;
            hasLocalizedInCurrentAttempt = false;
            failureCountedForCurrentAttempt = false;
            SetRelocalizationState(RelocalizationState.Searching);
        }

        private void CountLocalizationSuccess()
        {
            if (hasLocalizedInCurrentAttempt)
            {
                return;
            }

            hasLocalizedInCurrentAttempt = true;
            hasEverLocalized = true;

            var seconds = localizationManager != null && localizationManager.TimeToLocalizationSeconds >= 0f
                ? localizationManager.TimeToLocalizationSeconds
                : Mathf.Max(0f, Time.time - currentLocalizationStartedAt);

            totalLocalizationSeconds += seconds;
            successfulLocalizations++;
            SetRelocalizationState(RelocalizationState.Idle);
        }

        private void CountLocalizationFailure(string reason)
        {
            if (failureCountedForCurrentAttempt)
            {
                return;
            }

            failureCountedForCurrentAttempt = true;
            failedLocalizations++;
            AnalyticsManager.Instance.TrackLocalizationFailure(reason);

            if (showDebugLogs)
            {
                Debug.Log($"Tracking quality localization failure: {reason}");
            }
        }

        private void HandleTrackingLost()
        {
            if (!hasEverLocalized)
            {
                CountLocalizationFailure("not_found");
                return;
            }

            if (!IsRecovering)
            {
                recoveryAttempts++;
                trackingLostEvents++;
                recoveryStartedAt = Time.time;
                LastRecoveryDurationSeconds = 0f;
                AnalyticsManager.Instance.Track("tracking_lost", ("target", LastTargetName));
            }

            SetRelocalizationState(RelocalizationState.Recovering);
        }

        private void CompleteRecoveryIfNeeded()
        {
            if (!IsRecovering && relocalizationState != RelocalizationState.Failed)
            {
                return;
            }

            LastRecoveryDurationSeconds = RecoveryElapsedSeconds;
            recoveryStartedAt = -1f;
            trackingRecoveredEvents++;
            AnalyticsManager.Instance.Track(
                "tracking_recovered",
                ("target", LastTargetName),
                ("seconds", LastRecoveryDurationSeconds.ToString("0.0")));
            SetRelocalizationState(RelocalizationState.Recovered);
        }

        private void SetRelocalizationState(RelocalizationState state)
        {
            if (relocalizationState == state)
            {
                return;
            }

            relocalizationState = state;
            if (showDebugLogs)
            {
                Debug.Log($"Relocalization state: {relocalizationState}");
            }
        }

        private static float CalculateConfidence(TargetStatus targetStatus)
        {
            switch (targetStatus.Status)
            {
                case Status.TRACKED:
                    return 1f;
                case Status.EXTENDED_TRACKED:
                    return 0.85f;
                case Status.LIMITED:
                    return 0.45f;
                default:
                    return 0f;
            }
        }

        private static TrackingConfidenceLevel CalculateConfidenceLevel(float confidence)
        {
            if (confidence >= 0.8f)
            {
                return TrackingConfidenceLevel.High;
            }

            if (confidence >= 0.4f)
            {
                return TrackingConfidenceLevel.Medium;
            }

            return confidence > 0f ? TrackingConfidenceLevel.Low : TrackingConfidenceLevel.None;
        }
    }
}
