using OystermouthCastle.Analytics;
using UnityEngine;

namespace OystermouthCastle.Core
{
    public sealed class ExperienceManager : MonoBehaviour
    {
        private const string DefaultExperienceId = "chapel";

        [SerializeField] private string currentExperienceId = DefaultExperienceId;

        public string CurrentExperienceId => string.IsNullOrWhiteSpace(currentExperienceId) ? DefaultExperienceId : currentExperienceId;

        private void Awake()
        {
            ApplyLaunchUrl(Application.absoluteURL);
        }

        private void Start()
        {
            AnalyticsManager.Instance.TrackAppLaunch();
            AnalyticsManager.Instance.TrackExperienceSelected(CurrentExperienceId);
            Debug.Log($"ExperienceManager: active experience ID = {CurrentExperienceId}");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                ApplyLaunchUrl(Application.absoluteURL);
            }
        }

        public void SetExperienceId(string experienceId)
        {
            currentExperienceId = string.IsNullOrWhiteSpace(experienceId) ? DefaultExperienceId : experienceId.Trim().ToLowerInvariant();
            AnalyticsManager.Instance.TrackExperienceSelected(CurrentExperienceId);
            Debug.Log($"ExperienceManager: active experience ID = {CurrentExperienceId}");
        }

        public void ApplyLaunchUrl(string url)
        {
            var parsedExperienceId = ParseExperienceId(url);
            if (!string.IsNullOrWhiteSpace(parsedExperienceId))
            {
                currentExperienceId = parsedExperienceId;
            }
            else if (string.IsNullOrWhiteSpace(currentExperienceId))
            {
                currentExperienceId = DefaultExperienceId;
            }
        }

        private static string ParseExperienceId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return DefaultExperienceId;
            }

            var lowerUrl = url.Trim().ToLowerInvariant();
            if (lowerUrl.Contains("chapel"))
            {
                return "chapel";
            }

            const string queryKey = "experience=";
            var queryIndex = lowerUrl.IndexOf(queryKey, System.StringComparison.Ordinal);
            if (queryIndex >= 0)
            {
                var start = queryIndex + queryKey.Length;
                var end = lowerUrl.IndexOf('&', start);
                return end >= 0 ? lowerUrl.Substring(start, end - start) : lowerUrl.Substring(start);
            }

            return DefaultExperienceId;
        }
    }
}
