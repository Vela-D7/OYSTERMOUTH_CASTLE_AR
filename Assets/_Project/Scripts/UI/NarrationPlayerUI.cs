using OystermouthCastle.Narration;
using OystermouthCastle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OystermouthCastle.UI
{
    public sealed class NarrationPlayerUI : MonoBehaviour
    {
        [SerializeField] private NarrationManager narrationManager;
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private TMP_Text playPauseLabel;
        [SerializeField] private AppLanguage language = AppLanguage.English;

        private bool suppressSliderEvent;
        private int lastCurrentSecond = -1;
        private int lastDurationSecond = -1;

        private void Awake()
        {
            if (playPauseButton != null)
            {
                playPauseButton.onClick.AddListener(HandlePlayPauseClicked);
            }

            if (replayButton != null)
            {
                replayButton.onClick.AddListener(HandleReplayClicked);
            }

            if (progressSlider != null)
            {
                progressSlider.minValue = 0f;
                progressSlider.maxValue = 1f;
                progressSlider.onValueChanged.AddListener(HandleProgressChanged);
            }
        }

        private void OnEnable()
        {
            if (narrationManager != null)
            {
                narrationManager.TrackStarted += HandleTrackStarted;
                narrationManager.TrackCompleted += HandleTrackCompleted;
                narrationManager.PlaybackStateChanged += HandlePlaybackStateChanged;
                narrationManager.LanguageChanged += HandleLanguageChanged;
                language = narrationManager.Language;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (narrationManager != null)
            {
                narrationManager.TrackStarted -= HandleTrackStarted;
                narrationManager.TrackCompleted -= HandleTrackCompleted;
                narrationManager.PlaybackStateChanged -= HandlePlaybackStateChanged;
                narrationManager.LanguageChanged -= HandleLanguageChanged;
            }
        }

        private void Update()
        {
            RefreshProgress();
        }

        private void HandlePlayPauseClicked()
        {
            narrationManager?.TogglePlayPause();
        }

        private void HandleReplayClicked()
        {
            narrationManager?.Replay();
        }

        private void HandleProgressChanged(float value)
        {
            if (suppressSliderEvent)
            {
                return;
            }

            narrationManager?.SeekNormalized(value);
        }

        private void HandleTrackStarted(NarrationTrack track)
        {
            if (titleLabel != null)
            {
                titleLabel.text = track != null ? track.Title.Get(language) : string.Empty;
            }

            Refresh();
        }

        private void HandleTrackCompleted(NarrationTrack track)
        {
            Refresh();
        }

        private void HandlePlaybackStateChanged(bool isPlaying)
        {
            Refresh();
        }

        private void HandleLanguageChanged(AppLanguage value)
        {
            SetLanguage(value);
        }

        private void Refresh()
        {
            RefreshProgress();

            if (playPauseLabel != null)
            {
                playPauseLabel.text = narrationManager != null && narrationManager.IsPlaying ? "Pause" : "Play";
            }
        }

        private void RefreshProgress()
        {
            if (narrationManager == null)
            {
                return;
            }

            if (progressSlider != null)
            {
                suppressSliderEvent = true;
                progressSlider.value = narrationManager.Progress;
                suppressSliderEvent = false;
            }

            if (timeLabel != null)
            {
                var currentSecond = Mathf.Max(0, Mathf.FloorToInt(narrationManager.CurrentTime));
                var durationSecond = Mathf.Max(0, Mathf.FloorToInt(narrationManager.Duration));

                if (currentSecond != lastCurrentSecond || durationSecond != lastDurationSecond)
                {
                    lastCurrentSecond = currentSecond;
                    lastDurationSecond = durationSecond;
                    timeLabel.text = $"{FormatTime(currentSecond)} / {FormatTime(durationSecond)}";
                }
            }
        }

        public void SetLanguage(AppLanguage value)
        {
            language = value;
            if (titleLabel != null && narrationManager != null && narrationManager.CurrentTrack != null)
            {
                titleLabel.text = narrationManager.CurrentTrack.Title.Get(language);
            }
        }

        private static string FormatTime(int seconds)
        {
            return $"{seconds / 60:00}:{seconds % 60:00}";
        }
    }
}
