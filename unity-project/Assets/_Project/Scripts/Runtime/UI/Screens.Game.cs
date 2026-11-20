using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    // ------------------------------------------------------------------ pause
    public sealed class PauseMenu : MenuScreen
    {
        public override void Build(RectTransform root)
        {
            Root = root;
            var rt = Game.Act;
            string chapter = rt != null ? ChapterSelect.RomanAct(rt.Act.Id) + "  ·  " + Game.Loc[rt.Act.NameKey] : "";
            Frame(Game.Loc["ui.paused"], chapter);
            Button("ui.resume", () => Game.UI.Pop(this));
            Button("ui.journal", () => Game.UI.Push(new JournalScreen(0)));
            Button("ui.settings", () => Game.UI.Push(new SettingsScreen()));
            Button("ui.reload_checkpoint", () => Game.UI.Push(new ConfirmScreen(Game.Loc["ui.reload_checkpoint"], Game.Loc["ui.reload_body"], () => Game.Flow.ReloadCheckpoint(), null)));
            Button("ui.main_menu", () => Game.UI.Push(new ConfirmScreen(Game.Loc["ui.main_menu"], Game.Loc["ui.menu_body"], () =>
            {
                Game.Act?.SaveCheckpointNow("quit");
                Game.Flow.ToMenu();
            }, null)));
            Button("ui.quit_desktop", () => Game.UI.Push(new ConfirmScreen(Game.Loc["ui.quit_desktop"], Game.Loc["ui.quit_body"], () =>
            {
                Game.Act?.SaveCheckpointNow("quit");
                Application.Quit();
            }, null)));
            // where you are, what you are doing
            if (rt != null)
            {
                var g = rt.Session.CurrentGoal;
                var info = UIKit.Label(root, "Goal", (g != null ? Game.Loc["ui.objective"] + "\n<color=#dcd4c7>" + Game.Loc[g.TextKey] + "</color>" : "") +
                    "\n\n" + Game.Loc.F("ui.pause_stats", rt.State.DrawingCount, Mathf.RoundToInt(rt.Session.Lantern.Oil)), 26, FontRole.Ui, TextAnchor.UpperLeft, UIKit.Dim);
                UIKit.Anchor((RectTransform)info.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-120, -260), new Vector2(620, 400));
            }
            Finish();
        }
    }

    // ------------------------------------------------------------------ journal
    /// <summary>Objective, notes read, drawings found, and a reminder of the controls.</summary>
    public sealed class JournalScreen : UIScreen
    {
        int _tab;
        static readonly string[] Tabs = { "ui.journal.objective", "ui.journal.notes", "ui.journal.drawings", "ui.journal.controls" };
        RectTransform _page;
        readonly List<Text> _tabLabels = new List<Text>();

        public JournalScreen(int tab) { _tab = tab; }

        public override void Build(RectTransform root)
        {
            Root = root;
            _tabLabels.Clear();
            var bg = UIKit.Image(root, "Bg", new Color(0.02f, 0.02f, 0.025f, 0.92f));
            UIKit.Stretch((RectTransform)bg.transform);
            var paper = UIKit.Image(root, "Paper", new Color(0.82f, 0.77f, 0.68f, 1f));
            UIKit.Anchor((RectTransform)paper.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1500, 860));
            var tex = Resources.Load<Texture2D>("Undertow/Textures/parchment_a");
            if (tex != null) paper.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            var row = UIKit.Rect(root, "Tabs");
            UIKit.Anchor(row, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(1500, 60));
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 50;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = false;
            h.childForceExpandWidth = false;
            for (int i = 0; i < Tabs.Length; i++)
            {
                int k = i;
                var t = UIKit.Label(row, "Tab", Game.Loc[Tabs[i]], 30, FontRole.Title, TextAnchor.MiddleCenter);
                ((RectTransform)t.transform).sizeDelta = new Vector2(t.preferredWidth + 20, 60);
                t.raycastTarget = true;
                var b = t.gameObject.AddComponent<Button>();
                b.transition = Selectable.Transition.None;
                b.navigation = new Navigation { mode = Navigation.Mode.None };
                b.onClick.AddListener(() => { _tab = k; UIKit.PlayMove(); Page(); });
                _tabLabels.Add(t);
            }
            _page = UIKit.Rect(paper.transform, "Page");
            UIKit.Stretch(_page, 90, 70, 90, 70);
            MenuScreen.Hints(root, Game.Input.UsingGamepad ? "[LB] [RB]" : "[Q] [E]");
            Page();
        }

        void Page()
        {
            foreach (Transform c in _page) UnityEngine.Object.Destroy(c.gameObject);
            for (int i = 0; i < _tabLabels.Count; i++) _tabLabels[i].color = i == _tab ? UIKit.Accent : UIKit.Dim;
            var ink = new Color(0.16f, 0.13f, 0.1f);
            var rt = Game.Act;
            var items = new List<Selectable>();
            switch (_tab)
            {
                case 0:
                {
                    string text = "";
                    if (rt != null)
                    {
                        text += "<size=44>" + ChapterSelect.RomanAct(rt.Act.Id) + "  ·  " + Game.Loc[rt.Act.NameKey] + "</size>\n<i>" + Game.Loc[rt.Act.LineKey] + "</i>\n\n";
                        var g = rt.Session.CurrentGoal;
                        if (g != null) text += Game.Loc["ui.objective"] + ":  " + Game.Loc[g.TextKey] + "\n\n";
                        // what Adrian has noticed so far (one line per goal reached)
                        foreach (var goal in rt.Act.Goals)
                            if (rt.State.Has(goal.Flag) && Game.Loc.Has("journal." + goal.Flag)) text += "—  " + Game.Loc["journal." + goal.Flag] + "\n";
                    }
                    var t = UIKit.Label(_page, "Text", text, 30, FontRole.Hand, TextAnchor.UpperLeft, ink);
                    UIKit.Stretch((RectTransform)t.transform);
                    break;
                }
                case 1:
                {
                    var content = UIKit.ScrollList(_page, out var scroll, 4f);
                    var read = rt != null ? rt.State.NotesRead : new List<string>();
                    var all = ContentDb.Acts.SelectMany(a => a.Items).Where(i => i.HasText && i.Kind != ItemKind.Drawing).ToList();
                    foreach (var it in all)
                    {
                        if (!read.Contains(it.Id)) continue;
                        var item = it;
                        var b = MenuButton.Create<MenuButton>(content, Game.Loc[it.TitleKey], 30, () => Game.UI.Push(new NoteReader(item, 0)), 54f);
                        b.Label.font = UIKit.F(FontRole.Hand);
                        b.Label.color = ink;
                        items.Add(b);
                    }
                    if (items.Count == 0)
                    {
                        var none = UIKit.Label(content, "None", Game.Loc["ui.journal.none"], 30, FontRole.Hand, TextAnchor.UpperLeft, ink);
                        UIKit.Size(none, 60);
                    }
                    var count = UIKit.Label(_page, "Count", Game.Loc.F("ui.journal.count", items.Count, all.Count), 24, FontRole.Ui, TextAnchor.LowerRight, new Color(0.3f, 0.26f, 0.2f));
                    UIKit.Anchor((RectTransform)count.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(0, -50), new Vector2(500, 40));
                    UIKit.ChainVertical(items);
                    break;
                }
                case 2:
                {
                    var grid = UIKit.Rect(_page, "Grid");
                    UIKit.Stretch(grid);
                    var g = grid.gameObject.AddComponent<GridLayoutGroup>();
                    g.cellSize = new Vector2(400, 330);
                    g.spacing = new Vector2(30, 24);
                    g.childAlignment = TextAnchor.MiddleCenter;
                    g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    g.constraintCount = 3;
                    var found = rt != null ? rt.State.Drawings : new List<int>();
                    for (int i = 1; i <= 6; i++)
                    {
                        var cell = UIKit.Rect(grid, "D" + i);
                        var frame = cell.gameObject.AddComponent<Image>();
                        frame.color = new Color(0, 0, 0, 0.08f);
                        var pic = UIKit.Rect(cell, "Pic");
                        UIKit.Stretch(pic, 12, 12, 12, 46);
                        if (found.Contains(i))
                        {
                            var raw = pic.gameObject.AddComponent<RawImage>();
                            raw.texture = Resources.Load<Texture2D>("Undertow/Textures/drawing" + i + "_a");
                            int k = i;
                            var b = cell.gameObject.AddComponent<Button>();
                            b.onClick.AddListener(() =>
                            {
                                var it = ContentDb.Acts.SelectMany(a => a.Items).FirstOrDefault(x => x.Kind == ItemKind.Drawing && x.Drawing == k);
                                if (it != null) Game.UI.Push(new NoteReader(it, 0));
                            });
                            items.Add(b);
                        }
                        else
                        {
                            var q = UIKit.Label(pic, "Q", "?", 100, FontRole.Title, TextAnchor.MiddleCenter, new Color(0.3f, 0.26f, 0.2f, 0.5f));
                            UIKit.Stretch((RectTransform)q.transform);
                        }
                        var cap = UIKit.Label(cell, "Cap", found.Contains(i) ? Game.Loc["drawing." + i] : "", 24, FontRole.Hand, TextAnchor.MiddleCenter, ink);
                        UIKit.Anchor((RectTransform)cap.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(380, 40));
                    }
                    break;
                }
                case 3:
                {
                    var inp = Game.Input;
                    string Line(string k, InputAction a) => Game.Loc[k] + "  —  " + inp.Tag(a);
                    string text = string.Join("\n", new[]
                    {
                        Game.Loc["ui.input.move"] + "  —  " + (inp.UsingGamepad ? "[L]" : "[W A S D]"),
                        Line("ui.input.interact", inp.Interact), Line("ui.input.lantern", inp.Lantern), Line("ui.input.listen", inp.Listen),
                        Line("ui.input.sprint", inp.Sprint), Line("ui.input.crouch", inp.Crouch), Line("ui.input.journal", inp.Journal),
                        Line("ui.input.photo", inp.Photo), Line("ui.input.hidehud", inp.HideHud), Line("ui.input.pause", inp.Pause),
                    }) + "\n\n<i>" + Game.Loc["ui.journal.tip"] + "</i>";
                    var t = UIKit.Label(_page, "Text", text, 30, FontRole.Hand, TextAnchor.UpperLeft, ink);
                    UIKit.Stretch((RectTransform)t.transform);
                    break;
                }
            }
            FirstSelected = items.Count > 0 ? items[0] : null;
            Select(FirstSelected);
        }

        public override void Tick()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            int d = 0;
            if ((k != null && k.qKey.wasPressedThisFrame) || (g != null && g.leftShoulder.wasPressedThisFrame)) d = -1;
            if ((k != null && k.eKey.wasPressedThisFrame) || (g != null && g.rightShoulder.wasPressedThisFrame)) d = 1;
            if ((k != null && (k.tabKey.wasPressedThisFrame || k.jKey.wasPressedThisFrame)) || (g != null && g.selectButton.wasPressedThisFrame)) { OnBack(); return; }
            if (d == 0) return;
            _tab = (_tab + d + Tabs.Length) % Tabs.Length;
            UIKit.PlayMove();
            Page();
        }
    }

    // ------------------------------------------------------------------ note reader
    /// <summary>
    /// A note, log, letter, carving, document, board or tape transcript, in the hand it was written in.
    /// Drawings show the picture itself. Narration plays if there is a recording.
    /// </summary>
    public sealed class NoteReader : UIScreen
    {
        readonly ItemDef _item;
        readonly int _drawingCount;
        AudioSource _voice;
        float _opened;

        public NoteReader(ItemDef item, int drawingCount)
        {
            _item = item;
            _drawingCount = drawingCount;
        }

        public override bool ShowCursor => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            _opened = Time.unscaledTime;
            var it = _item;
            var bg = UIKit.Image(root, "Bg", new Color(0, 0, 0, 0.78f));
            UIKit.Stretch((RectTransform)bg.transform);
            string style = it.Kind == ItemKind.Drawing ? "drawing" : it.NoteStyle;
            FontRole role;
            Color paperC, inkC;
            string paperTex;
            Vector2 size;
            switch (style)
            {
                case "child": role = FontRole.Hand; paperC = new Color(0.93f, 0.92f, 0.86f); inkC = new Color(0.16f, 0.22f, 0.42f); paperTex = "paper_a"; size = new Vector2(820, 900); break;
                case "letter": role = FontRole.Hand; paperC = new Color(0.9f, 0.86f, 0.76f); inkC = new Color(0.13f, 0.11f, 0.1f); paperTex = "paper_a"; size = new Vector2(900, 920); break;
                case "carving": role = FontRole.Title; paperC = new Color(0.26f, 0.21f, 0.16f); inkC = new Color(0.86f, 0.8f, 0.7f); paperTex = null; size = new Vector2(1100, 600); break;
                case "board": role = FontRole.Typewriter; paperC = new Color(0.78f, 0.72f, 0.6f); inkC = new Color(0.12f, 0.1f, 0.08f); paperTex = "parchment_a"; size = new Vector2(1000, 820); break;
                case "tape": role = FontRole.Ui; paperC = new Color(0.08f, 0.09f, 0.1f); inkC = new Color(0.8f, 0.84f, 0.86f); paperTex = null; size = new Vector2(1200, 640); break;
                case "drawing": role = FontRole.Hand; paperC = new Color(0.95f, 0.94f, 0.9f); inkC = new Color(0.2f, 0.2f, 0.25f); paperTex = null; size = new Vector2(1000, 940); break;
                case "document": role = FontRole.Document; paperC = new Color(0.86f, 0.8f, 0.66f); inkC = new Color(0.14f, 0.11f, 0.08f); paperTex = "parchment_a"; size = new Vector2(1000, 940); break;
                default: role = FontRole.Document; paperC = new Color(0.84f, 0.78f, 0.66f); inkC = new Color(0.15f, 0.12f, 0.09f); paperTex = "parchment_a"; size = new Vector2(980, 940); break;
            }
            var paper = UIKit.Image(root, "Paper", paperC);
            var prt = (RectTransform)paper.transform;
            UIKit.Anchor(prt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), size);
            prt.localRotation = Quaternion.Euler(0, 0, style == "child" ? 1.6f : (style == "letter" ? -0.8f : 0f));
            if (paperTex != null)
            {
                var tex = Resources.Load<Texture2D>("Undertow/Textures/" + paperTex);
                if (tex != null) paper.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            string title = Game.Loc[it.TitleKey];
            string body = Game.Loc[it.BodyKey];
            if (style == "drawing")
            {
                var pic = UIKit.Rect(prt, "Picture");
                UIKit.Anchor(pic, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(860, 645));
                var raw = pic.gameObject.AddComponent<RawImage>();
                raw.texture = Resources.Load<Texture2D>("Undertow/Textures/drawing" + it.Drawing + "_a");
                var cap = UIKit.Label(prt, "Caption", Game.Loc["drawing." + it.Drawing] + (Game.Loc.Has(it.BodyKey) ? "\n<size=26>" + body + "</size>" : ""), 34, FontRole.Hand, TextAnchor.UpperCenter, inkC);
                UIKit.Anchor((RectTransform)cap.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(880, 200));
                if (_drawingCount > 0)
                {
                    var n = UIKit.Label(root, "Count", Game.Loc.F("msg.drawing_found", _drawingCount, 6), 26, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Accent);
                    UIKit.Anchor((RectTransform)n.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(800, 40));
                }
            }
            else
            {
                var t = UIKit.Label(prt, "Title", style == "carving" ? Game.Loc.Upper(title) : title, 40, style == "child" ? FontRole.Hand : (style == "board" || (style == "document" && Game.Act != null && Game.Act.Act.Id == 3) ? FontRole.Typewriter : (style == "tape" ? FontRole.Ui : FontRole.Title)), TextAnchor.UpperCenter, inkC);
                UIKit.Anchor((RectTransform)t.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(size.x - 120, 70));
                var b = UIKit.Label(prt, "Body", body, style == "child" ? 38 : (style == "carving" ? 40 : 31), role, style == "carving" ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft, inkC);
                UIKit.Stretch((RectTransform)b.transform, 70, 140, 70, 90);
                b.lineSpacing = style == "child" ? 1.15f : 1.1f;
                if (style == "tape") b.fontStyle = FontStyle.Italic;
                // 1724 deeds and ledgers in the period face
                if (style == "document" && it.Act == 3) b.font = UIKit.F(FontRole.Typewriter);
                if (it.Speaker != null && Game.Loc.Has("speaker." + it.Speaker))
                {
                    var sig = UIKit.Label(prt, "Sign", "— " + Game.Loc["speaker." + it.Speaker], 28, style == "letter" || style == "child" ? FontRole.Hand : FontRole.Document, TextAnchor.LowerRight, inkC);
                    UIKit.Anchor((RectTransform)sig.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-70, 36), new Vector2(600, 46));
                }
            }
            var hint = UIKit.Label(root, "Hint", Game.Input.Tag(Game.Input.Interact) + "  " + Game.Loc["ui.close"], 22, FontRole.Ui, TextAnchor.LowerRight, UIKit.Faint);
            UIKit.Anchor((RectTransform)hint.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-60, 40), new Vector2(600, 40));
            // narration
            string clip = it.Voice ?? ("vo_note_" + it.Id);
            if (Game.Audio.Clip(clip) != null)
            {
                _voice = Game.Audio.Play2D(clip, Bus.Voice, 1f);
                if (_voice != null) _voice.ignoreListenerPause = true;
            }
            Game.Audio.Duck = 0.5f;
        }

        public override void Tick()
        {
            if (Time.unscaledTime - _opened < 0.35f) return;
            var k = Keyboard.current;
            var g = Gamepad.current;
            var m = Mouse.current;
            if ((k != null && (k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)) ||
                (g != null && g.buttonSouth.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame))
                OnBack();
        }

        public override void OnHide()
        {
            if (_voice != null) Game.Audio.Stop(_voice, 0.6f);
            Game.Audio.Duck = Game.Act != null && Game.Act.Listening ? 0.55f : 0f;
        }
    }

    // ------------------------------------------------------------------ the answer (Act III)
    /// <summary>One prompt, a few seconds, no menu: answer, or don't.</summary>
    public sealed class AnswerPrompt : UIScreen
    {
        readonly float _seconds;
        readonly Action<bool> _done;
        float _t;
        bool _closed;
        Image _ring;
        Text _label;
        CanvasGroup _group;

        public AnswerPrompt(float seconds, Action<bool> done)
        {
            _seconds = seconds;
            _done = done;
        }

        public override bool PausesGame => false;
        public override bool ShowCursor => false;
        public override bool DimBackground => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            _group = root.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _ring = UIKit.Image(root, "Ring", UIKit.Accent, UIKit.Ring);
            UIKit.Anchor((RectTransform)_ring.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(190, 190));
            _label = UIKit.Label(root, "Answer", Game.Input.Tag(Game.Input.Interact) + "  " + Game.Loc["ui.answer"], 44, FontRole.Title, TextAnchor.MiddleCenter, UIKit.Ink);
            UIKit.Anchor((RectTransform)_label.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(900, 80));
        }

        public override void Tick()
        {
            if (_closed) return;
            _t += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Clamp01(_t / 0.8f);
            float k = Mathf.Clamp01(_t / _seconds);
            float pulse = 1f + Mathf.Sin(_t * 5f) * 0.04f;
            ((RectTransform)_ring.transform).localScale = Vector3.one * Mathf.Lerp(1.6f, 0.75f, k) * pulse;
            _ring.color = new Color(UIKit.Accent.r, UIKit.Accent.g, UIKit.Accent.b, Mathf.Lerp(0.9f, 0.25f, k));
            var kb = Keyboard.current;
            var g = Gamepad.current;
            var m = Mouse.current;
            bool pressed = (Game.Input.Interact.enabled && Game.Input.Interact.WasPressedThisFrame()) ||
                           (kb != null && (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) ||
                           (g != null && g.buttonSouth.wasPressedThisFrame) || (m != null && m.leftButton.wasPressedThisFrame);
            if (_t > 0.6f && pressed) Close(true);
            else if (_t >= _seconds) Close(false);
        }

        void Close(bool answered)
        {
            _closed = true;
            if (answered) Game.Audio.Play2D("ui_answer", Bus.Ui, 0.9f);
            Game.UI.Pop(this);
            _done?.Invoke(answered);
        }

        public override void OnBack() { }   // there is no way out of this moment but through it
    }

    // ------------------------------------------------------------------ photo mode
    /// <summary>
    /// Frozen time, free camera a few metres around Adrian, FOV / roll / exposure / focus / filters,
    /// lantern and HUD hidden, screenshots saved to Pictures (or the save folder).
    /// </summary>
    public sealed class PhotoMode : UIScreen
    {
        Transform _cam;
        Vector3 _origin, _pos;
        float _yaw, _pitch, _roll, _fov, _ev, _focus = 3f;
        int _filter;
        bool _ui = true;
        Vector3 _savedPos;
        Quaternion _savedRot;
        float _savedFov;
        Text _info, _flash;
        float _flashT;
        GameObject _panel;
        int _shot;
        static readonly string[] Filters = { "ui.photo.f_none", "ui.photo.f_mono", "ui.photo.f_sepia", "ui.photo.f_cold" };

        public override bool ShowCursor => false;
        public override bool DimBackground => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var rt = Game.Act;
            var p = rt.Player;
            _cam = p.Cam.transform;
            _savedPos = _cam.localPosition;
            _savedRot = _cam.localRotation;
            _savedFov = p.Cam.fieldOfView;
            _origin = p.EyePos;
            _pos = _origin;
            var e = _cam.rotation.eulerAngles;
            _yaw = e.y;
            _pitch = e.x > 180f ? e.x - 360f : e.x;
            _fov = Game.Settings.Fov;
            rt.PhotoHide = 1f;
            p.Lantern.SetVisible(false);
            _panel = UIKit.Image(root, "Panel", new Color(0, 0, 0, 0.55f)).gameObject;
            UIKit.Anchor((RectTransform)_panel.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 40), new Vector2(560, 330));
            ((RectTransform)_panel.transform).pivot = Vector2.zero;
            _info = UIKit.Label(_panel.transform, "Info", "", 22, FontRole.Ui, TextAnchor.UpperLeft, UIKit.Ink);
            UIKit.Stretch((RectTransform)_info.transform, 20, 16, 20, 16);
            _flash = UIKit.Label(root, "Saved", "", 28, FontRole.Ui, TextAnchor.UpperCenter, UIKit.Accent);
            UIKit.Anchor((RectTransform)_flash.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1400, 50));
        }

        public override void Tick()
        {
            var rt = Game.Act;
            if (rt == null) { Game.UI.Pop(this); return; }
            float dt = Time.unscaledDeltaTime;
            var k = Keyboard.current;
            var g = Gamepad.current;
            var m = Mouse.current;
            // look
            Vector2 look = Vector2.zero;
            if (m != null) look += m.delta.ReadValue() * 0.08f * Game.Settings.MouseSensitivity;
            if (g != null) look += g.rightStick.ReadValue() * 110f * dt * Game.Settings.PadSensitivity;
            _yaw += look.x;
            _pitch = Mathf.Clamp(_pitch - look.y, -88f, 88f);
            // move, within reach of where Adrian stands
            Vector2 mv = Vector2.zero;
            if (k != null) mv += new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
            if (g != null) mv += g.leftStick.ReadValue();
            float up = 0f;
            if (k != null) up += (k.spaceKey.isPressed ? 1 : 0) - (k.leftCtrlKey.isPressed ? 1 : 0);
            if (g != null) up += (g.rightShoulder.isPressed ? 1 : 0) - (g.leftShoulder.isPressed ? 1 : 0);
            var rot = Quaternion.Euler(_pitch, _yaw, 0);
            var delta = (rot * new Vector3(mv.x, 0, mv.y) + Vector3.up * up) * 2.2f * dt;
            var next = _pos + delta;
            var off = next - _origin;
            if (off.magnitude > 3.5f) next = _origin + off.normalized * 3.5f;
            if (!Physics.SphereCast(_pos, 0.12f, (next - _pos).normalized, out _, (next - _pos).magnitude, PlayerRig.LosMask, QueryTriggerInteraction.Ignore)) _pos = next;
            // roll, fov, exposure, focus, filter
            if (k != null)
            {
                if (k.qKey.isPressed) _roll += 30f * dt;
                if (k.eKey.isPressed) _roll -= 30f * dt;
                if (k.rKey.wasPressedThisFrame) _roll = 0f;
                if (k.zKey.isPressed) _fov -= 25f * dt;
                if (k.xKey.isPressed) _fov += 25f * dt;
                if (k.cKey.isPressed) _ev -= 1f * dt;
                if (k.vKey.isPressed) _ev += 1f * dt;
                if (k.bKey.isPressed) _focus = Mathf.Max(0.3f, _focus - 3f * dt);
                if (k.nKey.isPressed) _focus += 3f * dt;
                if (k.fKey.wasPressedThisFrame) _filter = (_filter + 1) % Filters.Length;
                if (k.hKey.wasPressedThisFrame) _ui = !_ui;
                if (k.enterKey.wasPressedThisFrame || (m != null && m.leftButton.wasPressedThisFrame)) _shot = 2;
            }
            if (m != null) _fov -= m.scroll.ReadValue().y * 0.02f;
            if (g != null)
            {
                // triggers zoom; with X held they pull focus instead. Stick clicks set the exposure.
                float trig = g.rightTrigger.ReadValue() - g.leftTrigger.ReadValue();
                if (g.buttonWest.isPressed) _focus = Mathf.Max(0.3f, _focus + trig * 3f * dt);
                else _fov -= trig * 30f * dt;
                if (g.leftStickButton.isPressed) _ev -= 1f * dt;
                if (g.rightStickButton.isPressed) _ev += 1f * dt;
                if (g.dpad.left.isPressed) _roll += 30f * dt;
                if (g.dpad.right.isPressed) _roll -= 30f * dt;
                if (g.dpad.up.wasPressedThisFrame) _filter = (_filter + 1) % Filters.Length;
                if (g.buttonNorth.wasPressedThisFrame) _ui = !_ui;
                if (g.buttonSouth.wasPressedThisFrame) _shot = 2;
            }
            _fov = Mathf.Clamp(_fov, 20f, 120f);
            _ev = Mathf.Clamp(_ev, -3f, 3f);
            _cam.position = _pos;
            _cam.rotation = Quaternion.Euler(_pitch, _yaw, _roll);
            rt.Player.Cam.fieldOfView = rt.Player.VerticalFov(_fov);
            rt.World.Atmos.SetBrightness(Game.Settings.Brightness + _ev);
            rt.World.Atmos.SetDepthOfField(true, _focus, true);
            rt.World.Atmos.SetPhotoFilter(_filter);
            // a shot hides the panel for two frames, then captures the clean picture
            if (_shot > 0)
            {
                _panel.SetActive(false);
                _flash.enabled = false;
                if (--_shot == 0) Capture();
            }
            else
            {
                _panel.SetActive(_ui);
                _flash.enabled = true;
            }
            _info.text = Game.Loc["ui.photo.title"] + "\n" +
                Game.Loc["ui.photo.fov"] + "  " + Mathf.RoundToInt(_fov) + "°     " + Game.Loc["ui.photo.roll"] + "  " + Mathf.RoundToInt(_roll) + "°\n" +
                Game.Loc["ui.photo.exposure"] + "  " + _ev.ToString("+0.0;-0.0;0.0", System.Globalization.CultureInfo.InvariantCulture) + "     " + Game.Loc["ui.photo.focus"] + "  " + _focus.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " m\n" +
                Game.Loc["ui.photo.filter"] + "  " + Game.Loc[Filters[_filter]] + "\n\n" +
                "<size=18><color=#8e8e8c>" + (Game.Input.UsingGamepad ? Game.Loc["ui.photo.help_pad"] : Game.Loc["ui.photo.help_kb"]) + "</color></size>";
            if (_flashT > 0f)
            {
                _flashT -= dt;
                _flash.color = new Color(UIKit.Accent.r, UIKit.Accent.g, UIKit.Accent.b, Mathf.Clamp01(_flashT));
            }
        }

        void Capture()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Undertow");
                if (string.IsNullOrEmpty(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures))) dir = Path.Combine(Application.persistentDataPath, "Photos");
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, "Undertow_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", System.Globalization.CultureInfo.InvariantCulture) + ".png");
                ScreenCapture.CaptureScreenshot(file, 1);
                Game.Audio.Play2D("ui_shutter", Bus.Ui, 0.8f);
                // streamer mode: the folder path would show the Windows user name on stream
                _flash.text = Game.Loc["ui.photo.saved"] + "  " + (Game.Settings.StreamerMode ? Path.GetFileName(file) : file);
                _flashT = 3f;
                Game.Achievements.Unlock("photographer");
            }
            catch (Exception e)
            {
                Game.Warn("photo failed: " + e.Message);
            }
        }

        public override void OnHide()
        {
            var rt = Game.Act;
            if (rt == null) return;
            _cam.localPosition = _savedPos;
            _cam.localRotation = _savedRot;
            rt.Player.Cam.fieldOfView = _savedFov;
            rt.World.Atmos.SetBrightness(Game.Settings.Brightness);
            rt.World.Atmos.SetDepthOfField(false, 0f);
            rt.World.Atmos.SetPhotoFilter(0);
            rt.PhotoHide = 0f;
            rt.Player.Lantern.SetVisible(true);
        }
    }
}
