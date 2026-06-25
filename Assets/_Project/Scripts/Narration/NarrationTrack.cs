using OystermouthCastle.Core;
using UnityEngine;

namespace OystermouthCastle.Narration
{
    [CreateAssetMenu(menuName = "Oystermouth/Narration Track", fileName = "NarrationTrack")]
    public sealed class NarrationTrack : ScriptableObject
    {
        [SerializeField] private string trackId = "chapel-introduction";
        [SerializeField] private LocalizedText title;
        [SerializeField] private AudioClip englishAudio;
        [SerializeField] private AudioClip welshAudio;
        [TextArea(4, 12)] [SerializeField] private string englishSubtitle;
        [TextArea(4, 12)] [SerializeField] private string welshSubtitle;

        public string TrackId => trackId;
        public LocalizedText Title => title;

        public AudioClip GetAudio(AppLanguage language)
        {
            return language == AppLanguage.Welsh && welshAudio != null ? welshAudio : englishAudio;
        }

        public string GetSubtitle(AppLanguage language)
        {
            if (language == AppLanguage.Welsh && !string.IsNullOrWhiteSpace(welshSubtitle))
            {
                return welshSubtitle;
            }

            return englishSubtitle ?? string.Empty;
        }
    }
}
