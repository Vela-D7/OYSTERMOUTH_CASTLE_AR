using OystermouthCastle.Core;
using UnityEngine;

namespace OystermouthCastle.Content
{
    [CreateAssetMenu(menuName = "Oystermouth/Info Panel Content", fileName = "InfoPanelContent")]
    public sealed class InfoPanelContent : ScriptableObject
    {
        [SerializeField] private string contentId = "chapel-history";
        [SerializeField] private LocalizedText title;
        [SerializeField] private LocalizedText body;
        [SerializeField] private Sprite[] images;
        [SerializeField] private AudioClip englishAudio;
        [SerializeField] private AudioClip welshAudio;
        [TextArea(2, 8)] [SerializeField] private string researchSources;

        public string ContentId => contentId;
        public LocalizedText Title => title;
        public LocalizedText Body => body;
        public Sprite[] Images => images;
        public string ResearchSources => researchSources;

        public AudioClip GetAudio(AppLanguage language)
        {
            return language == AppLanguage.Welsh && welshAudio != null ? welshAudio : englishAudio;
        }
    }
}
