using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>Base for list menus: a title, a column of entries, input hints at the bottom.</summary>
    public abstract class MenuScreen : UIScreen
    {
        protected RectTransform Column;
        protected Text TitleText, SubText;
        protected readonly List<Selectable> Items = new List<Selectable>();
        protected virtual float ColumnX => 150f;
        protected virtual float ColumnWidth => 760f;
        protected virtual bool Backdrop => true;

        protected void Frame(string title, string sub = null, float top = 170f)
        {
            Items.Clear();
            if (Backdrop)
            {
                var shade = UIKit.Image(Root, "Shade", new Color(0, 0, 0, 0.62f), UIKit.White);
                UIKit.Anchor((RectTransform)shade.transform, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(ColumnX + ColumnWidth + 120f, 1080f));
                var fade = UIKit.Image(Root, "ShadeEdge", new Color(0, 0, 0, 0.62f), UIKit.Soft);
                UIKit.Anchor((RectTransform)fade.transform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ColumnX + ColumnWidth + 120f, 0), new Vector2(360f, 1800f));
            }
            TitleText = UIKit.Label(Root, "Title", title, 64, FontRole.Title, TextAnchor.LowerLeft);
            UIKit.Anchor((RectTransform)TitleText.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(ColumnX, -top + 90f), new Vector2(ColumnWidth + 200f, 90f));
            if (sub != null)
            {
                SubText = UIKit.Label(Root, "Sub", sub, 24, FontRole.Document, TextAnchor.UpperLeft, UIKit.Dim);
                UIKit.Anchor((RectTransform)SubText.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(ColumnX + 4f, -top - 6f), new Vector2(ColumnWidth, 60f));
                SubText.fontStyle = FontStyle.Italic;
            }
            var line = UIKit.Image(Root, "Rule", UIKit.Line);
            UIKit.Anchor((RectTransform)line.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(ColumnX, -top - (sub != null ? 64f : 20f)), new Vector2(ColumnWidth, 1f));
            Column = UIKit.Rect(Root, "Column");
            UIKit.Anchor(Column, new Vector2(0, 1), new Vector2(0, 1), new Vector2(ColumnX, -top - (sub != null ? 84f : 40f)), new Vector2(ColumnWidth, 760f));
            UIKit.VList(Column, 4f);
            Hints(Root, null, OnBack);
        }

        /// <summary>Key hints at the bottom right; with <paramref name="back"/>, also a clickable Back for the mouse and touch.</summary>
        public static void Hints(RectTransform root, string extra = null, Action back = null)
        {
            bool pad = Game.Input.UsingGamepad;
            string ok = pad ? "[A]" : "[Enter]";
            string esc = pad ? "[B]" : "[Esc]";
            var t = UIKit.Label(root, "Hints", (extra != null ? extra + "      " : "") + ok + "  " + Game.Loc["ui.select"] + "      " + esc + "  " + Game.Loc["ui.back"], 22, FontRole.Ui, TextAnchor.LowerRight, UIKit.Faint);
            UIKit.Anchor((RectTransform)t.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-60, 40), new Vector2(1200, 40));
            if (back != null)
                UIKit.Anchor(UIKit.TextButton(root, "Back", "<  " + Game.Loc["ui.back"], 26, back), new Vector2(0, 0), new Vector2(0, 0), new Vector2(60, 28), new Vector2(240, 58));
        }

        protected MenuButton Button(string key, Action click, bool disabled = false, int size = 38)
        {
            var b = MenuButton.Create<MenuButton>(Column, Game.Loc[key], size, click, size + 22f);
            b.Disabled = disabled;
            b.Refresh();
            Items.Add(b);
            return b;
        }

        protected MenuButton ButtonText(string text, Action click, bool disabled = false, int size = 34)
        {
            var b = MenuButton.Create<MenuButton>(Column, text, size, click, size + 22f);
            b.Disabled = disabled;
            b.Refresh();
            Items.Add(b);
            return b;
        }

        protected void Finish()
        {
            UIKit.ChainVertical(Items);
            foreach (var s in Items)
            {
                if (s is MenuButton mb && mb.Disabled) continue;
                FirstSelected = s;
                break;
            }
        }
    }

    // ------------------------------------------------------------------ main menu
    public sealed class MainMenu : MenuScreen
    {
        public override bool PausesGame => false;
        public override bool DimBackground => false;
        protected override bool Backdrop => true;

        public override void Build(RectTransform root)
        {
            Root = root;
            Items.Clear();
            var shade = UIKit.Image(root, "Shade", new Color(0, 0, 0, 0.5f), UIKit.White);
            UIKit.Anchor((RectTransform)shade.transform, new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, new Vector2(980f, 1080f));
            var edge = UIKit.Image(root, "ShadeEdge", new Color(0, 0, 0, 0.5f), UIKit.Soft);
            UIKit.Anchor((RectTransform)edge.transform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(980f, 0), new Vector2(420f, 1900f));
            var title = UIKit.Label(root, "Logo", "UNDERTOW", 132, FontRole.Title, TextAnchor.LowerLeft);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(140, -120), new Vector2(1200, 170));
            var sub = UIKit.Label(root, "Sub", Game.Loc["ui.subtitle"], 40, FontRole.Document, TextAnchor.UpperLeft, UIKit.Accent);
            UIKit.Anchor((RectTransform)sub.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -295), new Vector2(1000, 60));
            sub.fontStyle = FontStyle.Italic;
            Column = UIKit.Rect(root, "Column");
            UIKit.Anchor(Column, new Vector2(0, 1), new Vector2(0, 1), new Vector2(150, -400), new Vector2(620, 600));
            UIKit.VList(Column, 2f);
            bool any = Game.Saves.HasAnySave();
            if (any) Button("ui.continue", () => Game.Flow.Continue());
            Button("ui.new_game", () => Game.UI.Push(new SlotSelect(SlotSelect.Mode.New)));
            Button("ui.load_game", () => Game.UI.Push(new SlotSelect(SlotSelect.Mode.Load)), !any);
            Button("ui.chapters", () => Game.UI.Push(new ChapterSelect()));
            Button("ui.extras", () => Game.UI.Push(new ExtrasScreen()));
            Button("ui.settings", () => Game.UI.Push(new SettingsScreen()));
            Button("ui.credits", () => Game.UI.Push(new CreditsScreen(false)));
            Button("ui.quit", () => Game.UI.Push(new ConfirmScreen(Game.Loc["ui.quit"], Game.Loc["ui.quit_confirm"], () => Application.Quit(), null)));
            var foot = UIKit.Label(root, "Foot", "© LenstedReal", 20, FontRole.Ui, TextAnchor.LowerLeft, UIKit.Faint);
            UIKit.Anchor((RectTransform)foot.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(150, 40), new Vector2(600, 30));
            Hints(root);
            Finish();
        }

        public override void OnBack() { }   // the main menu has nowhere to go back to
    }

    // ------------------------------------------------------------------ save slots
    public sealed class SlotSelect : MenuScreen
    {
        public enum Mode { New, Load, Chapter }
        readonly Mode _mode;
        readonly int _chapter;

        public SlotSelect(Mode mode, int chapter = 1)
        {
            _mode = mode;
            _chapter = chapter;
        }

        public override bool PausesGame => false;

        public static string Describe(SaveSummary s)
        {
            if (!s.Valid) return Game.Loc["ui.slot_empty"];
            var act = ContentDb.Act(Mathf.Clamp(s.Act, 1, 4));
            string chapter = s.Act >= 5 ? Game.Loc["ui.slot_finished"] : (act != null ? Game.Loc[act.NameKey] : "");
            var t = TimeSpan.FromSeconds(s.PlaySeconds);
            string time = ((int)t.TotalHours).ToString("0") + ":" + t.Minutes.ToString("00");
            string when = "";
            if (DateTime.TryParse(s.SavedUtc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                when = dt.ToLocalTime().ToString("yyyy-MM-dd  HH:mm", System.Globalization.CultureInfo.InvariantCulture);
            return chapter + "   ·   " + time + "   ·   " + Game.Loc.F("ui.slot_drawings", s.Drawings) + "   ·   " + when;
        }

        public override void Build(RectTransform root)
        {
            Root = root;
            string title = _mode == Mode.Load ? Game.Loc["ui.load_game"] : (_mode == Mode.New ? Game.Loc["ui.new_game"] : Game.Loc["ui.chapters"]);
            Frame(title, Game.Loc[_mode == Mode.Load ? "ui.slot_pick_load" : "ui.slot_pick_new"]);
            for (int i = 0; i < SaveService.SlotCount; i++)
            {
                int slot = i;
                var s = Game.Saves.Summary(i);
                var b = ButtonText(Game.Loc.F("ui.slot", i + 1) + "\n<size=22><color=#8e8e8c>" + Describe(s) + "</color></size>", () => Pick(slot, s), _mode == Mode.Load && !s.Valid, 34);
                UIKit.Size(b, 96f);
            }
            if (_mode == Mode.Chapter) ButtonText(Game.Loc["ui.slot_nosave"], () => Game.Flow.StartChapter(_chapter, -1));
            if (_mode == Mode.Load)
            {
                var del = UIKit.Label(root, "DelHint", (Game.Input.UsingGamepad ? "[Y]" : "[Del]") + "  " + Game.Loc["ui.slot_delete"], 22, FontRole.Ui, TextAnchor.LowerLeft, UIKit.Faint);
                UIKit.Anchor((RectTransform)del.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(150, 40), new Vector2(600, 40));
            }
            Finish();
        }

        void Pick(int slot, SaveSummary s)
        {
            switch (_mode)
            {
                case Mode.Load:
                    if (s.Valid) Game.Flow.LoadSlot(slot);
                    break;
                case Mode.New:
                    if (s.Valid) Game.UI.Push(new ConfirmScreen(Game.Loc["ui.overwrite"], Game.Loc["ui.overwrite_body"], () => Game.Flow.NewGame(slot), null));
                    else Game.Flow.NewGame(slot);
                    break;
                case Mode.Chapter:
                    if (s.Valid) Game.UI.Push(new ConfirmScreen(Game.Loc["ui.overwrite"], Game.Loc["ui.overwrite_body"], () => Game.Flow.StartChapter(_chapter, slot), null));
                    else Game.Flow.StartChapter(_chapter, slot);
                    break;
            }
        }

        public override void Tick()
        {
            if (_mode != Mode.Load) return;
            bool del = (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.deleteKey.wasPressedThisFrame) ||
                       (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonNorth.wasPressedThisFrame);
            if (!del) return;
            var cur = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
            int idx = cur != null ? Items.FindIndex(x => x.gameObject == cur) : -1;
            if (idx < 0 || idx >= SaveService.SlotCount || !Game.Saves.Summary(idx).Valid) return;
            Game.UI.Push(new ConfirmScreen(Game.Loc["ui.slot_delete"], Game.Loc.F("ui.slot_delete_body", idx + 1), () =>
            {
                Game.Saves.Delete(idx);
                Game.UI.Pop(Game.UI.Top);
                Game.UI.Replace(new MainMenu());
                Game.UI.Push(new SlotSelect(Mode.Load));
            }, null, true));
        }
    }

    // ------------------------------------------------------------------ chapters
    public sealed class ChapterSelect : MenuScreen
    {
        public override bool PausesGame => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            Frame(Game.Loc["ui.chapters"], Game.Loc["ui.chapters_sub"]);
            for (int a = 1; a <= 4; a++)
            {
                int act = a;
                bool open = Game.Saves.ChapterUnlocked(a) || Game.Achievements.Profile.HighestActReached >= a;
                var def = ContentDb.Act(a);
                string name = open ? Game.Loc[def.NameKey] : Game.Loc["ui.locked"];
                string line = open ? Game.Loc[def.LineKey] : "";
                var b = ButtonText(RomanAct(a) + "   " + name + (line.Length > 0 ? "\n<size=22><color=#8e8e8c>" + line + "</color></size>" : ""), () => Game.UI.Push(new SlotSelect(SlotSelect.Mode.Chapter, act)), !open, 34);
                UIKit.Size(b, 96f);
            }
            Finish();
        }

        public static string RomanAct(int a) => a == 1 ? "I" : (a == 2 ? "II" : (a == 3 ? "III" : "IV"));
    }

    // ------------------------------------------------------------------ extras
    public sealed class ExtrasScreen : MenuScreen
    {
        public override bool PausesGame => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var p = Game.Achievements.Profile;
            Frame(Game.Loc["ui.extras"], Game.Loc.F("ui.extras_sub", p.UnlockedCount, AchievementProfile.All.Length));
            Button("ui.achievements", () => Game.UI.Push(new AchievementsScreen()));
            Button("ui.gallery", () => Game.UI.Push(new GalleryScreen()));
            Button("ui.endings", () => Game.UI.Push(new EndingsListScreen()));
            if (p.FinishedOnce) Button("ui.musicbox", () =>
            {
                // the third version: Isabel Crane's original, the right words and a major ending
                Game.Audio.StopMusic(1.5f);
                Game.Audio.Play2D("mb_lullaby_isabel", Bus.Music, 0.95f);
                Game.UI.Subtitles.Caption(Game.Loc["caption.isabel"], 6f);
                Game.Achievements.Unlock("isabel");
            });
            Finish();
        }
    }

    public sealed class AchievementsScreen : MenuScreen
    {
        public override bool PausesGame => false;
        protected override float ColumnWidth => 1300f;

        public override void Build(RectTransform root)
        {
            Root = root;
            var p = Game.Achievements.Profile;
            Frame(Game.Loc["ui.achievements"], Game.Loc.F("ui.extras_sub", p.UnlockedCount, AchievementProfile.All.Length), 150f);
            var area = UIKit.Rect(Column, "Area");
            UIKit.Size(area.gameObject.AddComponent<LayoutElement>(), 760f);
            var content = UIKit.ScrollList(area, out var scroll, 6f);
            foreach (var a in AchievementProfile.All)
            {
                bool got = p.IsUnlocked(a.Id);
                string title = got || !a.Hidden ? Game.Loc[a.TitleKey] : Game.Loc["ui.hidden_achievement"];
                string desc = got || !a.Hidden ? Game.Loc[a.DescKey] : Game.Loc["ui.hidden_achievement_desc"];
                var b = MenuButton.Create<MenuButton>(content, (got ? "<color=#ff9227>◆</color>  " : "<color=#5c5e60>◇</color>  ") + title + "\n<size=21><color=#8e8e8c>" + desc + "</color></size>", 30, null, 84f);
                b.Label.font = UIKit.F(FontRole.Ui);
                Items.Add(b);
            }
            Finish();
            // scroll the selected row into view
            foreach (var it in Items)
            {
                var mb = it as MenuButton;
                mb.OnClick = null;
            }
            _scroll = scroll;
        }

        ScrollRect _scroll;

        public override void Tick()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null || _scroll == null) return;
            var rt = es.currentSelectedGameObject.transform as RectTransform;
            if (rt == null || !rt.IsChildOf(_scroll.content)) return;
            float contentH = _scroll.content.rect.height, viewH = _scroll.viewport.rect.height;
            if (contentH <= viewH) return;
            float y = -rt.anchoredPosition.y;
            float target = Mathf.Clamp01((y - viewH * 0.5f) / (contentH - viewH));
            _scroll.verticalNormalizedPosition = Mathf.Lerp(_scroll.verticalNormalizedPosition, 1f - target, Time.unscaledDeltaTime * 10f);
        }
    }

    /// <summary>Vanessa's drawings found in any save slot.</summary>
    public sealed class GalleryScreen : UIScreen
    {
        public override bool PausesGame => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var bg = UIKit.Image(root, "Bg", new Color(0, 0, 0, 0.85f));
            UIKit.Stretch((RectTransform)bg.transform);
            var title = UIKit.Label(root, "Title", Game.Loc["ui.gallery"], 60, FontRole.Title, TextAnchor.MiddleCenter);
            UIKit.Anchor((RectTransform)title.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1400, 90));
            var found = new HashSet<int>();
            for (int i = 0; i < SaveService.SlotCount; i++)
                if (Game.Saves.Load(i, out var s)) foreach (var d in s.Drawings) found.Add(d);
            if (Game.Act != null) foreach (var d in Game.Act.State.Drawings) found.Add(d);
            var grid = UIKit.Rect(root, "Grid");
            UIKit.Anchor(grid, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(1500, 760));
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(440, 360);
            g.spacing = new Vector2(40, 30);
            g.childAlignment = TextAnchor.MiddleCenter;
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 3;
            Selectable first = null;
            var items = new List<Selectable>();
            for (int i = 1; i <= 6; i++)
            {
                var cell = UIKit.Rect(grid, "Cell" + i);
                var frame = cell.gameObject.AddComponent<Image>();
                frame.color = new Color(1, 1, 1, 0.06f);
                var sel = cell.gameObject.AddComponent<Button>();
                var colors = sel.colors;
                colors.selectedColor = new Color(1f, 0.573f, 0.153f, 0.6f);
                colors.highlightedColor = new Color(1f, 0.573f, 0.153f, 0.35f);
                sel.colors = colors;
                items.Add(sel);
                if (first == null) first = sel;
                var picRt = UIKit.Rect(cell, "Pic");
                UIKit.Stretch(picRt, 14, 14, 14, 54);
                if (found.Contains(i))
                {
                    var tex = Resources.Load<Texture2D>("Undertow/Textures/drawing" + i + "_a");
                    var raw = picRt.gameObject.AddComponent<RawImage>();
                    raw.texture = tex;
                    raw.color = tex != null ? Color.white : new Color(0.3f, 0.3f, 0.3f);
                }
                else
                {
                    var q = UIKit.Label(picRt, "Q", "?", 120, FontRole.Title, TextAnchor.MiddleCenter, UIKit.Faint);
                    UIKit.Stretch((RectTransform)q.transform);
                }
                var cap = UIKit.Label(cell, "Cap", found.Contains(i) ? Game.Loc["drawing." + i] : Game.Loc["ui.not_found"], 22, FontRole.Hand, TextAnchor.MiddleCenter, found.Contains(i) ? UIKit.Ink : UIKit.Faint);
                UIKit.Anchor((RectTransform)cap.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(420, 40));
            }
            FirstSelected = first;
            MenuScreen.Hints(root, null, OnBack);
        }
    }

    public sealed class EndingsListScreen : MenuScreen
    {
        public override bool PausesGame => false;

        public override void Build(RectTransform root)
        {
            Root = root;
            var p = Game.Achievements.Profile;
            Frame(Game.Loc["ui.endings"], Game.Loc.F("ui.endings_sub", p.EndingsSeen.Count, 3));
            foreach (var e in new[] { EndingId.Answer, EndingId.Surface, EndingId.Undertow })
            {
                bool seen = p.EndingsSeen.Contains((int)e);
                string key = "ending." + Endings.Key(e);
                var b = ButtonText((seen ? Game.Loc[key + ".title"] : "? ? ?") + "\n<size=22><color=#8e8e8c>" + (seen ? Game.Loc[key + ".line"] : Game.Loc["ui.ending_unseen"]) + "</color></size>", null, false, 34);
                UIKit.Size(b, 96f);
            }
            Finish();
        }
    }

    // ------------------------------------------------------------------ credits
    public sealed class CreditsScreen : UIScreen
    {
        readonly bool _endRoll;
        readonly Action _done;
        RectTransform _roll;
        float _y;
        float _height;

        public CreditsScreen(bool endRoll, Action done = null)
        {
            _endRoll = endRoll;
            _done = done;
        }

        public override bool PausesGame => false;
        public override bool ShowCursor => !_endRoll;

        public override void Build(RectTransform root)
        {
            Root = root;
            var bg = UIKit.Image(root, "Bg", new Color(0, 0, 0, _endRoll ? 1f : 0.88f));
            UIKit.Stretch((RectTransform)bg.transform);
            var view = UIKit.Rect(root, "View");
            UIKit.Stretch(view);
            view.gameObject.AddComponent<RectMask2D>();
            _roll = UIKit.Rect(view, "Roll");
            UIKit.Anchor(_roll, new Vector2(0.5f, 0), new Vector2(0.5f, 1), Vector2.zero, new Vector2(1400, 10));
            UIKit.VList(_roll, 10f, TextAnchor.UpperCenter);
            var fit = _roll.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            void Line(string text, int size, FontRole role, Color c, float gap = 0f)
            {
                var t = UIKit.Label(_roll, "L", text, size, role, TextAnchor.MiddleCenter, c);
                UIKit.Size(t, size * 1.5f + gap);
            }
            Line("UNDERTOW", 96, FontRole.Title, UIKit.Ink, 20f);
            Line(Game.Loc["ui.subtitle"], 36, FontRole.Document, UIKit.Accent, 60f);
            foreach (var block in Game.Loc["credits.body"].Split('|'))
            {
                var parts = block.Split('~');
                Line(parts[0].Trim(), 26, FontRole.Ui, UIKit.Dim, 4f);
                for (int i = 1; i < parts.Length; i++) Line(parts[i].Trim(), 36, FontRole.Document, UIKit.Ink);
                Line(" ", 30, FontRole.Ui, UIKit.Ink, 30f);
            }
            Line(Game.Loc["credits.thanks"], 34, FontRole.Document, UIKit.Ink, 80f);
            Line("LenstedReal", 44, FontRole.Title, UIKit.Accent, 200f);
            _y = -60f;
            if (_endRoll) Game.Audio.PlayMusic("mus_credits", 2f, 0.9f, false);
        }

        public override void Tick()
        {
            float speed = 62f;
            bool fast = (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.isPressed) ||
                        (UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonSouth.isPressed) ||
                        (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.isPressed);
            _y += speed * (fast ? 4f : 1f) * Time.unscaledDeltaTime;
            _roll.anchoredPosition = new Vector2(0, _y);
            _height = _roll.rect.height;
            if (_height > 20f && _y > _height + 1080f) Close();
        }

        public override void OnBack() => Close();

        void Close()
        {
            if (Root == null) return;
            Game.UI.Pop(this);
            _done?.Invoke();
        }
    }

    // ------------------------------------------------------------------ confirm
    public sealed class ConfirmScreen : UIScreen
    {
        readonly string _title, _body;
        readonly Action _yes, _no;
        readonly bool _danger;

        public ConfirmScreen(string title, string body, Action yes, Action no, bool danger = false)
        {
            _title = title; _body = body; _yes = yes; _no = no; _danger = danger;
        }

        public override bool PausesGame => true;

        public override void Build(RectTransform root)
        {
            Root = root;
            var dim = UIKit.Image(root, "Dim", new Color(0, 0, 0, 0.7f));
            UIKit.Stretch((RectTransform)dim.transform);
            var panel = UIKit.Image(root, "Panel", UIKit.PanelLight);
            UIKit.Anchor((RectTransform)panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(980, 420));
            var t = UIKit.Label(panel.transform, "Title", _title, 46, FontRole.Title, TextAnchor.MiddleCenter, _danger ? UIKit.Accent : UIKit.Ink);
            UIKit.Anchor((RectTransform)t.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(900, 70));
            var b = UIKit.Label(panel.transform, "Body", _body, 26, FontRole.Ui, TextAnchor.UpperCenter, UIKit.Dim);
            UIKit.Anchor((RectTransform)b.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(860, 150));
            var row = UIKit.Rect(panel.transform, "Row");
            UIKit.Anchor(row, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(700, 70));
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 40;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childForceExpandWidth = true;
            h.childControlWidth = true;
            h.childControlHeight = true;
            var no = MenuButton.Create<MenuButton>(row, Game.Loc["ui.no"], 34, () => { Game.UI.Pop(this); _no?.Invoke(); }, 64f);
            var yes = MenuButton.Create<MenuButton>(row, Game.Loc["ui.yes"], 34, () => { Game.UI.Pop(this); _yes?.Invoke(); }, 64f);
            no.Label.alignment = TextAnchor.MiddleCenter;
            yes.Label.alignment = TextAnchor.MiddleCenter;
            UIKit.ChainHorizontal(new List<Selectable> { no, yes });
            FirstSelected = no;
        }

        public override void OnBack()
        {
            UIKit.PlayBack();
            Game.UI.Pop(this);
            _no?.Invoke();
        }
    }
}
