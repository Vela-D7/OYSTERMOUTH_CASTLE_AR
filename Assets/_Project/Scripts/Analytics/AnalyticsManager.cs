using System;
using UnityEngine;

namespace OystermouthCastle.Analytics
{
    public sealed class AnalyticsManager : MonoBehaviour
    {
        private static AnalyticsManager instance;
        private float sessionStartTime;

        public static AnalyticsManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                var analyticsObject = new GameObject("AnalyticsManager");
                DontDestroyOnLoad(analyticsObject);
                instance = analyticsObject.AddComponent<AnalyticsManager>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            sessionStartTime = Time.realtimeSinceStartup;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                TrackSessionDuration();
            }
        }

        private void OnApplicationQuit()
        {
            TrackSessionDuration();
        }

        public void TrackAppLaunch()
        {
            Track("app_launch");
        }

        public void TrackExperienceSelected(string id)
        {
            Track("experience_selected", ("experience", id));
        }

        public void TrackLocalizationStarted()
        {
            Track("localization_started");
        }

        public void TrackLocalizationSuccess(float seconds)
        {
            Track("localization_success", ("seconds", seconds.ToString("0.0")));
        }

        public void TrackLocalizationFailed(float seconds)
        {
            Track("localization_failed", ("seconds", seconds.ToString("0.0")));
        }

        public void TrackNarrationStarted(string language)
        {
            Track("narration_started", ("language", language));
        }

        public void TrackNarrationCompleted(string language)
        {
            Track("narration_completed", ("language", language));
        }

        public void TrackBeforeAfterUsed()
        {
            Track("before_after_used");
        }

        public void TrackInfoOpened()
        {
            Track("info_opened");
        }

        public void TrackExitExperience()
        {
            Track("exit_experience");
        }

        public void TrackVisitorCompletion(string experienceId)
        {
            Track("visitor_completion", ("experience", experienceId));
        }

        public void TrackExperienceRating(string experienceId, int rating, bool skipped)
        {
            Track(
                "experience_rating",
                ("experience", experienceId),
                ("rating", skipped ? "skipped" : rating.ToString()),
                ("skipped", skipped.ToString().ToLowerInvariant()));
        }

        public void TrackQrScanSuccess(string experienceId, string source)
        {
            Track("qr_scan_success", ("experience", experienceId), ("source", source));
        }

        public void TrackQrScanFailed(string reason)
        {
            Track("qr_scan_failure", ("reason", reason));
        }

        public void TrackLocalizationSuccess(string targetName)
        {
            Track("localization_success", ("target", targetName));
        }

        public void TrackLocalizationFailure(string reason)
        {
            Track("localization_failure", ("reason", reason));
        }

        public void TrackNarrationCompletion(string trackId, string language)
        {
            Track("narration_completion", ("track", trackId), ("language", language));
        }

        public void TrackSessionDuration()
        {
            var duration = Mathf.Max(0f, Time.realtimeSinceStartup - sessionStartTime);
            Track("session_duration", ("seconds", duration.ToString("0.0")));
        }

        public void Track(string eventName, params (string key, string value)[] parameters)
        {
            Debug.Log(FormatEvent(eventName, parameters));
            // FirebaseAnalytics.LogEvent can be called here once the SDK is installed.
        }

        private static string FormatEvent(string eventName, (string key, string value)[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return $"Analytics: {eventName}";
            }

            var message = $"Analytics: {eventName}";
            for (var i = 0; i < parameters.Length; i++)
            {
                message += $" {parameters[i].key}={parameters[i].value}";
            }

            return message;
        }
    }
}
