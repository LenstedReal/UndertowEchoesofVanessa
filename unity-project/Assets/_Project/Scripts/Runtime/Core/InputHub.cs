using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Undertow
{
    /// <summary>
    /// Input System actions built in code (keyboard + mouse and gamepad), with rebinding saved as JSON.
    /// Tracks which device was used last so prompts show the right glyphs.
    /// </summary>
    public sealed class InputHub
    {
        public const string KbmGroup = "Keyboard&Mouse";
        public const string PadGroup = "Gamepad";

        public readonly InputActionAsset Asset;
        public readonly InputActionMap Player;
        public readonly InputAction Move, Look, Interact, Lantern, Listen, Sprint, Crouch, Journal, Pause, Photo, HideHud;

        public bool UsingGamepad { get; private set; }
        public event Action DeviceChanged;

        readonly string _overridesPath;
        float _rumbleUntil;
        InputActionRebindingExtensions.RebindingOperation _rebind;

        /// <summary>Actions shown in the controls menu (label key, action).</summary>
        public (string key, InputAction action)[] Rebindables => new[]
        {
            ("ui.input.move", Move), ("ui.input.interact", Interact), ("ui.input.lantern", Lantern), ("ui.input.listen", Listen),
            ("ui.input.sprint", Sprint), ("ui.input.crouch", Crouch), ("ui.input.journal", Journal), ("ui.input.photo", Photo),
            ("ui.input.hidehud", HideHud),
        };

        public InputHub(string overridesPath)
        {
            _overridesPath = overridesPath;
            Asset = ScriptableObject.CreateInstance<InputActionAsset>();
            Asset.name = "UndertowInput";
            Asset.AddControlScheme(KbmGroup).WithRequiredDevice("<Keyboard>").WithRequiredDevice("<Mouse>");
            Asset.AddControlScheme(PadGroup).WithRequiredDevice("<Gamepad>");
            Player = Asset.AddActionMap("Player");

            Move = Player.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w", KbmGroup).With("Down", "<Keyboard>/s", KbmGroup)
                .With("Left", "<Keyboard>/a", KbmGroup).With("Right", "<Keyboard>/d", KbmGroup);
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow", KbmGroup).With("Down", "<Keyboard>/downArrow", KbmGroup)
                .With("Left", "<Keyboard>/leftArrow", KbmGroup).With("Right", "<Keyboard>/rightArrow", KbmGroup);
            Move.AddBinding("<Gamepad>/leftStick", groups: PadGroup);

            Look = Player.AddAction("Look", InputActionType.Value, expectedControlLayout: "Vector2");
            Look.AddBinding("<Mouse>/delta", groups: KbmGroup);
            Look.AddBinding("<Gamepad>/rightStick", groups: PadGroup);

            Interact = Button("Interact", "<Keyboard>/e", "<Gamepad>/buttonSouth", "<Mouse>/leftButton");
            Lantern = Button("Lantern", "<Keyboard>/f", "<Gamepad>/buttonNorth");
            Listen = Button("Listen", "<Keyboard>/q", "<Gamepad>/leftTrigger", "<Mouse>/rightButton");
            Sprint = Button("Sprint", "<Keyboard>/leftShift", "<Gamepad>/leftStickPress");
            Crouch = Button("Crouch", "<Keyboard>/c", "<Gamepad>/buttonEast", "<Keyboard>/leftCtrl");
            Journal = Button("Journal", "<Keyboard>/tab", "<Gamepad>/select", "<Keyboard>/j");
            Pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            Photo = Button("Photo", "<Keyboard>/p", "<Gamepad>/dpad/down");
            HideHud = Button("HideHud", "<Keyboard>/h", "<Gamepad>/dpad/up");

            LoadOverrides();
            Asset.Enable();
        }

        InputAction Button(string name, string key, string pad, string altKey = null)
        {
            var a = Player.AddAction(name, InputActionType.Button);
            a.AddBinding(key, groups: KbmGroup);
            if (altKey != null) a.AddBinding(altKey, groups: KbmGroup);
            a.AddBinding(pad, groups: PadGroup);
            return a;
        }

        public void SetGameplay(bool enabled)
        {
            if (enabled) Player.Enable();
            else Player.Disable();
            // Pause must keep working while gameplay input is off
            Pause.Enable();
        }

        public void Tick()
        {
            bool pad = UsingGamepad;
            var g = Gamepad.current;
            if (g != null && (g.buttonSouth.wasPressedThisFrame || g.buttonEast.wasPressedThisFrame || g.buttonNorth.wasPressedThisFrame ||
                              g.buttonWest.wasPressedThisFrame || g.startButton.wasPressedThisFrame || g.selectButton.wasPressedThisFrame ||
                              g.leftStick.ReadValue().sqrMagnitude > 0.2f || g.rightStick.ReadValue().sqrMagnitude > 0.2f ||
                              g.dpad.ReadValue().sqrMagnitude > 0.2f || g.leftTrigger.ReadValue() > 0.3f || g.rightTrigger.ReadValue() > 0.3f))
                pad = true;
            var k = Keyboard.current;
            var m = Mouse.current;
            if ((k != null && k.anyKey.wasPressedThisFrame) || (m != null && (m.delta.ReadValue().sqrMagnitude > 4f || m.leftButton.wasPressedThisFrame)))
                pad = false;
            if (pad != UsingGamepad)
            {
                UsingGamepad = pad;
                DeviceChanged?.Invoke();
            }
            if (_rumbleUntil > 0f && Time.unscaledTime > _rumbleUntil)
            {
                _rumbleUntil = 0f;
                g?.SetMotorSpeeds(0f, 0f);
            }
        }

        public void Rumble(float low, float high, float seconds)
        {
            var g = Gamepad.current;
            if (g == null || Game.Settings == null || !Game.Settings.Rumble || !UsingGamepad) return;
            g.SetMotorSpeeds(Mathf.Clamp01(low), Mathf.Clamp01(high));
            _rumbleUntil = Time.unscaledTime + seconds;
        }

        public void StopRumble()
        {
            _rumbleUntil = 0f;
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }

        /// <summary>Display name of the first binding of the current device group, e.g. "E" or "A".</summary>
        public string Glyph(InputAction a)
        {
            if (a == null) return "?";
            string group = UsingGamepad ? PadGroup : KbmGroup;
            var s = a.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions, group);
            if (string.IsNullOrEmpty(s)) s = a.GetBindingDisplayString();
            int bar = s.IndexOf(" | ", StringComparison.Ordinal);
            if (bar > 0) s = s.Substring(0, bar);
            return s;
        }

        public string Tag(InputAction a) => "[" + Glyph(a) + "]";

        public int BindingIndex(InputAction a, bool gamepad)
        {
            string group = gamepad ? PadGroup : KbmGroup;
            for (int i = 0; i < a.bindings.Count; i++)
            {
                var b = a.bindings[i];
                if (b.isComposite || b.isPartOfComposite) continue;
                if (b.groups != null && b.groups.Contains(group)) return i;
            }
            return -1;
        }

        public bool Rebinding => _rebind != null;

        int _rebindEndFrame = -10;

        /// <summary>
        /// True while a rebind listens and for the frame it ended in. The key that cancelled it (Esc) is
        /// not swallowed by the Input System, so without this the same press would also close the menu.
        /// </summary>
        public bool RebindBusy => _rebind != null || Time.frameCount <= _rebindEndFrame + 1;

        public void StartRebind(InputAction a, bool gamepad, Action<bool> done)
        {
            int index = BindingIndex(a, gamepad);
            if (index < 0)
            {
                done?.Invoke(false);
                return;
            }
            CancelRebind();
            bool wasEnabled = a.enabled;
            a.Disable();
            _rebind = a.PerformInteractiveRebinding(index)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Gamepad>/start")
                .WithCancelingThrough("<Keyboard>/escape")
                .WithBindingGroup(gamepad ? PadGroup : KbmGroup)
                .OnMatchWaitForAnother(0.08f)
                .OnComplete(op =>
                {
                    op.Dispose();
                    _rebind = null;
                    _rebindEndFrame = Time.frameCount;
                    if (wasEnabled) a.Enable();
                    SaveOverrides();
                    done?.Invoke(true);
                })
                .OnCancel(op =>
                {
                    op.Dispose();
                    _rebind = null;
                    _rebindEndFrame = Time.frameCount;
                    if (wasEnabled) a.Enable();
                    done?.Invoke(false);
                })
                .Start();
        }

        public void CancelRebind()
        {
            if (_rebind == null) return;
            _rebind.Cancel();
            _rebind = null;
            _rebindEndFrame = Time.frameCount;
        }

        public void ResetBindings()
        {
            Asset.RemoveAllBindingOverrides();
            SaveOverrides();
        }

        void LoadOverrides()
        {
            try
            {
                if (File.Exists(_overridesPath)) Asset.LoadBindingOverridesFromJson(File.ReadAllText(_overridesPath));
            }
            catch (Exception e)
            {
                Game.Warn("bindings unreadable: " + e.Message);
            }
        }

        public void SaveOverrides()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_overridesPath));
                File.WriteAllText(_overridesPath, Asset.SaveBindingOverridesAsJson());
            }
            catch (Exception e)
            {
                Game.Warn("bindings not saved: " + e.Message);
            }
        }
    }
}
