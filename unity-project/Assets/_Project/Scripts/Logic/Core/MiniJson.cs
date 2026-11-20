using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Undertow.Logic
{
    /// <summary>
    /// Minimal JSON reader/writer (objects, arrays, strings, numbers, booleans, null).
    /// Objects parse to Dictionary&lt;string, object&gt;, arrays to List&lt;object&gt;, numbers to double.
    /// Culture-invariant on every machine (Turkish locale safe).
    /// </summary>
    public static class MiniJson
    {
        public static object Parse(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var p = new Parser(json);
            p.SkipWs();
            var v = p.ReadValue();
            p.SkipWs();
            if (!p.End) throw new FormatException("Trailing characters at " + p.Pos);
            return v;
        }

        public static bool TryParse(string json, out object value)
        {
            try
            {
                value = Parse(json);
                return true;
            }
            catch (Exception)
            {
                value = null;
                return false;
            }
        }

        public static string Serialize(object value, bool pretty = false)
        {
            var sb = new StringBuilder(256);
            Write(sb, value, pretty, 0);
            return sb.ToString();
        }

        // ---- reading helpers for parsed trees
        public static Dictionary<string, object> Obj(object o) => o as Dictionary<string, object>;
        public static List<object> Arr(object o) => o as List<object>;

        public static string Str(Dictionary<string, object> d, string key, string fallback = null)
            => d != null && d.TryGetValue(key, out var v) && v is string s ? s : fallback;

        public static double Num(Dictionary<string, object> d, string key, double fallback = 0)
            => d != null && d.TryGetValue(key, out var v) && v is double n ? n : fallback;

        public static int Int(Dictionary<string, object> d, string key, int fallback = 0)
            => d != null && d.TryGetValue(key, out var v) && v is double n ? (int)Math.Round(n) : fallback;

        public static float Float(Dictionary<string, object> d, string key, float fallback = 0f)
            => d != null && d.TryGetValue(key, out var v) && v is double n ? (float)n : fallback;

        public static bool Bool(Dictionary<string, object> d, string key, bool fallback = false)
            => d != null && d.TryGetValue(key, out var v) && v is bool b ? b : fallback;

        sealed class Parser
        {
            readonly string _s;
            int _i;

            public Parser(string s) { _s = s; }
            public bool End => _i >= _s.Length;
            public int Pos => _i;

            public void SkipWs()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '﻿') _i++;
                    else break;
                }
            }

            char Peek() => _i < _s.Length ? _s[_i] : '\0';

            void Expect(char c)
            {
                if (Peek() != c) throw new FormatException($"Expected '{c}' at {_i}");
                _i++;
            }

            public object ReadValue()
            {
                SkipWs();
                char c = Peek();
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return ReadString();
                    case 't': ReadWord("true"); return true;
                    case 'f': ReadWord("false"); return false;
                    case 'n': ReadWord("null"); return null;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber();
                        throw new FormatException($"Unexpected '{c}' at {_i}");
                }
            }

            void ReadWord(string w)
            {
                if (string.CompareOrdinal(_s, _i, w, 0, w.Length) != 0) throw new FormatException("Bad literal at " + _i);
                _i += w.Length;
            }

            Dictionary<string, object> ReadObject()
            {
                var d = new Dictionary<string, object>(StringComparer.Ordinal);
                Expect('{');
                SkipWs();
                if (Peek() == '}') { _i++; return d; }
                while (true)
                {
                    SkipWs();
                    var key = ReadString();
                    SkipWs();
                    Expect(':');
                    var v = ReadValue();
                    d[key] = v;
                    SkipWs();
                    if (Peek() == ',') { _i++; continue; }
                    Expect('}');
                    return d;
                }
            }

            List<object> ReadArray()
            {
                var l = new List<object>();
                Expect('[');
                SkipWs();
                if (Peek() == ']') { _i++; return l; }
                while (true)
                {
                    l.Add(ReadValue());
                    SkipWs();
                    if (Peek() == ',') { _i++; continue; }
                    Expect(']');
                    return l;
                }
            }

            string ReadString()
            {
                Expect('"');
                var sb = new StringBuilder();
                while (true)
                {
                    if (_i >= _s.Length) throw new FormatException("Unterminated string");
                    char c = _s[_i++];
                    if (c == '"') break;
                    if (c != '\\') { sb.Append(c); continue; }
                    if (_i >= _s.Length) throw new FormatException("Bad escape");
                    char e = _s[_i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length) throw new FormatException("Bad unicode escape");
                            sb.Append((char)int.Parse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            _i += 4;
                            break;
                        default: throw new FormatException("Bad escape \\" + e);
                    }
                }
                return sb.ToString();
            }

            double ReadNumber()
            {
                int start = _i;
                if (Peek() == '-') _i++;
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if ((c >= '0' && c <= '9') || c == '.' || c == 'e' || c == 'E' || c == '+' || c == '-') _i++;
                    else break;
                }
                return double.Parse(_s.Substring(start, _i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }

        static void Indent(StringBuilder sb, bool pretty, int depth)
        {
            if (!pretty) return;
            sb.Append('\n');
            sb.Append(' ', depth * 2);
        }

        static void Write(StringBuilder sb, object v, bool pretty, int depth)
        {
            switch (v)
            {
                case null: sb.Append("null"); return;
                case string s: WriteString(sb, s); return;
                case bool b: sb.Append(b ? "true" : "false"); return;
                case double d: sb.Append(FormatNumber(d)); return;
                case float f: sb.Append(FormatNumber(f)); return;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); return;
                case long l: sb.Append(l.ToString(CultureInfo.InvariantCulture)); return;
                case IDictionary dict:
                {
                    sb.Append('{');
                    bool first = true;
                    var keys = new List<string>();
                    foreach (var k in dict.Keys) keys.Add(Convert.ToString(k, CultureInfo.InvariantCulture));
                    keys.Sort(StringComparer.Ordinal);
                    foreach (var k in keys)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Indent(sb, pretty, depth + 1);
                        WriteString(sb, k);
                        sb.Append(pretty ? ": " : ":");
                        Write(sb, dict[k], pretty, depth + 1);
                    }
                    if (!first) Indent(sb, pretty, depth);
                    sb.Append('}');
                    return;
                }
                case IEnumerable list:
                {
                    sb.Append('[');
                    bool first = true;
                    foreach (var x in list)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Indent(sb, pretty, depth + 1);
                        Write(sb, x, pretty, depth + 1);
                    }
                    if (!first) Indent(sb, pretty, depth);
                    sb.Append(']');
                    return;
                }
                default:
                    if (v is Enum) { sb.Append(Convert.ToInt32(v, CultureInfo.InvariantCulture)); return; }
                    WriteString(sb, Convert.ToString(v, CultureInfo.InvariantCulture));
                    return;
            }
        }

        static string FormatNumber(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d)) return "0";
            if (Math.Abs(d - Math.Round(d)) < 1e-9 && Math.Abs(d) < 1e15) return ((long)Math.Round(d)).ToString(CultureInfo.InvariantCulture);
            return d.ToString("R", CultureInfo.InvariantCulture);
        }

        static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }
    }
}
