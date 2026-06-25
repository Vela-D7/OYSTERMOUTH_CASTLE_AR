using System.Collections.Generic;
using UnityEngine;

namespace OystermouthCastle.Core
{
    [CreateAssetMenu(menuName = "Oystermouth/Experience Registry", fileName = "ExperienceRegistry")]
    public sealed class ExperienceRegistry : ScriptableObject
    {
        [SerializeField] private List<ExperienceDefinition> experiences = new List<ExperienceDefinition>();
        [SerializeField] private ExperienceDefinition fallbackExperience;

        public IReadOnlyList<ExperienceDefinition> Experiences => experiences;
        public ExperienceDefinition FallbackExperience => fallbackExperience;

        public bool TryFind(string experienceIdOrAlias, out ExperienceDefinition experience)
        {
            experience = null;

            for (var i = 0; i < experiences.Count; i++)
            {
                var candidate = experiences[i];
                if (candidate != null && candidate.Matches(experienceIdOrAlias))
                {
                    experience = candidate;
                    return true;
                }
            }

            return false;
        }

        public ExperienceDefinition ResolveOrFallback(string experienceIdOrAlias)
        {
            return TryFind(experienceIdOrAlias, out var experience) ? experience : fallbackExperience;
        }
    }
}
