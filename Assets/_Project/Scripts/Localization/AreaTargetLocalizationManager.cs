using System;
using OystermouthCastle.Analytics;
using UnityEngine;
using Vuforia;

namespace OystermouthCastle.Localization
{
    public enum LocalizationState
    {
        Searching,
        Localized,
        Limited,
        Lost
    }

    [RequireComponent(typeof(TrackingQualityManager))]
    public sealed class AreaTargetLocalizationManager : MonoBehaviour
    {
        [SerializeField] private ObserverBehaviour areaTarget;
        [SerializeField] private float lostTrackingGraceSeconds = 1.25f;
        [SerializeField] private bool showDebugLogs = true;

        public event Action<LocalizationState> LocalizationStateChanged;

        public LocalizationState State { get; private set; } = LocalizationState.Searching;
        public bool IsLocalized => State == LocalizationState.Localized;
        public float SearchElapsedSeconds => localizationStartedAt > 0f && !IsLocalized ? Time.time - localizationStartedAt : 0f;
        public float TimeToLocalizationSeconds { get; private set; } = -1f;
        public ObserverBehaviour AreaTarget => areaTarget;

        private float lastTrackedTime;
        private float localizationStartedAt = -1f;
        private bool localizationSuccessTracked;
        private bool localizationFailureTracked;

        private void Reset()
        {
            areaTarget = GetComponentInChildren<ObserverBehaviour>();
        }

        private void Awake()
        {
            if (areaTarget == null)
            {
                areaTarget = FindObjectOfType<ObserverBehaviour>();
            }

            if (GetComponent<TrackingQualityManager>() == null)
            {
                gameObject.AddComponent<TrackingQualityManager>();
            }
        }

        private void OnEnable()
        {
            localizationStartedAt = Time.time;
            TimeToLocalizationSeconds = -1f;
            localizationSuccessTracked = false;
            localizationFailureTracked = false;
            AnalyticsManager.Instance.TrackLocalizationStarted();

            if (areaTarget != null)
            {
                areaTarget.OnTargetStatusChanged += HandleTargetStatusChanged;
                HandleTargetStatusChanged(areaTarget, areaTarget.TargetStatus);
            }
            else
            {
                SetState(LocalizationState.Searching);
            }
        }

        private void OnDisable()
        {
            if (areaTarget != null)
            {
                areaTarget.OnTargetStatusChanged -= HandleTargetStatusChanged;
            }
        }

        private void Update()
        {
            if (State == LocalizationState.Limited && Time.time - lastTrackedTime > lostTrackingGraceSeconds)
            {
                SetState(LocalizationState.Lost);
                TrackLocalizationFailureOnce();
            }
        }

        private void HandleTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
            {
                lastTrackedTime = Time.time;
                if (TimeToLocalizationSeconds < 0f && localizationStartedAt > 0f)
                {
                    TimeToLocalizationSeconds = Time.time - localizationStartedAt;
                }

                SetState(LocalizationState.Localized);
                AnalyticsManager.Instance.TrackLocalizationSuccess(behaviour.TargetName);
                if (!localizationSuccessTracked)
                {
                    AnalyticsManager.Instance.TrackLocalizationSuccess(Mathf.Max(0f, TimeToLocalizationSeconds));
                    localizationSuccessTracked = true;
                }
                return;
            }

            if (targetStatus.Status == Status.LIMITED)
            {
                SetState(LocalizationState.Limited);
                return;
            }

            SetState(LocalizationState.Searching);
        }

        private void SetState(LocalizationState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            if (showDebugLogs)
            {
                Debug.Log($"Area Target localization state: {State}");
            }

            LocalizationStateChanged?.Invoke(State);
        }

        private void TrackLocalizationFailureOnce()
        {
            if (localizationFailureTracked)
            {
                return;
            }

            var elapsed = localizationStartedAt > 0f ? Time.time - localizationStartedAt : 0f;
            AnalyticsManager.Instance.TrackLocalizationFailure("tracking_lost");
            AnalyticsManager.Instance.TrackLocalizationFailed(elapsed);
            localizationFailureTracked = true;
        }
    }
}
