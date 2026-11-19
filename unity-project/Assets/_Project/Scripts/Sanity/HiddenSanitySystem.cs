using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Sanity
{
    public class HiddenSanitySystem : MonoBehaviour
    {
        [SerializeField] private float currentSanity = 100.0f;
        [SerializeField] private float darknessDrainPerSecond = 2.4f;
        [SerializeField] private float lanternRecoveryPerSecond = 1.15f;
        [SerializeField] private float mimicProximityDrainPerSecond = 4.2f;

        private SanityState currentState = SanityState.Stable;

        public event Action<SanityState, SanityState> OnSanityStateChanged;
        public event Action<float> OnSanityValueUpdated;

        public float CurrentSanity => currentSanity;
        public SanityState CurrentState => currentState;

        public static SanityState EvaluateStateForValue(float sanityValue)
        {
            float clamped = Mathf.Clamp(sanityValue, 0f, 100f);
            if (clamped >= 75.0f) return SanityState.Stable;
            if (clamped >= 50.0f) return SanityState.Uneasy;
            if (clamped >= 25.0f) return SanityState.Disturbed;
            return SanityState.Breaking;
        }

        public void SetSanity(float value)
        {
            currentSanity = Mathf.Clamp(value, 0f, 100f);
            SanityState newState = EvaluateStateForValue(currentSanity);
            if (newState != currentState)
            {
                SanityState prev = currentState;
                currentState = newState;
                OnSanityStateChanged?.Invoke(prev, currentState);
            }
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.State.SanityValue = currentSanity;
            }
            OnSanityValueUpdated?.Invoke(currentSanity);
        }

        public void ApplySanityDelta(float delta)
        {
            SetSanity(currentSanity + delta);
        }

        public void TickSanity(float deltaTime, bool isLanternLit, bool nearMimicAnomaly, bool nearEchoSafeLight)
        {
            if (deltaTime <= 0f) return;

            float netRate = isLanternLit ? lanternRecoveryPerSecond : -darknessDrainPerSecond;
            if (nearMimicAnomaly) netRate -= mimicProximityDrainPerSecond;
            if (nearEchoSafeLight) netRate += lanternRecoveryPerSecond * 1.35f;

            SetSanity(currentSanity + netRate * deltaTime);
        }

        public float GetNormalizedDistortionFactor()
        {
            return 1.0f - (currentSanity / 100.0f);
        }
    }
}
