using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Act III — Port Saint-Clair, frozen at 11:40 in 1724. Knee-deep water in timber-framed streets,
    /// the clock tower, the governor's house with its mirror hall, and the stairs down into the sea.
    /// </summary>
    public sealed class Act3Dressing : IActDressing
    {
        Rng _rng;
        readonly List<WaterSurface> _streetWater = new List<WaterSurface>();
        WaterSurface _sea;
        static Quaternion Yaw(float d) => Quaternion.Euler(0, d, 0);
        static bool Building(RoomDef r) => r.Ceil;

        public LevelStyle Style(World w)
        {
            var st = new LevelStyle();
            st.FloorMat = r =>
            {
                if (r.Kind == RoomKind.Street) return r.IsRamp ? "stone" : "cobble";
                switch (r.Surface)
                {
                    case Surface.Wood: return "wood";
                    case Surface.Rug: return "wood";
                    default: return "stone";
                }
            };
            st.CeilMat = r =>
            {
                if (r.Id == "a3_customs" || r.Id == "a3_tavern" || r.Id == "a3_ware" || r.Id == "a3_tower") return "wood_dark";
                return "plaster";
            };
            st.FrameMat = o => o.Style == "arch" ? "stone" : "wood_dark";
            st.Wall = wp =>
            {
                var r = wp.Room;
                var o = wp.Other;
                if (r.Id == "a3_sea") return new WallLook(WallMode.Railing, "stone", "stone");
                if (r.Kind == RoomKind.Street && (o == null || o.Kind == RoomKind.Street)) return new WallLook(WallMode.Hidden, "plaster");
                bool stoneBuilding = IsStone(r) || (o != null && IsStone(o));
                if (r.Id == "a3_mirror" || (o != null && o.Id == "a3_mirror")) return new WallLook(WallMode.Solid, "temple");
                return new WallLook(WallMode.Solid, stoneBuilding ? "stone" : "plaster");
            };
            st.Steps = r => r.IsRamp;
            return st;
        }

        static bool IsStone(RoomDef r) => r.Id == "a3_chapel" || r.Id == "a3_tower" || r.Id == "a3_ware" || r.Id.StartsWith("a3_m") || r.Id == "a3_study" || r.Id == "a3_gallery" || r.Id == "a3_marisol";

        public void Dress(World w, GameState state)
        {
            _rng = new Rng(3171);
            var rng = _rng;
            bool drained = state.Has("a3_clocks");
            float wl = drained ? -0.3f : Act3Content.WaterLevel;
            _sea = WaterSurface.CreateSea(w.Root, new Vector3(10f, 0f, 20f), 360f, drained ? -0.9f : Act3Content.WaterLevel - 0.02f, 80);
            _sea.Swell = 0.06f;
            w.Waters.Add(_sea);
            foreach (var r in w.Act.Rooms)
            {
                if (float.IsNaN(r.Water)) continue;
                var ws = WaterSurface.CreateFlat(w.Root, "water_" + r.Id, r.Rect, wl, true);
                _streetWater.Add(ws);
                w.Waters.Add(ws);
            }
            Houses(w);
            BuildingShells(w);
            Harbour(w);
            Street(w);
            Customs(w);
            Tavern(w);
            Warehouse(w);
            Chapel(w);
            Square(w);
            House(w);
            Tower(w);
            Mansion(w);
            SeaStairs(w, state);
            Footprints(w);
        }

        // ------------------------------------------------------------------ the town around the streets
        void Houses(World w)
        {
            var rng = _rng;
            var look = new WreckKit.HouseLook();
            foreach (var wp in w.Level.Walls)
            {
                if (wp.Kind != EdgeKind.Void || wp.Room.Kind != RoomKind.Street || wp.Room.IsRamp) continue;
                if (wp.Room.Id == "a3_dock" && wp.AlongX && wp.A.Z < 0.5f) continue;   // the sea side of the dock
                var a = new Vector3(wp.A.X, 0, wp.A.Z);
                var b = new Vector3(wp.B.X, 0, wp.B.Z);
                float len = Vector3.Distance(a, b);
                var u = (b - a) / len;
                var n = new Vector3(wp.Normal.X, 0, wp.Normal.Z);
                float s = 0f;
                while (s < len - 0.5f)
                {
                    float wdt = Mathf.Min(rng.Range(4f, 7f), len - s);
                    if (len - s - wdt < 2f) wdt = len - s;
                    var fa = a + u * s - n * 0.02f;
                    var fb = a + u * (s + wdt) - n * 0.02f;
                    float depth = rng.Range(6f, 8f);
                    // shrink the house until it stays clear of every room behind it
                    for (int k = 0; k < 8; k++)
                    {
                        var c = (fa + fb) * 0.5f - n * (depth * 0.5f + 0.1f);
                        var half = new Vector3(Mathf.Abs(u.x) * wdt * 0.5f + Mathf.Abs(n.x) * depth * 0.5f, 0, Mathf.Abs(u.z) * wdt * 0.5f + Mathf.Abs(n.z) * depth * 0.5f);
                        var rect = new RectXZ(c.x - half.x + 0.05f, c.z - half.z + 0.05f, half.x * 2f - 0.1f, half.z * 2f - 0.1f);
                        bool hit = false;
                        foreach (var r in w.Act.Rooms) if (r.Rect.Overlaps(rect)) { hit = true; break; }
                        if (!hit) break;
                        depth *= 0.6f;
                    }
                    if (depth > 1.5f) WreckKit.House(w, "town", fa, fb, n, 0f, rng.Range(6.5f, 10f), depth, rng, look);
                    s += wdt;
                }
            }
        }

        /// <summary>Upper storeys and roofs over the rooms the player walks through.</summary>
        void BuildingShells(World w)
        {
            var rng = _rng;
            foreach (var r in w.Act.Rooms)
            {
                if (!Building(r)) continue;
                float top = r.Floor + r.CeilH + 0.25f;
                float total;
                string mat = IsStone(r) ? "stone" : "plaster";
                switch (r.Id)
                {
                    case "a3_tower": total = 23f; break;
                    case "a3_chapel": total = 8.5f; break;
                    case "a3_mirror": total = 9f; break;
                    case "a3_mhall": case "a3_gallery": total = 9f; break;
                    case "a3_study": case "a3_marisol": total = 9f; break;
                    default: total = Mathf.Max(top + 2.6f, rng.Range(6.5f, 8.5f)); break;
                }
                var rc = r.Rect;
                var c = new Vector3(rc.X + rc.W * 0.5f, (top + total) * 0.5f, rc.Z + rc.D * 0.5f);
                w.Kit.Box("shells", mat, c, new Vector3(rc.W + 0.4f, total - top, rc.D + 0.4f), Quaternion.identity, false, 63 & ~8, 2f);
                // windows on the upper storey all round
                for (float y = top + 0.9f; y < total - 1f; y += 2.8f)
                {
                    WindowRow(w, new Vector3(rc.X - 0.21f, y, rc.Z), new Vector3(rc.X - 0.21f, y, rc.MaxZ), Vector3.left, rng);
                    WindowRow(w, new Vector3(rc.MaxX + 0.21f, y, rc.Z), new Vector3(rc.MaxX + 0.21f, y, rc.MaxZ), Vector3.right, rng);
                    WindowRow(w, new Vector3(rc.X, y, rc.Z - 0.21f), new Vector3(rc.MaxX, y, rc.Z - 0.21f), Vector3.back, rng);
                    WindowRow(w, new Vector3(rc.X, y, rc.MaxZ + 0.21f), new Vector3(rc.MaxX, y, rc.MaxZ + 0.21f), Vector3.forward, rng);
                }
                if (r.Id == "a3_tower") continue;
                // pitched roof along the long side
                bool alongX = rc.W >= rc.D;
                float half = (alongX ? rc.D : rc.W) * 0.5f + 0.4f;
                float ridgeH = Mathf.Clamp(half * 0.8f, 1.5f, 4.5f);
                var rb = w.Kit.B("shells", "roof", 1.4f);
                Vector3 e0a, e0b, e1a, e1b, ra, rbb;
                if (alongX)
                {
                    e0a = new Vector3(rc.X - 0.4f, total, rc.Z - 0.4f); e0b = new Vector3(rc.MaxX + 0.4f, total, rc.Z - 0.4f);
                    e1a = new Vector3(rc.X - 0.4f, total, rc.MaxZ + 0.4f); e1b = new Vector3(rc.MaxX + 0.4f, total, rc.MaxZ + 0.4f);
                    ra = new Vector3(rc.X - 0.4f, total + ridgeH, rc.Z + rc.D * 0.5f); rbb = new Vector3(rc.MaxX + 0.4f, total + ridgeH, rc.Z + rc.D * 0.5f);
                    rb.Quad(e0a, e0b, rbb, ra);
                    rb.Quad(e1b, e1a, ra, rbb);
                }
                else
                {
                    e0a = new Vector3(rc.X - 0.4f, total, rc.Z - 0.4f); e0b = new Vector3(rc.X - 0.4f, total, rc.MaxZ + 0.4f);
                    e1a = new Vector3(rc.MaxX + 0.4f, total, rc.Z - 0.4f); e1b = new Vector3(rc.MaxX + 0.4f, total, rc.MaxZ + 0.4f);
                    ra = new Vector3(rc.X + rc.W * 0.5f, total + ridgeH, rc.Z - 0.4f); rbb = new Vector3(rc.X + rc.W * 0.5f, total + ridgeH, rc.MaxZ + 0.4f);
                    rb.Quad(e0b, e0a, ra, rbb);
                    rb.Quad(e1a, e1b, rbb, ra);
                }
                var gb = w.Kit.B("shells", mat, 2f);
                if (alongX)
                {
                    gb.Tri(e0a, ra, e1a); gb.Tri(e1a, ra, e0a);
                    gb.Tri(e0b, e1b, rbb); gb.Tri(e1b, e0b, rbb);
                }
                else
                {
                    gb.Tri(e0a, e1a, ra); gb.Tri(e1a, e0a, ra);
                    gb.Tri(e0b, rbb, e1b); gb.Tri(e1b, rbb, e0b);
                }
            }
        }

        void WindowRow(World w, Vector3 a, Vector3 b, Vector3 n, Rng rng)
        {
            float len = Vector3.Distance(a, b);
            int cols = Mathf.Max(1, Mathf.FloorToInt(len / 2.2f));
            var u = (b - a) / Mathf.Max(0.01f, len);
            var rot = Quaternion.LookRotation(n, Vector3.up);
            for (int i = 0; i < cols; i++)
            {
                if (rng.Chance(0.25f)) continue;
                var c = a + u * ((i + 0.5f) / cols * len) + Vector3.up * 0.6f + n * 0.01f;
                w.Kit.Box("shells", rng.Chance(0.05f) ? "glow_warm" : "glass", c, new Vector3(0.7f, 1.1f, 0.02f), rot);
                w.Kit.Box("shells", "wood_dark", c + n * 0.03f + Vector3.down * 0.58f, new Vector3(0.85f, 0.07f, 0.1f), rot);
                w.Kit.Box("shells", "wood_dark", c + n * 0.02f, new Vector3(0.04f, 1.1f, 0.03f), rot);
            }
        }

        // ------------------------------------------------------------------ the harbour
        void Harbour(World w)
        {
            var rng = _rng;
            // quay wall and the ruined pier running out into the flood
            w.Kit.Box("harbour", "stone", new Vector3(9f, -1.2f, -0.6f), new Vector3(22f, 2.4f, 1.2f));
            for (int i = 0; i < 14; i++)
            {
                float z = -2f - i * 1.6f;
                if (i > 9 && rng.Chance(0.5f)) continue;
                w.Kit.Box("harbour", "deck", new Vector3(8f + rng.Range(-0.1f, 0.1f), 0.32f - i * 0.02f, z), new Vector3(3.4f - (i > 8 ? rng.Range(0f, 1.6f) : 0f), 0.08f, 1.4f), Yaw(rng.Range(-3f, 3f)));
                foreach (float x in new[] { 6.4f, 9.6f })
                    w.Kit.B("harbour", "wood_dark").Cylinder(new Vector3(x, -2.5f, z), Quaternion.identity, 3.0f + rng.Range(-0.3f, 0.6f), 0.14f, 0.12f, 8);
            }
            // bollards with rotted hawsers along the quay edge (inside the dock, against the edge)
            foreach (float x in new[] { 1.6f, 15.8f })
            {
                var bp = new PropMesh();
                bp["darkmetal"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.16f, 0f), new Vector2(0.14f, 0.4f), new Vector2(0.2f, 0.5f), new Vector2(0.18f, 0.6f), new Vector2(0f, 0.62f) }, 12);
                DressUtil.Bake(w, "harbour", bp, new Vector3(x, 0f, 0.7f), Yaw(0), new Vector3(0.4f, 0.62f, 0.4f));
                w.Kit.B("harbour", "rope", 0.3f).Rope(new Vector3(x, 0.45f, 0.7f), new Vector3(x + (x < 8 ? -1.5f : 1.5f), Act3Content.WaterLevel - 0.1f, -1.6f), 0.2f, 0.035f, 8);
            }
            // ships of 1724, still at anchor in the fog
            foreach (var (pos, yaw, sc) in new[] { (new Vector3(-14f, -1.2f, -22f), 70f, 1.4f), (new Vector3(30f, -1.2f, -30f), -60f, 1.6f) })
            {
                var hull = new PropMesh();
                var b = hull["hull_dark"];
                for (int i = 0; i < 12; i++)
                {
                    float t0 = i / 12f, t1 = (i + 1) / 12f;
                    float w0 = Mathf.Sin(t0 * Mathf.PI) * 3.2f + 0.3f, w1 = Mathf.Sin(t1 * Mathf.PI) * 3.2f + 0.3f;
                    float z0 = (t0 - 0.5f) * 22f, z1 = (t1 - 0.5f) * 22f;
                    b.Quad(new Vector3(-w1, 0, z1), new Vector3(-w0, 0, z0), new Vector3(-w0, 3.5f, z0), new Vector3(-w1, 3.5f, z1));
                    b.Quad(new Vector3(w0, 0, z0), new Vector3(w1, 0, z1), new Vector3(w1, 3.5f, z1), new Vector3(w0, 3.5f, z0));
                }
                hull["wood_dark"].Cylinder(new Vector3(0, 3f, -3f), Quaternion.identity, 16f, 0.3f, 0.18f, 10);
                hull["wood_dark"].Cylinder(new Vector3(0, 3f, 5f), Quaternion.identity, 18f, 0.32f, 0.2f, 10);
                hull["wood_dark"].Cylinder(new Vector3(-5f, 13f, 5f), Quaternion.Euler(0, 0, -90), 10f, 0.15f, 0.12f, 8);
                hull.BakeInto(w.Kit, "harbour", pos, Yaw(yaw), sc);
            }
            // the harbour board (the act's first note) is lit by a single lamp on the quay
            DressUtil.Lantern(w, "dock_lamp", new Vector3(12.9f, 2.6f, 8.6f), true, 240f, 8f, true, false, 1.2f);
            var post = new PropMesh();
            post["wood_dark"].Box(new Vector3(0, 1.4f, 0), new Vector3(0.14f, 2.8f, 0.14f), Quaternion.identity);
            post["wood_dark"].Box(new Vector3(-0.25f, 2.75f, 0), new Vector3(0.55f, 0.08f, 0.08f), Quaternion.identity);
            DressUtil.Bake(w, "harbour", post, new Vector3(13.4f, 0f, 8.6f), Yaw(0), new Vector3(0.18f, 2.8f, 0.18f));
            Props.RopeCoil(0.4f).BakeInto(w.Kit, "harbour", new Vector3(2.4f, 0f, 3.2f), Yaw(0));
            DressUtil.Bake(w, "harbour", Props.Barrel(0.9f, 0.3f), new Vector3(16.8f, 0f, 14.5f), Yaw(20), new Vector3(0.6f, 0.9f, 0.6f));
            DressUtil.Bake(w, "harbour", Props.Barrel(0.9f, 0.3f), new Vector3(16.9f, 0f, 15.3f), Yaw(70), new Vector3(0.6f, 0.9f, 0.6f));
            DressUtil.Bake(w, "harbour", Props.Crate(new Vector3(0.9f, 0.8f, 0.9f)), new Vector3(1.1f, 0f, 15.8f), Yaw(8), new Vector3(0.9f, 0.8f, 0.9f));
        }

        // ------------------------------------------------------------------ street furniture
        void Street(World w)
        {
            var rng = _rng;
            // wall lamps on iron brackets, all dead since 1724
            for (float z = 20f; z < 94f; z += 9f)
            {
                foreach (float x in new[] { 5.02f, 10.98f })
                {
                    var p = new Vector3(x, 0f, z + (x < 8f ? 0f : 4.5f));
                    if (w.Act.RoomAt(p.x + (x < 8f ? -0.4f : 0.4f), p.z) != null && w.Act.RoomAt(p.x + (x < 8f ? -0.4f : 0.4f), p.z).Kind != RoomKind.Street) continue;
                    var n = x < 8f ? Vector3.right : Vector3.left;
                    w.Kit.Box("street", "iron", p + n * 0.3f + Vector3.up * 3.1f, new Vector3(Mathf.Abs(n.x) * 0.6f + 0.03f, 0.03f, 0.03f));
                    DressUtil.Lantern(w, "street_lamp_" + z + "_" + x, p + n * 0.58f + Vector3.up * 3.05f, false, 0, 0, false, false, 0.8f);
                }
            }
            // a cart left in the water, a fallen sign, barrels
            var cart = new PropMesh();
            cart["wood"].Box(new Vector3(0, 0.7f, 0), new Vector3(1.2f, 0.12f, 2.0f), Quaternion.identity);
            cart["wood"].Box(new Vector3(-0.6f, 0.95f, 0), new Vector3(0.06f, 0.4f, 2.0f), Quaternion.identity);
            cart["wood"].Box(new Vector3(0.6f, 0.95f, 0), new Vector3(0.06f, 0.4f, 2.0f), Quaternion.identity);
            foreach (float s in new[] { -0.68f, 0.68f })
            {
                var ring = new List<Vector3>();
                for (int i = 0; i <= 16; i++) { float a = i / 16f * Mathf.PI * 2f; ring.Add(new Vector3(s, 0.45f + Mathf.Sin(a) * 0.45f, -0.3f + Mathf.Cos(a) * 0.45f)); }
                cart["wood_dark"].Tube(ring, 0.04f, 5, 0.3f);
            }
            cart["wood"].Box(new Vector3(0, 0.6f, 1.6f), new Vector3(0.08f, 0.08f, 1.4f), Quaternion.Euler(-12, 4, 0));
            DressUtil.Bake(w, "street", cart, new Vector3(9.9f, 0f, 41.5f), Quaternion.Euler(0, 6, 4), new Vector3(1.3f, 1.2f, 2.2f));
            DressUtil.Bake(w, "street", Props.Barrel(0.9f, 0.3f), new Vector3(5.5f, 0f, 38.8f), Yaw(30), new Vector3(0.6f, 0.9f, 0.6f));
            Props.Barrel(0.9f, 0.3f).BakeInto(w.Kit, "street", new Vector3(6.0f, 0.3f, 39.7f), Quaternion.Euler(0, 75, 90));
            Props.Planks(rng, 4, 2f).BakeInto(w.Kit, "street", new Vector3(9.5f, 0.02f, 61.0f), Yaw(30));
            Props.Planks(rng, 3, 1.5f).BakeInto(w.Kit, "street", new Vector3(6.2f, 0.02f, 85.0f), Yaw(-20));
            // floating debris on the flood
            for (int i = 0; i < 24; i++)
            {
                var room = w.Act.Rooms[rng.Range(0, w.Act.Rooms.Count)];
                if (float.IsNaN(room.Water) || !(room.Kind == RoomKind.Street)) continue;
                var p = new Vector3(rng.Range(room.Rect.X + 0.4f, room.Rect.MaxX - 0.4f), Act3Content.WaterLevel + 0.01f, rng.Range(room.Rect.Z + 0.4f, room.Rect.MaxZ - 0.4f));
                var f = new PropMesh();
                if (rng.Chance(0.5f)) f["wood"].Box(Vector3.zero, new Vector3(rng.Range(0.3f, 1.1f), 0.04f, rng.Range(0.1f, 0.2f)), Yaw(rng.Range(0, 180)));
                else f["paper"].Box(Vector3.zero, new Vector3(0.21f, 0.004f, 0.28f), Yaw(rng.Range(0, 180)));
                var go = f.Spawn("float", w.Root, p, Quaternion.identity, false);
                var bob = go.AddComponent<Floater>();
                bob.Water = _streetWater.Find(x => x.Covers(p));
            }
        }

        // ------------------------------------------------------------------ interiors
        void Customs(World w)
        {
            var r = w.Act.Room("a3_customs");
            var rng = _rng;
            DressUtil.CeilingBeams(w, r, 1.6f);
            // counter across the room, shelves of ledgers behind it
            w.Kit.Box("a3_customs", "wood_dark", new Vector3(-3.4f, 0.55f, 29f), new Vector3(0.7f, 1.1f, 9f), true);
            w.Kit.Box("a3_customs", "wood", new Vector3(-3.4f, 1.12f, 29f), new Vector3(0.82f, 0.05f, 9.1f));
            for (int i = 0; i < 4; i++)
                DressUtil.Bake(w, "a3_customs", Props.Shelf(1.5f, 2.2f, 0.34f, rng, "wood_dark"), new Vector3(-8.82f, 0f, 24.2f + i * 1.7f), Yaw(90), new Vector3(0.34f, 2.2f, 1.5f));
            // a stopped wall clock: 11:40, the moment the town froze
            Clock(w, "a3_customs", new Vector3(-1f, 2.3f, 35.97f), Yaw(180), 11, 40);
            Props.Framed("seachart", 1.4f, 0.9f, "wood_dark").BakeInto(w.Kit, "a3_customs", new Vector3(-6.5f, 1.4f, 22.03f), Yaw(0));
            for (int i = 0; i < 3; i++) DressUtil.Bake(w, "a3_customs", Props.Chair(), new Vector3(-1.8f + i * 1.1f, 0f, 24.4f), Yaw(rng.Range(-30, 30) + 180), new Vector3(0.45f, 0.9f, 0.45f));
            DressUtil.Lantern(w, "customs_lamp", new Vector3(-2f, 2.95f, 29f), false, 0, 0, false, false, 0.5f);
            for (int i = 0; i < 7; i++)
                w.Kit.Box("a3_customs", "paper", new Vector3(rng.Range(-8f, 4f), Act3Content.WaterLevel + 0.005f, rng.Range(23f, 35f)), new Vector3(0.21f, 0.002f, 0.28f), Yaw(rng.Range(0, 180)));
        }

        void Tavern(World w)
        {
            var r = w.Act.Room("a3_tavern");
            var rng = _rng;
            DressUtil.CeilingBeams(w, r, 1.5f);
            // the bar: the counter note waits on it
            w.Kit.Box("a3_tavern", "wood_dark", new Vector3(21f, 0.54f, 33.2f), new Vector3(4.6f, 1.08f, 0.7f), true);
            w.Kit.Box("a3_tavern", "wood", new Vector3(21f, 1.065f, 33.2f), new Vector3(4.8f, 0.05f, 0.86f));
            DressUtil.Bake(w, "a3_tavern", Props.Shelf(2.6f, 2.0f, 0.34f, rng, "wood_dark"), new Vector3(21f, 0f, 37.82f), Yaw(180), new Vector3(2.6f, 2.0f, 0.34f));
            for (int i = 0; i < 10; i++) Props.Bottle(rng).BakeInto(w.Kit, "a3_tavern", new Vector3(19.9f + i * 0.24f, 1.32f, 37.78f), Yaw(0));
            foreach (var (x, z) in new[] { (14.0f, 27.5f), (17.5f, 27.0f), (14.5f, 33.5f) })
            {
                DressUtil.Bake(w, "a3_tavern", Props.Table(1.3f, 0.9f, 0.76f, "wood_dark"), new Vector3(x, 0f, z), Yaw(rng.Range(-10, 10)), new Vector3(1.3f, 0.76f, 0.9f));
                for (int k = 0; k < 3; k++)
                {
                    var a = k * 120f + rng.Range(-20f, 20f);
                    var off = Yaw(a) * new Vector3(0, 0, 0.95f);
                    if (rng.Chance(0.3f)) Props.Stool().BakeInto(w.Kit, "a3_tavern", new Vector3(x, 0.15f, z) + off, Quaternion.Euler(80, a, 0));
                    else Props.Stool().BakeInto(w.Kit, "a3_tavern", new Vector3(x, 0f, z) + off, Yaw(a));
                }
                var mug = new PropMesh();
                mug["wood_dark"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.045f, 0f), new Vector2(0.05f, 0.1f), new Vector2(0f, 0.1f) }, 8);
                mug.BakeInto(w.Kit, "a3_tavern", new Vector3(x + 0.2f, 0.76f, z - 0.1f), Yaw(0));
            }
            // hearth, cold for three hundred years
            var hearth = new PropMesh();
            hearth["stone"].Box(new Vector3(0, 1.0f, 0), new Vector3(2.0f, 2.0f, 0.6f), Quaternion.identity, 63 & ~16);
            hearth["black"].Box(new Vector3(0, 0.55f, 0.29f), new Vector3(1.2f, 1.0f, 0.02f), Quaternion.identity);
            hearth["stone"].Box(new Vector3(0, 2.05f, 0.1f), new Vector3(2.3f, 0.14f, 0.8f), Quaternion.identity);
            DressUtil.Bake(w, "a3_tavern", hearth, new Vector3(24.65f, 0f, 29.5f), Yaw(-90), new Vector3(0.6f, 2.0f, 2.0f));
            foreach (float x in new[] { 22.8f, 23.6f }) DressUtil.Bake(w, "a3_tavern", Props.Barrel(0.9f, 0.3f), new Vector3(x, 0f, 24.7f), Yaw(rng.Range(0, 360)), new Vector3(0.6f, 0.9f, 0.6f));
            Clock(w, "a3_tavern", new Vector3(16f, 2.2f, 37.97f), Yaw(180), 11, 40);
        }

        void Warehouse(World w)
        {
            var r = w.Act.Room("a3_ware");
            var rng = _rng;
            DressUtil.CeilingBeams(w, r, 1.8f);
            foreach (var (p, s) in new[] { (new Vector3(-10.2f, 0, 44.9f), 1.1f), (new Vector3(-9.0f, 0, 44.9f), 1.0f), (new Vector3(-10.2f, 0, 57.0f), 1.0f), (new Vector3(-10.3f, 0, 50.5f), 1.1f), (new Vector3(-1.0f, 0, 57.1f), 0.9f) })
            {
                DressUtil.Bake(w, "a3_ware", Props.Crate(new Vector3(s, s, s)), p, Yaw(rng.Range(-6, 6)), new Vector3(s, s * 2f, s));
                Props.Crate(new Vector3(s * 0.85f, s * 0.85f, s * 0.85f)).BakeInto(w.Kit, "a3_ware", p + Vector3.up * s, Yaw(rng.Range(-20, 20)));
            }
            // hoist and a net of cargo hanging from it
            w.Kit.Box("a3_ware", "wood_dark", new Vector3(-3f, 3.9f, 50f), new Vector3(0.25f, 0.25f, 12f));
            var rope = w.Kit.B("a3_ware", "rope", 0.3f);
            rope.Rope(new Vector3(-3f, 3.8f, 49f), new Vector3(-3f, 2.0f, 49f), 0f, 0.025f, 2);
            var net = new PropMesh();
            net["rope"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.05f, 0f), new Vector2(0.55f, 0.25f), new Vector2(0.6f, 0.6f), new Vector2(0.2f, 0.95f), new Vector2(0.03f, 1.0f) }, 10);
            net.BakeInto(w.Kit, "a3_ware", new Vector3(-3f, 1.0f, 49f), Yaw(0));
            for (int i = 0; i < 6; i++) DressUtil.Sack(w, "a3_ware", new Vector3(rng.Range(-9f, -5f), 0f, rng.Range(53f, 56.8f)), Yaw(rng.Range(0, 180)), rng);
            DressUtil.Lantern(w, "ware_lamp", new Vector3(-6f, 3.8f, 51f), false, 0, 0, false, false, 1.2f);
        }

        void Chapel(World w)
        {
            var r = w.Act.Room("a3_chapel");
            var rng = _rng;
            // pews facing the altar at the east end
            for (int row = 0; row < 6; row++)
                foreach (float z in new[] { 48.6f, 57.4f })
                {
                    var pew = new PropMesh();
                    pew["wood_dark"].Box(new Vector3(0, 0.42f, 0), new Vector3(0.45f, 0.06f, 3.4f), Quaternion.identity);
                    pew["wood_dark"].Box(new Vector3(-0.22f, 0.7f, 0), new Vector3(0.05f, 0.6f, 3.4f), Quaternion.Euler(0, 0, -8));
                    foreach (float e in new[] { -1.65f, 1.65f }) pew["wood_dark"].Box(new Vector3(0, 0.45f, e), new Vector3(0.5f, 0.9f, 0.06f), Quaternion.identity);
                    DressUtil.Bake(w, "a3_chapel", pew, new Vector3(13.4f + row * 1.25f, 0f, z), Yaw(0), new Vector3(0.5f, 0.9f, 3.4f));
                }
            // altar with candles, the only warm place in the town
            var altar = new PropMesh();
            altar["stone"].Box(new Vector3(0, 0.5f, 0), new Vector3(1.0f, 1.0f, 2.6f), Quaternion.identity);
            altar["cloth"].Box(new Vector3(0, 1.01f, 0), new Vector3(1.05f, 0.02f, 2.7f), Quaternion.identity);
            altar["cloth"].Box(new Vector3(-0.52f, 0.75f, 0), new Vector3(0.01f, 0.5f, 2.7f), Quaternion.identity);
            DressUtil.Bake(w, "a3_chapel", altar, new Vector3(25.6f, 0f, 53f), Yaw(0), new Vector3(1.0f, 1.0f, 2.6f));
            Props.Candles(rng, 9).BakeInto(w.Kit, "a3_chapel", new Vector3(25.6f, 1.02f, 52.4f), Yaw(0), 1.6f);
            Props.Candles(rng, 7).BakeInto(w.Kit, "a3_chapel", new Vector3(25.6f, 1.02f, 53.7f), Yaw(0), 1.6f);
            for (int i = 0; i < 6; i++) Props.Candles(rng, 4).BakeInto(w.Kit, "a3_chapel", new Vector3(26.6f, 0f, 47f + i * 2.4f), Yaw(0), 1.3f);
            // tall windows, cold blue
            foreach (float x in new[] { 14f, 18f, 22f })
                foreach (float z in new[] { 46.03f, 59.97f })
                {
                    var rot = Yaw(z < 50f ? 0f : 180f);
                    w.Kit.Box("a3_chapel", "glow_cold", new Vector3(x, 3.2f, z), new Vector3(1.0f, 2.6f, 0.02f), rot);
                    w.Kit.Box("a3_chapel", "stone", new Vector3(x, 1.85f, z + (z < 50f ? 0.06f : -0.06f)), new Vector3(1.3f, 0.12f, 0.14f), rot);
                }
            var cross = w.Kit.B("a3_chapel", "wood_dark");
            cross.Box(new Vector3(26.94f, 3.4f, 53f), new Vector3(0.08f, 2.2f, 0.16f), Quaternion.identity);
            cross.Box(new Vector3(26.94f, 3.9f, 53f), new Vector3(0.08f, 0.16f, 1.2f), Quaternion.identity);
            AltarLight = new Vector3(25.6f, 1.7f, 53f);
        }

        Vector3 AltarLight;

        void Square(World w)
        {
            var rng = _rng;
            // the well, and benches where nobody has sat for three centuries
            var well = new PropMesh();
            well["stone"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.9f, 0f), new Vector2(0.95f, 0.8f), new Vector2(0.75f, 0.85f), new Vector2(0.72f, 0.1f) }, 16);
            well["black"].Disc(new Vector3(0, 0.6f, 0), Quaternion.identity, 0.72f, 16);
            well["wood_dark"].Box(new Vector3(-0.85f, 1.3f, 0), new Vector3(0.12f, 1.4f, 0.12f), Quaternion.identity);
            well["wood_dark"].Box(new Vector3(0.85f, 1.3f, 0), new Vector3(0.12f, 1.4f, 0.12f), Quaternion.identity);
            well["wood_dark"].Cylinder(new Vector3(-0.9f, 1.75f, 0), Quaternion.Euler(0, 0, -90), 1.8f, 0.08f, 0.08f, 8);
            well["rope"].Tube(new[] { new Vector3(0, 1.75f, 0), new Vector3(0, 0.9f, 0) }, 0.02f, 4, 0.2f);
            DressUtil.Bake(w, "a3_square", well, new Vector3(14.5f, 0f, 74.5f), Yaw(15), new Vector3(1.9f, 0.85f, 1.9f));
            foreach (var (x, z, a) in new[] { (0.5f, 64.0f, 90f), (0.5f, 76.5f, 90f), (17.5f, 63.2f, 0f) })
            {
                var bench = new PropMesh();
                bench["wood"].Box(new Vector3(0, 0.45f, 0), new Vector3(1.8f, 0.06f, 0.45f), Quaternion.identity);
                foreach (float e in new[] { -0.75f, 0.75f }) bench["stone"].Box(new Vector3(e, 0.22f, 0), new Vector3(0.2f, 0.44f, 0.4f), Quaternion.identity);
                DressUtil.Bake(w, "a3_square", bench, new Vector3(x, 0f, z), Yaw(a), new Vector3(1.8f, 0.5f, 0.45f));
            }
            // a fountain basin around the statue's plinth (the statue itself is an item)
            var basin = new PropMesh();
            basin["stone"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(2.0f, 0f), new Vector2(2.05f, 0.45f), new Vector2(1.85f, 0.5f), new Vector2(1.8f, 0.1f) }, 24);
            DressUtil.Bake(w, "a3_square", basin, new Vector3(8f, 0f, 70f), Yaw(0));
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * 360f;
                w.Kit.Collider(new Vector3(8f, 0.25f, 70f) + Yaw(a) * new Vector3(0, 0, 1.93f), new Vector3(1.5f, 0.5f, 0.25f), Yaw(a));
            }
        }

        void House(World w)
        {
            var r = w.Act.Room("a3_house");
            var rng = _rng;
            DressUtil.CeilingBeams(w, r, 1.3f);
            var bed = Props.Bunk(1.9f, 1.2f, false);
            bed["cloth_blue"].Box(new Vector3(0, 0.48f, 0.3f), new Vector3(1.1f, 0.04f, 1.2f), Quaternion.identity);
            DressUtil.Bake(w, "a3_house", bed, new Vector3(-12.9f, 0f, 68.2f), Yaw(0), new Vector3(1.2f, 0.5f, 1.9f));
            // a cradle, rocking very slightly
            var cr = new PropMesh();
            cr["wood"].Box(new Vector3(0, 0.45f, 0), new Vector3(0.5f, 0.3f, 0.9f), Quaternion.identity, 63 & ~4);
            cr["cloth"].Box(new Vector3(0, 0.55f, 0), new Vector3(0.44f, 0.04f, 0.84f), Quaternion.identity);
            foreach (float z in new[] { -0.4f, 0.4f })
            {
                var rk = new List<Vector3>();
                for (int k = 0; k <= 8; k++) { float t = k / 8f * 2f - 1f; rk.Add(new Vector3(t * 0.35f, 0.04f + t * t * 0.08f, z)); }
                cr["wood_dark"].Tube(rk, 0.018f, 5, 0.2f);
            }
            var crGo = cr.Spawn("cradle", w.Root, new Vector3(-6.0f, 0f, 78.6f), Yaw(90));
            var sw = crGo.AddComponent<Sway>(); sw.Amp = 4f; sw.Speed = 0.4f;
            w.Kit.Collider(new Vector3(-6.0f, 0.35f, 78.6f), new Vector3(0.9f, 0.7f, 0.5f));
            DressUtil.Bake(w, "a3_house", Props.Table(1.2f, 0.8f, 0.76f, "wood"), new Vector3(-8.5f, 0f, 70.0f), Yaw(0), new Vector3(1.2f, 0.76f, 0.8f));
            foreach (float x in new[] { -9.2f, -7.8f }) DressUtil.Bake(w, "a3_house", Props.Chair(), new Vector3(x, 0f, 69.2f), Yaw(rng.Range(-15f, 15f)), new Vector3(0.45f, 0.9f, 0.45f));
            var hearth = new PropMesh();
            hearth["stone"].Box(new Vector3(0, 0.9f, 0), new Vector3(1.6f, 1.8f, 0.5f), Quaternion.identity, 63 & ~16);
            hearth["black"].Box(new Vector3(0, 0.5f, 0.24f), new Vector3(0.9f, 0.8f, 0.02f), Quaternion.identity);
            DressUtil.Bake(w, "a3_house", hearth, new Vector3(-9f, 0f, 79.73f), Yaw(180), new Vector3(1.6f, 1.8f, 0.5f));
            Clock(w, "a3_house", new Vector3(-4.03f, 2.0f, 75.6f), Yaw(-90), 11, 40);
            DressUtil.Shifty(w, "house_chair", Props.Chair(), new Vector3(-11.2f, 0f, 75.0f), Yaw(120), new Vector3(0.45f, 0.9f, 0.45f), rng);
        }

        // ------------------------------------------------------------------ the clock tower
        void Tower(World w)
        {
            var r = w.Act.Room("a3_tower");
            var rng = _rng;
            // three clock cases, one behind each dial, the works showing through their open backs
            float[] dx = { 5f, 8f, 11f };
            float[] dz = { 104.5f, 106f, 104.5f };
            for (int i = 0; i < 3; i++)
            {
                var c = new PropMesh();
                c["wood_dark"].Box(new Vector3(0, 1.1f, 0.25f), new Vector3(1.15f, 2.2f, 0.4f), Quaternion.identity);
                c["wood_dark"].Box(new Vector3(0, 2.3f, 0.25f), new Vector3(1.3f, 0.2f, 0.5f), Quaternion.identity);
                c["brass"].Cylinder(new Vector3(0, 0.6f, 0.04f), Quaternion.Euler(90, 0, 0), 0.02f, 0.12f, 0.12f, 14);
                c["iron"].Tube(new[] { new Vector3(0, 1.4f, 0.02f), new Vector3(0, 0.7f, 0.02f) }, 0.01f, 4, 0.2f);
                DressUtil.Bake(w, "a3_tower", c, new Vector3(dx[i], 0f, dz[i]), Yaw(0), new Vector3(1.15f, 2.2f, 0.4f), 1.1f);
            }
            // great gears overhead, and the pendulum that no longer swings
            for (int i = 0; i < 5; i++)
            {
                var gc = new Vector3(rng.Range(4f, 12f), rng.Range(4.5f, 7.2f), rng.Range(100f, 110f));
                float gr = rng.Range(0.5f, 1.3f);
                var rot = Quaternion.Euler(rng.Chance(0.5f) ? 90f : 0f, rng.Range(0, 90), 0);
                w.Kit.B("a3_tower", "brass").Cylinder(gc, rot, 0.12f, gr, gr, 22);
                for (int k = 0; k < 16; k++)
                {
                    var d = rot * (Quaternion.Euler(0, k * 22.5f, 0) * Vector3.forward);
                    w.Kit.B("a3_tower", "brass").Box(gc + rot * Vector3.up * 0.06f + d * (gr + 0.06f), new Vector3(0.1f, 0.12f, 0.14f), Quaternion.LookRotation(d, rot * Vector3.up));
                }
                w.Kit.B("a3_tower", "iron").Cylinder(gc - rot * Vector3.up * 0.3f, rot, 0.7f, 0.06f, 0.06f, 8);
            }
            w.Kit.B("a3_tower", "iron").Tube(new[] { new Vector3(8f, 7.9f, 109.5f), new Vector3(8f, 3.0f, 109.5f) }, 0.03f, 6, 0.3f);
            w.Kit.B("a3_tower", "brass").Cylinder(new Vector3(8f, 2.8f, 109.62f), Quaternion.Euler(90, 0, 0), 0.08f, 0.45f, 0.45f, 18);
            // stairs winding up the walls to the bells (blocked by collapse)
            for (int i = 0; i < 12; i++)
            {
                float y = 0.2f + i * 0.33f;
                w.Kit.Box("a3_tower", "stone", new Vector3(2.55f, y, 98f + i * 0.9f), new Vector3(1.1f, 0.2f, 0.85f), false);
            }
            w.Kit.Collider(new Vector3(2.55f, 2.1f, 103f), new Vector3(1.1f, 4.2f, 11f));
            Props.Rubble(rng, 12, 0.8f, "stone").BakeInto(w.Kit, "a3_tower", new Vector3(3.1f, 0f, 109.8f), Yaw(0));
            // outside: three faces near the top, all showing 11:40
            foreach (var (pos, rot) in new[] { (new Vector3(8f, 18f, 95.72f), Yaw(180)), (new Vector3(1.72f, 18f, 104f), Yaw(-90)), (new Vector3(14.28f, 18f, 104f), Yaw(90)) })
                Clock(w, "a3_tower_out", pos, rot, 11, 40, 2.2f);
            // the open belfry on top
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? 2.4f : 13.6f, z = i < 2 ? 96.4f : 111.6f;
                w.Kit.Box("a3_tower_out", "stone", new Vector3(x, 25f, z), new Vector3(0.8f, 4f, 0.8f));
            }
            w.Kit.Box("a3_tower_out", "roof", new Vector3(8f, 27.4f, 104f), new Vector3(12.8f, 0.6f, 16.8f));
            Props.Bell(1.0f, "brass").BakeInto(w.Kit, "a3_tower_out", new Vector3(8f, 26.6f, 104f), Yaw(0));
        }

        /// <summary>A clock face with hands set to h:m, facing +Z of rot.</summary>
        static void Clock(World w, string chunk, Vector3 pos, Quaternion rot, int h, int m, float scale = 1f)
        {
            var c = new PropMesh();
            c["wood_dark"].Cylinder(Vector3.zero, Quaternion.Euler(90, 0, 0), 0.05f, 0.24f, 0.24f, 18);
            c["paper"].Disc(new Vector3(0, 0, 0.052f), Quaternion.Euler(-90, 0, 0), 0.2f, 18);
            for (int i = 0; i < 12; i++)
                c["black"].Box(Quaternion.Euler(0, 0, -i * 30f) * new Vector3(0, 0.17f, 0) + new Vector3(0, 0, 0.055f), new Vector3(0.012f, 0.035f, 0.002f), Quaternion.Euler(0, 0, -i * 30f));
            float ha = ((h % 12) + m / 60f) * 30f, ma = m * 6f;
            c["black"].Box(Quaternion.Euler(0, 0, -ha) * new Vector3(0, 0.055f, 0) + new Vector3(0, 0, 0.058f), new Vector3(0.016f, 0.11f, 0.003f), Quaternion.Euler(0, 0, -ha));
            c["black"].Box(Quaternion.Euler(0, 0, -ma) * new Vector3(0, 0.08f, 0) + new Vector3(0, 0, 0.061f), new Vector3(0.01f, 0.16f, 0.003f), Quaternion.Euler(0, 0, -ma));
            c.BakeInto(w.Kit, chunk, pos, rot, scale);
        }

        // ------------------------------------------------------------------ the governor's house
        void Mansion(World w)
        {
            var rng = _rng;
            // facade on the square: columns and a pediment over the gate
            foreach (float z in new[] { 66.2f, 68.0f, 72.0f, 73.8f })
                Props.Column(4.0f, 0.24f, "stone").BakeInto(w.Kit, "mansion_out", new Vector3(19.55f, 0f, z), Yaw(0));
            w.Kit.Box("mansion_out", "stone", new Vector3(19.5f, 4.15f, 70f), new Vector3(1.1f, 0.3f, 8.6f));
            var ped = w.Kit.B("mansion_out", "stone", 2f);
            ped.Tri(new Vector3(18.95f, 4.3f, 65.7f), new Vector3(18.95f, 4.3f, 74.3f), new Vector3(18.95f, 6.0f, 70f));
            ped.Tri(new Vector3(18.95f, 4.3f, 74.3f), new Vector3(18.95f, 4.3f, 65.7f), new Vector3(18.95f, 6.0f, 70f));
            // the crest of the Crane family above the door
            w.Kit.B("mansion_out", "brass").Disc(new Vector3(18.93f, 5.0f, 70f), Quaternion.Euler(0, 0, 90), 0.4f, 16);
            w.Kit.B("mansion_out", "brass").Disc(new Vector3(18.93f, 5.0f, 70f), Quaternion.Euler(0, 0, -90), 0.4f, 16);

            // hall: chandelier, a staircase that goes nowhere now, portraits with their faces scraped away
            var mh = w.Act.Room("a3_mhall");
            var ch = new PropMesh();
            ch["brass"].Cylinder(new Vector3(0, 0, 0), Quaternion.identity, 0.06f, 0.7f, 0.7f, 18, false);
            ch["brass"].Cylinder(new Vector3(0, 0.5f, 0), Quaternion.identity, 0.05f, 0.45f, 0.45f, 16, false);
            ch["iron"].Tube(new[] { new Vector3(0, 0, 0), new Vector3(0, 1.6f, 0) }, 0.02f, 4, 0.3f);
            for (int i = 0; i < 8; i++)
            {
                var d = Yaw(i * 45f) * Vector3.forward;
                ch["candle"].Cylinder(d * 0.7f + Vector3.up * 0.03f, Quaternion.identity, 0.12f, 0.02f, 0.02f, 6);
            }
            ch.BakeInto(w.Kit, "a3_mhall", new Vector3(25f, mh.CeilH - 1.6f, 71f), Yaw(0));
            for (int i = 0; i < 10; i++)
                w.Kit.Box("a3_mhall", "stone", new Vector3(28.6f, 0.16f + i * 0.32f, 65.4f + i * 0.4f), new Vector3(2.4f, 0.32f, 0.4f), true);
            w.Kit.Box("a3_mhall", "wood_dark", new Vector3(27.5f, 2.4f, 67.2f), new Vector3(0.08f, 1.0f, 3.8f), Quaternion.Euler(-38, 0, 0));
            foreach (var (pos, yaw) in new[] { (new Vector3(20.03f, 1.6f, 66.0f), 90f), (new Vector3(20.03f, 1.6f, 75.2f), 90f), (new Vector3(23.0f, 1.6f, 77.97f), 180f) })
            {
                var pf = Props.Framed("portrait", 0.9f, 1.2f, "brass", 0.05f);
                pf["black"].Box(new Vector3(0, 0.82f, 0.058f), new Vector3(0.36f, 0.42f, 0.004f), Quaternion.Euler(0, 0, rng.Range(-12, 12)));
                pf.BakeInto(w.Kit, "a3_mhall", pos, Yaw(yaw));
            }
            DressUtil.Rug(w, mh, new Vector3(24.5f, 0, 71f), 3f, 8f, "cloth_red", 0f);

            // study: the desk with the pact, books, the governor's chair turned away
            var st = w.Act.Room("a3_study");
            DressUtil.Bake(w, "a3_study", Props.Shelf(2.4f, 2.6f, 0.36f, rng, "wood_dark"), new Vector3(39.82f, 0f, 68f), Yaw(-90), new Vector3(0.36f, 2.6f, 2.4f));
            DressUtil.Bake(w, "a3_study", Props.Shelf(2.0f, 2.6f, 0.36f, rng, "wood_dark"), new Vector3(33.5f, 0f, 64.18f), Yaw(0), new Vector3(2.0f, 2.6f, 0.36f));
            DressUtil.Bake(w, "a3_study", Props.Chair("wood_dark"), new Vector3(36f, 0f, 69.0f), Yaw(160), new Vector3(0.5f, 1.0f, 0.5f));
            Props.Candles(rng, 3).BakeInto(w.Kit, "a3_study", new Vector3(36.6f, 0.92f, 67.8f), Yaw(0));
            DressUtil.Rug(w, st, new Vector3(35f, 0, 68f), 3.2f, 2.4f, "cloth_red", 0f);
            Props.Framed("seachart", 1.6f, 1.0f, "brass").BakeInto(w.Kit, "a3_study", new Vector3(32.5f, 1.4f, 71.97f), Yaw(180));

            // Marisol's room: a small bed, a music box with a carved tide on the lid, her shoes by the door
            var mr = w.Act.Room("a3_marisol");
            var bed = Props.Bunk(1.6f, 0.8f, false);
            bed["cloth_red"].Box(new Vector3(0, 0.48f, 0.3f), new Vector3(0.74f, 0.03f, 0.9f), Quaternion.identity);
            bed["wood"].Box(new Vector3(0, 1.0f, -0.8f), new Vector3(0.8f, 1.1f, 0.06f), Quaternion.identity);
            DressUtil.Bake(w, "a3_marisol", bed, new Vector3(39.2f, 0f, 74.2f), Yaw(0), new Vector3(0.8f, 0.5f, 1.6f));
            Props.MusicBox(100f, true).BakeInto(w.Kit, "a3_marisol", new Vector3(31.4f, 0.75f, 79.4f), Yaw(160));
            DressUtil.Bake(w, "a3_marisol", Props.Table(0.7f, 0.5f, 0.75f, "wood"), new Vector3(31.4f, 0f, 79.4f), Yaw(0), new Vector3(0.7f, 0.75f, 0.5f));
            foreach (float x in new[] { 30.6f, 30.75f })
            {
                var shoe = new PropMesh();
                shoe["velvet"].Box(new Vector3(0, 0.025f, 0), new Vector3(0.07f, 0.05f, 0.15f), Quaternion.identity);
                shoe.BakeInto(w.Kit, "a3_marisol", new Vector3(x, 0f, 74.0f), Yaw(80));
            }
            DressUtil.Rug(w, mr, new Vector3(35f, 0, 76f), 2.4f, 2f, "cloth_blue", 10f);
            DressUtil.Shifty(w, "marisol_chair", Props.Chair("wood"), new Vector3(34.0f, 0f, 78.9f), Yaw(200), new Vector3(0.45f, 0.9f, 0.45f), rng);

            // gallery: portraits of every Crane child given to the sea, one empty frame at the end
            var gal = w.Act.Room("a3_gallery");
            for (int i = 0; i < 6; i++)
            {
                float x = 21.2f + i * 1.6f;
                if (Mathf.Abs(x - 25f) < 1.2f) continue;
                var pf = Props.Framed(i == 5 ? "black" : "portrait", 0.6f, 0.8f, "brass", 0.04f);
                pf.BakeInto(w.Kit, "a3_gallery", new Vector3(x, 1.5f, 78.03f), Yaw(0));
                Props.Framed(i == 4 ? "black" : "portrait", 0.6f, 0.8f, "brass", 0.04f).BakeInto(w.Kit, "a3_gallery", new Vector3(x, 1.5f, 87.97f), Yaw(180));
            }
            DressUtil.Rug(w, gal, new Vector3(26f, 0, 83f), 8f, 2.2f, "cloth_red", 0f);
            // a bench facing the family tree
            var bench = new PropMesh();
            bench["velvet"].Box(new Vector3(0, 0.45f, 0), new Vector3(1.6f, 0.12f, 0.5f), Quaternion.identity);
            foreach (float e in new[] { -0.7f, 0.7f }) bench["wood_dark"].Box(new Vector3(e, 0.2f, 0), new Vector3(0.08f, 0.4f, 0.45f), Quaternion.identity);
            DressUtil.Bake(w, "a3_gallery", bench, new Vector3(29.0f, 0f, 83f), Yaw(90), new Vector3(1.6f, 0.5f, 0.5f));

            // mirror hall: mirrors on every wall; one shows a tent and a boy with his hands over his ears
            var mi = w.Act.Room("a3_mirror");
            for (float x = 19.2f; x < 36f; x += 2.2f)
            {
                if (Mathf.Abs(x - 26f) < 1.6f || Mathf.Abs(x - 27f) < 1.8f) continue;
                Props.Framed("mirror_old", 1.4f, 3.2f, "brass", 0.06f).BakeInto(w.Kit, "a3_mirror", new Vector3(x, 0.6f, 88.03f), Yaw(0));
                Props.Framed("mirror_old", 1.4f, 3.2f, "brass", 0.06f).BakeInto(w.Kit, "a3_mirror", new Vector3(x, 0.6f, 103.97f), Yaw(180));
            }
            for (float z = 90f; z < 103f; z += 2.4f)
            {
                Props.Framed("mirror_old", 1.4f, 3.2f, "brass", 0.06f).BakeInto(w.Kit, "a3_mirror", new Vector3(18.03f, 0.6f, z), Yaw(90));
                if (Mathf.Abs(z - 96f) > 1.8f) Props.Framed("mirror_old", 1.4f, 3.2f, "brass", 0.06f).BakeInto(w.Kit, "a3_mirror", new Vector3(35.97f, 0.6f, z), Yaw(-90));
            }
            // the boy in the glass (hidden until the moment)
            var self = new PropMesh();
            self["silhouette"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.26f, 0f), new Vector2(0.3f, 0.3f), new Vector2(0.22f, 0.55f), new Vector2(0.12f, 0.62f), new Vector2(0.13f, 0.75f), new Vector2(0.0f, 0.82f) }, 12);
            self["silhouette"].Box(new Vector3(-0.14f, 0.7f, 0.02f), new Vector3(0.06f, 0.16f, 0.06f), Quaternion.Euler(0, 0, 20));
            self["silhouette"].Box(new Vector3(0.14f, 0.7f, 0.02f), new Vector3(0.06f, 0.16f, 0.06f), Quaternion.Euler(0, 0, -20));
            self["sail"].Box(new Vector3(0, 0.9f, -0.4f), new Vector3(1.6f, 0.04f, 1.2f), Quaternion.Euler(0, 0, 0));
            var sgo = self.Spawn("mirror_self", w.Root, new Vector3(27f, 0.5f, 103.75f), Yaw(180), false);
            sgo.SetActive(false);
            w.Anchors["mirror_self"] = sgo.transform;
            // black water lies on the floor of the hall, an inch deep, perfectly still
            var hallWater = WaterSurface.CreateFlat(w.Root, "mirror_water", new RectXZ(mi.Rect.X + 0.05f, mi.Rect.Z + 0.05f, mi.Rect.W - 0.1f, mi.Rect.D - 0.1f), 0.02f, false);
            w.Waters.Add(hallWater);
        }

        // ------------------------------------------------------------------ the sea stairs
        void SeaStairs(World w, GameState state)
        {
            var r = w.Act.Room("a3_sea");
            var rng = _rng;
            // balustrade posts and the stone urns at the top
            foreach (float z in new[] { 91.7f, 100.3f })
                Props.Column(1.2f, 0.16f, "stone").BakeInto(w.Kit, "a3_sea", new Vector3(36.3f, 0f, z), Yaw(0));
            // where the steps meet the sea, the water is black and does not move
            for (int i = 0; i < 6; i++)
                Props.Rubble(rng, 4, 0.4f, "stone").BakeInto(w.Kit, "a3_sea", new Vector3(rng.Range(38f, 45f), r.FloorAt(42f, 96f), rng.Range(92.5f, 99.5f)), Yaw(rng.Range(0, 360)));
            w.Anchor("sea_stairs", new Vector3(44.5f, -1.6f, 96f));
        }

        // ------------------------------------------------------------------ Vanessa's footprints through the town
        void Footprints(World w)
        {
            var rng = _rng;
            var b = w.Kit.B("prints", "footprint", 1f);
            var fpL = Props.Footprint(true); var fpR = Props.Footprint(false);
            var path = new[]
            {
                new Vector3(8f, 0f, 5f), new Vector3(8.2f, 0f, 18f), new Vector3(6.2f, 0f, 28.5f), new Vector3(8f, 0f, 30f), new Vector3(9.8f, 0f, 31f),
                new Vector3(8f, 0f, 40f), new Vector3(6f, 0f, 51f), new Vector3(8f, 0f, 52f), new Vector3(10f, 0f, 53f), new Vector3(8.4f, 0f, 60f),
                new Vector3(2f, 0f, 70f), new Vector3(-3.4f, 0f, 72f), new Vector3(2f, 0f, 74f), new Vector3(8f, 0f, 84f), new Vector3(8f, 0f, 95f),
            };
            int idx = 0;
            for (int s = 0; s < path.Length - 1; s++)
            {
                var a = path[s]; var c = path[s + 1];
                float len = Vector3.Distance(a, c);
                int steps = Mathf.Max(1, Mathf.FloorToInt(len / 0.42f));
                var dir = (c - a) / len;
                for (int i = 0; i < steps; i++)
                {
                    var p = Vector3.Lerp(a, c, i / (float)steps);
                    var side = Vector3.Cross(Vector3.up, dir) * ((idx % 2 == 0) ? -0.06f : 0.06f);
                    var room = w.Act.RoomAt(p.x, p.z);
                    if (room == null) continue;
                    p.y = room.FloorAt(p.x, p.z) + 0.003f;
                    b.Append(idx % 2 == 0 ? fpL : fpR, Matrix4x4.TRS(p + side, Quaternion.LookRotation(dir) * Yaw(rng.Range(-8, 8)), Vector3.one * 0.8f));
                    idx++;
                }
            }
        }

        // ------------------------------------------------------------------ light
        public void Light(World w, GameState state)
        {
            var act = w.Act;
            w.Atmos = Atmos.Create(w.Root, new AtmosPreset
            {
                FogDistance = act.FogDistance,
                FogTint = new Color(act.FogColor[0], act.FogColor[1], act.FogColor[2]),
                FogAlbedo = new Color(0.62f, 0.68f, 0.72f),
                FogHeight = 10f,
                SkyTop = new Color(0.025f, 0.032f, 0.05f),
                SkyMid = new Color(0.06f, 0.075f, 0.09f),
                SkyBottom = new Color(0.09f, 0.1f, 0.11f),
                SkyExposure = 1.4f,
                Exposure = 2.0f,
                Saturation = -28f,
                Contrast = 12f,
                Filter = new Color(0.9f, 0.95f, 1f),
                Bloom = 0.22f,
                Vignette = 0.3f,
            });
            w.MoonLux = 4f * act.Moon;
            w.Moon = LightKit.Directional("Moon", w.Root, Quaternion.Euler(28f, 150f, 0f), LightKit.Moon, w.MoonLux, true);
            foreach (var r in act.Rooms)
                if (r.Ceil) w.Atmos.Interior(r.Id, new Vector3(r.Rect.X + r.Rect.W * 0.5f, r.Floor + r.CeilH * 0.5f, r.Rect.Z + r.Rect.D * 0.5f), new Vector3(r.Rect.W, r.CeilH, r.Rect.D), r.Ambient, 34f);
            // the chapel candles: the one safe warmth in the town
            var al = LightKit.Point("chapel_candles", w.Root, AltarLight, LightKit.Candle, 520f, 12f, true, true, 1f, 0.3f);
            var fl = al.gameObject.AddComponent<Flicker>();
            fl.Target = al; fl.Base = 520f; fl.Amount = 0.12f; fl.Speed = 5f;
            w.FlickerLights.Add(al);
            w.LightZones.Add(new LightZone { Pos = AltarLight, Radius = 7f, Safe = true });
            // moonlight falls into the mirror hall through tall windows
            for (int i = 0; i < 3; i++)
                LightKit.Spot("mirror_moon_" + i, w.Root, new Vector3(21f + i * 6f, 5.6f, 88.4f), Quaternion.Euler(50f, 0f, 0f), LightKit.Moon, 1800f, 14f, 40f, i == 1, false, 1.4f);
            // the clocks are right: the water drains, the mansion gate opens
            w.On("a3_clocks", rt =>
            {
                foreach (var ws in _streetWater) if (ws != null) ws.AnimateTo(-0.3f, 9f);
                if (_sea != null) _sea.AnimateTo(-0.9f, 12f);
                Game.Audio.Play2D("sfx_water_drain", Bus.Effects, 1f);
                if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.water_drain"], 4f);
                rt.Player.Shake(0.3f, 3f);
            });
        }
    }

    /// <summary>Bobs a small floating thing on a water surface.</summary>
    public sealed class Floater : MonoBehaviour
    {
        public WaterSurface Water;
        float _ph;
        Vector3 _base;

        void Start()
        {
            _ph = Random.value * 10f;
            _base = transform.position;
        }

        void Update()
        {
            float level = Water != null ? Water.Level : _base.y;
            float t = Time.time + _ph;
            bool sunk = level < -0.05f;
            transform.position = new Vector3(_base.x + Mathf.Sin(t * 0.21f) * 0.12f, sunk ? 0.005f : level + 0.012f + Mathf.Sin(t * 1.3f) * 0.006f, _base.z + Mathf.Cos(t * 0.17f) * 0.12f);
            transform.rotation = Quaternion.Euler(Mathf.Sin(t * 1.1f) * 2f, t * 3f, Mathf.Cos(t * 0.9f) * 2f);
        }
    }
}
