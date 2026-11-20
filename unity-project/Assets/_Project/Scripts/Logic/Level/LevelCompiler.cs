using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public enum EdgeKind { Partition, Void, Drop, Retain }

    public sealed class FloorPiece
    {
        public RoomDef Room;
        public RectXZ Rect;
        public float Y0;        // height at the start of the ramp axis (min Z, or min X when AlongX)
        public float Y1;        // height at the end
        public bool AlongX;
        public bool Sloped => Math.Abs(Y0 - Y1) > 1e-3f;
        public float YAt(float x, float z)
        {
            if (!Sloped) return Y0;
            float t = AlongX ? (x - Rect.X) / Rect.W : (z - Rect.Z) / Rect.D;
            return Y0 + (Y1 - Y0) * MathX.Clamp01(t);
        }
    }

    public sealed class CeilPiece
    {
        public RoomDef Room;
        public RectXZ Rect;
        public float Y0;
        public float Y1;
        public bool AlongX;
    }

    /// <summary>A wall strip along a cell edge (merged). Heights interpolate linearly from A to B.</summary>
    public sealed class WallPiece
    {
        public RoomDef Room;
        public RoomDef Other;
        public V2 A;
        public V2 B;
        public float Bot0, Bot1, Top0, Top1;
        public V2 Normal;           // points into Room
        public EdgeKind Kind;
        public float Length => V2.Distance(A, B);
        public bool AlongX => Math.Abs(A.Z - B.Z) < 1e-4f;
    }

    public sealed class OpeningPiece
    {
        public DoorDef Door;
        public RoomDef A;
        public RoomDef B;
        public V2 Center;
        public float Width;
        public float Y;
        public bool EdgeAlongX;     // the opening lies on a line of constant Z
        public string Style;
    }

    public sealed class Surf
    {
        public RoomDef Room;
        public float Y;
        public bool Alive = true;
        public readonly List<RoomDef> Shared = new List<RoomDef>();
        public bool Covers(RoomDef r) => Room == r || Shared.Contains(r);
    }

    /// <summary>Cell grid with walkable surfaces per cell (several when floors are stacked).</summary>
    public sealed class LevelGrid
    {
        public const float Cell = 0.5f;
        public float MinX, MinZ;
        public int Nx, Nz;
        public readonly Dictionary<int, List<Surf>> Cells = new Dictionary<int, List<Surf>>();
        // walls between neighbouring surfaces: key = surf pair
        public readonly HashSet<(Surf, Surf)> Closed = new HashSet<(Surf, Surf)>();

        public int Key(int i, int j) => j * 100000 + i;
        public bool InRange(int i, int j) => i >= 0 && j >= 0 && i < Nx && j < Nz;
        public V2 Center(int i, int j) => new V2(MinX + (i + 0.5f) * Cell, MinZ + (j + 0.5f) * Cell);
        public (int, int) IndexOf(float x, float z) => ((int)Math.Floor((x - MinX) / Cell), (int)Math.Floor((z - MinZ) / Cell));

        public List<Surf> At(int i, int j) => Cells.TryGetValue(Key(i, j), out var l) ? l : null;

        public Surf Compatible(int i, int j, float y)
        {
            var l = At(i, j);
            if (l == null) return null;
            Surf best = null;
            float bd = float.MaxValue;
            foreach (var s in l)
            {
                if (!s.Alive) continue;
                float d = Math.Abs(s.Y - y);
                if (d <= LevelCompiler.Step && d < bd) { bd = d; best = s; }
            }
            return best;
        }

        /// <summary>Surface the player stands on at a point (closest to the given height).</summary>
        public Surf Standing(float x, float z, float yHint)
        {
            var (i, j) = IndexOf(x, z);
            var l = At(i, j);
            if (l == null) return null;
            Surf best = null;
            float bd = float.MaxValue;
            foreach (var s in l)
            {
                if (!s.Alive) continue;
                float d = Math.Abs(s.Y - yHint);
                if (d < bd) { bd = d; best = s; }
            }
            return best;
        }
    }

    public sealed class CompiledLevel
    {
        public ActDef Act;
        public LevelGrid Grid;
        public readonly List<FloorPiece> Floors = new List<FloorPiece>();
        public readonly List<CeilPiece> Ceilings = new List<CeilPiece>();
        public readonly List<WallPiece> Walls = new List<WallPiece>();
        public readonly List<OpeningPiece> Openings = new List<OpeningPiece>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly Dictionary<RoomDef, int> CellsPerRoom = new Dictionary<RoomDef, int>();
    }

    /// <summary>
    /// Turns room rectangles into 3D level geometry: floor slabs (with holes where stairs pass),
    /// ceilings, walls, railings, door openings, and a walk graph for validation.
    ///  - Overlapping rooms at the same height merge (the smaller room owns the cells, ramps always own theirs).
    ///  - A ramp passing under a floor cuts a hole in that floor where headroom would be too low.
    ///  - Between two rooms: open if both are open-air kinds, open at a ramp's two ends, open at an explicit
    ///    door; otherwise a wall. Around the walkable area: bulwarks, hull walls, facades or walls.
    /// </summary>
    public static class LevelCompiler
    {
        public const float Cell = LevelGrid.Cell;
        public const float Step = 0.36f;      // max height difference between neighbouring cells
        public const float Clear = 2.3f;      // headroom + slab thickness needed under a floor
        public const float Slab = 0.25f;
        public const float RailH = 1.05f;

        public static bool OpenKind(RoomKind k) => k == RoomKind.Deck || k == RoomKind.Grave || k == RoomKind.Street || k == RoomKind.Cove || k == RoomKind.Shore;

        public static float EdgeHeight(RoomDef r)
        {
            switch (r.Kind)
            {
                case RoomKind.Deck: return 1.1f;
                case RoomKind.Grave: return 7.5f;
                case RoomKind.Street: return 6.5f;
                case RoomKind.Cove: return 1.4f;
                case RoomKind.Shore: return 1.2f;
                case RoomKind.Hall: return 2.9f;
                case RoomKind.Temple: return 6f;
                default: return 2.6f;
            }
        }

        public static CompiledLevel Compile(ActDef act)
        {
            var lv = new CompiledLevel { Act = act };
            var g = new LevelGrid();
            lv.Grid = g;
            float minX = act.Rooms.Min(r => r.Rect.X) - 1f, minZ = act.Rooms.Min(r => r.Rect.Z) - 1f;
            float maxX = act.Rooms.Max(r => r.Rect.MaxX) + 1f, maxZ = act.Rooms.Max(r => r.Rect.MaxZ) + 1f;
            g.MinX = (float)Math.Floor(minX);
            g.MinZ = (float)Math.Floor(minZ);
            g.Nx = (int)Math.Ceiling((maxX - g.MinX) / Cell);
            g.Nz = (int)Math.Ceiling((maxZ - g.MinZ) / Cell);

            // 1. rasterise rooms
            foreach (var r in act.Rooms)
            {
                int n = 0;
                for (int j = 0; j < g.Nz; j++)
                {
                    float cz = g.MinZ + (j + 0.5f) * Cell;
                    if (cz < r.Rect.Z || cz >= r.Rect.MaxZ) continue;
                    for (int i = 0; i < g.Nx; i++)
                    {
                        float cx = g.MinX + (i + 0.5f) * Cell;
                        if (cx < r.Rect.X || cx >= r.Rect.MaxX) continue;
                        int k = g.Key(i, j);
                        if (!g.Cells.TryGetValue(k, out var l)) g.Cells[k] = l = new List<Surf>();
                        l.Add(new Surf { Room = r, Y = r.FloorAt(cx, cz) });
                        n++;
                    }
                }
                if (n == 0) lv.Errors.Add($"room {r.Id} covers no cells");
                if (r.IsRamp && r.SlopeDegrees > 35f) lv.Errors.Add($"ramp {r.Id} too steep ({r.SlopeDegrees:0.0}°)");
                if (r.Ceil && r.CeilH < 2.1f) lv.Errors.Add($"room {r.Id} ceiling too low ({r.CeilH})");
            }

            // 2. resolve stacked / overlapping surfaces
            foreach (var kv in g.Cells)
            {
                var l = kv.Value;
                if (l.Count < 2) continue;
                l.Sort((a, b) => b.Y.CompareTo(a.Y));
                for (int x = 0; x < l.Count; x++)
                {
                    for (int y = x + 1; y < l.Count; y++)
                    {
                        var u = l[x];
                        var d = l[y];
                        if (!u.Alive || !d.Alive) continue;
                        float dy = u.Y - d.Y;
                        if (dy <= Step)
                        {
                            var win = Prefer(act, u, d);
                            var lose = win == u ? d : u;
                            lose.Alive = false;
                            if (!win.Shared.Contains(lose.Room)) win.Shared.Add(lose.Room);
                        }
                        else if (dy < Clear)
                        {
                            if (d.Room.IsRamp && !u.Room.IsRamp) u.Alive = false;
                            else if (u.Room.IsRamp && !d.Room.IsRamp) d.Alive = false;
                            else if (u.Room.IsRamp && d.Room.IsRamp)
                            {
                                if (u.Room.Rect.Area >= d.Room.Rect.Area) u.Alive = false; else d.Alive = false;
                            }
                            else
                            {
                                lv.Errors.Add($"rooms {u.Room.Id} and {d.Room.Id} overlap with {dy:0.00} m clearance");
                                d.Alive = false;
                            }
                        }
                    }
                }
            }
            foreach (var r in act.Rooms) lv.CellsPerRoom[r] = 0;
            foreach (var l in g.Cells.Values) foreach (var s in l) if (s.Alive) lv.CellsPerRoom[s.Room]++;
            foreach (var kv in lv.CellsPerRoom) if (kv.Value == 0) lv.Errors.Add($"room {kv.Key.Id} has no walkable cells left");

            // 3. edges
            var rawWalls = new List<WallPiece>();
            var doorHits = new Dictionary<DoorDef, List<(V2 a, V2 b, float y)>>();
            int[] dxs = { 1, -1, 0, 0 };
            int[] dzs = { 0, 0, 1, -1 };
            foreach (var kv in g.Cells)
            {
                int ci = kv.Key % 100000, cj = kv.Key / 100000;
                foreach (var s in kv.Value)
                {
                    if (!s.Alive) continue;
                    for (int d = 0; d < 4; d++)
                    {
                        int ni = ci + dxs[d], nj = cj + dzs[d];
                        EdgeLine(g, ci, cj, d, out var ea, out var eb, out var normal);
                        var ns = g.InRange(ni, nj) ? g.Compatible(ni, nj, s.Y) : null;
                        float sTop = TopFor(g, ci, cj, s);
                        if (ns != null)
                        {
                            if (ns.Room == s.Room) continue;
                            bool canonical = d == 0 || d == 2;
                            bool open = false;
                            DoorDef door = FindDoor(act, s.Room, ns.Room, ea, eb);
                            if (door != null)
                            {
                                open = true;
                                if (canonical)
                                {
                                    if (!doorHits.TryGetValue(door, out var hl)) doorHits[door] = hl = new List<(V2, V2, float)>();
                                    hl.Add((ea, eb, Math.Min(s.Y, ns.Y)));
                                }
                            }
                            else if (!HasExplicitDoor(act, s.Room, ns.Room))
                            {
                                if (OpenKind(s.Room.Kind) && OpenKind(ns.Room.Kind)) open = true;
                                else if (IsRampEnd(s.Room, d) || IsRampEnd(ns.Room, d)) open = true;
                            }
                            else
                            {
                                // explicit door exists elsewhere on this pair: ramp ends still open fully
                                if (IsRampEnd(s.Room, d) && IsRampEnd(ns.Room, d)) open = true;
                            }
                            if (open) continue;
                            g.Closed.Add((s, ns));
                            g.Closed.Add((ns, s));
                            if (!canonical) continue;
                            float nTop = TopFor(g, ni, nj, ns);
                            float top;
                            bool sc = s.Room.Ceil, nc = ns.Room.Ceil;
                            if (sc || nc) top = Math.Max(sc ? sTop : float.MinValue, nc ? nTop : float.MinValue);
                            else top = Math.Min(sTop, nTop);
                            float bot = Math.Min(s.Y, ns.Y);
                            rawWalls.Add(new WallPiece { Room = s.Room, Other = ns.Room, A = ea, B = eb, Bot0 = bot, Bot1 = bot, Top0 = top, Top1 = top, Normal = normal, Kind = EdgeKind.Partition });
                            continue;
                        }
                        // no compatible neighbour
                        Surf higher = null, lower = null;
                        var nl = g.InRange(ni, nj) ? g.At(ni, nj) : null;
                        if (nl != null)
                        {
                            foreach (var o in nl)
                            {
                                if (!o.Alive) continue;
                                if (o.Y > s.Y + Step && (higher == null || o.Y < higher.Y)) higher = o;
                                if (o.Y < s.Y - Step && (lower == null || o.Y > lower.Y)) lower = o;
                            }
                        }
                        if (higher != null && higher.Y - s.Y < 6f)
                        {
                            float top = s.Room.Ceil ? Math.Max(sTop, higher.Y) : higher.Y;
                            rawWalls.Add(new WallPiece { Room = s.Room, Other = higher.Room, A = ea, B = eb, Bot0 = s.Y, Bot1 = s.Y, Top0 = top, Top1 = top, Normal = normal, Kind = EdgeKind.Retain });
                        }
                        else if (lower != null && !s.Room.Ceil)
                        {
                            rawWalls.Add(new WallPiece { Room = s.Room, Other = lower.Room, A = ea, B = eb, Bot0 = s.Y, Bot1 = s.Y, Top0 = s.Y + RailH, Top1 = s.Y + RailH, Normal = normal, Kind = EdgeKind.Drop });
                        }
                        else
                        {
                            rawWalls.Add(new WallPiece { Room = s.Room, Other = null, A = ea, B = eb, Bot0 = s.Y, Bot1 = s.Y, Top0 = sTop, Top1 = sTop, Normal = normal, Kind = EdgeKind.Void });
                        }
                    }
                }
            }
            // ramps: wall bottoms follow the slope exactly at segment ends
            foreach (var w in rawWalls)
            {
                if (!w.Room.IsRamp) continue;
                float b0 = w.Room.FloorAt(w.A.X + w.Normal.X * 0.01f, w.A.Z + w.Normal.Z * 0.01f);
                float b1 = w.Room.FloorAt(w.B.X + w.Normal.X * 0.01f, w.B.Z + w.Normal.Z * 0.01f);
                float h0 = w.Top0 - w.Bot0, h1 = w.Top1 - w.Bot1;
                if (w.Kind == EdgeKind.Drop || w.Kind == EdgeKind.Void || (w.Kind == EdgeKind.Partition && w.Room.Ceil))
                {
                    w.Top0 = b0 + h0;
                    w.Top1 = b1 + h1;
                }
                w.Bot0 = b0;
                w.Bot1 = b1;
            }
            lv.Walls.AddRange(MergeWalls(rawWalls));

            // 4. openings
            foreach (var door in act.Doors)
            {
                var ra = act.Room(door.A);
                var rb = act.Room(door.B);
                if (ra == null || rb == null)
                {
                    lv.Errors.Add($"door {door.A}<->{door.B}: unknown room");
                    continue;
                }
                if (!doorHits.TryGetValue(door, out var hits) || hits.Count == 0)
                {
                    lv.Errors.Add($"door {door.A}<->{door.B} at {door.At} matches no shared edge");
                    continue;
                }
                bool alongX = Math.Abs(hits[0].a.Z - hits[0].b.Z) < 1e-4f;
                float lo = hits.Min(h => alongX ? Math.Min(h.a.X, h.b.X) : Math.Min(h.a.Z, h.b.Z));
                float hi = hits.Max(h => alongX ? Math.Max(h.a.X, h.b.X) : Math.Max(h.a.Z, h.b.Z));
                float line = alongX ? hits[0].a.Z : hits[0].a.X;
                var c = alongX ? new V2((lo + hi) * 0.5f, line) : new V2(line, (lo + hi) * 0.5f);
                if (hi - lo < 0.9f) lv.Errors.Add($"door {door.A}<->{door.B} opening only {hi - lo:0.00} m wide");
                lv.Openings.Add(new OpeningPiece { Door = door, A = ra, B = rb, Center = c, Width = hi - lo, Y = hits.Min(h => h.y), EdgeAlongX = alongX, Style = door.Style });
            }

            // 5. floors & ceilings
            foreach (var r in act.Rooms)
            {
                var cells = new HashSet<(int, int)>();
                var ceilCells = new HashSet<(int, int)>();
                foreach (var kv in g.Cells)
                {
                    int ci = kv.Key % 100000, cj = kv.Key / 100000;
                    foreach (var s in kv.Value)
                    {
                        if (!s.Alive || s.Room != r) continue;
                        cells.Add((ci, cj));
                        if (r.Ceil)
                        {
                            float cy = s.Y + r.CeilH;
                            bool cut = false;
                            foreach (var o in kv.Value)
                            {
                                if (o == s || !o.Alive) continue;
                                if (o.Y < cy && o.Y + Clear > cy && o.Y < s.Y - Step) cut = true;
                                if (o.Y < cy && o.Y > s.Y + Step) cut = true; // stacked slab acts as ceiling
                            }
                            if (!cut) ceilCells.Add((ci, cj));
                        }
                    }
                }
                foreach (var rc in GreedyRects(cells))
                {
                    var rect = new RectXZ(g.MinX + rc.i0 * Cell, g.MinZ + rc.j0 * Cell, (rc.i1 - rc.i0 + 1) * Cell, (rc.j1 - rc.j0 + 1) * Cell);
                    lv.Floors.Add(MakeFloor(r, rect));
                }
                foreach (var rc in GreedyRects(ceilCells))
                {
                    var rect = new RectXZ(g.MinX + rc.i0 * Cell, g.MinZ + rc.j0 * Cell, (rc.i1 - rc.i0 + 1) * Cell, (rc.j1 - rc.j0 + 1) * Cell);
                    var f = MakeFloor(r, rect);
                    lv.Ceilings.Add(new CeilPiece { Room = r, Rect = rect, Y0 = f.Y0 + r.CeilH, Y1 = f.Y1 + r.CeilH, AlongX = f.AlongX });
                }
            }
            return lv;
        }

        static FloorPiece MakeFloor(RoomDef r, RectXZ rect)
        {
            if (!r.IsRamp) return new FloorPiece { Room = r, Rect = rect, Y0 = r.Floor, Y1 = r.Floor };
            if (r.RampAlongX)
                return new FloorPiece { Room = r, Rect = rect, Y0 = r.FloorAt(rect.X, 0), Y1 = r.FloorAt(rect.MaxX, 0), AlongX = true };
            return new FloorPiece { Room = r, Rect = rect, Y0 = r.FloorAt(0, rect.Z), Y1 = r.FloorAt(0, rect.MaxZ) };
        }

        static Surf Prefer(ActDef act, Surf a, Surf b)
        {
            if (a.Room.IsRamp != b.Room.IsRamp) return a.Room.IsRamp ? a : b;
            float aa = a.Room.Rect.Area, ba = b.Room.Rect.Area;
            if (Math.Abs(aa - ba) > 1e-3f) return aa < ba ? a : b;
            return act.Rooms.IndexOf(a.Room) <= act.Rooms.IndexOf(b.Room) ? a : b;
        }

        /// <summary>True when moving in direction d leaves the ramp through one of its two ends.</summary>
        static bool IsRampEnd(RoomDef r, int d)
        {
            if (!r.IsRamp) return false;
            return r.RampAlongX ? d == 0 || d == 1 : d == 2 || d == 3;
        }

        static void EdgeLine(LevelGrid g, int i, int j, int d, out V2 a, out V2 b, out V2 normal)
        {
            float x0 = g.MinX + i * Cell, x1 = x0 + Cell, z0 = g.MinZ + j * Cell, z1 = z0 + Cell;
            switch (d)
            {
                case 0: a = new V2(x1, z0); b = new V2(x1, z1); normal = new V2(-1, 0); break;
                case 1: a = new V2(x0, z0); b = new V2(x0, z1); normal = new V2(1, 0); break;
                case 2: a = new V2(x0, z1); b = new V2(x1, z1); normal = new V2(0, -1); break;
                default: a = new V2(x0, z0); b = new V2(x1, z0); normal = new V2(0, 1); break;
            }
        }

        static float TopFor(LevelGrid g, int i, int j, Surf s)
        {
            float top = s.Room.Ceil ? s.Y + s.Room.CeilH : s.Y + EdgeHeight(s.Room);
            var l = g.At(i, j);
            if (l != null)
            {
                foreach (var o in l)
                {
                    if (o == s || !o.Alive) continue;
                    if (o.Y > s.Y + Step && o.Y - Slab < top) top = Math.Max(s.Y + 2.0f, o.Y - Slab);
                }
            }
            return top;
        }

        static bool HasExplicitDoor(ActDef act, RoomDef a, RoomDef b)
        {
            foreach (var d in act.Doors)
                if ((d.A == a.Id && d.B == b.Id) || (d.A == b.Id && d.B == a.Id)) return true;
            return false;
        }

        static DoorDef FindDoor(ActDef act, RoomDef a, RoomDef b, V2 ea, V2 eb)
        {
            foreach (var d in act.Doors)
            {
                if (!((d.A == a.Id && d.B == b.Id) || (d.A == b.Id && d.B == a.Id))) continue;
                bool alongX = Math.Abs(ea.Z - eb.Z) < 1e-4f;
                var mid = new V2((ea.X + eb.X) * 0.5f, (ea.Z + eb.Z) * 0.5f);
                float perp = alongX ? Math.Abs(mid.Z - d.At.Z) : Math.Abs(mid.X - d.At.X);
                float along = alongX ? Math.Abs(mid.X - d.At.X) : Math.Abs(mid.Z - d.At.Z);
                if (perp <= 1.6f && along <= d.Width * 0.5f) return d;
            }
            return null;
        }

        struct RectIdx { public int i0, j0, i1, j1; }

        static List<RectIdx> GreedyRects(HashSet<(int, int)> cells)
        {
            var res = new List<RectIdx>();
            var left = new HashSet<(int, int)>(cells);
            var order = cells.OrderBy(c => c.Item2).ThenBy(c => c.Item1).ToList();
            foreach (var c in order)
            {
                if (!left.Contains(c)) continue;
                int i0 = c.Item1, j0 = c.Item2, i1 = i0;
                while (left.Contains((i1 + 1, j0))) i1++;
                int j1 = j0;
                while (true)
                {
                    bool full = true;
                    for (int i = i0; i <= i1; i++) if (!left.Contains((i, j1 + 1))) { full = false; break; }
                    if (!full) break;
                    j1++;
                }
                for (int j = j0; j <= j1; j++) for (int i = i0; i <= i1; i++) left.Remove((i, j));
                res.Add(new RectIdx { i0 = i0, j0 = j0, i1 = i1, j1 = j1 });
            }
            return res;
        }

        static List<WallPiece> MergeWalls(List<WallPiece> raw)
        {
            var res = new List<WallPiece>();
            var groups = raw.GroupBy(w => (w.Room, w.Other, w.Kind, w.Normal.X, w.Normal.Z, line: w.AlongX ? Round(w.A.Z) : Round(w.A.X)));
            foreach (var grp in groups)
            {
                bool alongX = grp.First().AlongX;
                var list = grp.OrderBy(w => alongX ? Math.Min(w.A.X, w.B.X) : Math.Min(w.A.Z, w.B.Z)).Select(w => Normalize(w, alongX)).ToList();
                WallPiece cur = null;
                foreach (var w in list)
                {
                    if (cur != null && Contiguous(cur, w, alongX))
                    {
                        cur.B = w.B;
                        cur.Bot1 = w.Bot1;
                        cur.Top1 = w.Top1;
                        continue;
                    }
                    if (cur != null) res.Add(cur);
                    cur = w;
                }
                if (cur != null) res.Add(cur);
            }
            return res;
        }

        static float Round(float v) => (float)Math.Round(v * 1000f) / 1000f;

        static WallPiece Normalize(WallPiece w, bool alongX)
        {
            float sa = alongX ? w.A.X : w.A.Z, sb = alongX ? w.B.X : w.B.Z;
            if (sa <= sb) return w;
            return new WallPiece { Room = w.Room, Other = w.Other, A = w.B, B = w.A, Bot0 = w.Bot1, Bot1 = w.Bot0, Top0 = w.Top1, Top1 = w.Top0, Normal = w.Normal, Kind = w.Kind };
        }

        static bool Contiguous(WallPiece a, WallPiece b, bool alongX)
        {
            float ae = alongX ? a.B.X : a.B.Z, bs = alongX ? b.A.X : b.A.Z;
            if (Math.Abs(ae - bs) > 1e-3f) return false;
            if (Math.Abs(a.Bot1 - b.Bot0) > 0.02f || Math.Abs(a.Top1 - b.Top0) > 0.02f) return false;
            float la = a.Length, lb = b.Length;
            if (la < 1e-4f || lb < 1e-4f) return true;
            float sa = (a.Bot1 - a.Bot0) / la, sb = (b.Bot1 - b.Bot0) / lb;
            float ta = (a.Top1 - a.Top0) / la, tb = (b.Top1 - b.Top0) / lb;
            return Math.Abs(sa - sb) < 0.01f && Math.Abs(ta - tb) < 0.01f;
        }
    }
}
