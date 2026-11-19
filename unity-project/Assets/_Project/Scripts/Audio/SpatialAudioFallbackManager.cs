using System;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Audio
{
    public class SpatialAudioFallbackManager : MonoBehaviour
    {
        [SerializeField] private bool streamerSafeLimiterEnabled = true;
        [SerializeField] private float streamerMaxPeakDbFs = -1.5f;
        [SerializeField] private LayerMask occlusionGeometryMask = ~0;

        private readonly List<AudioClip> runtimeSynthesizedClips = new List<AudioClip>();

        public bool IsFmodNativeAvailable { get; private set; }
        public bool IsSteamAudioHrtfAvailable { get; private set; }
        public bool StreamerSafeLimiterEnabled
        {
            get => streamerSafeLimiterEnabled;
            set => streamerSafeLimiterEnabled = value;
        }

        private void Awake()
        {
            DetectOptionalAudioPlugins();
        }

        private void OnDestroy()
        {
            ReleaseSynthesizedClips();
        }

        public void ReleaseSynthesizedClips()
        {
            for (int i = 0; i < runtimeSynthesizedClips.Count; i++)
            {
                if (runtimeSynthesizedClips[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(runtimeSynthesizedClips[i]);
                    else
                        DestroyImmediate(runtimeSynthesizedClips[i]);
                }
            }
            runtimeSynthesizedClips.Clear();
        }

        public void DetectOptionalAudioPlugins()
        {
            IsFmodNativeAvailable = Type.GetType("FMODUnity.RuntimeManager, FMODUnity") != null;
            IsSteamAudioHrtfAvailable = Type.GetType("SteamAudio.SteamAudioManager, SteamAudio") != null;
        }

        public float ComputeOcclusionCutoffHz(Vector3 listenerPos, Vector3 emitterPos, int obstructingWallCount)
        {
            if (obstructingWallCount <= 0) return 22000.0f;
            float factor = Mathf.Pow(0.45f, Mathf.Clamp(obstructingWallCount, 1, 5));
            return Mathf.Clamp(22000.0f * factor, 380.0f, 22000.0f);
        }

        public float ApplyStreamerSafeLimiter(float inputLinearAmplitude)
        {
            if (!streamerSafeLimiterEnabled) return inputLinearAmplitude;
            float maxLinear = Mathf.Pow(10.0f, streamerMaxPeakDbFs / 20.0f);
            return Mathf.Clamp(inputLinearAmplitude, -maxLinear, maxLinear);
        }

        public static float GetBellFrequencyHz(BellNote note)
        {
            switch (note)
            {
                case BellNote.Do: return 261.63f; // C4
                case BellNote.Mi: return 329.63f; // E4
                case BellNote.La: return 440.00f; // A4
                default: return 440.00f;
            }
        }

        public float[] SynthesizeBellStrikePcm(BellNote note, int sampleRate = 44100, float durationSeconds = 1.25f)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            float[] buffer = new float[sampleCount];
            float f0 = GetBellFrequencyHz(note);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float env = Mathf.Exp(-3.4f * t);
                float sample =
                    0.52f * Mathf.Sin(2f * Mathf.PI * f0 * t) +
                    0.26f * Mathf.Sin(2f * Mathf.PI * (f0 * 2.0f) * t) * Mathf.Exp(-4.5f * t) +
                    0.14f * Mathf.Sin(2f * Mathf.PI * (f0 * 2.42f) * t) * Mathf.Exp(-6.2f * t) +
                    0.08f * Mathf.Sin(2f * Mathf.PI * (f0 * 3.85f) * t) * Mathf.Exp(-8.8f * t);
                buffer[i] = ApplyStreamerSafeLimiter(sample * env);
            }
            return buffer;
        }
    }
}
