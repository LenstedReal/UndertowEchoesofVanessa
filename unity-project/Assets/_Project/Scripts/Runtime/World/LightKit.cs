using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Undertow
{
    /// <summary>
    /// HDRP lights in physical units. Reference: the lantern is ~520 lm; with the act exposure (EV100 ≈ 2)
    /// a wall 1.5 m away reads mid-bright, 5 m away almost black. Shadows scale with the quality preset.
    /// </summary>
    public static class LightKit
    {
        public static readonly Color Lantern = new Color(1f, 0.573f, 0.153f);       // #FF9227
        public static readonly Color Candle = new Color(1f, 0.68f, 0.36f);
        public static readonly Color Moon = new Color(0.62f, 0.72f, 0.9f);
        public static readonly Color Sea = new Color(0.4f, 0.9f, 0.92f);

        public static int ShadowRes(bool important)
        {
            int q = Game.Settings != null ? Game.Settings.Quality : 2;
            if (!important) return q >= 3 ? 512 : (q >= 2 ? 256 : 0);
            switch (q)
            {
                case 0: return 256;
                case 1: return 512;
                case 2: return 1024;
                default: return 2048;
            }
        }

        static Light Make(string name, Transform parent, Vector3 pos, Quaternion rot, LightType type, out HDAdditionalLightData hd)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            hd = go.AddHDLight(type);
            return go.GetComponent<Light>();
        }

        static void Shadows(Light l, HDAdditionalLightData hd, bool want, bool important)
        {
            int res = ShadowRes(important);
            bool on = want && res > 0;
            hd.EnableShadows(on);
            if (!on) return;
            hd.SetShadowResolutionOverride(true);
            hd.SetShadowResolution(res);
        }

        public static Light Point(string name, Transform parent, Vector3 pos, Color color, float lumens, float range, bool shadows = false, bool important = false, float volumetric = 1f, float radius = 0.05f)
        {
            var l = Make(name, parent, pos, Quaternion.identity, LightType.Point, out var hd);
            l.color = color;
            l.lightUnit = LightUnit.Lumen;
            l.intensity = lumens;
            l.range = range;
            hd.shapeRadius = radius;
            hd.volumetricDimmer = volumetric;
            hd.affectsVolumetric = volumetric > 0f;
            Shadows(l, hd, shadows, important);
            return l;
        }

        public static Light Spot(string name, Transform parent, Vector3 pos, Quaternion rot, Color color, float lumens, float range, float angle, bool shadows = false, bool important = false, float volumetric = 1f)
        {
            var l = Make(name, parent, pos, rot, LightType.Spot, out var hd);
            l.color = color;
            l.lightUnit = LightUnit.Lumen;
            l.intensity = lumens;
            l.range = range;
            l.spotAngle = angle;
            l.innerSpotAngle = angle * 0.55f;
            hd.volumetricDimmer = volumetric;
            hd.affectsVolumetric = volumetric > 0f;
            Shadows(l, hd, shadows, important);
            return l;
        }

        public static Light Directional(string name, Transform parent, Quaternion rot, Color color, float lux, bool shadows = true)
        {
            var l = Make(name, parent, Vector3.zero, rot, LightType.Directional, out var hd);
            l.color = color;
            l.lightUnit = LightUnit.Lux;
            l.intensity = lux;
            hd.volumetricDimmer = 1f;
            hd.affectsVolumetric = true;
            hd.EnableShadows(shadows && (Game.Settings == null || Game.Settings.Quality >= 1));
            return l;
        }

        public static void SetLumens(Light l, float lumens)
        {
            if (l == null) return;
            l.intensity = lumens;
        }
    }

    /// <summary>Organic flicker for flames (lantern, candles, braziers). Strength is set by its owner.</summary>
    public sealed class Flicker : MonoBehaviour
    {
        public Light Target;
        public float Base = 400f;
        public float Amount = 0.12f;
        public float Speed = 9f;
        public float Strength = 1f;     // 0..1, the owner scales it (oil, wind)
        public float Wind;              // 0..1 extra gusty flicker
        public Renderer Flame;
        public float FlameNits = 40f;
        Material _flameMat;
        float _seed;

        void Start()
        {
            _seed = Random.value * 100f;
            if (Flame != null)
            {
                _flameMat = new Material(Flame.sharedMaterial);
                Flame.sharedMaterial = _flameMat;
            }
        }

        void Update()
        {
            float t = Time.time * Speed + _seed;
            float n = Mathf.PerlinNoise(t, _seed) * 2f - 1f;
            float g = Wind > 0f ? (Mathf.PerlinNoise(t * 2.3f, _seed + 7f) * 2f - 1f) * Wind * 0.35f : 0f;
            float k = Mathf.Max(0f, Strength * (1f + n * Amount + g));
            if (Target != null) Target.intensity = Base * k;
            if (_flameMat != null) MatLib.SetEmission(_flameMat, new Color(1f, 0.55f, 0.16f), FlameNits * Mathf.Max(0.05f, k));
            if (Flame != null) Flame.transform.localScale = new Vector3(1f, Mathf.Max(0.05f, 0.85f + 0.3f * k + g * 0.4f), 1f);
        }

        void OnDestroy()
        {
            if (_flameMat != null) Destroy(_flameMat);
        }
    }
}
