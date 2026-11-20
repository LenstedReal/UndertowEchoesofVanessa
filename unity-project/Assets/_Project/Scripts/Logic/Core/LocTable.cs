using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Undertow.Logic
{
    /// <summary>A language the game ships with.</summary>
    public sealed class LanguageInfo
    {
        public string Code;          // file name: en, tr, de, fr, es, pt-BR, ru, zh-Hans, ja, ko
        public string NativeName;    // shown in the language picker
        public bool Cjk;             // needs an OS font with CJK glyphs
        public bool Reviewed;        // true when a native speaker approved the text

        public LanguageInfo(string code, string nativeName, bool cjk, bool reviewed)
        {
            Code = code;
            NativeName = nativeName;
            Cjk = cjk;
            Reviewed = reviewed;
        }
    }

    /// <summary>
    /// Localisation tables. File format (UTF-8): one entry per line, "key = value".
    /// "\n" in a value is a line break, lines starting with '#' are comments.
    /// English is the source; every other table falls back to English per key.
    /// </summary>
    public sealed class LocTable
    {
        public static readonly LanguageInfo[] Languages =
        {
            new LanguageInfo("en", "English", false, true),
            new LanguageInfo("tr", "Türkçe", false, true),
            new LanguageInfo("de", "Deutsch", false, false),
            new LanguageInfo("fr", "Français", false, false),
            new LanguageInfo("es", "Español (Latinoamérica)", false, false),
            new LanguageInfo("pt-BR", "Português (Brasil)", false, false),
            new LanguageInfo("ru", "Русский", false, false),
            new LanguageInfo("zh-Hans", "简体中文", true, false),
            new LanguageInfo("ja", "日本語", true, false),
            new LanguageInfo("ko", "한국어", true, false),
        };

        public static LanguageInfo Find(string code)
        {
            foreach (var l in Languages) if (string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase)) return l;
            return null;
        }

        readonly Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.Ordinal);
        public string Code { get; }
        public LocTable Fallback { get; set; }
        public int Count => _map.Count;
        public IEnumerable<string> Keys => _map.Keys;

        public LocTable(string code) { Code = code; }

        public static LocTable Parse(string code, string text, List<string> errors = null)
        {
            var t = new LocTable(code);
            if (string.IsNullOrEmpty(text)) return t;
            int lineNo = 0;
            foreach (var raw in text.Split('\n'))
            {
                lineNo++;
                var line = raw.TrimEnd('\r');
                if (line.Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal)) continue;
                int eq = line.IndexOf(" = ", StringComparison.Ordinal);
                if (eq <= 0)
                {
                    errors?.Add($"{code}:{lineNo}: missing ' = '");
                    continue;
                }
                var key = line.Substring(0, eq).Trim().TrimStart('﻿');
                var value = Unescape(line.Substring(eq + 3));
                if (t._map.ContainsKey(key)) errors?.Add($"{code}:{lineNo}: duplicate key {key}");
                t._map[key] = value;
            }
            return t;
        }

        static string Unescape(string v)
        {
            if (v.IndexOf('\\') < 0) return v;
            var sb = new StringBuilder(v.Length);
            for (int i = 0; i < v.Length; i++)
            {
                char c = v[i];
                if (c == '\\' && i + 1 < v.Length)
                {
                    char n = v[i + 1];
                    if (n == 'n') { sb.Append('\n'); i++; continue; }
                    if (n == 't') { sb.Append('\t'); i++; continue; }
                    if (n == '\\') { sb.Append('\\'); i++; continue; }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        public bool Has(string key) => _map.ContainsKey(key);

        public bool TryGet(string key, out string value)
        {
            if (key != null && _map.TryGetValue(key, out value)) return true;
            if (Fallback != null && key != null) return Fallback.TryGet(key, out value);
            value = null;
            return false;
        }

        /// <summary>Text for key; missing keys show as "#key" so they are easy to spot in a playtest.</summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return TryGet(key, out var v) ? v : "#" + key;
        }

        public string Format(string key, params object[] args)
        {
            var f = Get(key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, f, args);
            }
            catch (FormatException)
            {
                return f;
            }
        }

        public void Set(string key, string value) => _map[key] = value;

        /// <summary>Counts words of a value (for the 80-word note rule). CJK counts characters / 2.</summary>
        public static int WordCount(string text, bool cjk)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (cjk)
            {
                int n = 0;
                foreach (char c in text) if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c)) n++;
                return (n + 1) / 2;
            }
            int words = 0;
            bool inWord = false;
            foreach (char c in text)
            {
                bool w = !char.IsWhiteSpace(c);
                if (w && !inWord) words++;
                inWord = w;
            }
            return words;
        }
    }
}
