using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OystermouthCastle.Experience
{
    public sealed class BeforeAfterController : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] private float reconstructionAmount = 1f;
        [SerializeField] private Slider slider;
        [SerializeField] private Renderer[] reconstructionRenderers;
        [SerializeField] private bool controlGameObjectVisibility = true;

        private readonly List<Material> runtimeMaterials = new List<Material>();

        public float ReconstructionAmount => reconstructionAmount;

        private void Awake()
        {
            CacheRuntimeMaterials();
            if (slider != null)
            {
                slider.onValueChanged.AddListener(SetReconstructionAmount);
                slider.value = reconstructionAmount;
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(SetReconstructionAmount);
            }
        }

        private void OnValidate()
        {
            ApplyAmount();
        }

        public void SetReconstructionAmount(float value)
        {
            reconstructionAmount = Mathf.Clamp01(value);
            ApplyAmount();
        }

        public void SetSlider(Slider value)
        {
            if (slider == value)
            {
                return;
            }

            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(SetReconstructionAmount);
            }

            slider = value;
            if (slider != null)
            {
                slider.SetValueWithoutNotify(reconstructionAmount);
                slider.onValueChanged.AddListener(SetReconstructionAmount);
            }
        }

        private void CacheRuntimeMaterials()
        {
            runtimeMaterials.Clear();
            for (var i = 0; i < reconstructionRenderers.Length; i++)
            {
                var targetRenderer = reconstructionRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                var materials = targetRenderer.materials;
                runtimeMaterials.AddRange(materials);
            }
        }

        private void ApplyAmount()
        {
            var visible = reconstructionAmount > 0.01f;
            for (var i = 0; i < reconstructionRenderers.Length; i++)
            {
                var targetRenderer = reconstructionRenderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.enabled = !controlGameObjectVisibility || visible;

                var materials = Application.isPlaying ? targetRenderer.materials : targetRenderer.sharedMaterials;
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    ApplyMaterialAlpha(materials[materialIndex], reconstructionAmount);
                }
            }
        }

        private static void ApplyMaterialAlpha(Material material, float alpha)
        {
            if (material == null || !material.HasProperty("_BaseColor"))
            {
                return;
            }

            var color = material.GetColor("_BaseColor");
            color.a = alpha;
            material.SetColor("_BaseColor", color);
        }
    }
}
