using System;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    public enum FontRole { Ui, Title, Document, Hand, Typewriter }

    /// <summary>
    /// Runtime localisation: tables from Resources/Undertow/Loc/&lt;code&gt;.txt, English fallback per key,
    /// and the fonts each language needs (OFL fonts shipped in Resources, Windows system fonts for CJK).
    /// </summary>
    public sealed class Loc
    {
        public LocTable Table { get; private set; }
        public LanguageInfo Language { get; private set; }
        public event Action Changed;

        readonly Dictionary<FontRole, Font> _fonts = new Dictionary<FontRole, Font>();
        static Font _cjkCache;
        static string _cjkFor;

        public string this[string key] => Table != null ? Table.Get(key) : "#" + key;
        public string F(string key, params object[] args) => Table != null ? Table.Format(key, args) : "#" + key;
        public bool Has(string key) => Table != null && Table.TryGet(key, out _);

        /// <summary>Upper case for carved and painted lettering. Turkish keeps i → İ and ı → I.</summary>
        public string Upper(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            if (Language != null && Language.Code == "tr") s = s.Replace('i', '\u0130').Replace('\u0131', 'I');
            return s.ToUpperInvariant();
        }

        public static string SystemLanguageCode()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Portuguese: return "pt-BR";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional: return "zh-Hans";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                default: return "en";
            }
        }

        public void Load(string code)
        {
            var info = LocTable.Find(code) ?? LocTable.Find("en");
            var errors = new List<string>();
            var en = LocTable.Parse("en", ReadTable("en"), errors);
            LocTable t = en;
            if (info.Code != "en")
            {
                t = LocTable.Parse(info.Code, ReadTable(info.Code), errors);
                t.Fallback = en;
            }
            foreach (var e in errors) Game.Warn("loc: " + e);
            Table = t;
            Language = info;
            LoadFonts(info);
            Changed?.Invoke();
        }

        static string ReadTable(string code)
        {
            var ta = Resources.Load<TextAsset>("Undertow/Loc/" + code);
            if (ta == null)
            {
                Game.Warn("missing localisation table " + code);
                return string.Empty;
            }
            return ta.text;
        }

        void LoadFonts(LanguageInfo info)
        {
            _fonts.Clear();
            Font ui = Resources.Load<Font>("Undertow/Fonts/FiraSans-Regular");
            Font title = Resources.Load<Font>("Undertow/Fonts/CormorantGaramond-SemiBold");
            Font doc = Resources.Load<Font>("Undertow/Fonts/EBGaramond");
            Font hand = Resources.Load<Font>("Undertow/Fonts/Caveat");
            Font type = Resources.Load<Font>("Undertow/Fonts/IMFellEnglish");   // period type: 1724 notices and deeds
            if (info.Code == "ru") type = doc;   // IM Fell English has no Cyrillic glyphs
            Font fallback = ui ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (info.Cjk)
            {
                var cjk = CjkFont(info.Code);
                if (cjk != null)
                {
                    ui = cjk;
                    title = cjk;
                    doc = cjk;
                    hand = cjk;
                    type = cjk;
                }
            }
            _fonts[FontRole.Ui] = ui ?? fallback;
            _fonts[FontRole.Title] = title ?? fallback;
            _fonts[FontRole.Document] = doc ?? fallback;
            _fonts[FontRole.Hand] = hand ?? fallback;
            _fonts[FontRole.Typewriter] = type ?? doc ?? fallback;
        }

        static Font CjkFont(string code)
        {
            if (_cjkCache != null && _cjkFor == code) return _cjkCache;
            string[] names;
            switch (code)
            {
                case "ja": names = new[] { "Yu Gothic UI", "Yu Gothic", "Meiryo UI", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Noto Sans JP" }; break;
                case "ko": names = new[] { "Malgun Gothic", "Gulim", "Noto Sans CJK KR", "Noto Sans KR" }; break;
                default: names = new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei", "DengXian", "Noto Sans CJK SC", "Noto Sans SC" }; break;
            }
            try
            {
                _cjkCache = Font.CreateDynamicFontFromOSFont(names, 32);
                _cjkFor = code;
            }
            catch (Exception e)
            {
                Game.Warn("CJK font unavailable: " + e.Message);
                _cjkCache = null;
            }
            return _cjkCache;
        }

        public Font FontFor(FontRole role) => _fonts.TryGetValue(role, out var f) && f != null ? f : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>Speaker display name and colour for subtitles.</summary>
        public string SpeakerName(VoiceKind k)
        {
            switch (k)
            {
                case VoiceKind.Echo: return this["speaker.echo"];
                case VoiceKind.Mimic: return this["speaker.mimic"];
                case VoiceKind.Adrian: return this["speaker.adrian"];
                case VoiceKind.Thorne: return this["speaker.thorne"];
                case VoiceKind.Ruth: return this["speaker.ruth"];
                case VoiceKind.Choir: return this["speaker.choir"];
                default: return this["speaker.whisper"];
            }
        }

        public static Color SpeakerColor(VoiceKind k)
        {
            switch (k)
            {
                case VoiceKind.Echo: return new Color(0.98f, 0.82f, 0.62f);
                case VoiceKind.Mimic: return new Color(0.66f, 0.84f, 0.92f);
                case VoiceKind.Adrian: return new Color(0.85f, 0.85f, 0.82f);
                case VoiceKind.Thorne: return new Color(0.82f, 0.74f, 0.6f);
                case VoiceKind.Ruth: return new Color(0.92f, 0.76f, 0.8f);
                case VoiceKind.Choir: return new Color(0.7f, 0.9f, 0.86f);
                default: return new Color(0.75f, 0.75f, 0.78f);
            }
        }
    }
}
