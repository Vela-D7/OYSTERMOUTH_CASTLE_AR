using OystermouthCastle.Experience;
using OystermouthCastle.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OystermouthCastle.UI
{
    public sealed class BeforeAfterSliderPresenter : MonoBehaviour
    {
        [SerializeField] private BeforeAfterController beforeAfterController;
        [SerializeField] private Transform parentOverride;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text currentLabel;
        [SerializeField] private TMP_Text reconstructedLabel;
        [SerializeField] private GameObject legacyToggleButton;
        [SerializeField] private Vector2 anchoredPosition = new Vector2(0f, 136f);
        [SerializeField] private Vector2 panelSize = new Vector2(720f, 118f);
        [SerializeField] private bool createRuntimeUi = true;
        [SerializeField] private bool hideLegacyToggle = true;

        private const string RuntimeRootName = "BeforeAfterComparisonSlider";
        private bool analyticsTrackedForDrag;

        private void Reset()
        {
            beforeAfterController = FindObjectOfType<BeforeAfterController>();
        }

        private void Awake()
        {
            if (beforeAfterController == null)
            {
                beforeAfterController = FindObjectOfType<BeforeAfterController>();
            }

            if (legacyToggleButton == null)
            {
                legacyToggleButton = GameObject.Find("Before After Button");
            }

            if (hideLegacyToggle && legacyToggleButton != null)
            {
                legacyToggleButton.SetActive(false);
            }

            if (createRuntimeUi && slider == null)
            {
                CreateRuntimeUi();
            }

            WireSlider();
            RefreshLabels();
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(HandleSliderChanged);
            }
        }

        private void CreateRuntimeUi()
        {
            var parent = parentOverride != null ? parentOverride : FindHudParent();
            if (parent == null)
            {
                return;
            }

            var existing = parent.Find(RuntimeRootName);
            if (existing != null)
            {
                slider = existing.GetComponentInChildren<Slider>(true);
                var labels = existing.GetComponentsInChildren<TMP_Text>(true);
                for (var i = 0; i < labels.Length; i++)
                {
                    if (labels[i].name == "Current Chapel Label")
                    {
                        currentLabel = labels[i];
                    }
                    else if (labels[i].name == "Reconstructed Chapel Label")
                    {
                        reconstructedLabel = labels[i];
                    }
                }

                return;
            }

            var root = new GameObject(RuntimeRootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(parent, false);

            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = panelSize;

            var background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.09f, 0.08f, 0.72f);
            background.raycastTarget = true;

            currentLabel = CreateLabel("Current Chapel Label", root.transform, "Current Chapel", TextAlignmentOptions.Left);
            reconstructedLabel = CreateLabel("Reconstructed Chapel Label", root.transform, "Reconstructed Chapel", TextAlignmentOptions.Right);
            CreateArrowLabel("Left Arrow", root.transform, "<");
            CreateArrowLabel("Right Arrow", root.transform, ">");
            slider = CreateSlider(root.transform);
        }

        private Transform FindHudParent()
        {
            var arHudLayer = GameObject.Find("ARHudLayer");
            if (arHudLayer != null)
            {
                return arHudLayer.transform;
            }

            var canvas = FindObjectOfType<Canvas>();
            return canvas != null ? canvas.transform : null;
        }

        private TMP_Text CreateLabel(string name, Transform parent, string text, TextAlignmentOptions alignment)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = alignment == TextAlignmentOptions.Left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rect.anchorMax = alignment == TextAlignmentOptions.Left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rect.pivot = alignment == TextAlignmentOptions.Left ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rect.anchoredPosition = alignment == TextAlignmentOptions.Left ? new Vector2(28f, -18f) : new Vector2(-28f, -18f);
            rect.sizeDelta = new Vector2(280f, 34f);

            var label = labelObject.GetComponent<TMP_Text>();
            label.text = text;
            label.fontSize = 21f;
            label.alignment = alignment;
            label.color = new Color(0.95f, 0.91f, 0.79f, 1f);
            label.enableWordWrapping = false;
            return label;
        }

        private void CreateArrowLabel(string name, Transform parent, string text)
        {
            var labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);

            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(text == "<" ? 0f : 1f, 0.5f);
            rect.anchorMax = new Vector2(text == "<" ? 0f : 1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(text == "<" ? 40f : -40f, -20f);
            rect.sizeDelta = new Vector2(32f, 32f);

            var label = labelObject.GetComponent<TMP_Text>();
            label.text = text;
            label.fontSize = 26f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.71f, 0.54f, 0.23f, 1f);
            label.enableWordWrapping = false;
        }

        private Slider CreateSlider(Transform parent)
        {
            var sliderObject = new GameObject("Current Reconstruction Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);

            var sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0f, -22f);
            sliderRect.sizeDelta = new Vector2(560f, 42f);

            var background = CreateSliderGraphic("Background", sliderObject.transform, new Color(0.95f, 0.91f, 0.79f, 0.36f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(10f, 12f);
            fillAreaRect.offsetMax = new Vector2(-10f, -12f);

            var fill = CreateSliderGraphic("Fill", fillArea.transform, new Color(0.71f, 0.54f, 0.23f, 0.94f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            var handle = CreateSliderGraphic("Handle", sliderObject.transform, new Color(0.95f, 0.91f, 0.79f, 1f), new Vector2(0.5f, 0.5f), new Vector2(34f, 44f));

            var createdSlider = sliderObject.GetComponent<Slider>();
            createdSlider.minValue = 0f;
            createdSlider.maxValue = 1f;
            createdSlider.wholeNumbers = false;
            createdSlider.targetGraphic = handle;
            createdSlider.fillRect = fill.rectTransform;
            createdSlider.handleRect = handle.rectTransform;
            createdSlider.value = beforeAfterController != null ? beforeAfterController.ReconstructionAmount : 1f;

            background.raycastTarget = true;
            fill.raycastTarget = false;
            handle.raycastTarget = true;
            return createdSlider;
        }

        private Image CreateSliderGraphic(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var graphicObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            graphicObject.transform.SetParent(parent, false);

            var rect = graphicObject.GetComponent<RectTransform>();
            rect.anchorMin = sizeDelta == Vector2.zero ? Vector2.zero : new Vector2(0.5f, 0.5f);
            rect.anchorMax = sizeDelta == Vector2.zero ? Vector2.one : new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta == Vector2.zero ? Vector2.zero : sizeDelta;
            if (sizeDelta == Vector2.zero)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var image = graphicObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private void WireSlider()
        {
            if (slider == null || beforeAfterController == null)
            {
                return;
            }

            slider.onValueChanged.RemoveListener(HandleSliderChanged);
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(beforeAfterController.ReconstructionAmount);
            slider.onValueChanged.AddListener(HandleSliderChanged);
            beforeAfterController.SetSlider(slider);
        }

        private void HandleSliderChanged(float value)
        {
            if (beforeAfterController != null)
            {
                beforeAfterController.SetReconstructionAmount(value);
            }

            if (!analyticsTrackedForDrag)
            {
                analyticsTrackedForDrag = true;
                AnalyticsManager.Instance.TrackBeforeAfterUsed();
            }
        }

        private void RefreshLabels()
        {
            if (currentLabel != null)
            {
                currentLabel.text = "Current Chapel";
            }

            if (reconstructedLabel != null)
            {
                reconstructedLabel.text = "Reconstructed Chapel";
            }
        }
    }
}
