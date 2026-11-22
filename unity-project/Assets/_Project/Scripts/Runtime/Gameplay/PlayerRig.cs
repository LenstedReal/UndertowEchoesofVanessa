using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// First-person body: CharacterController movement, look, crouch, sprint, wading, head bob, FOV,
    /// footsteps, interaction focus, camera shake and cinematic look-at.
    /// </summary>
    public sealed class PlayerRig : MonoBehaviour
    {
        public CharacterController Body;
        public Transform Head;
        public Transform Shaker;
        public Camera Cam;
        public HDAdditionalCameraData CamData;
        public AudioListener Listener;
        public LanternRig Lantern;
        public ActRuntime Rt;

        public const float StandHeight = Balance.PlayerHeight;
        public const float CrouchHeight = 1.2f;
        public const float StandEye = 1.62f;
        public const float CrouchEye = 1.06f;
        public const int LosMask = ~(1 << 2);

        float _yaw, _pitch;
        float _height = StandHeight, _eye = StandEye;
        bool _crouch, _sprintLatched;
        Vector3 _vel;
        float _vy;
        float _stepAcc, _bobT, _bobAmp;
        float _shakeAmp, _shakeUntil, _shakeSeed;
        float _fovKick, _fovListen;
        float _roll;
        int _lockCount;
        Coroutine _look;
        int _lookGen;
        bool _lookLocked;

        public bool InputLocked => _lockCount > 0;
        public bool Sprinting { get; private set; }
        public bool Crouching => _crouch;
        public bool Wading { get; private set; }
        public bool Grounded => Body != null && Body.isGrounded;
        public float Speed { get; private set; }
        public Interactable Focus { get; private set; }
        public Vector3 Feet => transform.position;
        public Vector3 EyePos => Cam != null ? Cam.transform.position : transform.position + Vector3.up * _eye;
        public Vector3 Forward => Cam != null ? Cam.transform.forward : transform.forward;
        public float Yaw => _yaw;
        public float Pitch => _pitch;

        public static PlayerRig Create(ActRuntime rt, Vector3 feet, float yaw)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(rt.transform, false);
            go.transform.position = feet;
            var p = go.AddComponent<PlayerRig>();
            p.Rt = rt;
            p.Build(yaw);
            return p;
        }

        void Build(float yaw)
        {
            Body = gameObject.AddComponent<CharacterController>();
            Body.radius = Balance.PlayerRadius;
            Body.height = StandHeight;
            Body.center = new Vector3(0, StandHeight * 0.5f, 0);
            Body.stepOffset = Balance.StepHeight;
            Body.slopeLimit = 52f;
            Body.skinWidth = 0.03f;
            Body.minMoveDistance = 0f;

            Head = new GameObject("Head").transform;
            Head.SetParent(transform, false);
            Head.localPosition = new Vector3(0, StandEye, 0);
            Shaker = new GameObject("Shaker").transform;
            Shaker.SetParent(Head, false);
            var camGo = new GameObject("Camera");
            camGo.transform.SetParent(Shaker, false);
            camGo.tag = "MainCamera";
            Cam = camGo.AddComponent<Camera>();
            Cam.nearClipPlane = 0.04f;
            Cam.farClipPlane = 420f;
            CamData = camGo.AddComponent<HDAdditionalCameraData>();
            CamData.dithering = true;
            CamData.stopNaNs = true;
            // Default + Ignore Raycast: the interior volumes (ambient and fog per room) sit on layer 2
            // so they never block interaction rays; the default mask (layer 0 only) would skip them.
            CamData.volumeLayerMask = (1 << 0) | (1 << 2);
            Listener = camGo.AddComponent<AudioListener>();
            Game.Audio.BindListener(Listener);
            Lantern = LanternRig.Create(this);
            _yaw = yaw;
            _pitch = 0f;
            _shakeSeed = Random.value * 100f;
            ApplySettings();
            Game.Settings.Changed += ApplySettings;
            ApplyLook();
        }

        void OnDestroy()
        {
            if (Game.Settings != null) Game.Settings.Changed -= ApplySettings;
        }

        public void ApplySettings()
        {
            var s = Game.Settings;
            switch (s.AntiAliasing)
            {
                case 0: CamData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None; break;
                case 1: CamData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing; break;
                case 3: CamData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing; break;
                default:
                    CamData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    CamData.SMAAQuality = s.Quality >= 2 ? HDAdditionalCameraData.SMAAQualityLevel.High : HDAdditionalCameraData.SMAAQualityLevel.Medium;
                    break;
            }
        }

        // ------------------------------------------------------------------ public controls
        public void Lock() => _lockCount++;
        public void Unlock() => _lockCount = Mathf.Max(0, _lockCount - 1);

        public void Teleport(Vector3 feet, float yaw, float pitch = 0f)
        {
            Body.enabled = false;
            transform.position = feet;
            Body.enabled = true;
            _yaw = yaw;
            _pitch = pitch;
            _vel = Vector3.zero;
            _vy = 0f;
            ApplyLook();
        }

        public void Shake(float amount, float seconds)
        {
            float k = Game.Settings != null ? Game.Settings.CameraShake : 1f;
            if (k <= 0.001f) return;
            _shakeAmp = Mathf.Max(_shakeAmp, amount * k);
            _shakeUntil = Mathf.Max(_shakeUntil, Time.time + seconds);
            Game.Input.Rumble(0.25f * amount, 0.45f * amount, Mathf.Min(seconds, 0.6f));
        }

        /// <summary>Turns the view towards a point over 'seconds' (input is locked meanwhile when lockInput).</summary>
        public CustomYieldInstruction LookAt(Vector3 target, float seconds, bool lockInput = true)
        {
            if (_look != null)
            {
                // a newer look replaces the old one: release its input lock, and its waiter (below) moves on
                StopCoroutine(_look);
                _look = null;
                if (_lookLocked) { _lookLocked = false; Unlock(); }
            }
            int gen = ++_lookGen;
            _look = StartCoroutine(LookCo(target, seconds, lockInput));
            // Never yield on the Coroutine itself: a stopped coroutine would leave its waiter suspended forever.
            return new WaitWhile(() => _look != null && _lookGen == gen);
        }

        IEnumerator LookCo(Vector3 target, float seconds, bool lockInput)
        {
            if (lockInput) { Lock(); _lookLocked = true; }
            var dir = target - EyePos;
            float ty = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float tp = -Mathf.Atan2(dir.y, new Vector2(dir.x, dir.z).magnitude) * Mathf.Rad2Deg;
            float y0 = _yaw, p0 = _pitch;
            float dy = Mathf.DeltaAngle(y0, ty);
            for (float t = 0; t < seconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / seconds);
                _yaw = y0 + dy * k;
                _pitch = Mathf.Lerp(p0, tp, k);
                ApplyLook();
                yield return null;
            }
            _yaw = ty;
            _pitch = Mathf.Clamp(tp, -85f, 85f);
            ApplyLook();
            if (lockInput && _lookLocked) { _lookLocked = false; Unlock(); }
            _look = null;
        }

        // ------------------------------------------------------------------ update
        void Update()
        {
            if (Rt == null || Game.Paused) return;
            float dt = Time.deltaTime;
            bool control = !InputLocked && Game.UI.ScreenCount == 0 && !Game.UI.Cinematic && !Game.Flow.Busy;
            if (control) LookInput(dt);
            Move(dt, control);
            UpdateFov(dt);
            Bob(dt);
            ShakeTick();
            ApplyLook();
            FindFocus();
        }

        void LookInput(float dt)
        {
            var s = Game.Settings;
            Vector2 d;
            var ctrl = Game.Input.Look.activeControl;
            bool stick = ctrl != null && ctrl.device is Gamepad;
            if (stick)
            {
                var v = Game.Input.Look.ReadValue<Vector2>();
                // response curve: fine aim near the centre, fast turns at full tilt
                float m = v.magnitude;
                if (m < 0.12f) v = Vector2.zero;
                else v = v / m * Mathf.Pow((m - 0.12f) / 0.88f, 1.6f);
                d = v * (165f * s.PadSensitivity * dt);
                d.y *= 0.75f;
            }
            else
            {
                var m = Mouse.current;
                d = m != null ? m.delta.ReadValue() * (0.075f * s.MouseSensitivity) : Vector2.zero;
            }
            if (Lantern != null && Rt.Listening) d *= 0.65f;
            _yaw += d.x;
            _pitch += s.InvertY ? d.y : -d.y;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
        }

        void Move(float dt, bool control)
        {
            var s = Game.Settings;
            Vector2 mv = control ? Vector2.ClampMagnitude(Game.Input.Move.ReadValue<Vector2>(), 1f) : Vector2.zero;
            // crouch
            if (control)
            {
                if (s.ToggleCrouch) { if (Game.Input.Crouch.WasPressedThisFrame()) _crouch = !_crouch; }
                else _crouch = Game.Input.Crouch.IsPressed();
            }
            if (!_crouch && _height < StandHeight - 0.01f && !CanStand()) _crouch = true;
            float targetH = _crouch ? CrouchHeight : StandHeight;
            float newH = Mathf.MoveTowards(_height, targetH, dt * 3.2f);
            if (!Mathf.Approximately(newH, _height))
            {
                _height = newH;
                Body.height = _height;
                Body.center = new Vector3(0, _height * 0.5f, 0);
            }
            _eye = Mathf.Lerp(CrouchEye, StandEye, Mathf.InverseLerp(CrouchHeight, StandHeight, _height));
            // sprint
            bool sprintHeld = control && Game.Input.Sprint.IsPressed();
            if (s.ToggleSprint)
            {
                if (control && Game.Input.Sprint.WasPressedThisFrame()) _sprintLatched = !_sprintLatched;
                if (mv.sqrMagnitude < 0.04f) _sprintLatched = false;
                sprintHeld = _sprintLatched;
            }
            Sprinting = sprintHeld && !_crouch && mv.y > 0.35f;
            // water
            float depth = Rt.WaterDepthAt(transform.position);
            Wading = depth > 0.12f;
            float speed = _crouch ? Balance.CrouchSpeed : (Sprinting ? Balance.SprintSpeed : Balance.WalkSpeed);
            if (mv.y < -0.1f) speed *= 0.8f;
            if (Wading) speed *= Mathf.Lerp(1f, Balance.WadeFactor, Mathf.InverseLerp(0.12f, 0.5f, depth));
            if (Rt.Listening) speed *= 0.6f;
            var fwd = Quaternion.Euler(0, _yaw, 0) * Vector3.forward;
            var right = Quaternion.Euler(0, _yaw, 0) * Vector3.right;
            var target = (fwd * mv.y + right * mv.x) * speed;
            float accel = target.sqrMagnitude > _vel.sqrMagnitude ? 10f : 14f;
            _vel = Vector3.MoveTowards(_vel, target, accel * dt);
            if (Body.isGrounded) _vy = -2f;
            else _vy = Mathf.Max(_vy - 19.6f * dt, -30f);
            var flags = Body.Move((_vel + Vector3.up * _vy) * dt);
            var hv = Body.velocity; hv.y = 0f;
            Speed = hv.magnitude;
            if ((flags & CollisionFlags.Sides) != 0 && Speed < _vel.magnitude * 0.5f) _vel = Vector3.Lerp(_vel, hv, 0.5f);
            // fell off the world somehow: back to the last safe point
            if (transform.position.y < Rt.KillY) Rt.Rescue("fell");
            // footsteps
            if (Body.isGrounded && Speed > 0.4f)
            {
                float stride = _crouch ? 0.55f : (Sprinting ? 0.92f : 0.7f);
                _stepAcc += Speed * dt;
                if (_stepAcc >= stride)
                {
                    _stepAcc = 0f;
                    Footstep(depth);
                }
            }
            else _stepAcc = Mathf.Min(_stepAcc, 0.3f);
            // roll a little into strafes
            _roll = Mathf.Lerp(_roll, -mv.x * 1.1f * s.HeadBob, dt * 4f);
        }

        bool CanStand()
        {
            var bottom = transform.position + Vector3.up * (Body.radius + 0.05f);
            float dist = StandHeight - Body.radius * 2f - 0.05f;
            return !Physics.SphereCast(bottom, Body.radius * 0.95f, Vector3.up, out _, dist, LosMask, QueryTriggerInteraction.Ignore);
        }

        void Footstep(float depth)
        {
            var room = Rt.World.RoomAt(transform.position);
            string surf = "wood";
            if (depth > 0.08f) surf = "water";
            else if (room != null)
            {
                switch (room.Surface)
                {
                    case Surface.WetWood: surf = "wetwood"; break;
                    case Surface.Stone: surf = "stone"; break;
                    case Surface.Sand: surf = "sand"; break;
                    case Surface.Shallow: surf = "stone"; break;   // drained streets: wet stone
                    case Surface.Metal: surf = "metal"; break;
                    case Surface.Rug: surf = "rug"; break;
                    default: surf = "wood"; break;
                }
            }
            int n = Random.Range(1, 5);
            float vol = _crouch ? 0.35f : (Sprinting ? 0.95f : 0.65f);
            Game.Audio.PlayAt("step_" + surf + "_" + n, transform.position + Vector3.up * 0.05f, Bus.Effects, vol, Random.Range(0.93f, 1.07f), 1f, 18f);
            Rt.OnFootstep(Sprinting);
        }

        void UpdateFov(float dt)
        {
            float listen = Rt.Listening ? -7f : 0f;
            _fovListen = Mathf.Lerp(_fovListen, listen, dt * 3f);
            _fovKick = Mathf.Lerp(_fovKick, Sprinting ? 3.5f : 0f, dt * 2.5f);
            float h = Mathf.Clamp(Game.Settings.Fov, 60f, 120f) + _fovKick + _fovListen;
            float aspect = Cam.pixelHeight > 0 ? Cam.pixelWidth / (float)Cam.pixelHeight : 16f / 9f;
            float v = 2f * Mathf.Atan(Mathf.Tan(h * 0.5f * Mathf.Deg2Rad) / aspect) * Mathf.Rad2Deg;
            Cam.fieldOfView = Mathf.Clamp(v, 30f, 110f);
        }

        /// <summary>Vertical FOV for a horizontal FOV at the current aspect (used by photo mode too).</summary>
        public float VerticalFov(float horizontal)
        {
            float aspect = Cam.pixelHeight > 0 ? Cam.pixelWidth / (float)Cam.pixelHeight : 16f / 9f;
            return 2f * Mathf.Atan(Mathf.Tan(horizontal * 0.5f * Mathf.Deg2Rad) / aspect) * Mathf.Rad2Deg;
        }

        void Bob(float dt)
        {
            float k = Game.Settings.HeadBob;
            bool moving = Body.isGrounded && Speed > 0.3f;
            float target = moving ? Mathf.Clamp01(Speed / Balance.SprintSpeed) : 0f;
            _bobAmp = Mathf.Lerp(_bobAmp, target, dt * 6f);
            _bobT += dt * (moving ? Mathf.Lerp(6.5f, 10.5f, target) : 2f);
            float y = Mathf.Sin(_bobT) * 0.032f * _bobAmp * k;
            float x = Mathf.Cos(_bobT * 0.5f) * 0.022f * _bobAmp * k;
            // breathing when still
            float breathe = Mathf.Sin(Time.time * 1.3f) * 0.006f * (1f - _bobAmp);
            Head.localPosition = new Vector3(x, _eye + y + breathe, 0f);
        }

        void ShakeTick()
        {
            float amp = 0f;
            if (Time.time < _shakeUntil) amp = _shakeAmp * Mathf.Clamp01((_shakeUntil - Time.time) * 2f);
            else _shakeAmp = 0f;
            // sanity tremor is folded in by the runtime
            amp += Rt.Tremor * (Game.Settings.CameraShake);
            if (amp <= 0.0001f)
            {
                Shaker.localRotation = Quaternion.identity;
                return;
            }
            float t = Time.time * 18f;
            Shaker.localRotation = Quaternion.Euler(
                (Mathf.PerlinNoise(t, _shakeSeed) - 0.5f) * 3.2f * amp,
                (Mathf.PerlinNoise(_shakeSeed, t) - 0.5f) * 3.2f * amp,
                (Mathf.PerlinNoise(t * 0.7f, _shakeSeed + 3f) - 0.5f) * 2f * amp);
        }

        void ApplyLook()
        {
            transform.rotation = Quaternion.Euler(0, _yaw, 0);
            if (Head != null) Head.localRotation = Quaternion.Euler(_pitch, 0, _roll);
        }

        // ------------------------------------------------------------------ interaction focus
        void FindFocus()
        {
            Interactable best = null;
            if (!InputLocked && Game.UI.ScreenCount == 0 && !Game.UI.Cinematic)
            {
                var eye = EyePos;
                var fwd = Forward;
                float bestScore = 0f;
                // direct hit on an item's trigger wins
                if (Physics.Raycast(eye, fwd, out var hit, Balance.InteractRange + 0.2f, LosMask, QueryTriggerInteraction.Collide))
                {
                    var ia = hit.collider.GetComponentInParent<Interactable>();
                    if (ia != null && Usable(ia)) { best = ia; bestScore = 2f; }
                }
                if (best == null)
                {
                    foreach (var kv in Rt.World.Items)
                    {
                        var ia = kv.Value;
                        if (ia == null || !Usable(ia)) continue;
                        var to = ia.Point - eye;
                        float d = to.magnitude;
                        if (d > Balance.InteractRange || d < 0.01f) continue;
                        float ang = Vector3.Angle(fwd, to);
                        float maxAng = Mathf.Lerp(24f, 10f, d / Balance.InteractRange);
                        if (ang > maxAng) continue;
                        if (Physics.Raycast(eye, to / d, out var h2, Mathf.Max(0f, d - 0.3f), LosMask, QueryTriggerInteraction.Ignore) && !h2.collider.transform.IsChildOf(ia.transform)) continue;
                        float score = (1f - ang / maxAng) * 0.7f + (1f - d / Balance.InteractRange) * 0.3f;
                        if (score > bestScore) { bestScore = score; best = ia; }
                    }
                }
            }
            Focus = best;
            var hud = Game.UI.Hud;
            hud.LookingAtInteractable = best != null;
            hud.PromptText = best != null ? Game.Input.Tag(Game.Input.Interact) + "  " + best.Prompt : null;
            hud.PromptDimmed = best != null && best.Item.Need != null && !Rt.Session.NeedsMet(best.Item);
        }

        bool Usable(Interactable ia)
        {
            if (ia.Gone || !ia.gameObject.activeInHierarchy) return false;
            var it = ia.Item;
            if (!Rt.Session.IsInteractable(it)) return false;
            var room = Rt.Act.Room(it.Room);
            if (room != null && !Rt.Session.RoomOpen(room)) return false;
            return true;
        }
    }
}
