using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Undertow
{
    /// <summary>Look of a place: fog, sky, exposure and grading.</summary>
    public sealed class AtmosPreset
    {
        public float FogDistance = 18f;
        public Color FogAlbedo = new Color(0.62f, 0.68f, 0.7f);
        public Color FogTint = new Color(0.3f, 0.36f, 0.38f);
        public float FogHeight = 6f;          // fog thins out above this height
        public float FogAnisotropy = 0.55f;
        public Color SkyTop = new Color(0.03f, 0.04f, 0.06f);
        public Color SkyMid = new Color(0.07f, 0.09f, 0.11f);
        public Color SkyBottom = new Color(0.1f, 0.12f, 0.13f);
        public float SkyExposure = 1.5f;      // EV applied to the sky colours
        public float Exposure = 2.0f;         // fixed EV100 of the camera
        public float Saturation = -18f;
        public float Contrast = 12f;
        public Color Filter = new Color(0.94f, 0.98f, 1f);
        public float Bloom = 0.22f;
        public float Vignette = 0.28f;
    }

    /// <summary>
    /// Global volume for an act plus a second volume whose weight follows hidden sanity
    /// (vignette, chromatic aberration, grain, lens warp, desaturation). Settings toggles apply live.
    /// </summary>
    public sealed class Atmos : MonoBehaviour
    {
        public Volume Global;
        public Volume Sanity;
        VolumeProfile _gp, _sp;
        Exposure _exposure;
        Fog _fog;
        FilmGrain _grain;
        ChromaticAberration _ca, _sca;
        DepthOfField _dof;
        MotionBlur _blur;
        ScreenSpaceAmbientOcclusion _ssao;
        ContactShadows _contact;
        Vignette _sVignette;
        FilmGrain _sGrain;
        LensDistortion _sLens;
        ColorAdjustments _sColor;
        ColorAdjustments _color;
        AtmosPreset _preset;
        float _stress;
        float _flash;
        float _baseFog;

        public static Atmos Create(Transform parent, AtmosPreset p)
        {
            var go = new GameObject("Atmosphere");
            go.transform.SetParent(parent, false);
            var a = go.AddComponent<Atmos>();
            a.Build(p);
            return a;
        }

        void Build(AtmosPreset p)
        {
            _preset = p;
            _gp = ScriptableObject.CreateInstance<VolumeProfile>();
            _gp.name = "ActProfile";

            var env = _gp.Add<VisualEnvironment>(false);
            env.skyType.Override((int)SkyType.Gradient);
            env.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            var sky = _gp.Add<GradientSky>(false);
            sky.top.Override(p.SkyTop);
            sky.middle.Override(p.SkyMid);
            sky.bottom.Override(p.SkyBottom);
            sky.gradientDiffusion.Override(1.4f);
            sky.skyIntensityMode.Override(SkyIntensityMode.Exposure);
            sky.exposure.Override(p.SkyExposure);

            _fog = _gp.Add<Fog>(false);
            _fog.enabled.Override(true);
            _fog.colorMode.Override(FogColorMode.ConstantColor);
            _fog.color.Override(p.FogTint);
            _fog.meanFreePath.Override(p.FogDistance);
            _baseFog = p.FogDistance;
            _fog.baseHeight.Override(-2f);
            _fog.maximumHeight.Override(p.FogHeight);
            _fog.maxFogDistance.Override(400f);
            _fog.enableVolumetricFog.Override(true);
            _fog.albedo.Override(p.FogAlbedo);
            _fog.anisotropy.Override(p.FogAnisotropy);
            _fog.depthExtent.Override(48f);
            _fog.globalLightProbeDimmer.Override(0.5f);

            _exposure = _gp.Add<Exposure>(false);
            _exposure.mode.Override(ExposureMode.Fixed);
            _exposure.fixedExposure.Override(p.Exposure);
            _exposure.compensation.Override(0f);

            var tone = _gp.Add<Tonemapping>(false);
            tone.mode.Override(TonemappingMode.ACES);

            var bloom = _gp.Add<Bloom>(false);
            bloom.intensity.Override(p.Bloom);
            bloom.scatter.Override(0.68f);
            bloom.threshold.Override(0.6f);

            var color = _gp.Add<ColorAdjustments>(false);
            color.saturation.Override(p.Saturation);
            color.contrast.Override(p.Contrast);
            color.colorFilter.Override(p.Filter);
            _color = color;

            var vig = _gp.Add<Vignette>(false);
            vig.intensity.Override(p.Vignette);
            vig.smoothness.Override(0.45f);
            vig.color.Override(Color.black);

            _grain = _gp.Add<FilmGrain>(false);
            _grain.type.Override(FilmGrainLookup.Medium3);
            _grain.intensity.Override(0.18f);
            _grain.response.Override(0.85f);

            _ca = _gp.Add<ChromaticAberration>(false);
            _ca.intensity.Override(0.06f);

            _dof = _gp.Add<DepthOfField>(false);
            _dof.focusMode.Override(DepthOfFieldMode.Off);

            _blur = _gp.Add<MotionBlur>(false);
            _blur.intensity.Override(0f);

            _ssao = _gp.Add<ScreenSpaceAmbientOcclusion>(false);
            _ssao.intensity.Override(0.9f);
            _ssao.radius.Override(1.2f);
            _ssao.directLightingStrength.Override(0.15f);

            _contact = _gp.Add<ContactShadows>(false);
            _contact.enable.Override(true);
            _contact.length.Override(0.12f);

            Global = gameObject.AddComponent<Volume>();
            Global.isGlobal = true;
            Global.priority = 10f;
            Global.sharedProfile = _gp;

            // sanity overlay
            _sp = ScriptableObject.CreateInstance<VolumeProfile>();
            _sp.name = "SanityProfile";
            _sVignette = _sp.Add<Vignette>(false);
            _sVignette.intensity.Override(0.55f);
            _sVignette.smoothness.Override(0.6f);
            _sVignette.color.Override(new Color(0.02f, 0.03f, 0.035f));
            _sGrain = _sp.Add<FilmGrain>(false);
            _sGrain.type.Override(FilmGrainLookup.Large02);
            _sGrain.intensity.Override(0.5f);
            _sca = _sp.Add<ChromaticAberration>(false);
            _sca.intensity.Override(0.55f);
            _sLens = _sp.Add<LensDistortion>(false);
            _sLens.intensity.Override(-0.18f);
            _sColor = _sp.Add<ColorAdjustments>(false);
            _sColor.saturation.Override(-55f);
            _sColor.contrast.Override(18f);
            _sColor.colorFilter.Override(new Color(0.84f, 0.92f, 0.95f));
            var sgo = new GameObject("SanityVolume");
            sgo.transform.SetParent(transform, false);
            Sanity = sgo.AddComponent<Volume>();
            Sanity.isGlobal = true;
            Sanity.priority = 20f;
            Sanity.sharedProfile = _sp;
            Sanity.weight = 0f;

            Game.Settings.Changed += ApplySettings;
            ApplySettings();
        }

        void OnDestroy()
        {
            if (Game.Settings != null) Game.Settings.Changed -= ApplySettings;
            if (_gp != null) Destroy(_gp);
            if (_sp != null) Destroy(_sp);
            foreach (var kv in _interiorProfiles) if (kv.Value != null) Destroy(kv.Value);
        }

        public void ApplySettings()
        {
            var s = Game.Settings;
            _exposure.compensation.Override(s.Brightness);
            _grain.active = s.FilmGrain;
            _sGrain.active = s.FilmGrain;
            _ca.active = s.ChromaticAberration;
            _sca.active = s.ChromaticAberration;
            _blur.intensity.Override(s.MotionBlur ? 0.4f : 0f);
            _ssao.active = s.Quality >= 1;
            _contact.active = s.Quality >= 2;
            int level = s.Quality >= 3 ? (int)ScalableSettingLevelParameter.Level.High : (s.Quality >= 1 ? (int)ScalableSettingLevelParameter.Level.Medium : (int)ScalableSettingLevelParameter.Level.Low);
            _fog.quality.Override(level);
            _ssao.quality.Override(level);
        }

        readonly Dictionary<int, VolumeProfile> _interiorProfiles = new Dictionary<int, VolumeProfile>();

        /// <summary>
        /// Local volume for an enclosed space: no baked GI here, so the sky's ambient light must be scaled down
        /// by hand or cabins would glow like the open deck. 'ambient' is the room's 0..1 fill amount.
        /// </summary>
        public Volume Interior(string name, Vector3 center, Vector3 size, float ambient, float fogDistance = 40f)
        {
            int key = Mathf.RoundToInt(Mathf.Clamp01(ambient) * 20f) * 1000 + Mathf.RoundToInt(fogDistance);
            if (!_interiorProfiles.TryGetValue(key, out var prof))
            {
                prof = ScriptableObject.CreateInstance<VolumeProfile>();
                prof.name = "Interior_" + key;
                var ilc = prof.Add<IndirectLightingController>(false);
                float mul = Mathf.Lerp(0.05f, 0.7f, Mathf.Clamp01(ambient));
                ilc.indirectDiffuseLightingMultiplier.Override(mul);
                ilc.reflectionLightingMultiplier.Override(Mathf.Lerp(0.1f, 0.8f, Mathf.Clamp01(ambient)));
                var fog = prof.Add<Fog>(false);
                fog.meanFreePath.Override(fogDistance);
                _interiorProfiles[key] = prof;
            }
            var go = new GameObject("Interior_" + name);
            go.transform.SetParent(transform, false);
            go.transform.position = center;
            go.layer = 2; // Ignore Raycast: never blocks interaction or line-of-sight checks
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = size;
            var v = go.AddComponent<Volume>();
            v.isGlobal = false;
            v.blendDistance = 0.9f;
            v.priority = 12f;
            v.sharedProfile = prof;
            return v;
        }

        /// <summary>Exposure compensation sweep used by the brightness calibration screen.</summary>
        public void SetBrightness(float ev) => _exposure.compensation.Override(ev);

        public void SetFogDistance(float d) => _fog.meanFreePath.Override(Mathf.Max(1f, d));
        public void RestoreFog() => _fog.meanFreePath.Override(_baseFog);

        /// <summary>Lightning / flash: momentarily brighter exposure.</summary>
        public void Flash(float amount) => _flash = Mathf.Max(_flash, amount);

        /// <summary>Hidden sanity drives the overlay (0 = calm, 1 = breaking). Accessibility setting scales it.</summary>
        public void SetStress(float stress01)
        {
            _stress = Mathf.Clamp01(stress01);
        }

        /// <summary>Photo mode looks: 0 none, 1 black and white, 2 sepia, 3 cold.</summary>
        public void SetPhotoFilter(int f)
        {
            if (_color == null) return;
            switch (f)
            {
                case 1: _color.saturation.Override(-100f); _color.colorFilter.Override(Color.white); break;
                case 2: _color.saturation.Override(-85f); _color.colorFilter.Override(new Color(1f, 0.86f, 0.66f)); break;
                case 3: _color.saturation.Override(-40f); _color.colorFilter.Override(new Color(0.72f, 0.88f, 1f)); break;
                default: _color.saturation.Override(_preset.Saturation); _color.colorFilter.Override(_preset.Filter); break;
            }
        }

        public void SetDepthOfField(bool on, float focus, bool force = false)
        {
            if (!Game.Settings.DepthOfField && !force) on = false;
            _dof.focusMode.Override(on ? DepthOfFieldMode.Manual : DepthOfFieldMode.Off);
            if (on)
            {
                _dof.nearFocusStart.Override(0f);
                _dof.nearFocusEnd.Override(Mathf.Max(0.05f, focus * 0.6f));
                _dof.farFocusStart.Override(focus * 1.3f);
                _dof.farFocusEnd.Override(focus * 3f);
            }
        }

        void Update()
        {
            float fx = Game.Settings.SanityEffects;
            // ease: effects start around Uneasy and are strong at Breaking
            float w = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 0.9f, _stress)) * fx;
            Sanity.weight = Mathf.MoveTowards(Sanity.weight, w, Time.deltaTime * 0.5f);
            if (_flash > 0f)
            {
                _flash = Mathf.MoveTowards(_flash, 0f, Time.deltaTime * 2.5f);
                _exposure.compensation.Override(Game.Settings.Brightness + _flash * 2.2f);
            }
        }
    }
}
