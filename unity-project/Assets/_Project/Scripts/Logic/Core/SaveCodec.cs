using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Undertow.Logic
{
    /// <summary>Summary shown in the load menu without restoring the whole state.</summary>
    public sealed class SaveSummary
    {
        public int Slot;
        public int Act;
        public string Checkpoint;
        public double PlaySeconds;
        public string SavedUtc;   // ISO 8601
        public int Drawings;
        public bool Valid;
    }

    /// <summary>GameState &lt;-&gt; JSON. Unknown keys are ignored, missing keys keep defaults, so old saves load.</summary>
    public static class SaveCodec
    {
        public const string Magic = "undertow-save";

        public static string Encode(GameState s, int slot, DateTime utcNow)
        {
            var d = new Dictionary<string, object>
            {
                ["magic"] = Magic,
                ["version"] = GameState.CurrentVersion,
                ["slot"] = slot,
                ["savedUtc"] = utcNow.ToString("o", CultureInfo.InvariantCulture),
                ["act"] = s.Act,
                ["checkpoint"] = s.Checkpoint ?? "start",
                ["flags"] = s.Flags.OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToList(),
                ["notes"] = s.NotesRead.Cast<object>().ToList(),
                ["drawings"] = s.Drawings.Select(x => (object)x).ToList(),
                ["ints"] = s.Ints.ToDictionary(kv => kv.Key, kv => (object)kv.Value),
                ["oil"] = (double)s.Oil,
                ["sanity"] = (double)s.Sanity,
                ["lantern"] = s.LanternLit,
                ["playSeconds"] = s.PlaySeconds,
                ["act2Path"] = (int)s.Act2Path,
                ["answered"] = s.Answered,
                ["altar"] = (int)s.Altar,
                ["ending"] = (int)s.Ending,
                ["stats"] = new Dictionary<string, object>
                {
                    ["listens"] = s.Listens,
                    ["examines"] = s.Examines,
                    ["bells"] = s.BellsRung,
                    ["runOuts"] = s.OilRunOuts,
                    ["worstTier"] = s.WorstTier,
                    ["actWorstTier"] = s.ActWorstTier,
                    ["actRunOuts"] = s.ActOilRunOuts,
                    ["braziers"] = s.BraziersLit,
                },
            };
            return MiniJson.Serialize(d, true);
        }

        public static bool TryDecode(string json, out GameState state, out SaveSummary summary)
        {
            state = null;
            summary = new SaveSummary();
            if (string.IsNullOrWhiteSpace(json)) return false;
            if (!MiniJson.TryParse(json, out var root)) return false;
            var d = MiniJson.Obj(root);
            if (d == null || MiniJson.Str(d, "magic") != Magic) return false;
            var s = new GameState
            {
                Act = MathX.Clamp(MiniJson.Int(d, "act", 1), 1, 5),
                Checkpoint = MiniJson.Str(d, "checkpoint", "start"),
                Oil = MathX.Clamp(MiniJson.Float(d, "oil", Balance.StartOil), 0f, Balance.OilMax),
                Sanity = MathX.Clamp(MiniJson.Float(d, "sanity", Balance.SanityStart), 0f, Balance.SanityMax),
                LanternLit = MiniJson.Bool(d, "lantern", true),
                PlaySeconds = Math.Max(0, MiniJson.Num(d, "playSeconds", 0)),
                Act2Path = (EchoPath)MathX.Clamp(MiniJson.Int(d, "act2Path", 0), 0, 2),
                Answered = MathX.Clamp(MiniJson.Int(d, "answered", -1), -1, 1),
                Altar = (AltarChoice)MathX.Clamp(MiniJson.Int(d, "altar", 0), 0, 2),
                Ending = (EndingId)MathX.Clamp(MiniJson.Int(d, "ending", 0), 0, 3),
            };
            var flags = MiniJson.Arr(d.TryGetValue("flags", out var fo) ? fo : null);
            if (flags != null) foreach (var f in flags) if (f is string fs) s.Flags.Add(fs);
            var notes = MiniJson.Arr(d.TryGetValue("notes", out var no) ? no : null);
            if (notes != null) foreach (var n in notes) if (n is string ns) s.MarkNote(ns);
            var draws = MiniJson.Arr(d.TryGetValue("drawings", out var dr) ? dr : null);
            if (draws != null) foreach (var x in draws) if (x is double dx) s.AddDrawing((int)dx);
            var ints = MiniJson.Obj(d.TryGetValue("ints", out var io) ? io : null);
            if (ints != null) foreach (var kv in ints) if (kv.Value is double iv) s.Ints[kv.Key] = (int)iv;
            var st = MiniJson.Obj(d.TryGetValue("stats", out var so) ? so : null);
            if (st != null)
            {
                s.Listens = MiniJson.Int(st, "listens");
                s.Examines = MiniJson.Int(st, "examines");
                s.BellsRung = MiniJson.Int(st, "bells");
                s.OilRunOuts = MiniJson.Int(st, "runOuts");
                s.WorstTier = MiniJson.Int(st, "worstTier");
                s.ActWorstTier = MiniJson.Int(st, "actWorstTier");
                s.ActOilRunOuts = MiniJson.Int(st, "actRunOuts");
                s.BraziersLit = MiniJson.Int(st, "braziers");
            }
            summary.Slot = MiniJson.Int(d, "slot", 0);
            summary.Act = s.Act;
            summary.Checkpoint = s.Checkpoint;
            summary.PlaySeconds = s.PlaySeconds;
            summary.SavedUtc = MiniJson.Str(d, "savedUtc", "");
            summary.Drawings = s.DrawingCount;
            summary.Valid = true;
            state = s;
            return true;
        }

        public static string FormatPlayTime(double seconds)
        {
            var t = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return t.TotalHours >= 1
                ? string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", (int)t.TotalHours, t.Minutes, t.Seconds)
                : string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}", t.Minutes, t.Seconds);
        }
    }
}
