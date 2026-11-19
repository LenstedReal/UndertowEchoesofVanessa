using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Lantern
{
    public class LanternController : MonoBehaviour
    {
        [Header("Fuel Configuration")]
        [SerializeField] private float maxFuelCapacity = 100.0f;
        [SerializeField] private float currentFuel = 65.0f;
        [SerializeField] private float burnRatePerSecond = 0.42f;
        [SerializeField] private float sputterThresholdNormalized = 0.18f;

        [Header("HDRP Warm Flame Light (2400K Amber)")]
        [SerializeField] private Light flamePointLight;
        [SerializeField] private float baseLightIntensityLumens = 420.0f;
        [SerializeField] private float baseRangeMeters = 11.5f;
        [SerializeField] private Color warmFlameColor = new Color(1.0f, 0.58f, 0.18f, 1.0f);

        [Header("Physical Pendulum Sway")]
        [SerializeField] private Transform lanternCagePivot;
        [SerializeField] private float maxSwayDegrees = 11.0f;
        [SerializeField] private float swayDamping = 7.5f;

        private bool isLit = true;
        private float flickerSeed;
        private Vector3 currentSwayAngles;

        public event Action<float> OnFuelChanged;
        public event Action<bool> OnLanternLitStateChanged;

        public float CurrentFuel => currentFuel;
        public float MaxFuelCapacity => maxFuelCapacity;
        public float FuelNormalized => maxFuelCapacity > 0f ? Mathf.Clamp01(currentFuel / maxFuelCapacity) : 0f;
        public bool IsLit => isLit && currentFuel > 0.0001f;

        private void Awake()
        {
            flickerSeed = 17.3f;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuelCapacity);
            if (flamePointLight == null)
                flamePointLight = GetComponentInChildren<Light>();
            if (lanternCagePivot == null)
                lanternCagePivot = transform;
        }

        private void OnEnable()
        {
            UpdateLightOutput(0f);
        }

        private void Start()
        {
            if (ActProgressionManager.Instance != null)
            {
                SetFuelNormalized(ActProgressionManager.Instance.State.LanternFuelNormalized);
            }
            UpdateLightOutput(0f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleLantern();
            }

            TickLantern(Time.deltaTime, Vector3.zero);
        }

        public bool ToggleLantern()
        {
            if (!isLit && currentFuel <= 0.0001f)
            {
                isLit = false;
                UpdateLightOutput(0f);
                OnLanternLitStateChanged?.Invoke(false);
                return false;
            }

            isLit = !isLit;
            UpdateLightOutput(0f);
            OnLanternLitStateChanged?.Invoke(IsLit);
            return IsLit;
        }

        public void SetFuelNormalized(float normalized)
        {
            currentFuel = Mathf.Clamp01(normalized) * maxFuelCapacity;
            if (currentFuel <= 0.0001f)
            {
                currentFuel = 0f;
                if (isLit)
                {
                    isLit = false;
                    OnLanternLitStateChanged?.Invoke(false);
                }
            }
            else if (!isLit)
            {
                isLit = true;
                OnLanternLitStateChanged?.Invoke(true);
            }
            SyncProgressionFuel();
            UpdateLightOutput(0f);
            OnFuelChanged?.Invoke(FuelNormalized);
        }

        public float AddFuel(float fuelUnits)
        {
            if (fuelUnits <= 0f) return 0f;
            float before = currentFuel;
            currentFuel = Mathf.Clamp(currentFuel + fuelUnits, 0f, maxFuelCapacity);
            float added = currentFuel - before;
            SyncProgressionFuel();
            UpdateLightOutput(0f);
            OnFuelChanged?.Invoke(FuelNormalized);
            return added;
        }

        public void TickLantern(float deltaTime, Vector3 localPlayerAcceleration)
        {
            if (deltaTime < 0f) return;

            if (isLit && currentFuel > 0f)
            {
                currentFuel = Mathf.Max(0f, currentFuel - burnRatePerSecond * deltaTime);
                SyncProgressionFuel();
                OnFuelChanged?.Invoke(FuelNormalized);

                if (currentFuel <= 0.0001f)
                {
                    currentFuel = 0f;
                    isLit = false;
                    OnLanternLitStateChanged?.Invoke(false);
                }
            }

            UpdateLightOutput(deltaTime);
            UpdatePendulumSway(deltaTime, localPlayerAcceleration);
        }

        private void SyncProgressionFuel()
        {
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.State.LanternFuelNormalized = FuelNormalized;
            }
        }

        private void UpdateLightOutput(float deltaTime)
        {
            if (flamePointLight == null) return;

            if (!IsLit)
            {
                flamePointLight.enabled = false;
                flamePointLight.intensity = 0f;
                return;
            }

            flamePointLight.enabled = true;
            flamePointLight.color = warmFlameColor;

            float t = flickerSeed + deltaTime;
            float organicFlicker = 0.90f + 0.10f * Mathf.PerlinNoise(t * 6.5f, 0.4f);

            if (FuelNormalized <= sputterThresholdNormalized)
            {
                float sputterFactor = Mathf.Clamp01(FuelNormalized / Mathf.Max(0.01f, sputterThresholdNormalized));
                float dip = Mathf.PerlinNoise(t * 17.0f, 3.1f) > 0.65f ? 0.28f : 1.0f;
                organicFlicker *= Mathf.Lerp(0.35f, 1.0f, sputterFactor) * dip;
            }

            flamePointLight.intensity = baseLightIntensityLumens * organicFlicker;
            flamePointLight.range = baseRangeMeters * Mathf.Lerp(0.75f, 1.05f, organicFlicker);
        }

        private void UpdatePendulumSway(float deltaTime, Vector3 localAccel)
        {
            if (lanternCagePivot == null || deltaTime <= 0f) return;

            Vector3 targetAngles = new Vector3(
                Mathf.Clamp(-localAccel.z * 3.5f, -maxSwayDegrees, maxSwayDegrees),
                0f,
                Mathf.Clamp(localAccel.x * 3.5f, -maxSwayDegrees, maxSwayDegrees)
            );
            currentSwayAngles = Vector3.Lerp(currentSwayAngles, targetAngles, Mathf.Clamp01(swayDamping * deltaTime));
            lanternCagePivot.localRotation = Quaternion.Euler(currentSwayAngles);
        }
    }
}
