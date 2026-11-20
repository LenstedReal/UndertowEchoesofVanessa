using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Settings with five tabs (Game, Accessibility, Display, Audio, Controls). Every change applies live and
    /// is saved when the screen closes. LB/RB or Q/E (and the tab row) switch tabs.
    /// </summary>
    public sealed class SettingsScreen : UIScreen
    {
        static readonly string[] Tabs = { "ui.tab.game", "ui.tab.access", "ui.tab.display", "ui.tab.audio", "ui.tab.controls" };
        int _tab;
        RectTransform _body, _tabRow;
        ScrollRect _scroll;
        Text _desc;
        readonly List<Selectable> _rows = new List<Selectable>();
        readonly Dictionary<Selectable, string> _help = new Dictionary<Selectable, string>();
        readonly List<Text> _tabLabels = new List<Text>();
        Resolution[] _resolutions;

        public SettingsScreen(int tab = 0) { _tab = tab; }

        public override void Build(RectTransform root)
        {
            Root = root;
            _tabLabels.Clear();
            var bg = UIKit.Image(root, "Bg", new Color(0.01f, 0.012f, 0.015f, 0.9f));
            UIKit.Stretch((RectTransform)bg.transform);
            var title = UIKit.Label(root, "Title", Game.Loc["ui.settings"], 60, FontRole.Title, TextAnchor.LowerLeft);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -40), new Vector2(900, 90));
            _tabRow = UIKit.Rect(root, "Tabs");
            UIKit.Anchor(_tabRow, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -150), new Vector2(1600, 56));
            var h = _tabRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 44;
            h.childControlWidth = false;
            h.childForceExpandWidth = false;
            for (int i = 0; i < Tabs.Length; i++)
            {
                int k = i;
                var t = UIKit.Label(_tabRow, "Tab" + i, Game.Loc[Tabs[i]], 30, FontRole.Ui, TextAnchor.MiddleLeft);
                var rt = (RectTransform)t.transform;
                rt.sizeDelta = new Vector2(t.preferredWidth + 10f, 56f);
                t.raycastTarget = true;
                var b = t.gameObject.AddComponent<Button>();
                b.transition = Selectable.Transition.None;
                b.navigation = new Navigation { mode = Navigation.Mode.None };
                b.onClick.AddListener(() => SetTab(k));
                _tabLabels.Add(t);
            }
            var tabHint = UIKit.Label(root, "TabHint", Game.Input.UsingGamepad ? "[LB]  [RB]" : "[Q]  [E]", 22, FontRole.Ui, TextAnchor.MiddleRight, UIKit.Faint);
            UIKit.Anchor((RectTransform)tabHint.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-80, -150), new Vector2(300, 56));
            var rule = UIKit.Image(root, "Rule", UIKit.Line);
            UIKit.Anchor((RectTransform)rule.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -212), new Vector2(1620, 1));
            var area = UIKit.Rect(root, "Area");
            UIKit.Anchor(area, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -232), new Vector2(1100, 700));
            _body = UIKit.ScrollList(area, out _scroll, 2f);
            _desc = UIKit.Label(root, "Desc", "", 24, FontRole.Ui, TextAnchor.UpperLeft, UIKit.Dim);
            UIKit.Anchor((RectTransform)_desc.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(1320, -240), new Vector2(460, 600));
            MenuScreen.Hints(root, (Game.Input.UsingGamepad ? "[X]" : "[R]") + "  " + Game.Loc["ui.reset_tab"]);
            _resolutions = Screen.resolutions.Select(r => new Resolution { width = r.width, height = r.height }).GroupBy(r => r.width * 100000 + r.height).Select(g => g.First()).OrderBy(r => r.width * 100000 + r.height).ToArray();
            Fill();
        }

        void SetTab(int t)
        {
            if (t == _tab) return;
            _tab = (t + Tabs.Length) % Tabs.Length;
            UIKit.PlayMove();
            Fill();
            Select(FirstSelected);
        }

        void Fill()
        {
            foreach (Transform c in _body) UnityEngine.Object.Destroy(c.gameObject);
            _rows.Clear();
            _help.Clear();
            for (int i = 0; i < _tabLabels.Count; i++) _tabLabels[i].color = i == _tab ? UIKit.Accent : UIKit.Dim;
            var s = Game.Settings;
            var loc = Game.Loc;
            string On(bool b) => loc[b ? "ui.on" : "ui.off"];
            string Pct(float v) => Mathf.RoundToInt(v * 100f) + "%";
            switch (_tab)
            {
                case 0:
                    Option("ui.opt.language", () => LocTable.Find(s.Language)?.NativeName ?? Game.Loc.Language.NativeName, d =>
                    {
                        var langs = LocTable.Languages;
                        int i = Array.FindIndex(langs, l => l.Code == Game.Loc.Language.Code);
                        i = (i + d + langs.Length) % langs.Length;
                        s.Language = langs[i].Code;
                        s.LanguageChosen = true;
                        Changed();
                        Game.Loc.Load(s.Language);   // the UI rebuilds every screen in the new language
                    }, "ui.help.language");
                    Option("ui.opt.subtitles", () => On(s.Subtitles), d => { s.Subtitles = !s.Subtitles; Changed(); }, "ui.help.subtitles");
                    Option("ui.opt.speaker_names", () => On(s.SpeakerNames), d => { s.SpeakerNames = !s.SpeakerNames; Changed(); });
                    Option("ui.opt.objective_hints", () => On(s.ObjectiveHints), d => { s.ObjectiveHints = !s.ObjectiveHints; Changed(); }, "ui.help.objective_hints");
                    Option("ui.opt.crosshair", () => On(s.Crosshair), d => { s.Crosshair = !s.Crosshair; Changed(); });
                    Option("ui.opt.hud", () => On(s.HudVisible), d => { s.HudVisible = !s.HudVisible; Changed(); }, "ui.help.hud");
                    Slider("ui.opt.fov", () => Mathf.InverseLerp(70f, 120f, s.Fov), v => { s.Fov = Mathf.Round(Mathf.Lerp(70f, 120f, v)); Changed(); }, () => Mathf.RoundToInt(s.Fov) + "°", 1f / 50f, "ui.help.fov");
                    Slider("ui.opt.headbob", () => s.HeadBob, v => { s.HeadBob = v; Changed(); }, () => Pct(s.HeadBob), 0.1f, "ui.help.headbob");
                    Slider("ui.opt.shake", () => s.CameraShake, v => { s.CameraShake = v; Changed(); }, () => Pct(s.CameraShake), 0.1f);
                    Option("ui.opt.show_version", () => On(s.ShowVersion), d => { s.ShowVersion = !s.ShowVersion; Changed(); });
                    Option("ui.opt.streamer", () => On(s.StreamerMode), d => { s.StreamerMode = !s.StreamerMode; Changed(); }, "ui.help.streamer");
                    break;
                case 1:
                    Option("ui.opt.subtitle_size", () => loc["ui.size." + Mathf.Clamp(s.SubtitleSize, 0, 3)], d => { s.SubtitleSize = (s.SubtitleSize + d + 4) % 4; Changed(); });
                    Slider("ui.opt.subtitle_bg", () => s.SubtitleBackground, v => { s.SubtitleBackground = v; Changed(); }, () => Pct(s.SubtitleBackground), 0.05f);
                    Option("ui.opt.captions", () => On(s.SoundCaptions), d => { s.SoundCaptions = !s.SoundCaptions; Changed(); }, "ui.help.captions");
                    Option("ui.opt.arrows", () => On(s.DirectionArrows), d => { s.DirectionArrows = !s.DirectionArrows; Changed(); }, "ui.help.arrows");
                    Slider("ui.opt.sanity_fx", () => s.SanityEffects, v => { s.SanityEffects = v; Changed(); }, () => Pct(s.SanityEffects), 0.1f, "ui.help.sanity_fx");
                    Option("ui.opt.motion_blur", () => On(s.MotionBlur), d => { s.MotionBlur = !s.MotionBlur; Changed(); });
                    Option("ui.opt.toggle_sprint", () => On(s.ToggleSprint), d => { s.ToggleSprint = !s.ToggleSprint; Changed(); });
                    Option("ui.opt.toggle_crouch", () => On(s.ToggleCrouch), d => { s.ToggleCrouch = !s.ToggleCrouch; Changed(); });
                    Option("ui.opt.toggle_listen", () => On(s.ToggleListen), d => { s.ToggleListen = !s.ToggleListen; Changed(); });
                    Option("ui.opt.mono", () => On(s.Mono), d => { s.Mono = !s.Mono; Changed(); }, "ui.help.mono");
                    break;
                case 2:
                    Option("ui.opt.display_mode", () => loc["ui.display." + s.DisplayMode], d => { s.DisplayMode = (s.DisplayMode + d + 3) % 3; ChangedApply(); });
                    Option("ui.opt.resolution", () => ResText(), d => StepRes(d));
                    Option("ui.opt.vsync", () => On(s.VSync), d => { s.VSync = !s.VSync; ChangedApply(); });
                    Option("ui.opt.frame_cap", () => s.FrameCap == 0 ? loc["ui.unlimited"] : s.FrameCap + " fps", d => { s.FrameCapIndex = (s.FrameCapIndex + d + GameSettings.FrameCaps.Length) % GameSettings.FrameCaps.Length; ChangedApply(); }, "ui.help.frame_cap");
                    Option("ui.opt.quality", () => loc[GameSettings.QualityKeys[Mathf.Clamp(s.Quality, 0, 3)]], d => { s.Quality = (s.Quality + d + 4) % 4; ChangedApply(); }, "ui.help.quality");
                    Option("ui.opt.aa", () => loc["ui.aa." + Mathf.Clamp(s.AntiAliasing, 0, 3)], d => { s.AntiAliasing = (s.AntiAliasing + d + 4) % 4; Changed(); });
                    Slider("ui.opt.brightness", () => Mathf.InverseLerp(-2f, 2f, s.Brightness), v => { s.Brightness = Mathf.Round(Mathf.Lerp(-2f, 2f, v) * 10f) / 10f; Changed(); }, () => (s.Brightness >= 0 ? "+" : "") + s.Brightness.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture), 0.025f, "ui.help.brightness");
                    ActionRow("ui.opt.calibrate", () => Game.UI.Push(new BrightnessScreen(false)));
                    Option("ui.opt.film_grain", () => On(s.FilmGrain), d => { s.FilmGrain = !s.FilmGrain; Changed(); });
                    Option("ui.opt.chromatic", () => On(s.ChromaticAberration), d => { s.ChromaticAberration = !s.ChromaticAberration; Changed(); });
                    Option("ui.opt.dof", () => On(s.DepthOfField), d => { s.DepthOfField = !s.DepthOfField; Changed(); });
                    break;
                case 3:
                    Slider("ui.opt.master", () => s.Master, v => { s.Master = v; Changed(); }, () => Pct(s.Master), 0.05f);
                    Slider("ui.opt.music", () => s.Music, v => { s.Music = v; Changed(); }, () => Pct(s.Music), 0.05f);
                    Slider("ui.opt.effects", () => s.Effects, v => { s.Effects = v; Changed(); }, () => Pct(s.Effects), 0.05f);
                    Slider("ui.opt.voice", () => s.Voice, v => { s.Voice = v; Changed(); }, () => Pct(s.Voice), 0.05f);
                    Slider("ui.opt.ambience", () => s.Ambience, v => { s.Ambience = v; Changed(); }, () => Pct(s.Ambience), 0.05f);
                    Option("ui.opt.limiter", () => On(s.Limiter), d => { s.Limiter = !s.Limiter; Changed(); }, "ui.help.limiter");
                    break;
                case 4:
                    Slider("ui.opt.mouse", () => Mathf.InverseLerp(0.1f, 4f, s.MouseSensitivity), v => { s.MouseSensitivity = Mathf.Round(Mathf.Lerp(0.1f, 4f, v) * 20f) / 20f; Changed(); }, () => s.MouseSensitivity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), 0.0128f);
                    Slider("ui.opt.pad", () => Mathf.InverseLerp(0.2f, 3f, s.PadSensitivity), v => { s.PadSensitivity = Mathf.Round(Mathf.Lerp(0.2f, 3f, v) * 20f) / 20f; Changed(); }, () => s.PadSensitivity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), 0.0178f);
                    Option("ui.opt.invert_y", () => On(s.InvertY), d => { s.InvertY = !s.InvertY; Changed(); });
                    Option("ui.opt.rumble", () => On(s.Rumble), d => { s.Rumble = !s.Rumble; Changed(); });
                    foreach (var (key, action) in Game.Input.Rebindables) BindRow(key, action);
                    ActionRow("ui.opt.reset_bindings", () => Game.UI.Push(new ConfirmScreen(Game.Loc["ui.opt.reset_bindings"], Game.Loc["ui.reset_bindings_body"], () => { Game.Input.ResetBindings(); Fill(); Select(FirstSelected); }, null)));
                    break;
            }
            UIKit.ChainVertical(_rows);
            FirstSelected = _rows.Count > 0 ? _rows[0] : null;
            _scroll.verticalNormalizedPosition = 1f;
        }

        // ------------------------------------------------------------------ rows
        void Track(Selectable s, string helpKey)
        {
            _rows.Add(s);
            if (helpKey != null) _help[s] = helpKey;
        }

        void Option(string key, Func<string> read, Action<int> step, string help = null)
        {
            var o = MenuOption.Make(_body, Game.Loc[key], read, step);
            Track(o, help);
        }

        void Slider(string key, Func<float> get, Action<float> set, Func<string> text, float step, string help = null)
        {
            var sl = MenuSlider.MakeSlider(_body, Game.Loc[key], get, set, text, step);
            Track(sl, help);
        }

        void ActionRow(string key, Action click)
        {
            var b = MenuButton.Create<MenuButton>(_body, Game.Loc[key], 27, click, 52f);
            b.Label.font = UIKit.F(FontRole.Ui);
            Track(b, null);
        }

        void BindRow(string key, InputAction action)
        {
            var b = MenuButton.Create<MenuButton>(_body, Game.Loc[key], 27, null, 52f);
            b.Label.font = UIKit.F(FontRole.Ui);
            var vr = UIKit.Rect(b.transform, "Keys");
            UIKit.Anchor(vr, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(520, 48));
            var val = vr.gameObject.AddComponent<Text>();
            val.font = UIKit.F(FontRole.Ui);
            val.fontSize = 24;
            val.alignment = TextAnchor.MiddleRight;
            val.color = UIKit.Ink;
            val.raycastTarget = false;
            void Show()
            {
                int ki = Game.Input.BindingIndex(action, false), pi = Game.Input.BindingIndex(action, true);
                string k = ki >= 0 ? action.GetBindingDisplayString(ki) : (action == Game.Input.Move ? "W A S D" : "—");
                string p = pi >= 0 ? action.GetBindingDisplayString(pi) : "—";
                val.text = k + "   <color=#5c5e60>|</color>   " + p;
            }
            Show();
            b.OnClick = () =>
            {
                bool pad = Game.Input.UsingGamepad;
                if (Game.Input.BindingIndex(action, pad) < 0) return;
                val.text = "<color=#ff9227>" + Game.Loc["ui.press_key"] + "</color>";
                Game.Input.StartRebind(action, pad, ok => { Show(); UIKit.PlayConfirm(); });
            };
            Track(b, "ui.help.rebind");
        }

        string ResText()
        {
            var s = Game.Settings;
            int w = s.ResolutionWidth > 0 ? s.ResolutionWidth : Screen.currentResolution.width;
            int h = s.ResolutionHeight > 0 ? s.ResolutionHeight : Screen.currentResolution.height;
            return w + " × " + h;
        }

        void StepRes(int d)
        {
            if (_resolutions == null || _resolutions.Length == 0) return;
            var s = Game.Settings;
            int w = s.ResolutionWidth > 0 ? s.ResolutionWidth : Screen.currentResolution.width;
            int h = s.ResolutionHeight > 0 ? s.ResolutionHeight : Screen.currentResolution.height;
            int i = Array.FindIndex(_resolutions, r => r.width == w && r.height == h);
            if (i < 0) i = _resolutions.Length - 1;
            i = Mathf.Clamp(i + d, 0, _resolutions.Length - 1);
            s.ResolutionWidth = _resolutions[i].width;
            s.ResolutionHeight = _resolutions[i].height;
            ChangedApply();
        }

        static void Changed() => Game.Settings.NotifyChanged();

        static void ChangedApply()
        {
            Game.Settings.Apply();
            Game.Settings.NotifyChanged();
        }

        // ------------------------------------------------------------------ input
        public override void Tick()
        {
            if (Game.Input.Rebinding) return;
            var k = Keyboard.current;
            var g = Gamepad.current;
            if ((k != null && k.qKey.wasPressedThisFrame) || (g != null && g.leftShoulder.wasPressedThisFrame)) SetTab(_tab - 1);
            else if ((k != null && k.eKey.wasPressedThisFrame) || (g != null && g.rightShoulder.wasPressedThisFrame)) SetTab(_tab + 1);
            else if ((k != null && k.rKey.wasPressedThisFrame) || (g != null && g.buttonWest.wasPressedThisFrame)) ResetTab();
            var es = EventSystem.current;
            var sel = es != null && es.currentSelectedGameObject != null ? es.currentSelectedGameObject.GetComponent<Selectable>() : null;
            _desc.text = sel != null && _help.TryGetValue(sel, out var key) ? Game.Loc[key] : "";
            // keep the selected row on screen
            if (sel != null && sel.transform.IsChildOf(_scroll.content))
            {
                var rt = (RectTransform)sel.transform;
                float contentH = _scroll.content.rect.height, viewH = _scroll.viewport.rect.height;
                if (contentH > viewH)
                {
                    float y = -rt.anchoredPosition.y;
                    float target = Mathf.Clamp01((y - viewH * 0.5f) / (contentH - viewH));
                    _scroll.verticalNormalizedPosition = Mathf.Lerp(_scroll.verticalNormalizedPosition, 1f - target, Time.unscaledDeltaTime * 12f);
                }
            }
        }

        void ResetTab()
        {
            var s = Game.Settings;
            switch (_tab)
            {
                case 0:
                case 1:
                case 4: s.ResetSection("gameplay"); break;
                case 2: s.ResetSection("graphics"); s.Apply(); break;
                case 3: s.ResetSection("audio"); break;
            }
            UIKit.PlayConfirm();
            Fill();
            Select(FirstSelected);
        }

        public override void OnBack()
        {
            if (Game.Input.Rebinding) { Game.Input.CancelRebind(); return; }
            Game.Settings.Save(Game.Saves.SettingsPath);
            base.OnBack();
        }

        public override void OnHide()
        {
            Game.Settings.Save(Game.Saves.SettingsPath);
        }
    }
}
