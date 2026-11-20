using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public sealed class AchievementDef
    {
        public readonly string Id;
        public readonly bool Hidden;     // description hidden until unlocked
        public string TitleKey => "ach." + Id + ".title";
        public string DescKey => "ach." + Id + ".desc";

        public AchievementDef(string id, bool hidden = false)
        {
            Id = id;
            Hidden = hidden;
        }
    }

    /// <summary>
    /// Local achievements (Steam later through a bridge with the same ids).
    /// The profile is global: it survives new games and slot deletion.
    /// </summary>
    public sealed class AchievementProfile
    {
        public static readonly AchievementDef[] All =
        {
            new AchievementDef("lullaby_bells"),
            new AchievementDef("starboard_ring"),
            new AchievementDef("act1"),
            new AchievementDef("carving"),
            new AchievementDef("follow_echo"),
            new AchievementDef("follow_mimic", true),
            new AchievementDef("act2"),
            new AchievementDef("three_clocks"),
            new AchievementDef("family_tree"),
            new AchievementDef("answered"),
            new AchievementDef("silent", true),
            new AchievementDef("act3"),
            new AchievementDef("beam"),
            new AchievementDef("braziers"),
            new AchievementDef("cassette"),
            new AchievementDef("act4"),
            new AchievementDef("ending_answer", true),
            new AchievementDef("ending_surface", true),
            new AchievementDef("ending_undertow", true),
            new AchievementDef("four_drawings"),
            new AchievementDef("all_drawings"),
            new AchievementDef("all_notes"),
            new AchievementDef("listener"),
            new AchievementDef("steady"),
            new AchievementDef("never_dark"),
            new AchievementDef("brink", true),
            new AchievementDef("photographer"),
            new AchievementDef("isabel", true),
        };

        public const int ListenerCount = 25;

        public static AchievementDef Find(string id) => All.FirstOrDefault(a => a.Id == id);

        readonly HashSet<string> _unlocked = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<int> EndingsSeen = new HashSet<int>();
        public bool FinishedOnce;
        public int HighestActReached = 1;

        public bool IsUnlocked(string id) => _unlocked.Contains(id);
        public int UnlockedCount => _unlocked.Count;
        public IEnumerable<string> Unlocked => _unlocked;

        /// <summary>Returns true only the first time (so the toast shows once).</summary>
        public bool Unlock(string id)
        {
            if (Find(id) == null) return false;
            return _unlocked.Add(id);
        }

        public string Encode()
        {
            var d = new Dictionary<string, object>
            {
                ["unlocked"] = _unlocked.OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToList(),
                ["endings"] = EndingsSeen.OrderBy(x => x).Select(x => (object)x).ToList(),
                ["finished"] = FinishedOnce,
                ["highestAct"] = HighestActReached,
            };
            return MiniJson.Serialize(d, true);
        }

        public static AchievementProfile Decode(string json)
        {
            var p = new AchievementProfile();
            if (string.IsNullOrEmpty(json) || !MiniJson.TryParse(json, out var root)) return p;
            var d = MiniJson.Obj(root);
            if (d == null) return p;
            var list = MiniJson.Arr(d.TryGetValue("unlocked", out var u) ? u : null);
            if (list != null) foreach (var x in list) if (x is string s) p.Unlock(s);
            var ends = MiniJson.Arr(d.TryGetValue("endings", out var e) ? e : null);
            if (ends != null) foreach (var x in ends) if (x is double n) p.EndingsSeen.Add((int)n);
            p.FinishedOnce = MiniJson.Bool(d, "finished");
            p.HighestActReached = MathX.Clamp(MiniJson.Int(d, "highestAct", 1), 1, 4);
            return p;
        }
    }
}
