using System;
using System.Collections.Generic;
using UnityEngine;

namespace OystermouthCastle.Core
{
    [CreateAssetMenu(menuName = "Oystermouth/Experience Definition", fileName = "ExperienceDefinition")]
    public sealed class ExperienceDefinition : ScriptableObject
    {
        [Header("Routing")]
        [SerializeField] private string experienceId = "chapel-fresco";
        [SerializeField] private string sceneName = "QR";
        [SerializeField] private List<string> qrAliases = new List<string> { "chapel", "chapel-fresco" };

        [Header("Display")]
        [SerializeField] private LocalizedText title;
        [SerializeField] private LocalizedText shortDescription;
        [SerializeField] private Sprite thumbnail;

        [Header("Availability")]
        [SerializeField] private bool availableOffline = true;
        [SerializeField] private bool requiresAreaTarget = true;

        public string ExperienceId => experienceId;
        public string SceneName => sceneName;
        public IReadOnlyList<string> QrAliases => qrAliases;
        public LocalizedText Title => title;
        public LocalizedText ShortDescription => shortDescription;
        public Sprite Thumbnail => thumbnail;
        public bool AvailableOffline => availableOffline;
        public bool RequiresAreaTarget => requiresAreaTarget;

        public bool Matches(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = Normalize(value);
            if (Normalize(experienceId) == normalized || Normalize(sceneName) == normalized)
            {
                return true;
            }

            for (var i = 0; i < qrAliases.Count; i++)
            {
                if (Normalize(qrAliases[i]) == normalized)
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }

        private void OnValidate()
        {
            experienceId = SanitizeId(experienceId);
        }

        private static string SanitizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "experience";
            }

            return value.Trim().ToLowerInvariant().Replace(' ', '-');
        }
    }
}
