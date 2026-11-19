using UnityEngine;
using Undertow.World;

namespace Undertow.Audio
{
    /// <summary>
    /// Original procedural fallback ambience. It is deliberately not represented as
    /// recorded voice acting or a native FMOD bank. It supplies a quiet, streamer-safe
    /// wind/sea bed until licensed/recorded production audio is imported.
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(AudioLowPassFilter))]
    public sealed class UndertowProceduralAmbience : MonoBehaviour
    {
        [SerializeField] private bool underwaterMode;
        [SerializeField, Range(0f, 1f)] private float baseVolume = 0.24f;
        [SerializeField] private int sampleRate = 22050;
        [SerializeField] private float clipDuration = 10f;
        [SerializeField] private float normalCutoff = 18000f;
        [SerializeField] private float listenCutoff = 320f;
        private AudioSource source;
        private AudioLowPassFilter lowPass;
        private ListenMechanicController listen;
        private float[] samples;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            lowPass = GetComponent<AudioLowPassFilter>();
            listen = FindObjectOfType<ListenMechanicController>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = baseVolume;
            source.dopplerLevel = 0f;
            lowPass.cutoffFrequency = normalCutoff;
            source.clip = BuildLoop();
        }

        private void Start()
        {
            if (source.clip != null) source.Play();
        }

        private AudioClip BuildLoop()
        {
            int count = Mathf.Clamp(sampleRate, 11025, 44100) * Mathf.Clamp(Mathf.RoundToInt(clipDuration), 6, 20);
            samples = new float[count];
            System.Random rng = new System.Random(61723 + (underwaterMode ? 1 : 0));
            float filteredNoise = 0f;
            float phase = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                filteredNoise += (white - filteredNoise) * (underwaterMode ? 0.035f : 0.012f);
                float swell = Mathf.Pow(Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(t * (underwaterMode ? 0.48f : 0.31f) * Mathf.PI * 2f)), 2.1f);
                float windBand = Mathf.Sin(phase) * (underwaterMode ? 0.055f : 0.10f);
                float seaBand = filteredNoise * (underwaterMode ? 0.22f : 0.38f) * (0.30f + swell * 0.70f);
                float lowDrone = Mathf.Sin(t * Mathf.PI * 2f * (underwaterMode ? 38f : 31f)) * (underwaterMode ? 0.018f : 0.012f);
                samples[i] = Mathf.Clamp((seaBand + windBand + lowDrone) * (underwaterMode ? 0.72f : 0.82f), -0.32f, 0.32f);
                phase += Mathf.PI * 2f * (underwaterMode ? 0.19f : 0.13f) / sampleRate;
            }
            // Equal-power style edge crossfade to avoid a hard click at the loop boundary.
            int fade = Mathf.Min(sampleRate / 3, count / 4);
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                float a = samples[count - fade + i];
                float b = samples[i];
                float blended = Mathf.Lerp(a, b, k);
                samples[count - fade + i] = blended;
                samples[i] = Mathf.Lerp(b, a, k * 0.18f);
            }
            AudioClip clip = AudioClip.Create(underwaterMode ? "Undertow_Original_Underwater_Ambience" : "Undertow_Original_Storm_Sea_Ambience", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void Update()
        {
            if (listen == null) listen = FindObjectOfType<ListenMechanicController>();
            bool listening = listen != null && listen.IsListening;
            lowPass.cutoffFrequency = Mathf.Lerp(normalCutoff, listenCutoff, listening ? listen.ListenBlend : 0f);
            float target = baseVolume * (listening ? 0.18f : 1f);
            source.volume = Mathf.MoveTowards(source.volume, target, Time.deltaTime * 0.55f);
        }

        private void OnDestroy()
        {
            if (source != null && source.clip != null)
            {
                if (Application.isPlaying) Destroy(source.clip); else DestroyImmediate(source.clip);
            }
        }
    }
}
