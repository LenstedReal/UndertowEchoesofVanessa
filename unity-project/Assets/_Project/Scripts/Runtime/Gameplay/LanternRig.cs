using UnityEngine;
using UnityEngine.Rendering;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// The oil lantern in Adrian's left hand: a view model that swings with the walk, a shadowed point light
    /// whose strength follows the oil, a flame that gutters when oil is low, and the sounds of lighting it.
    /// </summary>
    public sealed class LanternRig : MonoBehaviour
    {
        public Light Light;
        public Flicker Flicker;
        public Transform Model;
        /// <summary>Set by a story moment: the flame is blown out whatever the oil says.</summary>
        public bool Smothered;
        PlayerRig _p;
        Vector3 _restPos = new Vector3(-0.27f, -0.33f, 0.46f);
        Vector3 _swing;
        Vector3 _lastFwd;
        float _visible = 1f;
        float _raise;
        bool _lit;
        Renderer _flame;
        Renderer[] _renderers;

        public const float Lumens = 560f;
        public const float Range = 11f;

        public static LanternRig Create(PlayerRig p)
        {
            var go = new GameObject("Lantern");
            go.transform.SetParent(p.Cam.transform, false);
            go.transform.localPosition = new Vector3(-0.27f, -0.33f, 0.46f);
            var l = go.AddComponent<LanternRig>();
            l._p = p;
            l.Build();
            return l;
        }

        void Build()
        {
            Model = new GameObject("Model").transform;
            Model.SetParent(transform, false);
            var pm = Props.HangingLantern(false);
            // handle loop the hand holds
            var handle = new System.Collections.Generic.List<Vector3>();
            for (int i = 0; i <= 10; i++)
            {
                float a = i / 10f * Mathf.PI;
                handle.Add(new Vector3(Mathf.Cos(a) * 0.07f, 0.12f + Mathf.Sin(a) * 0.09f, 0));
            }
            pm["iron"].Tube(handle, 0.007f, 5, 0.2f);
            var body = pm.Spawn("body", Model, new Vector3(0, -0.08f, 0), Quaternion.identity, false);
            body.transform.localScale = Vector3.one * 0.78f;
            var fm = new PropMesh();
            fm["flame"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0f, 0f), new Vector2(0.016f, 0.018f), new Vector2(0.011f, 0.055f), new Vector2(0f, 0.08f) }, 6);
            var flame = fm.Spawn("flame", Model, new Vector3(0, -0.08f - 0.28f * 0.78f, 0), Quaternion.identity, false);
            flame.transform.localScale = Vector3.one * 0.78f;
            _flame = flame.GetComponentInChildren<Renderer>();
            _renderers = Model.GetComponentsInChildren<Renderer>();
            foreach (var r in _renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
            Light = LightKit.Point("LanternLight", Model, new Vector3(0, -0.08f - 0.22f * 0.78f, 0.02f), LightKit.Lantern, Lumens, Range, true, true, 1f, 0.03f);
            Flicker = Model.gameObject.AddComponent<Flicker>();
            Flicker.Target = Light;
            Flicker.Base = Lumens;
            Flicker.Amount = 0.07f;
            Flicker.Speed = 6.5f;
            Flicker.Flame = _flame;
            Flicker.FlameNits = 60f;
            _lastFwd = transform.forward;
        }

        /// <summary>Called by the runtime every frame with the lantern model state.</summary>
        public void Sync(LanternModel m, float wind, float hide01)
        {
            if (m.Lit != _lit)
            {
                _lit = m.Lit;
                Game.Audio.PlayAt(_lit ? "sfx_lantern_on" : "sfx_lantern_off", transform.position, Bus.Effects, 0.7f, 1f, 0.5f, 8f);
            }
            float strength = Smothered ? 0f : m.Strength01;
            if (m.Lit && m.Critical) strength *= 0.75f + 0.25f * Mathf.PerlinNoise(Time.time * 7f, 3.1f);
            Flicker.Strength = strength;
            Flicker.Wind = wind;
            Flicker.Amount = m.Low ? 0.16f : 0.07f;
            Light.enabled = m.Lit && strength > 0.01f;
            if (_flame != null) _flame.enabled = m.Lit && !Smothered;
            _visible = Mathf.MoveTowards(_visible, 1f - hide01, Time.deltaTime * 3f);
        }

        /// <summary>Shows or hides the lantern itself (its light keeps shining): photo mode.</summary>
        public void SetVisible(bool on)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers) if (r != null) r.enabled = on && (r != _flame || _lit);
        }

        /// <summary>Lift the lantern a little (examining, listening to a voice in the dark).</summary>
        public void Raise(float amount) => _raise = Mathf.Clamp01(amount);

        void LateUpdate()
        {
            if (_p == null) return;
            float dt = Time.deltaTime;
            // lag behind camera turns, bob with the walk
            var fwd = _p.Cam.transform.forward;
            var turn = Vector3.SignedAngle(_lastFwd, fwd, Vector3.up);
            float pitchTurn = Vector3.SignedAngle(_lastFwd, fwd, _p.Cam.transform.right);
            _lastFwd = fwd;
            _swing = Vector3.Lerp(_swing, new Vector3(-turn * 0.0022f, pitchTurn * 0.0016f, 0f), dt * 8f);
            float walk = Mathf.Clamp01(_p.Speed / Balance.SprintSpeed);
            float t = Time.time * (_p.Sprinting ? 9.5f : 6.5f);
            var bob = new Vector3(Mathf.Cos(t * 0.5f) * 0.012f, Mathf.Abs(Mathf.Sin(t * 0.5f)) * 0.018f, 0f) * walk;
            float lower = Mathf.Lerp(0f, -0.38f, 1f - _visible);
            var pos = _restPos + _swing + bob + new Vector3(0.06f * _raise, lower + 0.1f * _raise, 0.04f * _raise);
            transform.localPosition = Vector3.Lerp(transform.localPosition, pos, dt * 12f);
            // the lantern hangs from its handle: it swings against the movement
            float swingZ = -turn * 0.9f + Mathf.Sin(t * 0.5f) * 4f * walk;
            float swingX = pitchTurn * 0.6f + Mathf.Cos(t) * 2f * walk;
            Model.localRotation = Quaternion.Slerp(Model.localRotation, Quaternion.Euler(Mathf.Clamp(swingX, -18f, 18f), 0f, Mathf.Clamp(swingZ, -22f, 22f)), dt * 6f);
        }
    }
}
