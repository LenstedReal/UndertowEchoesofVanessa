using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>First start: language, content warning, brightness, the studio card, then the main menu.</summary>
    public static class BootSequence
    {
        public static bool Done { get; private set; }

        public static IEnumerator Run()
        {
            var s = Game.Settings;
            Game.UI.SetFade(1f);
            yield return null;
            if (!s.LanguageChosen)
            {
                bool done = false;
                Game.UI.Push(new LanguageScreen(() => done = true));
                yield return Game.UI.Fade(0f, 0.8f);
                while (!done) yield return null;
                yield return Game.UI.Fade(1f, 0.4f);
            }
            if (!s.WarningSeen)
            {
                bool done = false;
                Game.UI.Push(new WarningScreen(() => done = true));
                yield return Game.UI.Fade(0f, 0.8f);
                while (!done) yield return null;
                yield return Game.UI.Fade(1f, 0.5f);
            }
            if (!s.Calibrated)
            {
                bool done = false;
                Game.UI.Push(new BrightnessScreen(true, () => done = true));
                yield return Game.UI.Fade(0f, 0.8f);
                while (!done) yield return null;
                yield return Game.UI.Fade(1f, 0.5f);
            }
            s.Save(Game.Saves.SettingsPath);
            yield return Splash();
            Done = true;
            Game.UI.Replace(new MainMenu());
            if (!Game.Audio.MusicPlaying) Game.Audio.PlayMusic("mus_menu", 3f, 0.8f);
            yield return Game.UI.Fade(0f, 1.8f);
        }

        static IEnumerator Splash()
        {
            var layer = UIKit.Stretch(UIKit.Rect(Game.UI.OverlayLayer, "Splash"));
            var bg = UIKit.Image(layer, "Bg", Color.black);
            UIKit.Stretch((RectTransform)bg.transform);
            var group = layer.gameObject.AddComponent<CanvasGroup>();
            var logoTex = Resources.Load<Texture2D>("Undertow/UI/lenstedreal_logo");
            if (logoTex != null && logoTex.width > 32)
            {
                var logo = UIKit.Rect(layer, "Logo");
                UIKit.Anchor(logo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(240, 240));
                logo.gameObject.AddComponent<RawImage>().texture = logoTex;
            }
            var t = UIKit.Label(layer, "Text", Game.Loc["boot.presents"], 30, FontRole.Document, TextAnchor.MiddleCenter, UIKit.Dim);
            UIKit.Anchor((RectTransform)t.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(1000, 60));
            t.fontStyle = FontStyle.Italic;
            // the fade overlay sits above; draw the splash on top of it
            layer.SetAsLastSibling();
            float total = 4.2f;
            for (float e = 0f; e < total; e += Time.unscaledDeltaTime)
            {
                float a = e < 1f ? e : (e > total - 1f ? total - e : 1f);
                group.alpha = Mathf.Clamp01(a);
                if (e > 0.6f && AnyPressed()) break;
                yield return null;
            }
            UnityEngine.Object.Destroy(layer.gameObject);
        }

        public static bool AnyPressed()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            var m = Mouse.current;
            return (k != null && k.anyKey.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame) ||
                   (g != null && (g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame || g.buttonEast.wasPressedThisFrame));
        }
    }

    public sealed class LanguageScreen : MenuScreen
    {
        readonly Action _done;
        public LanguageScreen(Action done) { _done = done; }
        public override bool PausesGame => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var bg = UIKit.Image(root, "Bg", Color.black);
            UIKit.Stretch((RectTransform)bg.transform);
            bg.transform.SetAsFirstSibling();
            Frame("Language · Dil · Sprache · Langue · Idioma · 言語", null, 160f);
            string current = Game.Loc.Language.Code;
            Selectable pick = null;
            foreach (var l in LocTable.Languages)
            {
                var lang = l;
                var b = ButtonText(l.NativeName + (l.Reviewed ? "" : "  <size=20><color=#5c5e60>(draft)</color></size>"), () => Choose(lang.Code), false, 32);
                if (l.Code == current) pick = b;
            }
            Finish();
            if (pick != null) FirstSelected = pick;
        }

        void Choose(string code)
        {
            var s = Game.Settings;
            s.Language = code;
            s.LanguageChosen = true;
            s.Save(Game.Saves.SettingsPath);
            Game.Loc.Load(code);
            Game.UI.Pop(this);
            _done?.Invoke();
        }

        public override void OnBack() => Choose(Game.Loc.Language.Code);
    }

    public sealed class WarningScreen : UIScreen
    {
        readonly Action _done;
        float _t;
        public WarningScreen(Action done) { _done = done; }
        public override bool PausesGame => false;
        public override bool ShowCursor => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var bg = UIKit.Image(root, "Bg", Color.black);
            UIKit.Stretch((RectTransform)bg.transform);
            var title = UIKit.Label(root, "Title", Game.Loc["boot.warning.title"], 52, FontRole.Title, TextAnchor.MiddleCenter, UIKit.Accent);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(1400, 80));
            var body = UIKit.Label(root, "Body", Game.Loc["boot.warning.body"], 30, FontRole.Document, TextAnchor.UpperCenter, UIKit.Ink);
            UIKit.Anchor((RectTransform)body.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(1300, 480));
            var any = UIKit.Label(root, "Any", Game.Loc["boot.press_any"], 24, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Faint);
            UIKit.Anchor((RectTransform)any.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1000, 40));
        }

        public override void Tick()
        {
            _t += Time.unscaledDeltaTime;
            if (_t > 1.5f && BootSequence.AnyPressed()) Close();
        }

        public override void OnBack() { if (_t > 1.5f) Close(); }

        void Close()
        {
            Game.Settings.WarningSeen = true;
            Game.UI.Pop(this);
            _done?.Invoke();
        }
    }

    /// <summary>
    /// Brightness: three marks drawn as the game's darkest tones would appear at the chosen exposure.
    /// The left one should be barely visible.
    /// </summary>
    public sealed class BrightnessScreen : UIScreen
    {
        readonly bool _first;
        readonly Action _done;
        Image[] _marks;
        Text _value;
        bool _closed;
        static readonly float[] Levels = { 0.006f, 0.018f, 0.05f };

        public BrightnessScreen(bool first, Action done = null)
        {
            _first = first;
            _done = done;
        }

        public override bool PausesGame => !_first;

        public override void Build(RectTransform root)
        {
            Root = root;
            var bg = UIKit.Image(root, "Bg", Color.black);
            UIKit.Stretch((RectTransform)bg.transform);
            var title = UIKit.Label(root, "Title", Game.Loc["boot.bright.title"], 50, FontRole.Title, TextAnchor.MiddleCenter);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(1400, 80));
            var body = UIKit.Label(root, "Body", Game.Loc["boot.bright.body"], 28, FontRole.Ui, TextAnchor.UpperCenter, UIKit.Dim);
            UIKit.Anchor((RectTransform)body.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -220), new Vector2(1300, 120));
            _marks = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                _marks[i] = UIKit.Image(root, "Mark" + i, Color.black, UIKit.Dot);
                UIKit.Anchor((RectTransform)_marks[i].transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 320f, 0), new Vector2(200, 200));
            }
            _value = UIKit.Label(root, "Value", "", 36, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Ink);
            UIKit.Anchor((RectTransform)_value.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(600, 60));
            var hint = UIKit.Label(root, "Hint", "<  >   " + Game.Loc["boot.bright.adjust"] + "      " + (Game.Input.UsingGamepad ? "[A]" : "[Enter]") + "  " + Game.Loc["ui.confirm"], 24, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Faint);
            UIKit.Anchor((RectTransform)hint.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1200, 40));
            // mouse and touch: a confirm button above the hint, arrows beside the value.
            // Confirm comes first: with a pad, the UI root selects the first button and A submits it.
            UIKit.Anchor(UIKit.TextButton(root, "Confirm", Game.Loc["ui.confirm"], 32, Close), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(360, 72));
            UIKit.Anchor(UIKit.TextButton(root, "Darker", "<", 44, () => Step(-1)), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-240, -230), new Vector2(100, 84));
            UIKit.Anchor(UIKit.TextButton(root, "Brighter", ">", 44, () => Step(1)), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(240, -230), new Vector2(100, 84));
            Refresh();
        }

        void Step(int d)
        {
            var s = Game.Settings;
            s.Brightness = Mathf.Clamp(Mathf.Round((s.Brightness + d * 0.1f) * 10f) / 10f, -2f, 2f);
            s.NotifyChanged();
            UIKit.PlayMove();
            Refresh();
        }

        void Refresh()
        {
            float ev = Game.Settings.Brightness;
            float k = Mathf.Pow(2f, ev);
            for (int i = 0; i < 3; i++)
            {
                float lin = Mathf.Clamp01(Levels[i] * k);
                float g = Mathf.LinearToGammaSpace(lin);
                _marks[i].color = new Color(g, g, g, 1f);
            }
            _value.text = (ev >= 0 ? "+" : "") + ev.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " EV";
        }

        public override void Tick()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            int d = 0;
            if ((k != null && (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame)) || (g != null && (g.dpad.left.wasPressedThisFrame || g.leftStick.left.wasPressedThisFrame))) d = -1;
            if ((k != null && (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame)) || (g != null && (g.dpad.right.wasPressedThisFrame || g.leftStick.right.wasPressedThisFrame))) d = 1;
            if (d != 0) Step(d);
            if ((k != null && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame)) || (g != null && g.buttonSouth.wasPressedThisFrame)) Close();
        }

        public override void OnBack() => Close();

        void Close()
        {
            if (_closed) return;
            _closed = true;
            Game.Settings.Calibrated = true;
            Game.Settings.Save(Game.Saves.SettingsPath);
            UIKit.PlayConfirm();
            Game.UI.Pop(this);
            _done?.Invoke();
        }
    }
}
