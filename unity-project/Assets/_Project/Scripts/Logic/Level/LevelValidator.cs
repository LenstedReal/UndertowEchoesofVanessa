using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public sealed class ValidationReport
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Log = new List<string>();
        public bool Ok => Errors.Count == 0;
        public override string ToString() => string.Join("\n", Errors.Select(e => "ERROR " + e).Concat(Warnings.Select(w => "WARN " + w)));
    }

    /// <summary>
    /// Static checks on content + a full walkthrough of every act on the compiled walk graph:
    /// the player starts at the spawn, can only walk through open doors, past opened blockers,
    /// and must reach every goal in order. If this passes, the act cannot soft-lock.
    /// </summary>
    public static class LevelValidator
    {
        public const float ReachXZ = 1.75f;

        public static ValidationReport ValidateStatic(ActDef act, CompiledLevel lv)
        {
            var rep = new ValidationReport();
            rep.Errors.AddRange(lv.Errors);
            rep.Warnings.AddRange(lv.Warnings);
            var ids = new HashSet<string>();
            foreach (var r in act.Rooms) if (!ids.Add(r.Id)) rep.Errors.Add("duplicate room id " + r.Id);
            foreach (var i in act.Items)
            {
                if (!ids.Add(i.Id)) rep.Errors.Add("duplicate id " + i.Id);
                var room = act.Room(i.Room);
                if (room == null) { rep.Errors.Add($"item {i.Id}: unknown room {i.Room}"); continue; }
                if (!room.Rect.Contains(i.Pos)) rep.Errors.Add($"item {i.Id} at {i.Pos} is outside its room {room.Id} {room.Rect}");
                if (i.Slots != null)
                {
                    foreach (var s in i.Slots)
                        if (!room.Rect.Expanded(-0.4f).Contains(s)) rep.Errors.Add($"crate {i.Id} slot {s} outside room interior");
                    if (i.Goal < 0 || i.Goal >= i.Slots.Length) rep.Errors.Add($"crate {i.Id} goal index out of range");
                }
                if (i.Kind == ItemKind.Clock && (i.Options == null || i.Answer < 0 || i.Answer >= i.Options.Length)) rep.Errors.Add($"dial {i.Id} answer out of range");
                if ((i.Kind == ItemKind.Bell || i.Kind == ItemKind.Crate || i.Kind == ItemKind.Lever || i.Kind == ItemKind.Clock || i.Kind == ItemKind.Mirror || i.Kind == ItemKind.Listen) && (i.Puzzle == null || !ActSession.Puzzles.ContainsKey(i.Puzzle)))
                    rep.Errors.Add($"item {i.Id}: missing or unknown puzzle '{i.Puzzle}'");
                if (i.Kind == ItemKind.Drawing && (i.Drawing < 1 || i.Drawing > 6)) rep.Errors.Add($"drawing {i.Id} has index {i.Drawing}");
            }
            var spawnRoom = act.RoomAt(act.Spawn.X, act.Spawn.Z);
            if (spawnRoom == null || spawnRoom.OpenFlag != null) rep.Errors.Add($"spawn {act.Spawn} not in an open room");
            foreach (var g in act.Goals)
                if (act.RoomAt(g.Pos.X, g.Pos.Z) == null) rep.Errors.Add($"goal {g.Flag} position {g.Pos} outside all rooms");
            foreach (var v in act.Voices)
            {
                if (v.TriggerRoom != null && act.Room(v.TriggerRoom) == null) rep.Errors.Add($"voice {v.Id}: unknown trigger room {v.TriggerRoom}");
            }
            foreach (var b in act.Blockers)
            {
                int cells = 0;
                foreach (var kv in lv.Grid.Cells)
                {
                    int ci = kv.Key % 100000, cj = kv.Key / 100000;
                    var c = lv.Grid.Center(ci, cj);
                    if (b.Rect.Contains(c) && kv.Value.Any(s => s.Alive)) cells++;
                }
                if (cells == 0) rep.Errors.Add($"blocker {b.Id} blocks no walkable cell");
            }
            int drawings = act.Items.Count(i => i.Kind == ItemKind.Drawing);
            rep.Log.Add($"act {act.Id}: {act.Rooms.Count} rooms, {act.Items.Count} items, {drawings} drawings, {lv.Floors.Count} floors, {lv.Walls.Count} walls, {lv.Openings.Count} openings");
            return rep;
        }

        static bool BlockedAt(ActDef act, GameState st, V2 c)
        {
            foreach (var b in act.Blockers)
                if (!st.Has(b.Until) && b.Rect.Contains(c)) return true;
            return false;
        }

        /// <summary>Breadth-first search over walkable surfaces from the surface under 'from'.</summary>
        public static HashSet<Surf> Reachable(CompiledLevel lv, GameState st, V2 from, float yHint)
        {
            var g = lv.Grid;
            var res = new HashSet<Surf>();
            var start = g.Standing(from.X, from.Z, yHint);
            if (start == null) return res;
            var pos = new Dictionary<Surf, (int, int)>();
            foreach (var kv in g.Cells)
                foreach (var s in kv.Value)
                    if (s.Alive) pos[s] = (kv.Key % 100000, kv.Key / 100000);
            var q = new Queue<Surf>();
            q.Enqueue(start);
            res.Add(start);
            int[] dx = { 1, -1, 0, 0 };
            int[] dz = { 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                var s = q.Dequeue();
                var (i, j) = pos[s];
                for (int d = 0; d < 4; d++)
                {
                    int ni = i + dx[d], nj = j + dz[d];
                    var n = g.Compatible(ni, nj, s.Y);
                    if (n == null || res.Contains(n)) continue;
                    if (g.Closed.Contains((s, n))) continue;
                    if (n.Room.OpenFlag != null && !st.Has(n.Room.OpenFlag)) continue;
                    if (BlockedAt(lv.Act, st, g.Center(ni, nj))) continue;
                    res.Add(n);
                    q.Enqueue(n);
                }
            }
            return res;
        }

        /// <summary>A reachable surface from which the item can be used (within reach, at a sensible height).</summary>
        public static Surf StandFor(CompiledLevel lv, HashSet<Surf> reach, ItemDef item)
        {
            var g = lv.Grid;
            var room = lv.Act.Room(item.Room);
            float floor = room != null ? room.FloorAt(item.Pos.X, item.Pos.Z) : 0f;
            float target = floor + item.Height;
            Surf best = null;
            float bd = float.MaxValue;
            foreach (var kv in g.Cells)
            {
                int ci = kv.Key % 100000, cj = kv.Key / 100000;
                var c = g.Center(ci, cj);
                float d = V2.Distance(c, item.Pos);
                if (d > ReachXZ) continue;
                foreach (var s in kv.Value)
                {
                    if (!s.Alive || !reach.Contains(s)) continue;
                    float eye = s.Y + 1.6f;
                    if (Math.Abs(eye - target) > 2.4f) continue;
                    if (d < bd) { bd = d; best = s; }
                }
            }
            return best;
        }

        public static float FloorY(ActDef act, V2 p)
        {
            var r = act.RoomAt(p.X, p.Z);
            return r != null ? r.FloorAt(p.X, p.Z) : 0f;
        }
    }

    /// <summary>Plays an act from start to finish using only legal moves. Options choose the story branches.</summary>
    public static class Walkthrough
    {
        public sealed class Options
        {
            public bool Act2FollowEcho = true;
            public bool Act3Answer = true;
            public bool AltarEcho = true;
        }

        public static ValidationReport Run(ActDef act, GameState state, Options opt = null)
        {
            opt = opt ?? new Options();
            var lv = LevelCompiler.Compile(act);
            var rep = LevelValidator.ValidateStatic(act, lv);
            var session = new ActSession(act, state);
            var here = act.Spawn;
            float hereY = LevelValidator.FloorY(act, here);
            int guard = 0;

            bool Use(string itemId, string why)
            {
                var item = act.Item(itemId);
                if (item == null) { rep.Errors.Add($"{why}: no item {itemId}"); return false; }
                var reach = LevelValidator.Reachable(lv, session.State, here, hereY);
                var stand = LevelValidator.StandFor(lv, reach, item);
                if (stand == null)
                {
                    rep.Errors.Add($"{why}: item {itemId} in {item.Room} is not reachable from {here} (reachable cells {reach.Count}, flags {string.Join(",", session.State.Flags.OrderBy(x => x))})");
                    return false;
                }
                var (si, sj) = FindIndex(lv, stand);
                here = lv.Grid.Center(si, sj);
                hereY = stand.Y;
                var r = session.Interact(itemId);
                rep.Log.Add($"  {itemId}: {r.Outcome}{(r.FlagsSet.Count > 0 ? " +" + string.Join(",", r.FlagsSet) : "")}");
                if (r.Outcome == Outcome.NeedMissing || r.Outcome == Outcome.None)
                {
                    rep.Errors.Add($"{why}: using {itemId} gave {r.Outcome}");
                    return false;
                }
                return true;
            }

            foreach (var goal in act.Goals)
            {
                if (++guard > 200) break;
                if (session.State.Has(goal.Flag)) continue;
                string why = $"act {act.Id} goal {goal.Flag}";
                bool ok = true;
                switch (goal.Flag)
                {
                    case "a1_music":
                    case "a2_bells":
                    {
                        string pz = goal.Flag == "a1_music" ? "a1" : "a2";
                        if (pz == "a2") ok &= Use("a2_listen", why);
                        else ok &= Use("a1_listen", why);
                        foreach (var idx in ActSession.Puzzles[pz].Sequence)
                        {
                            var bell = act.Items.First(i => i.Puzzle == pz && i.Kind == ItemKind.Bell && i.Index == idx);
                            ok &= Use(bell.Id, why);
                        }
                        break;
                    }
                    case "a1_crates":
                    case "a2_clear":
                    {
                        string pz = goal.Flag == "a1_crates" ? "a1c" : "a2x";
                        foreach (var c in act.Items.Where(i => i.Puzzle == pz && i.Kind == ItemKind.Crate))
                        {
                            int n = 0;
                            while (session.CrateSlot(c) != c.Goal && n++ < 6) ok &= Use(c.Id, why);
                        }
                        break;
                    }
                    case "a3_clocks":
                        foreach (var d in act.Items.Where(i => i.Puzzle == "a3" && i.Kind == ItemKind.Clock))
                        {
                            int n = 0;
                            while (session.DialValue(d) != d.Answer && n++ < 6) ok &= Use(d.Id, why);
                        }
                        break;
                    case "a4_m1":
                    case "a4_m2":
                    case "a4_m3":
                    case "a4_m4":
                    {
                        int k = goal.Flag[4] - '1';
                        var m = act.Items.First(i => i.Puzzle == "a4" && i.Kind == ItemKind.Mirror && i.Index == k);
                        int n = 0;
                        while (session.MirrorTurns(m) % 4 != m.Answer % 4 && n++ < 6) ok &= Use(m.Id, why);
                        break;
                    }
                    case "a2_left":
                        ok &= Use(opt.Act2FollowEcho ? "a2_exit_cove" : "a2_exit_gate", why);
                        break;
                    case "a3_choice":
                        ok &= Use("a3_mirror_main", why);
                        session.RecordAnswer(opt.Act3Answer);
                        rep.Log.Add("  answer: " + opt.Act3Answer);
                        break;
                    case "a3_left":
                        if (!session.State.Has("a3_left")) ok &= Use("a3_exit", why);
                        break;
                    case "a4_end":
                        ok &= Use(opt.AltarEcho ? "a4_choice_echo" : "a4_choice_mimic", why);
                        break;
                    default:
                    {
                        var item = act.Items.FirstOrDefault(i => i.Flag == goal.Flag && i.Kind != ItemKind.Bell)
                                   ?? act.Items.FirstOrDefault(i => i.Flag == goal.Flag);
                        if (item == null) { rep.Errors.Add($"{why}: nothing sets this flag"); ok = false; break; }
                        ok &= Use(item.Id, why);
                        break;
                    }
                }
                if (!ok) break;
                if (!session.State.Has(goal.Flag)) rep.Errors.Add($"{why}: flag not set after its steps");
            }
            if (act.EndFlag != null && !session.State.Has(act.EndFlag)) rep.Errors.Add($"act {act.Id} did not reach its end flag {act.EndFlag}");

            // every item must have been reachable at the end (optional ones too, as a warning)
            var finalReach = LevelValidator.Reachable(lv, session.State, act.Spawn, LevelValidator.FloorY(act, act.Spawn));
            foreach (var it in act.Items)
            {
                if (LevelValidator.StandFor(lv, finalReach, it) != null) continue;
                if (it.Kind == ItemKind.Exit || it.Kind == ItemKind.Boat || it.Kind == ItemKind.Choice) continue;
                if (it.Optional) rep.Warnings.Add($"optional item {it.Id} unreachable at the end of act {act.Id}");
                else rep.Errors.Add($"item {it.Id} unreachable at the end of act {act.Id}");
            }
            rep.Log.Add($"act {act.Id} end: {session.State}");
            return rep;
        }

        static (int, int) FindIndex(CompiledLevel lv, Surf s)
        {
            foreach (var kv in lv.Grid.Cells)
                if (kv.Value.Contains(s)) return (kv.Key % 100000, kv.Key / 100000);
            return (0, 0);
        }
    }
}
