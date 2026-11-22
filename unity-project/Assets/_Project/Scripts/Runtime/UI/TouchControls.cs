using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Undertow
{
    /// <summary>
    /// TEST BUILD ONLY. On-screen buttons for testing through a phone stream (cloud PC + Moonlight), where taps reach the
    /// game as mouse clicks and no gamepad does. Every control is a single tap: walking is a toggle, turning is a step,
    /// so one finger can play the whole game. A tap on the world turns the view towards that point; a tap on the
    /// crosshair uses what it points at.
    /// Before the Steam release set <see cref="Enabled"/> to false (every hook then does nothing),
    /// or delete this file together with its calls (search for "TouchControls").
    /// </summary>
    public static class TouchControls
    {
        /// <summary>TEST BUILD: set to false for the Steam release.</summary>
        public static readonly bool Enabled = true;

        /// <summary>True while the overlay is on screen and the player can act.</summary>
        public static bool Active { get; private set; }

        static RectTransform _layer;
        static int _forward, _strafe;
        static bool _crouch, _sprint, _listen, _level;
        static int _interactAt = -100, _lanternAt = -100, _journalAt = -100, _pauseAt = -100;
        static float _pendingYaw, _pendingPitch;
        static readonly List<KeyValuePair<Image, Func<bool>>> Toggles = new List<KeyValuePair<Image, Func<bool>>>();

        static readonly Color Off = new Color(0f, 0f, 0f, 0.42f);
        static readonly Color On = new Color(1f, 0.573f, 0.153f, 0.55f);

        // ------------------------------------------------------------------ read by gameplay
        public static Vector2 Move => Active ? Vector2.ClampMagnitude(new Vector2(_strafe, _forward), 1f) : Vector2.zero;
        public static bool Crouching => Active && _crouch;
        public static bool Sprinting => Active && _sprint;
        public static bool Listening => Active && _listen;
        public static bool TakeInteract() => Take(ref _interactAt);
        public static bool TakeLantern() => Take(ref _lanternAt);
        public static bool TakeJournal() => Take(ref _journalAt);
        public static bool TakePause() => Take(ref _pauseAt);

        // A tap is dispatched by the event system, before or after the reader's Update: last frame's tap still counts.
        static bool Take(ref int frame)
        {
            if (!Active || Time.frameCount - frame > 1) return false;
            frame = -100;
            return true;
        }

        /// <summary>The turn asked for by taps and buttons, a little per frame (x yaw, y pitch, degrees).</summary>
        public static Vector2 TakeLook(float dt, float currentPitch)
        {
            if (!Active)
            {
                _pendingYaw = _pendingPitch = 0f;
                _level = false;
                return Vector2.zero;
            }
            if (_level)
            {
                _pendingPitch = -currentPitch;
                _level = false;
            }
            return new Vector2(Approach(ref _pendingYaw, 240f * dt), Approach(ref _pendingPitch, 160f * dt));
        }

        static float Approach(ref float pending, float maxStep)
        {
            float step = Mathf.Clamp(pending, -maxStep, maxStep);
            pending -= step;
            return step;
        }

        // ------------------------------------------------------------------ overlay
        /// <summary>Builds the overlay, hidden, just above the HUD. Called once by <see cref="UIRoot"/>.</summary>
        public static void Build(RectTransform canvasRoot)
        {
            if (!Enabled) return;
            Toggles.Clear();
            _layer = UIKit.Stretch(UIKit.Rect(canvasRoot, "TouchControls"));

            var world = UIKit.Image(_layer, "World", new Color(0f, 0f, 0f, 0f));
            UIKit.Stretch((RectTransform)world.transform);
            world.raycastTarget = true;
            world.gameObject.AddComponent<TouchWorldTap>();

            var left = new Vector2(0f, 0f);
            var right = new Vector2(1f, 0f);
            // left: walking and turning
            Button(left, 100, 490, 150, "DÜZ BAK", () => _level = true);
            Toggle(left, 260, 490, 150, "İLERİ", () => _forward = _forward == 1 ? 0 : 1, () => _forward == 1);
            Toggle(left, 420, 490, 150, "KOŞ", () => _sprint = !_sprint, () => _sprint);
            Toggle(left, 100, 400, 150, "SOL", () => _strafe = _strafe == -1 ? 0 : -1, () => _strafe == -1);
            Button(left, 260, 400, 150, "DUR", () => { _forward = _strafe = 0; _sprint = false; });
            Toggle(left, 420, 400, 150, "SAĞ", () => _strafe = _strafe == 1 ? 0 : 1, () => _strafe == 1);
            Toggle(left, 100, 310, 150, "EĞİL", () => _crouch = !_crouch, () => _crouch);
            Toggle(left, 260, 310, 150, "GERİ", () => _forward = _forward == -1 ? 0 : -1, () => _forward == -1);
            Button(left, 100, 220, 150, "< DÖN", () => _pendingYaw -= 45f);
            Button(left, 260, 220, 150, "ARKA", () => _pendingYaw += 180f);
            Button(left, 420, 220, 150, "DÖN >", () => _pendingYaw += 45f);
            // right: acting
            Button(right, -220, 490, 310, "AL / KULLAN", () => _interactAt = Time.frameCount);
            Button(right, -300, 400, 150, "FENER", () => _lanternAt = Time.frameCount);
            Toggle(right, -140, 400, 150, "DİNLE", () => _listen = !_listen, () => _listen);
            Button(right, -300, 310, 150, "GÜNLÜK", () => _journalAt = Time.frameCount);
            Button(right, -140, 310, 150, "MENÜ", () => _pauseAt = Time.frameCount);

            _layer.gameObject.SetActive(false);
            Active = false;
        }

        static Image Button(Vector2 anchor, float x, float y, float w, string text, Action onClick)
        {
            var rt = UIKit.TextButton(_layer, text, text, 24, () =>
            {
                onClick();
                UIKit.PlayMove();
                // a tapped button must not stay selected, or Enter / Space would press it again
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }, Off);
            UIKit.Anchor(rt, anchor, new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(w, 80f));
            return rt.GetComponent<Image>();
        }

        static void Toggle(Vector2 anchor, float x, float y, float w, string text, Action onClick, Func<bool> isOn)
        {
            Toggles.Add(new KeyValuePair<Image, Func<bool>>(Button(anchor, x, y, w, text, onClick), isOn));
        }

        /// <summary>Every frame from <see cref="UIRoot"/>: the overlay shows only while the player can act.</summary>
        public static void Tick()
        {
            if (!Enabled || _layer == null)
            {
                Active = false;
                return;
            }
            var ui = Game.UI;
            var act = Game.Act;
            bool show = ui != null && ui.InGame && ui.ScreenCount == 0 && !ui.Cinematic && !ui.Loading &&
                        act != null && act.Player != null && !act.Player.InputLocked &&
                        Game.Flow != null && !Game.Flow.Busy && !Game.Input.UsingGamepad;
            if (show != Active)
            {
                Active = show;
                _layer.gameObject.SetActive(show);
                if (!show)
                {
                    // never keep walking into whatever opened (a note, a cutscene, the pause menu)
                    _forward = _strafe = 0;
                    _sprint = _listen = false;
                    _pendingYaw = _pendingPitch = 0f;
                }
            }
            if (!show) return;
            foreach (var t in Toggles) t.Key.color = t.Value() ? On : Off;
        }

        /// <summary>A tap on the world: near the crosshair it uses the focus, elsewhere it turns the view there.</summary>
        internal static void WorldTap(Vector2 screenPos)
        {
            if (!Active) return;
            float dx = screenPos.x - Screen.width * 0.5f;
            float dy = screenPos.y - Screen.height * 0.5f;
            float near = Screen.height * 0.08f;
            if (dx * dx + dy * dy < near * near)
            {
                _interactAt = Time.frameCount;
                return;
            }
            var cam = Game.Act != null && Game.Act.Player != null ? Game.Act.Player.Cam : null;
            float vfov = cam != null ? cam.fieldOfView : 70f;
            float f = Screen.height * 0.5f / Mathf.Tan(vfov * 0.5f * Mathf.Deg2Rad);
            _pendingYaw = Mathf.Atan2(dx, f) * Mathf.Rad2Deg;
            _pendingPitch = -Mathf.Atan2(dy, Mathf.Sqrt(dx * dx + f * f)) * Mathf.Rad2Deg;
        }
    }

    /// <summary>The transparent catcher behind the touch buttons.</summary>
    public sealed class TouchWorldTap : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            TouchControls.WorldTap(eventData.position);
        }
    }
}
