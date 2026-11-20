using System;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    public enum WallMode { Solid, Bulwark, Railing, Hidden, None }

    public struct WallLook
    {
        public WallMode Mode;
        public string Mat;
        public string Trim;
        public WallLook(WallMode mode, string mat, string trim = null)
        {
            Mode = mode;
            Mat = mat;
            Trim = trim ?? mat;
        }
    }

    /// <summary>How an act wants its compiled level to look.</summary>
    public sealed class LevelStyle
    {
        public Func<RoomDef, string> FloorMat = r => "deck";
        public Func<RoomDef, bool> FloorVisible = r => true;    // false: collider only, the dressing draws the floor
        public Func<RoomDef, string> CeilMat = r => "panel";
        public Func<WallPiece, WallLook> Wall = w => new WallLook(WallMode.Solid, "panel");
        public Func<RoomDef, bool> Steps = r => r.IsRamp;
        public Func<OpeningPiece, string> FrameMat = o => "wood_dark";
        public float WallThickness = 0.2f;
        public float DoorHeight = 2.15f;
    }

    /// <summary>Builds floors, stairs, ceilings, walls, railings and door frames from a CompiledLevel.</summary>
    public static class GeometryBuilder
    {
        public static void Build(Kit kit, CompiledLevel lv, LevelStyle st)
        {
            foreach (var f in lv.Floors) Floor(kit, f, st);
            foreach (var c in lv.Ceilings) Ceiling(kit, c, st);
            foreach (var w in lv.Walls) Wall(kit, w, st);
            foreach (var o in lv.Openings) Opening(kit, lv, o, st);
        }

        static string Chunk(RoomDef r) => r != null ? r.Id : "level";

        static void Floor(Kit kit, FloorPiece f, LevelStyle st)
        {
            string mat = st.FloorMat(f.Room);
            if (mat == null) return;
            var r = f.Rect;
            string ch = Chunk(f.Room);
            if (!f.Sloped)
            {
                // rotated a quarter turn so the planks of a floor run along +z (fore and aft on the ship)
                if (st.FloorVisible(f.Room)) kit.Box(ch, mat, new Vector3(r.X + r.W * 0.5f, f.Y0 - 0.15f, r.Z + r.D * 0.5f), new Vector3(r.D, 0.3f, r.W), Quaternion.Euler(0f, 90f, 0f), false, 63, 2f);
                kit.Collider(new Vector3(r.X + r.W * 0.5f, f.Y0 - 0.3f, r.Z + r.D * 0.5f), new Vector3(r.W, 0.6f, r.D));
                return;
            }
            // sloped: smooth collider for the character, stepped visuals for stairs
            float run = f.AlongX ? r.W : r.D;
            float dy = f.Y1 - f.Y0;
            float len = Mathf.Sqrt(run * run + dy * dy);
            float ang = Mathf.Atan2(dy, run) * Mathf.Rad2Deg;
            var center = new Vector3(r.X + r.W * 0.5f, (f.Y0 + f.Y1) * 0.5f, r.Z + r.D * 0.5f);
            Quaternion rot = f.AlongX ? Quaternion.Euler(0f, 0f, ang) : Quaternion.Euler(-ang, 0f, 0f);
            var colSize = f.AlongX ? new Vector3(len, 0.5f, r.D) : new Vector3(r.W, 0.5f, len);
            kit.Collider(center + rot * new Vector3(0, -0.25f, 0), colSize, rot);
            if (st.Steps(f.Room))
            {
                float rise = 0.19f;
                int n = Mathf.Max(2, Mathf.CeilToInt(Mathf.Abs(dy) / rise));
                float lo = Mathf.Min(f.Y0, f.Y1) - 0.35f;
                for (int i = 0; i < n; i++)
                {
                    float t0 = i / (float)n, t1 = (i + 1) / (float)n;
                    float tm = (t0 + t1) * 0.5f;
                    float top = f.Y0 + dy * tm + Mathf.Abs(dy) / n * 0.5f;
                    Vector3 c, s;
                    if (f.AlongX)
                    {
                        c = new Vector3(r.X + r.W * tm, (top + lo) * 0.5f, r.Z + r.D * 0.5f);
                        s = new Vector3(r.W / n + 0.01f, top - lo, r.D);
                    }
                    else
                    {
                        c = new Vector3(r.X + r.W * 0.5f, (top + lo) * 0.5f, r.Z + r.D * tm);
                        s = new Vector3(r.W, top - lo, r.D / n + 0.01f);
                    }
                    kit.Box(ch, mat, c, s, Quaternion.identity, false, 63, 1.5f);
                }
            }
            else
            {
                var b = kit.B(ch, mat, 2f);
                Vector3 p00, p10, p11, p01;
                if (f.AlongX)
                {
                    p00 = new Vector3(r.X, f.Y0, r.Z); p10 = new Vector3(r.MaxX, f.Y1, r.Z);
                    p11 = new Vector3(r.MaxX, f.Y1, r.MaxZ); p01 = new Vector3(r.X, f.Y0, r.MaxZ);
                }
                else
                {
                    p00 = new Vector3(r.X, f.Y0, r.Z); p10 = new Vector3(r.MaxX, f.Y0, r.Z);
                    p11 = new Vector3(r.MaxX, f.Y1, r.MaxZ); p01 = new Vector3(r.X, f.Y1, r.MaxZ);
                }
                b.Quad(p00, p10, p11, p01);
                var down = Vector3.down * 0.3f;
                b.Quad(p10 + down, p00 + down, p01 + down, p11 + down);
            }
        }

        static void Ceiling(Kit kit, CeilPiece c, LevelStyle st)
        {
            string mat = st.CeilMat(c.Room);
            if (mat == null) return;
            var r = c.Rect;
            var b = kit.B(Chunk(c.Room), mat, 2f);
            Vector3 p00, p10, p11, p01;
            if (c.AlongX)
            {
                p00 = new Vector3(r.X, c.Y0, r.Z); p10 = new Vector3(r.MaxX, c.Y1, r.Z);
                p11 = new Vector3(r.MaxX, c.Y1, r.MaxZ); p01 = new Vector3(r.X, c.Y0, r.MaxZ);
            }
            else
            {
                p00 = new Vector3(r.X, c.Y0, r.Z); p10 = new Vector3(r.MaxX, c.Y0, r.Z);
                p11 = new Vector3(r.MaxX, c.Y1, r.MaxZ); p01 = new Vector3(r.X, c.Y1, r.MaxZ);
            }
            // facing down
            b.Quad(p01, p11, p10, p00);
            var up = Vector3.up * 0.25f;
            b.Quad(p00 + up, p10 + up, p11 + up, p01 + up);
        }

        static void Wall(Kit kit, WallPiece w, LevelStyle st)
        {
            var look = st.Wall(w);
            if (look.Mode == WallMode.None) return;
            var a = new Vector3(w.A.X, 0, w.A.Z);
            var bb = new Vector3(w.B.X, 0, w.B.Z);
            var n = new Vector3(w.Normal.X, 0, w.Normal.Z);
            float len = w.Length;
            if (len < 0.01f) return;
            float top = Mathf.Max(w.Top0, w.Top1);
            float bot = Mathf.Min(w.Bot0, w.Bot1);
            float t = st.WallThickness;
            bool partition = w.Kind == EdgeKind.Partition;
            // collider: always at least 1.15 m above the walking surface so nobody falls off an edge
            float colTop = Mathf.Max(top, Mathf.Max(w.Bot0, w.Bot1) + 1.15f);
            var mid = (a + bb) * 0.5f;
            var dir = (bb - a).normalized;
            var rot = Quaternion.LookRotation(dir, Vector3.up);
            Vector3 offset = partition ? Vector3.zero : -n * (t * 0.5f);
            kit.Collider(mid + offset + Vector3.up * ((colTop + bot) * 0.5f), new Vector3(t, colTop - bot, len), rot);
            string ch = Chunk(w.Room);
            switch (look.Mode)
            {
                case WallMode.Solid:
                    Slab(kit.B(ch, look.Mat, 2f), a, bb, n, w.Bot0, w.Bot1, w.Top0, w.Top1, t, partition);
                    break;
                case WallMode.Bulwark:
                {
                    Slab(kit.B(ch, look.Mat, 2f), a, bb, n, w.Bot0, w.Bot1, w.Top0 - 0.08f, w.Top1 - 0.08f, t, false);
                    // cap rail
                    var capC = mid - n * (t * 0.5f) + Vector3.up * ((w.Top0 + w.Top1) * 0.5f - 0.04f);
                    kit.Box(ch, look.Trim, capC, new Vector3(t + 0.12f, 0.08f, len + 0.02f), rot);
                    break;
                }
                case WallMode.Railing:
                {
                    var mb = kit.B(ch, look.Trim, 1f);
                    int posts = Mathf.Max(1, Mathf.RoundToInt(len / 1.1f));
                    for (int i = 0; i <= posts; i++)
                    {
                        float k = i / (float)posts;
                        var p = Vector3.Lerp(a, bb, k);
                        float y0 = Mathf.Lerp(w.Bot0, w.Bot1, k);
                        mb.Box(p + Vector3.up * (y0 + 0.52f) - n * 0.04f, new Vector3(0.07f, 1.04f, 0.07f), rot);
                    }
                    var ra = a + Vector3.up * (w.Bot0 + 1.04f) - n * 0.04f;
                    var rb = bb + Vector3.up * (w.Bot1 + 1.04f) - n * 0.04f;
                    mb.Tube(new[] { ra, rb }, 0.035f, 6, 0.5f);
                    mb.Tube(new[] { ra + Vector3.down * 0.5f, rb + Vector3.down * 0.5f }, 0.022f, 5, 0.5f);
                    break;
                }
            }
        }

        /// <summary>Wall slab between the edge line and an outward offset (or centred on the line for partitions).</summary>
        public static void Slab(MeshBuilder b, Vector3 a, Vector3 c, Vector3 n, float bot0, float bot1, float top0, float top1, float t, bool centered)
        {
            Vector3 inner = centered ? n * (t * 0.5f) : Vector3.zero;
            Vector3 outer = centered ? -n * (t * 0.5f) : -n * t;
            Vector3 A(float y) => a + inner + Vector3.up * y;
            Vector3 B(float y) => c + inner + Vector3.up * y;
            Vector3 Ao(float y) => a + outer + Vector3.up * y;
            Vector3 Bo(float y) => c + outer + Vector3.up * y;
            // decide orientation so the inner face looks along n
            var test = Vector3.Cross(c - a, Vector3.up);
            bool flip = Vector3.Dot(test, n) < 0f;
            if (!flip)
            {
                b.Quad(A(bot0), B(bot1), B(top1), A(top0));
                b.Quad(Bo(bot1), Ao(bot0), Ao(top0), Bo(top1));
                b.Quad(A(top0), B(top1), Bo(top1), Ao(top0));
                b.Quad(Ao(bot0), A(bot0), A(top0), Ao(top0));
                b.Quad(B(bot1), Bo(bot1), Bo(top1), B(top1));
            }
            else
            {
                b.Quad(B(bot1), A(bot0), A(top0), B(top1));
                b.Quad(Ao(bot0), Bo(bot1), Bo(top1), Ao(top0));
                b.Quad(B(top1), A(top0), Ao(top0), Bo(top1));
                b.Quad(A(bot0), Ao(bot0), Ao(top0), A(top0));
                b.Quad(Bo(bot1), B(bot1), B(top1), Bo(top1));
            }
        }

        static void Opening(Kit kit, CompiledLevel lv, OpeningPiece o, LevelStyle st)
        {
            if (o.Style == "gap" || o.Style == "hatch" || o.Style == "grate") return;
            float roomTop = Mathf.Max(o.A.Ceil ? o.A.Floor + o.A.CeilH : 0f, o.B.Ceil ? o.B.Floor + o.B.CeilH : 0f);
            bool enclosed = o.A.Ceil || o.B.Ceil;
            float y = o.Y;
            float doorH = o.Style == "arch" ? Mathf.Min(Mathf.Max(st.DoorHeight + 0.4f, o.Width * 0.9f), Mathf.Max(st.DoorHeight, roomTop - y - 0.3f)) : st.DoorHeight;
            var along = o.EdgeAlongX ? Vector3.right : Vector3.forward;
            var across = o.EdgeAlongX ? Vector3.forward : Vector3.right;
            var c = new Vector3(o.Center.X, y, o.Center.Z);
            float half = o.Width * 0.5f;
            string mat = st.FrameMat(o);
            string ch = o.A.Id;
            var rot = Quaternion.LookRotation(across, Vector3.up);
            // header wall above the opening
            if (enclosed && roomTop > y + doorH + 0.05f)
            {
                var wallMat = st.Wall(new WallPiece { Room = o.A, Other = o.B, Kind = EdgeKind.Partition }).Mat ?? mat;
                float h = roomTop - (y + doorH);
                kit.Box(ch, wallMat, c + Vector3.up * (doorH + h * 0.5f), new Vector3(o.Width + 0.02f, h, st.WallThickness), rot);
            }
            if (o.Style == "door")
            {
                kit.Box(ch, mat, c - along * (half - 0.06f) + Vector3.up * (doorH * 0.5f), new Vector3(0.12f, doorH, st.WallThickness + 0.08f), rot);
                kit.Box(ch, mat, c + along * (half - 0.06f) + Vector3.up * (doorH * 0.5f), new Vector3(0.12f, doorH, st.WallThickness + 0.08f), rot);
                kit.Box(ch, mat, c + Vector3.up * (doorH - 0.06f), new Vector3(o.Width, 0.14f, st.WallThickness + 0.1f), rot);
                kit.Box(ch, mat, c + Vector3.up * 0.02f, new Vector3(o.Width, 0.04f, st.WallThickness + 0.12f), rot);
            }
            else if (o.Style == "arch")
            {
                float postW = 0.28f;
                kit.Box(ch, mat, c - along * (half + postW * 0.3f) + Vector3.up * (doorH * 0.4f), new Vector3(postW, doorH * 0.8f, st.WallThickness + 0.18f), rot);
                kit.Box(ch, mat, c + along * (half + postW * 0.3f) + Vector3.up * (doorH * 0.4f), new Vector3(postW, doorH * 0.8f, st.WallThickness + 0.18f), rot);
                // voussoirs: a rough semicircle under the header
                int seg = 9;
                float rise = Mathf.Min(doorH * 0.2f, half);
                for (int i = 0; i < seg; i++)
                {
                    float k0 = i / (float)seg, k1 = (i + 1) / (float)seg;
                    float x0 = Mathf.Lerp(-half, half, k0), x1 = Mathf.Lerp(-half, half, k1);
                    float h0 = doorH - rise + rise * Mathf.Sqrt(Mathf.Max(0f, 1f - (x0 / half) * (x0 / half)));
                    float h1 = doorH - rise + rise * Mathf.Sqrt(Mathf.Max(0f, 1f - (x1 / half) * (x1 / half)));
                    float hm = Mathf.Min(h0, h1);
                    var pc = c + along * ((x0 + x1) * 0.5f) + Vector3.up * ((hm + doorH) * 0.5f + 0.06f);
                    kit.Box(ch, mat, pc, new Vector3(x1 - x0 + 0.02f, doorH - hm + 0.12f, st.WallThickness + 0.14f), rot);
                }
            }
        }
    }
}
