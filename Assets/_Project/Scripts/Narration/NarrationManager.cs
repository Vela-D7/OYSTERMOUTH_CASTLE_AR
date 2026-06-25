using System;
using OystermouthCastle.Analytics;
using OystermouthCastle.Core;
using OystermouthCastle.Localization;
using UnityEngine;

namespace OystermouthCastle.Narration
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class NarrationManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private TrackingQualityManager trackingQualityManager;
        [SerializeField] private AppLanguage language = AppLanguage.English;
        [SerializeField] private NarrationTrack[] tracks;
        [SerializeField] private AudioClip chapelAmbientClip;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.18f;
        [SerializeField] private bool playAmbientOnEnable = true;
        [SerializeField] private bool autoPauseWhenTrackingLost = true;

        public event Action<NarrationTrack, string> SubtitleChanged;
        public event Action<NarrationTrack> TrackStarted;
        public event Action<NarrationTrack> TrackCompleted;
        public event Action<bool> PlaybackStateChanged;
        public event Action<AppLanguage> LanguageChanged;

        private NarrationTrack currentTrack;
        private bool isPaused;
        private bool pausedByTrackingLoss;
        private AudioClip generatedAmbientClip;

        public NarrationTrack CurrentTrack => currentTrack;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public bool IsPaused => isPaused;
        public bool IsTrackingSuspended => pausedByTrackingLoss;
        public AppLanguage Language => language;
        public float CurrentTime => audioSource != null ? audioSource.time : 0f;
        public float Duration => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
        public float Progress => Duration > 0f ? Mathf.Clamp01(CurrentTime / Duration) : 0f;

        private void Reset()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            trackingQualityManager = FindObjectOfType<TrackingQualityManager>();
        }

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (trackingQualityManager == null)
            {
                trackingQualityManager = FindObjectOfType<TrackingQualityManager>();
            }

            ConfigureNarrationSource();
            ConfigureAmbientSource();
        }

        private void OnEnable()
        {
            if (trackingQualityManager != null)
            {
                trackingQualityManager.TrackingQualityChanged += HandleTrackingQualityChanged;
                HandleTrackingQualityChanged();
            }

            if (playAmbientOnEnable)
            {
                StartAmbient();
            }
        }

        private void OnDisable()
        {
            if (trackingQualityManager != null)
            {
                trackingQualityManager.TrackingQualityChanged -= HandleTrackingQualityChanged;
            }
        }

        private void Update()
        {
            if (currentTrack != null && !isPaused && !audioSource.isPlaying && HasReachedClipEnd())
            {
                var completed = currentTrack;
                currentTrack = null;
                AnalyticsManager.Instance.TrackNarrationCompletion(completed.TrackId, language.ToString());
                TrackCompleted?.Invoke(completed);
                PlaybackStateChanged?.Invoke(false);
            }
        }

        public void SetLanguage(AppLanguage newLanguage)
        {
            if (language == newLanguage)
            {
                return;
            }

            var wasPlaying = IsPlaying;
            var wasPaused = isPaused;
            var normalizedTime = Progress;
            language = newLanguage;
            LanguageChanged?.Invoke(language);

            if (currentTrack == null)
            {
                return;
            }

            var clip = currentTrack.GetAudio(language);
            if (clip == null)
            {
                Debug.LogWarning($"Narration track '{currentTrack.TrackId}' has no audio for {language}.");
                SubtitleChanged?.Invoke(currentTrack, currentTrack.GetSubtitle(language));
                return;
            }

            audioSource.clip = clip;
            audioSource.time = Mathf.Clamp(normalizedTime * clip.length, 0f, Mathf.Max(0f, clip.length - 0.05f));
            SubtitleChanged?.Invoke(currentTrack, currentTrack.GetSubtitle(language));
            TrackStarted?.Invoke(currentTrack);

            if (wasPlaying && !IsTrackingCurrentlyLost())
            {
                audioSource.Play();
                isPaused = false;
                PlaybackStateChanged?.Invoke(true);
            }
            else
            {
                audioSource.Play();
                audioSource.Pause();
                isPaused = wasPaused || pausedByTrackingLoss;
                PlaybackStateChanged?.Invoke(false);
            }
        }

        public void PlayTrack(string trackId)
        {
            if (tracks == null)
            {
                Debug.LogWarning($"Narration track '{trackId}' was not found because no tracks are configured.");
                return;
            }

            for (var i = 0; i < tracks.Length; i++)
            {
                if (tracks[i] != null && tracks[i].TrackId == trackId)
                {
                    PlayTrack(tracks[i]);
                    return;
                }
            }

            Debug.LogWarning($"Narration track '{trackId}' was not found.");
        }

        public void PlayTrack(NarrationTrack track)
        {
            if (track == null)
            {
                return;
            }

            var clip = track.GetAudio(language);
            if (clip == null)
            {
                Debug.LogWarning($"Narration track '{track.TrackId}' has no audio for {language}.");
                return;
            }

            currentTrack = track;
            isPaused = false;
            pausedByTrackingLoss = false;
            audioSource.clip = clip;
            audioSource.Play();

            AnalyticsManager.Instance.TrackNarrationStarted(language.ToString());
            SubtitleChanged?.Invoke(track, track.GetSubtitle(language));
            TrackStarted?.Invoke(track);
            PlaybackStateChanged?.Invoke(true);
        }

        public void TogglePlayPause()
        {
            if (currentTrack == null)
            {
                if (tracks != null && tracks.Length > 0)
                {
                    PlayTrack(tracks[0]);
                }

                return;
            }

            if (audioSource.isPlaying)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        public void Pause()
        {
            if (audioSource == null || !audioSource.isPlaying)
            {
                return;
            }

            audioSource.Pause();
            isPaused = true;
            pausedByTrackingLoss = false;
            PlaybackStateChanged?.Invoke(false);
        }

        public void Resume()
        {
            if (audioSource == null || currentTrack == null)
            {
                return;
            }

            if (IsTrackingCurrentlyLost())
            {
                pausedByTrackingLoss = true;
                isPaused = true;
                PlaybackStateChanged?.Invoke(false);
                return;
            }

            pausedByTrackingLoss = false;
            isPaused = false;
            audioSource.UnPause();
            PlaybackStateChanged?.Invoke(true);
        }

        public void Replay()
        {
            if (currentTrack != null)
            {
                if (IsTrackingCurrentlyLost())
                {
                    pausedByTrackingLoss = true;
                    isPaused = true;
                    PlaybackStateChanged?.Invoke(false);
                    return;
                }

                audioSource.time = 0f;
                isPaused = false;
                pausedByTrackingLoss = false;
                audioSource.Play();
                PlaybackStateChanged?.Invoke(true);
            }
            else if (tracks != null && tracks.Length > 0)
            {
                PlayTrack(tracks[0]);
            }
        }

        public void SeekNormalized(float normalizedTime)
        {
            if (audioSource == null || audioSource.clip == null)
            {
                return;
            }

            audioSource.time = Mathf.Clamp01(normalizedTime) * audioSource.clip.length;
        }

        public void Stop()
        {
            audioSource.Stop();
            currentTrack = null;
            isPaused = false;
            pausedByTrackingLoss = false;
            SubtitleChanged?.Invoke(null, string.Empty);
            PlaybackStateChanged?.Invoke(false);
        }

        public void StartAmbient()
        {
            if (ambientAudioSource == null)
            {
                return;
            }

            if (ambientAudioSource.clip == null)
            {
                ambientAudioSource.clip = chapelAmbientClip != null ? chapelAmbientClip : GetOrCreateGeneratedAmbientClip();
            }

            if (ambientAudioSource.clip != null && !ambientAudioSource.isPlaying)
            {
                ambientAudioSource.Play();
            }
        }

        public void StopAmbient()
        {
            if (ambientAudioSource != null)
            {
                ambientAudioSource.Stop();
            }
        }

        private void HandleTrackingQualityChanged()
        {
            if (!autoPauseWhenTrackingLost || currentTrack == null)
            {
                return;
            }

            if (IsTrackingCurrentlyLost())
            {
                if (IsPlaying)
                {
                    audioSource.Pause();
                    isPaused = true;
                    pausedByTrackingLoss = true;
                    PlaybackStateChanged?.Invoke(false);
                }

                return;
            }

            if (pausedByTrackingLoss)
            {
                pausedByTrackingLoss = false;
                Resume();
            }
        }

        private bool HasReachedClipEnd()
        {
            return audioSource != null
                   && audioSource.clip != null
                   && audioSource.time >= audioSource.clip.length - 0.05f;
        }

        private bool IsTrackingCurrentlyLost()
        {
            return trackingQualityManager != null &&
                   (trackingQualityManager.IsRecovering ||
                    trackingQualityManager.RelocalizationState == RelocalizationState.Failed ||
                    trackingQualityManager.ConfidenceLevel == TrackingConfidenceLevel.None);
        }

        private void ConfigureNarrationSource()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        private void ConfigureAmbientSource()
        {
            if (ambientAudioSource == null)
            {
                ambientAudioSource = gameObject.AddComponent<AudioSource>();
            }

            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.loop = true;
            ambientAudioSource.spatialBlend = 0f;
            ambientAudioSource.volume = ambientVolume;

            if (chapelAmbientClip != null)
            {
                ambientAudioSource.clip = chapelAmbientClip;
            }
        }

        private AudioClip GetOrCreateGeneratedAmbientClip()
        {
            if (generatedAmbientClip != null)
            {
                return generatedAmbientClip;
            }

            const int sampleRate = 24000;
            const int seconds = 8;
            var samples = sampleRate * seconds;
            var data = new float[samples];

            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var slowAir = Mathf.Sin(2f * Mathf.PI * 87f * t) * 0.018f;
                var roomHum = Mathf.Sin(2f * Mathf.PI * 174f * t + 0.7f) * 0.01f;
                var shimmer = Mathf.Sin(2f * Mathf.PI * 349f * t + Mathf.Sin(t * 0.35f)) * 0.004f;
                var breath = Mathf.PerlinNoise(t * 0.18f, 0.37f) * 0.012f - 0.006f;
                data[i] = (slowAir + roomHum + shimmer + breath) * 0.6f;
            }

            generatedAmbientClip = AudioClip.Create("Generated Chapel Ambient", samples, 1, sampleRate, false);
            generatedAmbientClip.SetData(data, 0);
            return generatedAmbientClip;
        }
    }
}
