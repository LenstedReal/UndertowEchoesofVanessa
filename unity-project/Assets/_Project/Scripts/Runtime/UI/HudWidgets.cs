using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>Minimal HUD: crosshair, interaction prompt, short messages, oil gauge, objective line, listen compass, saving icon.</summary>
    public sealed class Hud
    {
        readonly Image _dot, _ring, _oilIcon, _oilBack, _oilFill, _listenArrow, _saveIcon, _listenVignette;
        readonly Text _prompt, _message, _objective, _saveText;
        float _messageUntil, _objectiveUntil, _oilShowUntil, _saveUntil;
        float _ringScale = 1f;
        float _lastOil = -1f;

        public bool LookingAtInteractable;
        public string PromptText;
        public bool PromptDimmed;
        public float Oil01 = 1f;
        public bool OilLow;
        public bool Listening;
        public Vector3? ListenTarget;

        public Hud(RectTransform layer)
        {
            _ring = UIKit.Image(layer, "Ring", new Color(1, 1, 1, 0f), UIKit.Ring);
            UIKit.Anchor((RectTransform)_ring.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(30, 30));
            _dot = UIKit.Image(layer, "Dot", new Color(1, 1, 1, 0.55f), UIKit.Dot);
            UIKit.Anchor((RectTransform)_dot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(7, 7));

            _prompt = UIKit.Label(layer, "Prompt", "", 27, FontRole.Ui, TextAnchor.MiddleCenter);
            UIKit.Anchor((RectTransform)_prompt.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -46), new Vector2(1100, 44));
            var sh = _prompt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);

            _message = UIKit.Label(layer, "Message", "", 26, FontRole.Ui, TextAnchor.MiddleCenter, new Color(0.92f, 0.88f, 0.8f, 0f));
            UIKit.Anchor((RectTransform)_message.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -96), new Vector2(1300, 80));
            _message.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.9f);

            _objective = UIKit.Label(layer, "Objective", "", 26, FontRole.Document, TextAnchor.UpperLeft, new Color(0.9f, 0.86f, 0.78f, 0f));
            UIKit.Anchor((RectTransform)_objective.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(56, -46), new Vector2(820, 120));
            _objective.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.85f);

            _oilIcon = UIKit.Image(layer, "OilIcon", new Color(1f, 0.57f, 0.15f, 0f), UIKit.Soft);
            UIKit.Anchor((RectTransform)_oilIcon.transform, new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(64, 64), new Vector2(44, 44));
            _oilBack = UIKit.Image(layer, "OilBack", new Color(1, 1, 1, 0f));
            UIKit.Anchor((RectTransform)_oilBack.transform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(96, 64), new Vector2(180, 4));
            _oilFill = UIKit.Image(_oilBack.transform, "Fill", new Color(1f, 0.57f, 0.15f, 0f));
            var fr = (RectTransform)_oilFill.transform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(1, 1);
            fr.pivot = new Vector2(0, 0.5f);
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = Vector2.zero;

            _listenVignette = UIKit.Image(layer, "ListenVignette", new Color(0.45f, 0.65f, 0.7f, 0f), UIKit.Vignette);
            UIKit.Stretch((RectTransform)_listenVignette.transform, -200, -200, -200, -200);
            _listenArrow = UIKit.Image(layer, "ListenArrow", new Color(0.85f, 0.95f, 1f, 0f), UIKit.Dot);
            UIKit.Anchor((RectTransform)_listenArrow.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16, 16));

            _saveIcon = UIKit.Image(layer, "Save", new Color(1f, 0.57f, 0.15f, 0f), UIKit.Ring);
            UIKit.Anchor((RectTransform)_saveIcon.transform, new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(-60, 120), new Vector2(30, 30));
            _saveText = UIKit.Label(layer, "SaveText", "", 20, FontRole.Ui, TextAnchor.MiddleRight, new Color(1, 1, 1, 0f));
            UIKit.Anchor((RectTransform)_saveText.transform, new Vector2(1, 0), new Vector2(1, 0.5f), new Vector2(-86, 120), new Vector2(300, 30));
        }

        public void ApplySettings()
        {
            _dot.enabled = Game.Settings.Crosshair;
            _ring.enabled = Game.Settings.Crosshair;
        }

        public void OnLanguageChanged()
        {
            _prompt.font = UIKit.F(FontRole.Ui);
            _message.font = UIKit.F(FontRole.Ui);
            _objective.font = UIKit.F(FontRole.Document);
            _saveText.font = UIKit.F(FontRole.Ui);
        }

        public void Message(string text, float seconds = 3.2f)
        {
            _message.text = text;
            _messageUntil = Time.unscaledTime + seconds;
        }

        public void Objective(string text, float seconds = 7f)
        {
            if (!Game.Settings.ObjectiveHints || string.IsNullOrEmpty(text)) return;
            _objective.text = text;
            _objectiveUntil = Time.unscaledTime + seconds;
        }

        public void Saving()
        {
            _saveText.text = Game.Loc["hud.saving"];
            _saveUntil = Time.unscaledTime + 2.4f;
        }

        public void ShowOil(float seconds = 3f) => _oilShowUntil = Time.unscaledTime + seconds;

        static float FadeFor(float until, float now, float fadeTime = 0.6f) => Mathf.Clamp01((until - now) / fadeTime);

        public void Tick()
        {
            float now = Time.unscaledTime;
            float dt = Time.unscaledDeltaTime;
            // crosshair
            _ringScale = Mathf.MoveTowards(_ringScale, LookingAtInteractable ? 1f : 0.4f, dt * 6f);
            _ring.rectTransform.localScale = Vector3.one * _ringScale;
            _ring.color = new Color(1f, 0.6f, 0.2f, LookingAtInteractable ? 0.75f : 0f);
            _dot.color = LookingAtInteractable ? new Color(1f, 0.7f, 0.35f, 0.9f) : new Color(1, 1, 1, 0.45f);
            // prompt
            _prompt.text = PromptText ?? "";
            _prompt.color = PromptDimmed ? new Color(0.65f, 0.65f, 0.65f, 0.9f) : UIKit.Ink;
            // message & objective
            var mc = _message.color;
            mc.a = FadeFor(_messageUntil, now);
            _message.color = mc;
            var oc = _objective.color;
            oc.a = FadeFor(_objectiveUntil, now, 1.2f) * Mathf.Clamp01((now - (_objectiveUntil - 7f)) * 2f);
            _objective.color = oc;
            // oil gauge
            if (_lastOil >= 0f && Mathf.Abs(Oil01 - _lastOil) > 0.05f) ShowOil();
            if (_lastOil < 0f || Mathf.Abs(Oil01 - _lastOil) > 0.05f) _lastOil = Oil01;
            float oa = OilLow ? 0.65f + 0.35f * Mathf.Sin(now * 4f) : FadeFor(_oilShowUntil, now, 1f);
            _oilIcon.color = new Color(1f, 0.57f, 0.15f, oa * 0.9f);
            _oilBack.color = new Color(1, 1, 1, oa * 0.18f);
            _oilFill.color = new Color(1f, OilLow ? 0.35f : 0.57f, 0.15f, oa);
            ((RectTransform)_oilFill.transform).anchorMax = new Vector2(Mathf.Clamp01(Oil01), 1);
            // listen
            float la = _listenVignette.color.a;
            _listenVignette.color = new Color(0.45f, 0.65f, 0.7f, Mathf.MoveTowards(la, Listening ? 0.35f : 0f, dt * 2f));
            var cam = Camera.main;
            if (Listening && ListenTarget.HasValue && cam != null && Game.Settings.DirectionArrows)
            {
                var local = cam.transform.InverseTransformPoint(ListenTarget.Value);
                var dir = new Vector2(local.x, local.z);
                if (dir.sqrMagnitude > 0.01f)
                {
                    dir.Normalize();
                    ((RectTransform)_listenArrow.transform).anchoredPosition = new Vector2(dir.x, dir.y) * 120f;
                    _listenArrow.color = new Color(0.85f, 0.95f, 1f, 0.55f + 0.3f * Mathf.Sin(now * 5f));
                }
            }
            else _listenArrow.color = new Color(0.85f, 0.95f, 1f, 0f);
            // saving
            float sa = FadeFor(_saveUntil, now, 0.8f);
            _saveIcon.color = new Color(1f, 0.57f, 0.15f, sa);
            _saveIcon.rectTransform.Rotate(0, 0, -200f * dt);
            _saveText.color = new Color(1, 1, 1, sa * 0.7f);
        }
    }

    /// <summary>
    /// Subtitles with speaker names and colours, [sound captions], Echo in italics, Mimic upright,
    /// adjustable size and background, and a direction marker when the voice is behind you.
    /// </summary>
    public sealed class Subtitles
    {
        sealed class Line
        {
            public string Speaker;
            public Color Color;
            public string Text;
            public bool Italic;
            public bool Caption;
            public Vector3? From;
            public float Until;
        }

        static readonly int[] Sizes = { 26, 32, 40, 50 };
        readonly RectTransform _box;
        readonly Image _bg;
        readonly Text _text;
        readonly List<Line> _lines = new List<Line>();
        readonly Queue<Line> _queue = new Queue<Line>();

        public Subtitles(RectTransform layer)
        {
            _box = UIKit.Rect(layer, "Subtitles");
            UIKit.Anchor(_box, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1300, 100));
            _bg = _box.gameObject.AddComponent<Image>();
            _bg.sprite = UIKit.White;
            _bg.raycastTarget = false;
            _text = UIKit.Label(_box, "Text", "", 32, FontRole.Ui, TextAnchor.MiddleCenter);
            UIKit.Stretch((RectTransform)_text.transform, 24, 10, 24, 10);
            _text.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.8f);
            ApplySettings();
            Clear();
        }

        public void ApplySettings()
        {
            _text.fontSize = Sizes[Mathf.Clamp(Game.Settings.SubtitleSize, 0, 3)];
            _text.font = UIKit.F(FontRole.Ui);
        }

        public void Clear()
        {
            _lines.Clear();
            _queue.Clear();
            Render();
        }

        /// <summary>A spoken line. Shown only when subtitles are on.</summary>
        public void Say(VoiceKind speaker, string text, float seconds, Vector3? from = null)
        {
            if (!Game.Settings.Subtitles || string.IsNullOrEmpty(text)) return;
            var l = new Line
            {
                Speaker = speaker == VoiceKind.Whisper ? null : Game.Loc.SpeakerName(speaker),   // the endings' narrator has no name tag
                Color = Loc.SpeakerColor(speaker),
                Text = text,
                Italic = speaker == VoiceKind.Echo || speaker == VoiceKind.Adrian,
                From = from,
                Until = seconds,
            };
            Enqueue(l);
        }

        /// <summary>[Sound caption] for deaf and hard-of-hearing players.</summary>
        public void Caption(string text, float seconds = 3f, Vector3? from = null)
        {
            if (!Game.Settings.Subtitles || !Game.Settings.SoundCaptions || string.IsNullOrEmpty(text)) return;
            Enqueue(new Line { Text = "[" + text + "]", Caption = true, Color = new Color(0.8f, 0.8f, 0.8f), From = from, Until = seconds });
        }

        void Enqueue(Line l)
        {
            if (_lines.Count < 2)
            {
                l.Until = Time.unscaledTime + l.Until;
                _lines.Add(l);
                Render();
            }
            else _queue.Enqueue(l);
        }

        public void Tick()
        {
            bool changed = false;
            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime > _lines[i].Until)
                {
                    _lines.RemoveAt(i);
                    changed = true;
                }
            }
            while (_lines.Count < 2 && _queue.Count > 0)
            {
                var l = _queue.Dequeue();
                l.Until = Time.unscaledTime + l.Until;
                _lines.Add(l);
                changed = true;
            }
            if (changed || _lines.Exists(l => l.From.HasValue)) Render();
        }

        void Render()
        {
            if (_lines.Count == 0)
            {
                _text.text = "";
                _bg.color = new Color(0, 0, 0, 0);
                return;
            }
            var sb = new System.Text.StringBuilder();
            var cam = Camera.main;
            for (int i = 0; i < _lines.Count; i++)
            {
                var l = _lines[i];
                if (i > 0) sb.Append('\n');
                string arrowL = "", arrowR = "";
                if (l.From.HasValue && cam != null && Game.Settings.DirectionArrows)
                {
                    var local = cam.transform.InverseTransformPoint(l.From.Value);
                    float ang = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                    if (ang < -40f) arrowL = "<color=#ffffffaa>◄</color> ";
                    else if (ang > 40f) arrowR = " <color=#ffffffaa>►</color>";
                }
                sb.Append(arrowL);
                if (!l.Caption && Game.Settings.SpeakerNames && !string.IsNullOrEmpty(l.Speaker))
                    sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(l.Color)).Append('>').Append(l.Speaker).Append(":</color> ");
                string body = l.Italic || l.Caption ? "<i>" + l.Text + "</i>" : l.Text;
                if (l.Caption) body = "<color=#cfcfcf>" + body + "</color>";
                sb.Append(body).Append(arrowR);
            }
            _text.text = sb.ToString();
            float h = _text.preferredHeight + 24f;
            _box.sizeDelta = new Vector2(1300, Mathf.Max(60f, h));
            _bg.color = new Color(0, 0, 0, Game.Settings.SubtitleBackground * 0.85f);
        }
    }

    /// <summary>Top-right notifications (achievements, journal updates, drawings).</summary>
    public sealed class Toasts
    {
        sealed class Toast
        {
            public RectTransform Rt;
            public CanvasGroup Group;
            public float Born;
            public float Life;
        }

        readonly RectTransform _layer;
        readonly List<Toast> _live = new List<Toast>();

        public Toasts(RectTransform layer)
        {
            _layer = UIKit.Rect(layer, "Toasts");
            UIKit.Anchor(_layer, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(520, 600));
        }

        public void Achievement(AchievementDef a)
        {
            Show(Game.Loc["toast.achievement"], Game.Loc[a.TitleKey], true);
            Game.Audio?.Play2D("ui_achievement", Bus.Ui, 0.55f);
        }

        public void Show(string header, string body, bool accent = false, float life = 4.8f)
        {
            var rt = UIKit.Rect(_layer, "Toast");
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(500, 92);
            var bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = UIKit.White;
            bg.color = new Color(0.03f, 0.035f, 0.04f, 0.86f);
            bg.raycastTarget = false;
            var bar = UIKit.Image(rt, "Bar", accent ? UIKit.Accent : new Color(1, 1, 1, 0.4f));
            UIKit.Anchor((RectTransform)bar.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(4, 92));
            var h = UIKit.Label(rt, "H", header, 19, FontRole.Ui, TextAnchor.UpperLeft, accent ? UIKit.Accent : UIKit.Dim);
            UIKit.Stretch((RectTransform)h.transform, 22, 12, 14, 50);
            var b = UIKit.Label(rt, "B", body, 26, FontRole.Title, TextAnchor.UpperLeft, UIKit.Ink);
            UIKit.Stretch((RectTransform)b.transform, 22, 38, 14, 6);
            var g = rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            _live.Add(new Toast { Rt = rt, Group = g, Born = Time.unscaledTime, Life = life });
        }

        public void Tick()
        {
            float now = Time.unscaledTime;
            float y = 0f;
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                var t = _live[i];
                float age = now - t.Born;
                if (age > t.Life)
                {
                    Object.Destroy(t.Rt.gameObject);
                    _live.RemoveAt(i);
                }
            }
            foreach (var t in _live)
            {
                float age = now - t.Born;
                t.Group.alpha = Mathf.Clamp01(age * 4f) * Mathf.Clamp01((t.Life - age) * 1.5f);
                var target = new Vector2(0, -y);
                t.Rt.anchoredPosition = Vector2.Lerp(t.Rt.anchoredPosition, target, Time.unscaledDeltaTime * 10f);
                y += 102f;
            }
        }
    }
}
