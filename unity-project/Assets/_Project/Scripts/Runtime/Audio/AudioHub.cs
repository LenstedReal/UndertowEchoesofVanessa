using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Undertow
{
    public enum Bus { Music, Effects, Voice, Ambience, Ui }

    /// <summary>
    /// Unity built-in audio, organised in buses with volumes from the settings.
    /// Clips come from Resources/Undertow/Audio. Loops are tracked so volume changes and ducking
    /// apply live. A soft limiter on the listener makes sure nothing ever spikes (streamer safety).
    /// </summary>
    public sealed class AudioHub : MonoBehaviour
    {
        sealed class Voice
        {
            public AudioSource Source;
            public Bus Bus;
            public float Base;
            public bool Important;      // stays loud while listening, gets occlusion
            public float Occlusion;     // 0 clear, 1 behind a wall
            public AudioLowPassFilter Lowpass;
        }

        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        readonly HashSet<string> _missing = new HashSet<string>();
        readonly List<Voice> _live = new List<Voice>();
        readonly Stack<AudioSource> _free = new Stack<AudioSource>();
        Transform _poolRoot;

        AudioSource _ambA, _ambB, _music;
        bool _ambOnA = true;
        string _ambId, _musicId;
        float _ambFade = 1f;
        float _duck;            // 0..1 listen-mode ducking of non-important sounds
        float _silence;         // 0..1 "silence before a key moment"
        float _silenceUntil;
        float _occlusionTimer;

        public AudioListener Listener { get; private set; }
        public ListenerFx ListenerFx { get; private set; }
        public float Duck { get => _duck; set => _duck = Mathf.Clamp01(value); }

        public void Init()
        {
            _poolRoot = new GameObject("AudioPool").transform;
            _poolRoot.SetParent(transform, false);
            _ambA = MakeLoopSource("AmbienceA");
            _ambB = MakeLoopSource("AmbienceB");
            _music = MakeLoopSource("Music");
            _music.ignoreListenerPause = true;
            Game.Settings.Changed += RefreshVolumes;
        }

        AudioSource MakeLoopSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var s = go.AddComponent<AudioSource>();
            s.loop = true;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.priority = 32;
            return s;
        }

        public AudioClip Clip(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_clips.TryGetValue(id, out var c)) return c;
            c = Resources.Load<AudioClip>("Undertow/Audio/" + id);
            if (c == null && _missing.Add(id)) Game.Warn("missing audio clip " + id);
            _clips[id] = c;
            return c;
        }

        public float BusVolume(Bus b)
        {
            var s = Game.Settings;
            switch (b)
            {
                case Bus.Music: return s.Music;
                case Bus.Voice: return s.Voice;
                case Bus.Ambience: return s.Ambience;
                default: return s.Effects;
            }
        }

        float Gain(Voice v)
        {
            float g = v.Base * BusVolume(v.Bus);
            if (!v.Important && v.Bus != Bus.Ui) g *= 1f - 0.75f * _duck;
            if (v.Bus != Bus.Ui && v.Bus != Bus.Voice) g *= 1f - _silence;
            if (v.Important) g *= 1f - 0.4f * v.Occlusion;
            return g;
        }

        AudioSource Rent()
        {
            AudioSource s;
            if (_free.Count > 0)
            {
                s = _free.Pop();
                s.gameObject.SetActive(true);
            }
            else
            {
                var go = new GameObject("Src");
                go.transform.SetParent(_poolRoot, false);
                s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
            }
            s.loop = false;
            s.pitch = 1f;
            s.spatialBlend = 0f;
            s.dopplerLevel = 0f;
            s.spread = 0f;
            s.rolloffMode = AudioRolloffMode.Logarithmic;
            s.minDistance = 1f;
            s.maxDistance = 30f;
            s.priority = 128;
            s.transform.SetParent(_poolRoot, false);
            return s;
        }

        void Return(Voice v)
        {
            if (v.Source == null) return;
            v.Source.Stop();
            v.Source.clip = null;
            if (v.Lowpass != null) v.Lowpass.enabled = false;
            v.Source.transform.SetParent(_poolRoot, false);
            v.Source.gameObject.SetActive(false);
            _free.Push(v.Source);
        }

        Voice StartVoice(string id, Bus bus, float volume, float pitch, bool loop, bool important)
        {
            var clip = Clip(id);
            if (clip == null) return null;
            var s = Rent();
            s.clip = clip;
            s.loop = loop;
            s.pitch = pitch;
            s.ignoreListenerPause = bus == Bus.Ui;
            var v = new Voice { Source = s, Bus = bus, Base = volume, Important = important };
            s.volume = Gain(v);
            _live.Add(v);
            return v;
        }

        public AudioSource Play2D(string id, Bus bus = Bus.Effects, float volume = 1f, float pitch = 1f)
        {
            var v = StartVoice(id, bus, volume, pitch, false, bus == Bus.Voice);
            if (v == null) return null;
            v.Source.Play();
            return v.Source;
        }

        public AudioSource PlayAt(string id, Vector3 pos, Bus bus = Bus.Effects, float volume = 1f, float pitch = 1f, float minDist = 1.2f, float maxDist = 28f, bool important = false)
        {
            var v = StartVoice(id, bus, volume, pitch, false, important);
            if (v == null) return null;
            Place(v.Source, pos, minDist, maxDist);
            v.Source.Play();
            return v.Source;
        }

        /// <summary>Looping 3D source that follows a transform. Stop it with Stop().</summary>
        public AudioSource Loop(string id, Transform follow, Bus bus = Bus.Effects, float volume = 1f, float minDist = 1.5f, float maxDist = 30f, bool important = false)
        {
            var v = StartVoice(id, bus, volume, 1f, true, important);
            if (v == null) return null;
            v.Source.transform.SetParent(follow, false);
            v.Source.transform.localPosition = Vector3.zero;
            v.Source.spatialBlend = 1f;
            v.Source.minDistance = minDist;
            v.Source.maxDistance = maxDist;
            v.Source.time = UnityEngine.Random.Range(0f, Mathf.Max(0f, v.Source.clip.length - 0.1f));
            v.Source.Play();
            return v.Source;
        }

        static void Place(AudioSource s, Vector3 pos, float minDist, float maxDist)
        {
            s.transform.position = pos;
            s.spatialBlend = 1f;
            s.minDistance = minDist;
            s.maxDistance = maxDist;
        }

        public void Stop(AudioSource s, float fade = 0f)
        {
            if (s == null) return;
            var v = _live.Find(x => x.Source == s);
            if (v == null) return;
            if (fade <= 0f)
            {
                _live.Remove(v);
                Return(v);
                return;
            }
            StartCoroutine(FadeOut(v, fade));
        }

        IEnumerator FadeOut(Voice v, float t)
        {
            float b = v.Base;
            for (float e = 0f; e < t && v.Source != null; e += Time.unscaledDeltaTime)
            {
                v.Base = b * (1f - e / t);
                yield return null;
            }
            _live.Remove(v);
            Return(v);
        }

        public void SetVolume(AudioSource s, float volume)
        {
            var v = _live.Find(x => x.Source == s);
            if (v != null) v.Base = volume;
        }

        public void StopAllWorld()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                if (_live[i].Bus == Bus.Ui) continue;
                Return(_live[i]);
                _live.RemoveAt(i);
            }
        }

        // ---------------------------------------------------------------- ambience & music
        public void SetAmbience(string id, float fade = 2.5f)
        {
            if (id == _ambId) return;
            _ambId = id;
            var clip = Clip(id);
            var next = _ambOnA ? _ambB : _ambA;
            _ambOnA = !_ambOnA;
            next.clip = clip;
            next.volume = 0f;
            if (clip != null) next.Play();
            _ambFade = 0f;
            _ambFadeTime = Mathf.Max(0.05f, fade);
        }

        float _ambFadeTime = 2.5f, _musicTarget, _musicFadeTime = 2f;

        public void PlayMusic(string id, float fade = 2f, float volume = 1f, bool loop = true)
        {
            if (id == _musicId && _music.isPlaying) { _musicTarget = volume; return; }
            _musicId = id;
            _music.clip = Clip(id);
            _music.loop = loop;
            _music.volume = 0f;
            _musicTarget = volume;
            _musicFadeTime = Mathf.Max(0.05f, fade);
            if (_music.clip != null) _music.Play();
        }

        public void StopMusic(float fade = 2f)
        {
            _musicTarget = 0f;
            _musicFadeTime = Mathf.Max(0.05f, fade);
            _musicId = null;
        }

        public bool MusicPlaying => _music != null && _music.isPlaying && _musicTarget > 0f;

        /// <summary>Cuts ambience and music for a moment (the design rule: 2 s of silence before key moments).</summary>
        public void Silence(float seconds)
        {
            _silenceUntil = Time.unscaledTime + seconds;
        }

        public void RefreshVolumes()
        {
            foreach (var v in _live) if (v.Source != null) v.Source.volume = Gain(v);
            AudioListener.volume = Game.Settings.Master;
            if (ListenerFx != null)
            {
                ListenerFx.LimiterOn = Game.Settings.Limiter || Game.Settings.StreamerMode;   // streamer mode: broadcast-safe peaks
                ListenerFx.Mono = Game.Settings.Mono;
            }
        }

        /// <summary>Called by whoever owns the active camera.</summary>
        public void BindListener(AudioListener l)
        {
            Listener = l;
            if (l == null) return;
            ListenerFx = l.GetComponent<ListenerFx>() ?? l.gameObject.AddComponent<ListenerFx>();
            if (l.GetComponent<AudioReverbFilter>() == null)
            {
                var rf = l.gameObject.AddComponent<AudioReverbFilter>();
                rf.reverbPreset = AudioReverbPreset.Off;
            }
            RefreshVolumes();
        }

        public void SetReverb(AudioReverbPreset preset)
        {
            if (Listener == null) return;
            var rf = Listener.GetComponent<AudioReverbFilter>();
            if (rf != null && rf.reverbPreset != preset) rf.reverbPreset = preset;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            // silence before key moments
            float silenceTarget = Time.unscaledTime < _silenceUntil ? 1f : 0f;
            _silence = Mathf.MoveTowards(_silence, silenceTarget, dt * (silenceTarget > _silence ? 2.5f : 0.6f));
            // ambience crossfade
            _ambFade = Mathf.Min(1f, _ambFade + dt / _ambFadeTime);
            float ambBus = Game.Settings.Ambience * (1f - 0.75f * _duck) * (1f - _silence);
            var cur = _ambOnA ? _ambA : _ambB;
            var old = _ambOnA ? _ambB : _ambA;
            cur.volume = ambBus * _ambFade;
            old.volume = ambBus * (1f - _ambFade);
            if (_ambFade >= 1f && old.isPlaying) old.Stop();
            // music
            float m = _music.volume;
            float mTarget = _musicTarget * Game.Settings.Music * (1f - 0.6f * _duck) * (1f - _silence);
            _music.volume = Mathf.MoveTowards(m, mTarget, dt / _musicFadeTime);
            if (_musicTarget <= 0f && _music.volume <= 0.001f && _music.isPlaying) _music.Stop();

            // live sources: finished one-shots return to the pool
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var v = _live[i];
                if (v.Source == null) { _live.RemoveAt(i); continue; }
                if (!v.Source.loop && !v.Source.isPlaying && v.Source.time <= 0f)
                {
                    Return(v);
                    _live.RemoveAt(i);
                    continue;
                }
                v.Source.volume = Gain(v);
            }

            // occlusion for important 3D sources, 5 times per second
            _occlusionTimer -= dt;
            if (_occlusionTimer <= 0f && Listener != null)
            {
                _occlusionTimer = 0.2f;
                var lp = Listener.transform.position;
                foreach (var v in _live)
                {
                    if (!v.Important || v.Source == null || v.Source.spatialBlend < 0.5f) continue;
                    var sp = v.Source.transform.position;
                    var dir = sp - lp;
                    float dist = dir.magnitude;
                    bool blocked = dist > 0.5f && Physics.Raycast(lp, dir / dist, dist - 0.3f, ~0, QueryTriggerInteraction.Ignore);
                    float target = blocked ? 1f : 0f;
                    v.Occlusion = Mathf.MoveTowards(v.Occlusion, target, 0.35f);
                    if (v.Lowpass == null) v.Lowpass = v.Source.GetComponent<AudioLowPassFilter>() ?? v.Source.gameObject.AddComponent<AudioLowPassFilter>();
                    v.Lowpass.enabled = true;
                    v.Lowpass.cutoffFrequency = Mathf.Lerp(22000f, 1100f, v.Occlusion);
                }
            }
        }
    }

    /// <summary>
    /// Sits next to the AudioListener and processes the final mix on the audio thread:
    /// a soft-knee peak limiter (no sound ever clips or jumps out) and an optional mono fold-down.
    /// </summary>
    public sealed class ListenerFx : MonoBehaviour
    {
        public volatile bool LimiterOn = true;
        public volatile bool Mono;
        float _env = 0f;
        float _gain = 1f;
        const float Threshold = 0.5f;
        const float Ceiling = 0.89f;

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (channels <= 0) return;
            if (Mono && channels >= 2)
            {
                for (int i = 0; i < data.Length; i += channels)
                {
                    float s = 0f;
                    for (int c = 0; c < channels; c++) s += data[i + c];
                    s /= channels;
                    for (int c = 0; c < channels; c++) data[i + c] = s;
                }
            }
            if (!LimiterOn) return;
            for (int i = 0; i < data.Length; i += channels)
            {
                float peak = 0f;
                for (int c = 0; c < channels; c++)
                {
                    float a = data[i + c] < 0f ? -data[i + c] : data[i + c];
                    if (a > peak) peak = a;
                }
                // fast attack, slow release envelope
                _env = peak > _env ? _env + (peak - _env) * 0.4f : _env + (peak - _env) * 0.0004f;
                float target = 1f;
                if (_env > Threshold)
                {
                    // 4:1 above the threshold
                    float over = _env - Threshold;
                    float outLevel = Threshold + over * 0.25f;
                    target = outLevel / _env;
                }
                _gain += (target - _gain) * (target < _gain ? 0.3f : 0.0008f);
                for (int c = 0; c < channels; c++)
                {
                    float v = data[i + c] * _gain;
                    if (v > Ceiling) v = Ceiling;
                    else if (v < -Ceiling) v = -Ceiling;
                    data[i + c] = v;
                }
            }
        }
    }
}
