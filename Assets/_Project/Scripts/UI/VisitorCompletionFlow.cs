using System;
using System.Collections.Generic;
using System.IO;
using OystermouthCastle.Analytics;
using OystermouthCastle.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OystermouthCastle.UI
{
    public sealed class VisitorCompletionFlow : MonoBehaviour
    {
        private const string RootName = "Visitor Completion Flow";
        private const string DefaultExperienceId = "chapel";
        private const string ExitButtonName = "Exit Button";
        private const string ExitSceneName = "MainMenu";

        private static VisitorCompletionFlow instance;

        private readonly HashSet<Button> boundExitButtons = new HashSet<Button>();
        private Canvas rootCanvas;
        private GameObject thankYouScreen;
        private GameObject ratingScreen;
        private TMP_Text ratingStatusLabel;
        private string activeExperienceId = DefaultExperienceId;
        private int pendingRating;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var root = new GameObject(RootName);
            DontDestroyOnLoad(root);
            instance = root.AddComponent<VisitorCompletionFlow>();
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
            SceneManager.sceneLoaded += HandleSceneLoaded;
            BuildUi();
            BindExitButtons();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HideAll();
            boundExitButtons.Clear();
            BindExitButtons();
        }

        private void BindExitButtons()
        {
            var buttons = FindObjectsOfType<Button>(true);
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null || boundExitButtons.Contains(button) || button.name != ExitButtonName)
                {
                    continue;
                }

                button.onClick.AddListener(ShowThankYouScreen);
                boundExitButtons.Add(button);
            }
        }

        public void ShowThankYouScreen()
        {
            activeExperienceId = ResolveExperienceId();
            pendingRating = 0;
            AnalyticsManager.Instance.TrackVisitorCompletion(activeExperienceId);
            ShowOnly(thankYouScreen);
        }

        public void ShowRatingScreen()
        {
            ratingStatusLabel.text = "Choose a rating to continue.";
            ShowOnly(ratingScreen);
        }

        public void SelectRating(int rating)
        {
            pendingRating = Mathf.Clamp(rating, 1, 5);
            ratingStatusLabel.text = $"Rating selected: {pendingRating}/5";
        }

        public void SubmitRating()
        {
            SaveFeedback(pendingRating, pendingRating <= 0);
            ExitExperience();
        }

        public void SkipRating()
        {
            SaveFeedback(0, true);
            ExitExperience();
        }

        public void ExitExperience()
        {
            HideAll();
            AnalyticsManager.Instance.TrackExitExperience();
            SceneLoader.LoadScene(ExitSceneName);
        }

        private void BuildUi()
        {
            rootCanvas = gameObject.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 5000;
            gameObject.AddComponent<GraphicRaycaster>();

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            thankYouScreen = CreateScreen("Thank You Screen");
            CreateLabel(
                thankYouScreen.transform,
                "Thank you for visiting Oystermouth Castle",
                54,
                FontStyles.Bold,
                new Vector2(0f, 160f),
                new Vector2(820f, 180f));
            CreateLabel(
                thankYouScreen.transform,
                "Your experience is complete. A quick rating helps improve the next visit.",
                32,
                FontStyles.Normal,
                new Vector2(0f, 20f),
                new Vector2(760f, 160f));
            CreateButton(thankYouScreen.transform, "Continue", new Vector2(0f, -170f), ShowRatingScreen);

            ratingScreen = CreateScreen("Rate Experience Screen");
            CreateLabel(
                ratingScreen.transform,
                "Rate Experience",
                54,
                FontStyles.Bold,
                new Vector2(0f, 230f),
                new Vector2(820f, 120f));
            CreateLabel(
                ratingScreen.transform,
                "How was this AR visit?",
                34,
                FontStyles.Normal,
                new Vector2(0f, 130f),
                new Vector2(760f, 100f));

            var ratingRow = CreateRect("Rating Buttons", ratingScreen.transform, new Vector2(0f, 10f), new Vector2(760f, 110f));
            var layout = ratingRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            for (var rating = 1; rating <= 5; rating++)
            {
                var capturedRating = rating;
                CreateRatingButton(ratingRow, capturedRating);
            }

            ratingStatusLabel = CreateLabel(
                ratingScreen.transform,
                "Choose a rating to continue.",
                28,
                FontStyles.Normal,
                new Vector2(0f, -115f),
                new Vector2(760f, 80f));
            CreateButton(ratingScreen.transform, "Submit", new Vector2(-170f, -255f), SubmitRating);
            CreateButton(ratingScreen.transform, "Skip", new Vector2(170f, -255f), SkipRating);

            HideAll();
        }

        private GameObject CreateScreen(string name)
        {
            var screen = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            screen.transform.SetParent(transform, false);

            var rect = screen.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = screen.GetComponent<Image>();
            image.color = new Color32(36, 39, 40, 238);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(screen.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(920f, 820f);
            panel.GetComponent<Image>().color = new Color32(242, 231, 201, 255);

            return screen;
        }

        private TMP_Text CreateLabel(Transform parent, string text, float size, FontStyles style, Vector2 position, Vector2 sizeDelta)
        {
            var rect = CreateRect("Label", parent, position, sizeDelta);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.color = new Color32(37, 42, 44, 255);
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            return label;
        }

        private Button CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var rect = CreateRect(text + " Button", parent, position, new Vector2(300f, 92f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color32(77, 111, 130, 255);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            var label = CreateLabel(rect, text, 30f, FontStyles.Bold, Vector2.zero, new Vector2(260f, 68f));
            label.color = Color.white;
            return button;
        }

        private void CreateRatingButton(Transform parent, int rating)
        {
            var rect = CreateRect(rating.ToString(), parent, Vector2.zero, new Vector2(110f, 92f));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color32(181, 138, 58, 255);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectRating(rating));

            var label = CreateLabel(rect, rating.ToString(), 34f, FontStyles.Bold, Vector2.zero, new Vector2(90f, 68f));
            label.color = Color.white;
        }

        private RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 sizeDelta)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = sizeDelta;
            return rect;
        }

        private void ShowOnly(GameObject screen)
        {
            rootCanvas.enabled = true;
            thankYouScreen.SetActive(screen == thankYouScreen);
            ratingScreen.SetActive(screen == ratingScreen);
        }

        private void HideAll()
        {
            if (thankYouScreen != null)
            {
                thankYouScreen.SetActive(false);
            }

            if (ratingScreen != null)
            {
                ratingScreen.SetActive(false);
            }

            if (rootCanvas != null)
            {
                rootCanvas.enabled = false;
            }
        }

        private string ResolveExperienceId()
        {
            var experienceManager = FindObjectOfType<ExperienceManager>();
            return experienceManager != null ? experienceManager.CurrentExperienceId : DefaultExperienceId;
        }

        private void SaveFeedback(int rating, bool skipped)
        {
            var feedback = new VisitorFeedback
            {
                experienceId = activeExperienceId,
                rating = rating,
                skipped = skipped,
                sceneName = SceneManager.GetActiveScene().name,
                utcTimestamp = DateTime.UtcNow.ToString("o")
            };

            AnalyticsManager.Instance.TrackExperienceRating(activeExperienceId, rating, skipped);

            try
            {
                var json = JsonUtility.ToJson(feedback);
                var path = Path.Combine(Application.persistentDataPath, "visitor-feedback.jsonl");
                File.AppendAllText(path, json + Environment.NewLine);
                Debug.Log($"Visitor feedback saved: {path}");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Visitor feedback could not be saved locally: {exception.Message}");
            }
        }

        [Serializable]
        private struct VisitorFeedback
        {
            public string experienceId;
            public int rating;
            public bool skipped;
            public string sceneName;
            public string utcTimestamp;
        }
    }
}
