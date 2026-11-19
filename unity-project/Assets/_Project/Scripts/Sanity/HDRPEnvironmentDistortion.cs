using UnityEngine;
using Undertow.Core;

namespace Undertow.Sanity
{
    public class HDRPEnvironmentDistortion : MonoBehaviour
    {
        [SerializeField] private HiddenSanitySystem sanitySystem;
        [SerializeField] private Transform[] distortableCorridorSegments;

        public float CurrentChromaticAberration { get; private set; } = 0.04f;
        public float CurrentFogMeanFreePathMeters { get; private set; } = 28.0f;
        public float CurrentVignetteIntensity { get; private set; } = 0.22f;
        public float CurrentCorridorStretchMultiplier { get; private set; } = 1.0f;

        private void Awake()
        {
            if (sanitySystem == null)
                sanitySystem = GetComponent<HiddenSanitySystem>();
        }

        private void OnEnable()
        {
            if (sanitySystem == null)
                sanitySystem = GetComponent<HiddenSanitySystem>();
            if (sanitySystem != null)
                sanitySystem.OnSanityValueUpdated += UpdateDistortionFromSanity;
        }

        private void OnDisable()
        {
            if (sanitySystem != null)
                sanitySystem.OnSanityValueUpdated -= UpdateDistortionFromSanity;
        }

        public void UpdateDistortionFromSanity(float sanityValue)
        {
            SanityState state = HiddenSanitySystem.EvaluateStateForValue(sanityValue);

            switch (state)
            {
                case SanityState.Stable:
                    CurrentChromaticAberration = 0.04f;
                    CurrentFogMeanFreePathMeters = 28.0f;
                    CurrentVignetteIntensity = 0.22f;
                    CurrentCorridorStretchMultiplier = 1.0f;
                    break;
                case SanityState.Uneasy:
                    CurrentChromaticAberration = 0.16f;
                    CurrentFogMeanFreePathMeters = 22.0f;
                    CurrentVignetteIntensity = 0.31f;
                    CurrentCorridorStretchMultiplier = 1.025f;
                    break;
                case SanityState.Disturbed:
                    CurrentChromaticAberration = 0.38f;
                    CurrentFogMeanFreePathMeters = 15.5f;
                    CurrentVignetteIntensity = 0.42f;
                    CurrentCorridorStretchMultiplier = 1.065f;
                    break;
                case SanityState.Breaking:
                    CurrentChromaticAberration = 0.68f;
                    CurrentFogMeanFreePathMeters = 9.5f;
                    CurrentVignetteIntensity = 0.56f;
                    CurrentCorridorStretchMultiplier = 1.12f;
                    break;
            }

            if (distortableCorridorSegments != null)
            {
                for (int i = 0; i < distortableCorridorSegments.Length; i++)
                {
                    if (distortableCorridorSegments[i] != null)
                    {
                        distortableCorridorSegments[i].localScale = new Vector3(1f, 1f, CurrentCorridorStretchMultiplier);
                    }
                }
            }
        }
    }
}
