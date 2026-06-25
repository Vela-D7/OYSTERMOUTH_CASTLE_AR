using OystermouthCastle.Core;
using UnityEngine;

namespace OystermouthCastle.Accessibility
{
    public sealed class AccessibilityManager : MonoBehaviour
    {
        private const string LanguageKey = "oystermouth.language";
        private const string SubtitlesKey = "oystermouth.subtitles";
        private const string LargeTextKey = "oystermouth.largeText";
        private const string AudioOnlyKey = "oystermouth.audioOnly";

        [SerializeField] private AppLanguage language = AppLanguage.English;
        [SerializeField] private bool subtitlesEnabled = true;
        [SerializeField] private bool largeTextMode;
        [SerializeField] private bool audioOnlyMode;

        public AppLanguage Language => language;
        public bool SubtitlesEnabled => subtitlesEnabled;
        public bool LargeTextMode => largeTextMode;
        public bool AudioOnlyMode => audioOnlyMode;

        private void Awake()
        {
            Load();
        }

        public void SetLanguage(AppLanguage value)
        {
            language = value;
            Save();
        }

        public void SetSubtitlesEnabled(bool value)
        {
            subtitlesEnabled = value;
            Save();
        }

        public void SetLargeTextMode(bool value)
        {
            largeTextMode = value;
            Save();
        }

        public void SetAudioOnlyMode(bool value)
        {
            audioOnlyMode = value;
            Save();
        }

        private void Load()
        {
            language = (AppLanguage)PlayerPrefs.GetInt(LanguageKey, (int)language);
            subtitlesEnabled = PlayerPrefs.GetInt(SubtitlesKey, subtitlesEnabled ? 1 : 0) == 1;
            largeTextMode = PlayerPrefs.GetInt(LargeTextKey, largeTextMode ? 1 : 0) == 1;
            audioOnlyMode = PlayerPrefs.GetInt(AudioOnlyKey, audioOnlyMode ? 1 : 0) == 1;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(LanguageKey, (int)language);
            PlayerPrefs.SetInt(SubtitlesKey, subtitlesEnabled ? 1 : 0);
            PlayerPrefs.SetInt(LargeTextKey, largeTextMode ? 1 : 0);
            PlayerPrefs.SetInt(AudioOnlyKey, audioOnlyMode ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
