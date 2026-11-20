using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    public sealed partial class ActRuntime
    {
        // light / sanity / presence
        float _presence;
        Vector3 _lastLight;
        float _lastLightYaw;
        float _lastLightTimer;
        SanityTier _tier;
        bool _wasBreaking;
        float _heartT, _whisperT, _ambT, _thunderT, _creakT;
        bool _rescuing;
        public bool InLight { get; private set; }
        public bool InSafeLight { get; private set; }
        public bool InDark { get; private set; }

        // voices
        sealed class Pending { public VoiceCue Cue; public float At; }
        readonly List<Pending> _pending = new List<Pending>();
        readonly HashSet<string> _queued = new HashSet<string>();
        VoiceCue _lead;
        AudioSource _leadSource;
        Transform _leadPoint;
        float _leadSince;

        // hints, goals, listening
        readonly Dictionary<string, int> _hintShown = new Dictionary<string, int>();
        string _goalFlag;
        float _listenHeld;
        bool _listenToggled;
        float _seaLevel = float.NaN;

        void InitSystems()
        {
            _tier = Session.Sanity.Tier;
            foreach (var w in World.Waters) if (w != null && w.Sea) _seaLevel = w.Level;
            _ambT = Random.Range(8f, 16f);
            _thunderT = Random.Range(12f, 30f);
            _creakT = Random.Range(5f, 12f);
            // a lead voice the player had not reached yet keeps calling after a reload
            foreach (var v in Act.Voices)
                if (v.Leads && v.SetsFlag != null && State.Has("voice:" + v.Id) && !State.Has(v.SetsFlag) && Conditions(v)) StartLead(v);
        }

        void SystemsTick(float dt, Vector3 feet)
        {
            EvaluateLight(feet);
            bool sea = StaringAtSea();
            var input = new SanityInput { InSafeLight = InSafeLight, Dark = InDark, Moonlit = !InLight && !InSafeLight && !InDark, StaringAtSea = sea, Dt = dt };
            if (Listening) input.Dt = dt * 0.6f;   // listening calms a little
            if (Session.Sanity.Tick(input)) OnTierChanged();
            Session.SyncToState();
            SanityFx(dt);
            PresenceTick(dt, feet);
            VoicesTick(dt, feet);
            HintsTick(dt);
            GoalsTick();
            DistortTick();
            AmbientTick(dt, feet);
        }

        // ------------------------------------------------------------------ light
        void EvaluateLight(Vector3 feet)
        {
            var l = Session.Lantern;
            bool lantern = l.Lit && l.Strength01 > 0.3f;
            bool zone = false, safeZone = false;
            var chest = feet + Vector3.up * 1.1f;
            foreach (var z in World.LightZones)
            {
                if (!z.IsActive) continue;
                if ((z.Pos - chest).sqrMagnitude > z.Radius * z.Radius) continue;
                zone = true;
                if (z.Safe) safeZone = true;
            }
            InSafeLight = safeZone || (Room != null && Room.Safe);
            InLight = lantern || zone || InSafeLight;
            bool moon = Room != null && Room.Outdoor && Act.Moon > 0.15f;
            InDark = !InLight && !moon;
            if (InLight && Player.Grounded && !Player.Wading)
            {
                _lastLightTimer -= Time.deltaTime;
                if (_lastLightTimer <= 0f)
                {
                    _lastLightTimer = 0.5f;
                    _lastLight = feet;
                    _lastLightYaw = Player.Yaw;
                }
            }
        }

        bool StaringAtSea()
        {
            if (Room == null || !Room.SeaView || float.IsNaN(_seaLevel)) return false;
            var eye = Player.EyePos;
            var dir = Player.Forward;
            if (dir.y > 0.1f) return false;
            float dist = dir.y < -0.02f ? Mathf.Min(60f, (eye.y - _seaLevel) / -dir.y) : 60f;
            return !Physics.Raycast(eye, dir, dist, PlayerRig.LosMask, QueryTriggerInteraction.Ignore);
        }

        void OnTierChanged()
        {
            var t = Session.Sanity.Tier;
            if (t > _tier) Game.Audio.Play2D("sfx_sting_low", Bus.Effects, 0.4f + 0.15f * (int)t);
            if (t == SanityTier.Breaking) _wasBreaking = true;
            if (t == SanityTier.Stable && _wasBreaking)
            {
                _wasBreaking = false;
                Game.Achievements.Unlock("brink");
            }
            _tier = t;
        }

        void SanityFx(float dt)
        {
            float stress = Session.Sanity.Stress01;
            float p = Mathf.Clamp01(_presence);
            World.Atmos.SetStress(Mathf.Max(stress, p * 0.9f));
            var tier = Session.Sanity.Tier;
            Tremor = tier == SanityTier.Breaking ? 0.12f : (p > 0.6f ? (p - 0.6f) * 0.3f : 0f);
            // heartbeat when frightened or something is close in the dark
            float rate = tier >= SanityTier.Disturbed || p > 0.35f ? Mathf.Lerp(1.15f, 0.62f, Mathf.Max(p, tier == SanityTier.Breaking ? 1f : 0.4f)) : 0f;
            if (rate > 0f)
            {
                _heartT -= dt;
                if (_heartT <= 0f)
                {
                    _heartT = rate;
                    Game.Audio.Play2D("sfx_heartbeat", Bus.Effects, Mathf.Lerp(0.25f, 0.6f, Mathf.Max(p, stress)));
                    if (Game.Settings.Rumble) Game.Input.Rumble(0.18f * Mathf.Max(p, 0.4f), 0f, 0.09f);
                }
            }
            if (tier == SanityTier.Breaking || p > 0.7f)
            {
                _whisperT -= dt;
                if (_whisperT <= 0f)
                {
                    _whisperT = Random.Range(7f, 14f);
                    var behind = Player.Feet - Player.Forward * 2.2f + Vector3.up * 1.5f + Random.insideUnitSphere * 0.8f;
                    Game.Audio.PlayAt("sfx_whisper_" + Random.Range(1, 4), behind, Bus.Effects, 0.55f, Random.Range(0.92f, 1.05f), 0.8f, 9f);
                    if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.whisper"], 2f, behind);
                }
            }
        }

        // ------------------------------------------------------------------ presence: what happens in the dark
        void PresenceTick(float dt, Vector3 feet)
        {
            if (_rescuing || Finishing) return;
            if (InDark) _presence += Balance.PresenceRiseDark * dt * (Player.Sprinting ? 1.25f : 1f);
            else _presence -= Balance.PresenceFallLight * dt * (InLight ? 1f : 0.35f);
            _presence = Mathf.Clamp(_presence, 0f, 1.05f);
            if (_presence >= Balance.PresenceDisplace) Rescue("dark");
        }

        /// <summary>Taken by the dark (or fallen): fade, wake at the last light, a little poorer.</summary>
        public void Rescue(string reason)
        {
            if (_rescuing || Finishing) return;
            StartCoroutine(RescueCo(reason));
        }

        IEnumerator RescueCo(string reason)
        {
            _rescuing = true;
            Player.Lock();
            Game.Audio.PlayAt("sfx_undertow_breath", Player.EyePos - Player.Forward * 0.8f, Bus.Effects, 0.9f, 1f, 0.5f, 6f);
            if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.breath"], 2.5f);
            Player.Shake(0.6f, 1.2f);
            yield return Game.UI.Fade(1f, 1.1f);
            Player.Teleport(_lastLight + Vector3.up * 0.05f, _lastLightYaw);
            _presence = 0f;
            if (reason != "fell") Session.Sanity.Apply(-6f);
            if (Session.Lantern.Oil < 10f)
            {
                // never leave the player stuck in the dark with an empty lantern
                Session.Lantern.Restore(18f, true);
                Game.UI.Hud.Message(Game.Loc["msg.mercy_oil"], 5f);
            }
            Session.SyncToState();
            yield return new WaitForSeconds(0.6f);
            Game.UI.Hud.Message(Game.Loc[reason == "fell" ? "msg.rescued_fell" : "msg.rescued_dark"], 4.5f);
            yield return Game.UI.Fade(0f, 1.4f);
            Player.Unlock();
            _rescuing = false;
        }

        // ------------------------------------------------------------------ voices
        bool Conditions(VoiceCue v)
        {
            if (v.WhenFlag != null && !State.Has(v.WhenFlag)) return false;
            if (v.UnlessFlag != null && State.Has(v.UnlessFlag)) return false;
            return true;
        }

        void VoicesTick(float dt, Vector3 feet)
        {
            foreach (var v in Act.Voices)
            {
                if (_queued.Contains(v.Id) || State.Has("voice:" + v.Id)) continue;
                if (!Conditions(v)) continue;
                bool trig;
                if (v.TriggerRoom != null) trig = Room != null && Room.Id == v.TriggerRoom;
                else if (v.TriggerRadius > 0f) trig = new Vector2(feet.x - v.TriggerAt.X, feet.z - v.TriggerAt.Z).magnitude < v.TriggerRadius;
                else trig = true;
                if (!trig) continue;
                _queued.Add(v.Id);
                _pending.Add(new Pending { Cue = v, At = Time.time + v.Delay });
            }
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var p = _pending[i];
                if (Time.time < p.At) continue;
                _pending.RemoveAt(i);
                if (!Conditions(p.Cue) || Finishing) { _queued.Remove(p.Cue.Id); continue; }
                PlayVoice(p.Cue);
            }
            LeadTick(feet);
        }

        public Vector3 VoicePos(VoiceCue v)
        {
            var g = World.Ground(v.From.X, v.From.Z, Player.Feet.y + 1f);
            return new Vector3(v.From.X, g.y + v.Height, v.From.Z);
        }

        void PlayVoice(VoiceCue v)
        {
            State.Set("voice:" + v.Id);
            var from = VoicePos(v);
            string text = Game.Loc[v.LineKey];
            float seconds = Mathf.Clamp(2.2f + text.Split(' ').Length * 0.38f, 2.5f, 9f);
            string clip = v.Clip ?? ("vo_" + v.Id);
            bool inner = v.Kind == VoiceKind.Adrian;
            if (Game.Audio.Clip(clip) == null) clip = "vo_hum_" + v.Kind.ToString().ToLowerInvariant();
            if (inner) Game.Audio.Play2D(clip, Bus.Voice, 0.9f);
            else Game.Audio.PlayAt(clip, from, Bus.Voice, 1f, 1f, 2.5f, 45f, true);
            Game.UI.Subtitles.Say(v.Kind, text, seconds, inner ? (Vector3?)null : from);
            if (v.Kind == VoiceKind.Mimic) World.Atmos.Flash(0.08f);
            if (v.Leads) StartLead(v);
            else if (v.SetsFlag != null) RaiseFlag(v.SetsFlag);
        }

        void StartLead(VoiceCue v)
        {
            StopLead();
            _lead = v;
            _leadSince = Time.time;
            var go = new GameObject("lead_" + v.Id);
            go.transform.SetParent(World.Root, false);
            go.transform.position = VoicePos(v);
            _leadPoint = go.transform;
            string loop = v.Kind == VoiceKind.Mimic ? "lead_mimic" : "lead_echo";
            _leadSource = Game.Audio.Loop(loop, _leadPoint, Bus.Voice, 0.75f, 2f, 40f, true);
        }

        void StopLead()
        {
            if (_leadSource != null) Game.Audio.Stop(_leadSource, 1.2f);
            _leadSource = null;
            if (_leadPoint != null) Destroy(_leadPoint.gameObject, 1.5f);
            _leadPoint = null;
            _lead = null;
        }

        void LeadTick(Vector3 feet)
        {
            if (_lead == null) return;
            var from = VoicePos(_lead);
            float d = new Vector2(feet.x - from.x, feet.z - from.z).magnitude;
            bool reached = d < 2.1f && Mathf.Abs(feet.y + 1.2f - from.y) < 3f;
            bool expired = _lead.SetsFlag == null && Time.time - _leadSince > 40f;
            if (reached)
            {
                var v = _lead;
                StopLead();
                if (v.SetsFlag != null) RaiseFlag(v.SetsFlag);
                Game.Audio.PlayAt(v.Kind == VoiceKind.Mimic ? "sfx_mimic_reached" : "sfx_echo_reached", from, Bus.Voice, 0.7f);
                return;
            }
            if (expired) StopLead();
        }

        /// <summary>Where listening points: a calling voice first, then the current puzzle's sound, then the goal.</summary>
        public Vector3? ListenTarget()
        {
            if (_lead != null) return VoicePos(_lead);
            if (Room != null)
            {
                foreach (var it in Act.Items)
                {
                    if (it.Room != Room.Id || it.Puzzle == null || Session.PuzzleSolved(it.Puzzle)) continue;
                    if (it.Kind == ItemKind.Listen) return World.ItemPoint(it);
                }
            }
            if (!Game.Settings.ObjectiveHints) return null;
            var g = Session.CurrentGoal;
            if (g == null) return null;
            return World.Ground(g.Pos.X, g.Pos.Z, Player.Feet.y + 1f) + Vector3.up * 1.2f;
        }

        // ------------------------------------------------------------------ listening
        void ListenTick(bool control, float dt)
        {
            bool want;
            if (Game.Settings.ToggleListen)
            {
                if (control && Game.Input.Listen.WasPressedThisFrame()) _listenToggled = !_listenToggled;
                want = control && _listenToggled;
            }
            else want = control && Game.Input.Listen.IsPressed();
            if (want != Listening)
            {
                Listening = want;
                Game.Audio.Duck = want ? 0.55f : 0f;
                Game.Audio.PlayAt(want ? "sfx_listen_in" : "sfx_listen_out", Player.EyePos, Bus.Ui, 0.35f);
                if (!want) _listenHeld = 0f;
            }
            if (Listening)
            {
                _listenHeld += dt;
                if (_listenHeld >= 0.8f && _listenHeld - dt < 0.8f)
                {
                    State.Listens++;
                    if (State.Listens >= AchievementProfile.ListenerCount) Game.Achievements.Unlock("listener");
                }
            }
            var hud = Game.UI.Hud;
            hud.Listening = Listening;
            hud.ListenTarget = Listening ? ListenTarget() : null;
            Player.Lantern.Raise(Listening ? 0.6f : 0f);
        }

        // ------------------------------------------------------------------ hints and goals
        void HintsTick(float dt)
        {
            if (Room == null) return;
            Session.TickPuzzleTime(Room.Id, dt);
            if (!Game.Settings.ObjectiveHints) return;
            foreach (var p in ActSession.Puzzles.Values)
            {
                if (p.Room != Room.Id || p.Flag == null || State.Has(p.Flag)) continue;
                int lvl = Session.HintLevel(p.Id);
                _hintShown.TryGetValue(p.Id, out int shown);
                if (lvl <= shown) continue;
                _hintShown[p.Id] = lvl;
                string key = "hint." + p.Id + "." + lvl;
                if (!Game.Loc.Has(key)) continue;
                // hints come as Echo's whisper, never as a UI arrow
                Game.UI.Subtitles.Say(VoiceKind.Echo, Game.Loc[key], 5.5f);
                Game.Audio.Play2D("vo_hum_echo", Bus.Voice, 0.5f);
            }
        }

        void GoalsTick()
        {
            var g = Session.CurrentGoal;
            string f = g?.Flag;
            if (f == _goalFlag) return;
            _goalFlag = f;
            if (g != null && Game.Settings.ObjectiveHints) Game.UI.Hud.Objective(Game.Loc[g.TextKey]);
        }

        // ------------------------------------------------------------------ distortion: things move when unseen
        void DistortTick()
        {
            var tier = Session.Sanity.Tier;
            int want = tier == SanityTier.Stable ? 0 : (tier == SanityTier.Uneasy ? 1 : (tier == SanityTier.Disturbed ? 2 : 3));
            var eye = Player.EyePos;
            var fwd = Player.Forward;
            float cos = Mathf.Cos((Game.Settings.Fov * 0.5f + 14f) * Mathf.Deg2Rad);
            foreach (var d in World.Distortables)
            {
                if (d == null || d.Level == want) continue;
                var c = d.transform.position + Vector3.up * 0.4f;
                var to = c - eye;
                float dist = to.magnitude;
                bool seen = dist < 3.2f || (Vector3.Dot(fwd, to / Mathf.Max(0.01f, dist)) > cos && !Physics.Raycast(eye, to / dist, dist - 0.3f, PlayerRig.LosMask, QueryTriggerInteraction.Ignore));
                if (seen) continue;
                d.Level = want;
                float k = want == 0 ? 0f : (want == 1 ? 0.15f : (want == 2 ? 0.5f : 1f));
                d.transform.position = Vector3.Lerp(d.HomePos, d.AltPos, k);
                d.transform.rotation = Quaternion.Slerp(d.HomeRot, d.AltRot, k);
            }
        }

        // ------------------------------------------------------------------ ambient life: creaks, thunder, drips
        void AmbientTick(float dt, Vector3 feet)
        {
            _creakT -= dt;
            if (_creakT <= 0f)
            {
                _creakT = Random.Range(7f, 18f);
                string id;
                switch (Act.Id)
                {
                    case 1: id = "sfx_hull_creak_" + Random.Range(1, 4); break;
                    case 2: id = "sfx_wood_groan_" + Random.Range(1, 3); break;
                    case 3: id = "sfx_drip_" + Random.Range(1, 4); break;
                    default: id = "sfx_temple_settle_" + Random.Range(1, 3); break;
                }
                var p = feet + new Vector3(Random.Range(-9f, 9f), Random.Range(0.5f, 4f), Random.Range(-9f, 9f));
                Game.Audio.PlayAt(id, p, Bus.Ambience, Random.Range(0.35f, 0.7f), Random.Range(0.9f, 1.08f), 2f, 30f);
            }
            if (Act.Id == 1 || Act.Id == 2)
            {
                _thunderT -= dt;
                if (_thunderT <= 0f)
                {
                    _thunderT = Random.Range(28f, 70f);
                    StartCoroutine(Lightning());
                }
            }
            if (Act.Id == 4)
            {
                _ambT -= dt;
                if (_ambT <= 0f)
                {
                    _ambT = Random.Range(35f, 80f);
                    var far = feet + new Vector3(Random.Range(-60f, 60f), -10f, Random.Range(40f, 90f));
                    Game.Audio.PlayAt("sfx_whale_far", far, Bus.Ambience, 0.8f, Random.Range(0.85f, 1f), 20f, 220f);
                    if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.deep_call"], 3f, far);
                    Player.Shake(0.08f, 2.5f);
                }
            }
        }

        IEnumerator Lightning()
        {
            bool outdoor = Room != null && Room.Outdoor;
            float far = Random.Range(0.4f, 1f);
            World.Atmos.Flash(outdoor ? 0.7f * far : 0.25f * far);
            if (World.Moon != null)
            {
                float baseLux = World.MoonLux;
                World.Moon.intensity = baseLux * (outdoor ? 14f : 6f) * far;
                yield return new WaitForSeconds(0.09f);
                World.Moon.intensity = baseLux;
                yield return new WaitForSeconds(0.12f);
                World.Moon.intensity = baseLux * 6f * far;
                yield return new WaitForSeconds(0.07f);
                World.Moon.intensity = baseLux;
            }
            yield return new WaitForSeconds(Random.Range(1.2f, 3.6f) * (2f - far));
            Game.Audio.Play2D("sfx_thunder_" + Random.Range(1, 3), Bus.Ambience, 0.5f + 0.4f * far);
            if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.thunder"], 2.5f);
            if (far > 0.8f) Player.Shake(0.1f, 1.2f);
        }

        public void OnFootstep(bool sprinting)
        {
            // running in the dark is loud: the presence hears it
            if (InDark && sprinting) _presence += 0.01f;
        }
    }
}
