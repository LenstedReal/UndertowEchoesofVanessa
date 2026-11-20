using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Act II — the ship graveyard: plank walkways between the curved sides of wrecks, black water below,
    /// clearings on the decks of sunken hulks, lanterns that count the notes, and the split at the end.
    /// </summary>
    public sealed class Act2Dressing : IActDressing
    {
        const float WaterY = -0.7f;
        Rng _rng;
        static bool Corridor(RoomDef r) => r.Id == "a2_g1" || r.Id == "a2_g2" || r.Id == "a2_g3" || r.Id == "a2_g4" || r.Id == "a2_g5";
        static Quaternion Yaw(float d) => Quaternion.Euler(0, d, 0);

        public LevelStyle Style(World w)
        {
            var st = new LevelStyle();
            st.FloorMat = r =>
            {
                if (r.Kind == RoomKind.Shore || r.Kind == RoomKind.Cove) return "sand";
                if (r.Id == "a2_e") return "deck_dry";
                if (r.Id == "a2_tent") return "wood";
                return "deck";
            };
            st.FloorVisible = r => !Corridor(r) && r.Id != "a2_gate";
            st.CeilMat = r => r.Id == "a2_tent" ? "sail" : "deck_dry";
            st.FrameMat = o => "wood_dark";
            st.Wall = wp =>
            {
                var r = wp.Room;
                if (r.Id == "a2_tent" || (wp.Other != null && wp.Other.Id == "a2_tent")) return new WallLook(WallMode.Solid, "sail");
                if (r.Id == "a2_e" || (wp.Other != null && wp.Other.Id == "a2_e")) return new WallLook(WallMode.Solid, "hull_dark");
                if (wp.Kind == EdgeKind.Partition && (r.Kind == RoomKind.Cove || (wp.Other != null && wp.Other.Kind == RoomKind.Cove))) return new WallLook(WallMode.Solid, "stone");
                return new WallLook(WallMode.Hidden, "hull");
            };
            return st;
        }

        public void Dress(World w, GameState state)
        {
            _rng = new Rng(2207);
            var rng = _rng;
            var sea = WaterSurface.CreateSea(w.Root, new Vector3(6f, 0f, 150f), 440f, WaterY, 90);
            sea.Swell = 0.12f;
            w.Waters.Add(sea);

            // wreck sides along every open edge of the graveyard
            var look = new WreckKit.HullLook { Water = WaterY };
            foreach (var wp in w.Level.Walls)
            {
                var r = wp.Room;
                if (wp.Kind != EdgeKind.Void) continue;
                if (r.Kind != RoomKind.Grave) continue;
                if (r.Id == "a2_gate" && wp.AlongX && wp.A.Z > 298f) continue;   // the fog side stays open
                look.MinH = Corridor(r) ? 5f : 4f;
                look.MaxH = Corridor(r) ? 9.5f : 7f;
                WreckKit.HullWall(w, "hulls_" + r.Id, wp, rng, look);
            }
            foreach (var r in w.Act.Rooms)
            {
                if (Corridor(r) || r.Id == "a2_gate") Walkway(w, r);
            }
            Masts(w);
            Shore(w, w.Act.Room("a2_boat"), true);
            Shore(w, w.Act.Room("a2_cove"), false);
            ClearingB(w);
            Tent(w);
            ClearingC(w);
            ClearingD(w);
            HulkInterior(w);
            BellHulk(w);
            Gate(w);
            Cove(w);
            foreach (var id in new[] { "a2_b", "a2_c", "a2_d", "a2_hulk" })
                DressUtil.Puddles(w, w.Act.Room(id), 8, rng);
        }

        // ------------------------------------------------------------------ walkways
        void Walkway(World w, RoomDef r)
        {
            var rng = _rng;
            var planks = w.Kit.B("walk_" + r.Id, "deck", 1.6f);
            var under = w.Kit.B("walk_" + r.Id, "wood_dark", 1f);
            float x0 = r.Rect.X, x1 = r.Rect.MaxX;
            float z = r.Rect.Z;
            while (z < r.Rect.MaxZ)
            {
                float wdt = rng.Range(0.22f, 0.3f);
                if (z + wdt > r.Rect.MaxZ) wdt = r.Rect.MaxZ - z;
                float shortL = rng.Chance(0.12f) ? rng.Range(0.2f, 0.7f) : 0f;
                float shortR = rng.Chance(0.12f) ? rng.Range(0.2f, 0.7f) : 0f;
                var c = new Vector3((x0 + shortL + x1 - shortR) * 0.5f, -0.04f + rng.Range(-0.012f, 0.012f), z + wdt * 0.5f);
                planks.Box(c, new Vector3(x1 - x0 - shortL - shortR, 0.07f, wdt - 0.015f), Quaternion.Euler(rng.Range(-0.6f, 0.6f), rng.Range(-1.2f, 1.2f), rng.Range(-0.8f, 0.8f)));
                z += wdt + rng.Range(0.015f, 0.06f);
            }
            foreach (float x in new[] { x0 + 0.5f, (x0 + x1) * 0.5f, x1 - 0.5f })
                under.Box(new Vector3(x, -0.2f, r.Rect.Z + r.Rect.D * 0.5f), new Vector3(0.22f, 0.24f, r.Rect.D), Quaternion.identity);
            // pilings at the edges, crusted where the water laps
            for (float pz = r.Rect.Z + 1.2f; pz < r.Rect.MaxZ; pz += rng.Range(2.6f, 3.6f))
            {
                foreach (float x in new[] { x0 + 0.12f, x1 - 0.12f })
                {
                    float top = rng.Range(0.1f, 0.45f);
                    under.Cylinder(new Vector3(x, WaterY - 1.5f, pz), Quaternion.Euler(rng.Range(-3f, 3f), 0, rng.Range(-3f, 3f)), 1.5f - WaterY + top, 0.13f, 0.12f, 8);
                    w.Kit.B("walk_" + r.Id, "algae").Cylinder(new Vector3(x, WaterY - 0.1f, pz), Quaternion.identity, 0.45f, 0.14f, 0.135f, 8, false);
                    if (top > 0.2f) w.Kit.Collider(new Vector3(x, top * 0.5f, pz), new Vector3(0.24f, top, 0.24f));
                }
            }
            // a few things washed up on the planks
            for (int i = 0; i < 3; i++)
            {
                var p = new Vector3(rng.Chance(0.5f) ? x0 + 0.45f : x1 - 0.45f, 0f, rng.Range(r.Rect.Z + 2f, r.Rect.MaxZ - 2f));
                if (!DressUtil.Clear(w, p, 0.5f)) continue;
                switch (rng.Range(0, 4))
                {
                    case 0: DressUtil.Bake(w, "walk_" + r.Id, Props.RopeCoil(0.3f), p, Yaw(0)); break;
                    case 1: DressUtil.Bake(w, "walk_" + r.Id, Props.Bottle(rng), p, Quaternion.Euler(0, rng.Range(0, 360), 90)); break;
                    case 2: DressUtil.Bake(w, "walk_" + r.Id, Props.Barrel(0.8f, 0.28f), p, Yaw(rng.Range(0, 360)), new Vector3(0.56f, 0.8f, 0.56f)); break;
                    default: DressUtil.Bake(w, "walk_" + r.Id, Props.Planks(rng, 2, 1.2f), p, Yaw(rng.Range(-30, 30))); break;
                }
            }
        }

        void Masts(World w)
        {
            var rng = _rng;
            // broken masts propped across the corridors, high overhead; in g4 they form a tunnel of arches
            foreach (var r in w.Act.Rooms)
            {
                if (!Corridor(r)) continue;
                bool arches = r.Landmark == "arches";
                float step = arches ? 5.5f : rng.Range(9f, 13f);
                for (float z = r.Rect.Z + 4f; z < r.Rect.MaxZ - 3f; z += arches ? step : rng.Range(9f, 14f))
                {
                    bool fromWest = rng.Chance(0.5f);
                    float h0 = rng.Range(3.8f, 5.5f), h1 = rng.Range(6.5f, 9f);
                    var foot = new Vector3(fromWest ? r.Rect.X - 1.6f : r.Rect.MaxX + 1.6f, h0, z);
                    var tip = new Vector3(fromWest ? r.Rect.MaxX + 2.4f : r.Rect.X - 2.4f, h1, z + rng.Range(-2f, 2f));
                    WreckKit.LeaningMast(w, "masts", foot, tip, rng.Range(0.22f, 0.34f), rng, rng.Chance(0.4f));
                    if (arches)
                    {
                        // a second spar crossing the other way makes the pointed arch
                        var foot2 = new Vector3(fromWest ? r.Rect.MaxX + 1.6f : r.Rect.X - 1.6f, h0 + rng.Range(-0.5f, 0.5f), z + 0.4f);
                        var tip2 = new Vector3(fromWest ? r.Rect.X - 2.4f : r.Rect.MaxX + 2.4f, h1 + rng.Range(-0.5f, 0.5f), z + 0.6f);
                        WreckKit.LeaningMast(w, "masts", foot2, tip2, rng.Range(0.22f, 0.3f), rng, false);
                    }
                }
            }
            // tall masts standing in the clearings as landmarks against the moon
            foreach (var (x, z, h) in new[] { (-3.5f, 58f, 16f), (15.5f, 117f, 18f), (-3.5f, 175f, 15f), (17.0f, 270f, 20f) })
            {
                var pm = Props.Mast(h, 0.38f, h * 0.48f, 9f);
                pm.BakeInto(w.Kit, "masts", new Vector3(x, WaterY - 1f, z), Quaternion.Euler(_rng.Range(-6f, 6f), _rng.Range(0, 180), _rng.Range(-8f, 8f)));
                Props.Sail(6f, 4.5f, _rng, "sail", 0.45f).BakeInto(w.Kit, "masts", new Vector3(x, WaterY - 1f + h * 0.48f - 0.2f, z + 0.3f), Yaw(_rng.Range(0, 360)));
            }
        }

        // ------------------------------------------------------------------ beaches
        void Shore(World w, RoomDef r, bool landing)
        {
            var rng = _rng;
            // sand slopes down into the water past the edge of the walkable beach
            var sand = w.Kit.B("shore_" + r.Id, "sand", 3f);
            var rc = r.Rect;
            float m = 3.2f;
            var inner = new[] { new Vector3(rc.X, -0.02f, rc.Z), new Vector3(rc.MaxX, -0.02f, rc.Z), new Vector3(rc.MaxX, -0.02f, rc.MaxZ), new Vector3(rc.X, -0.02f, rc.MaxZ) };
            var outer = new[] { new Vector3(rc.X - m, WaterY - 0.6f, rc.Z - m), new Vector3(rc.MaxX + m, WaterY - 0.6f, rc.Z - m), new Vector3(rc.MaxX + m, WaterY - 0.6f, rc.MaxZ + m), new Vector3(rc.X - m, WaterY - 0.6f, rc.MaxZ + m) };
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                // seen from above and outside: outer edge first, going counter-clockwise
                sand.Quad(outer[i], outer[j], inner[j], inner[i]);
            }
            // rocks and driftwood along the waterline
            for (int i = 0; i < 18; i++)
            {
                float t = rng.Value();
                int side = rng.Range(0, 4);
                Vector3 p;
                switch (side)
                {
                    case 0: p = new Vector3(Mathf.Lerp(rc.X, rc.MaxX, t), 0, rc.Z - rng.Range(0.3f, 1.8f)); break;
                    case 1: p = new Vector3(rc.MaxX + rng.Range(0.3f, 1.8f), 0, Mathf.Lerp(rc.Z, rc.MaxZ, t)); break;
                    case 2: p = new Vector3(Mathf.Lerp(rc.X, rc.MaxX, t), 0, rc.MaxZ + rng.Range(0.3f, 1.8f)); break;
                    default: p = new Vector3(rc.X - rng.Range(0.3f, 1.8f), 0, Mathf.Lerp(rc.Z, rc.MaxZ, t)); break;
                }
                p.y = Mathf.Lerp(-0.05f, WaterY - 0.5f, Mathf.Clamp01(Mathf.Max(Mathf.Max(rc.X - p.x, p.x - rc.MaxX), Mathf.Max(rc.Z - p.z, p.z - rc.MaxZ)) / m));
                if (w.Act.RoomAt(p.x, p.z) != null && w.Act.RoomAt(p.x, p.z) != r) continue;
                if (rng.Chance(0.6f)) Props.Rubble(rng, rng.Range(2, 5), 0.5f, "stone").BakeInto(w.Kit, "shore_" + r.Id, p, Yaw(rng.Range(0, 360)), rng.Range(1.5f, 3f));
                else w.Kit.B("shore_" + r.Id, "wood_dark").Cylinder(p + Vector3.up * 0.1f, Quaternion.Euler(0, rng.Range(0, 180), 88), rng.Range(1.2f, 3f), 0.1f, 0.07f, 7);
            }
            if (!landing) return;
            // Adrian's boat, pulled up on the sand, his gear beside it, its lamp still lit
            var boat = Props.Rowboat();
            boat["wood_dark"].Box(new Vector3(0, 0.55f, -1.75f), new Vector3(0.5f, 0.5f, 0.35f), Quaternion.identity);
            boat["darkmetal"].Box(new Vector3(0, 0.25f, -2.0f), new Vector3(0.12f, 0.6f, 0.12f), Quaternion.identity);
            boat["wood_dark"].Cylinder(new Vector3(0, 0.45f, 0.9f), Quaternion.identity, 2.2f, 0.04f, 0.035f, 6);
            boat.BakeInto(w.Kit, "shore_a2_boat", new Vector3(2.6f, -0.05f, 12.2f), Quaternion.Euler(0, 205, 6), 1.5f);
            w.Kit.Collider(new Vector3(2.6f, 0.45f, 12.2f), new Vector3(2.4f, 0.9f, 5.6f), Yaw(205));
            BoatLamp = new Vector3(2.6f, 0f, 12.2f) + Yaw(205) * new Vector3(0, 3.9f, 1.35f);
            var bag = new PropMesh();
            bag["cloth_blue"].Box(new Vector3(0, 0.18f, 0), new Vector3(0.8f, 0.36f, 0.42f), Quaternion.Euler(0, 0, 3));
            bag["darkmetal"].Cylinder(new Vector3(0.55f, 0f, 0.1f), Quaternion.Euler(0, 0, 12), 0.7f, 0.1f, 0.1f, 10);
            bag["black"].Box(new Vector3(-0.1f, 0.38f, 0), new Vector3(0.36f, 0.04f, 0.2f), Quaternion.identity);
            DressUtil.Bake(w, "shore_a2_boat", bag, new Vector3(5.0f, 0f, 10.6f), Yaw(30), new Vector3(1.0f, 0.4f, 0.5f));
            // his footprints from the boat towards the walkway (boots, a man's stride)
            var fb = w.Kit.B("shore_a2_boat", "footprint", 1f);
            var fpL = Props.Footprint(true); var fpR = Props.Footprint(false);
            for (int i = 0; i < 14; i++)
            {
                var p = Vector3.Lerp(new Vector3(3.6f, 0f, 10.0f), new Vector3(6.0f, 0f, 15.0f), i / 13f);
                var side = (i % 2 == 0 ? -0.12f : 0.12f) * Vector3.right;
                fb.Append(i % 2 == 0 ? fpL : fpR, Matrix4x4.TRS(p + side, Yaw(25 + rng.Range(-6, 6)), Vector3.one * 1.7f));
            }
        }

        Vector3 BoatLamp;

        // ------------------------------------------------------------------ clearings
        void BellFrame(World w, string chunk, Vector3 floorPos, float pivotY, float span = 1.6f)
        {
            var fr = w.Kit.B(chunk, "wood_dark", 1f);
            float top = pivotY + 0.22f;
            foreach (float s in new[] { -0.5f, 0.5f })
            {
                fr.Box(floorPos + new Vector3(s * span, top * 0.5f, 0), new Vector3(0.18f, top, 0.18f), Quaternion.identity);
                fr.Box(floorPos + new Vector3(s * span * 1.25f, 0.6f, 0), new Vector3(0.12f, 1.4f, 0.12f), Quaternion.Euler(0, 0, s * -28f));
                w.Kit.Collider(floorPos + new Vector3(s * span, top * 0.5f, 0), new Vector3(0.2f, top, 0.2f));
            }
            fr.Box(floorPos + new Vector3(0, top + 0.08f, 0), new Vector3(span + 0.5f, 0.18f, 0.2f), Quaternion.identity);
            w.Kit.B(chunk, "iron").Cylinder(floorPos + new Vector3(0, pivotY, 0), Quaternion.identity, top - pivotY, 0.015f, 0.015f, 6, false);
        }

        static float Pivot(float height, int index) => height + (0.26f - index * 0.03f) * 0.71f + 0.05f;

        void ClearingB(World w)
        {
            var rng = _rng;
            BellFrame(w, "a2_b", new Vector3(6f, 0f, 62f), Pivot(1.8f, 0));
            // the first lantern: one light, one note
            LanternB = WreckKit.LanternPost(w, "count_b", new Vector3(8.6f, 0f, 62.6f), 2.7f, true, 320f, rng);
            DeckDebris(w, w.Act.Room("a2_b"), 10);
            // a capstan and a ship's wheel lying where they fell
            Props.Helm().BakeInto(w.Kit, "a2_b", new Vector3(11.5f, 0.12f, 57.2f), Quaternion.Euler(80, 30, 0));
            Grate(w, "a2_b", new Vector3(0.6f, 0f, 60.5f), 1.6f, 1.4f);
        }

        void ClearingC(World w)
        {
            var rng = _rng;
            DeckDebris(w, w.Act.Room("a2_c"), 12);
            // the debris crate's track is worn into the deck: three scuffed places
            foreach (var p in new[] { new Vector3(3f, 0, 114f), new Vector3(8f, 0, 114f), new Vector3(12.5f, 0, 118.5f) })
            {
                var ring = new List<Vector3>();
                for (int i = 0; i <= 14; i++) { float a = i / 14f * Mathf.PI * 2f; ring.Add(p + new Vector3(Mathf.Cos(a) * 0.68f, 0.006f, Mathf.Sin(a) * 0.68f)); }
                w.Kit.B("a2_c", p.x > 12f ? "chalk" : "wood_dark").Tube(ring, p.x > 12f ? 0.014f : 0.02f, 3, 0.2f);
            }
            // drag marks between them
            var drag = w.Kit.B("a2_c", "wood_dark", 1f);
            for (int i = 0; i < 10; i++) drag.Box(new Vector3(Mathf.Lerp(3.6f, 7.4f, i / 9f), 0.004f, 114f + rng.Range(-0.3f, 0.3f)), new Vector3(0.3f, 0.004f, 0.03f), Yaw(rng.Range(-5, 5)));
            for (int i = 0; i < 10; i++) drag.Box(Vector3.Lerp(new Vector3(8.5f, 0.004f, 114.5f), new Vector3(12f, 0.004f, 118f), i / 9f), new Vector3(0.3f, 0.004f, 0.03f), Yaw(-45 + rng.Range(-5, 5)));
        }

        void ClearingD(World w)
        {
            var rng = _rng;
            BellFrame(w, "a2_d", new Vector3(6f, 0f, 170f), Pivot(1.8f, 2));
            LanternsD = new[]
            {
                WreckKit.LanternPost(w, "count_d1", new Vector3(8.7f, 0f, 168.7f), 2.6f, true, 300f, rng),
                WreckKit.LanternPost(w, "count_d2", new Vector3(9.3f, 0f, 170.1f), 2.9f, true, 300f, rng),
                WreckKit.LanternPost(w, "count_d3", new Vector3(8.7f, 0f, 171.5f), 2.5f, true, 300f, rng),
            };
            DeckDebris(w, w.Act.Room("a2_d"), 10);
        }

        Light LanternB;
        Light[] LanternsD;

        void DeckDebris(World w, RoomDef r, int count)
        {
            var rng = _rng;
            for (int i = 0; i < count * 3 && count > 0; i++)
            {
                bool edge = rng.Chance(0.75f);
                float x = edge ? (rng.Chance(0.5f) ? r.Rect.X + rng.Range(0.5f, 1.5f) : r.Rect.MaxX - rng.Range(0.5f, 1.5f)) : rng.Range(r.Rect.X + 1f, r.Rect.MaxX - 1f);
                float z = rng.Range(r.Rect.Z + 1f, r.Rect.MaxZ - 1f);
                if (edge && rng.Chance(0.5f)) { z = rng.Chance(0.5f) ? r.Rect.Z + rng.Range(0.6f, 1.4f) : r.Rect.MaxZ - rng.Range(0.6f, 1.4f); x = rng.Range(r.Rect.X + 1f, r.Rect.MaxX - 1f); }
                var p = new Vector3(x, 0f, z);
                if (!DressUtil.Clear(w, p, 0.8f)) continue;
                // keep the line between the room's entrances clear
                if (Mathf.Abs(p.x - 6f) < 2.4f) continue;
                count--;
                var rot = Yaw(rng.Range(0, 360));
                switch (rng.Range(0, 7))
                {
                    case 0: DressUtil.Bake(w, r.Id, Props.Barrel(0.9f, 0.3f), p, rot, new Vector3(0.6f, 0.9f, 0.6f)); break;
                    case 1: { float s = rng.Range(0.6f, 1f); DressUtil.Bake(w, r.Id, Props.Crate(new Vector3(s, s, s)), p, rot, new Vector3(s, s, s)); break; }
                    case 2: DressUtil.Bake(w, r.Id, Props.Barrel(0.9f, 0.3f), p + Vector3.up * 0.3f, rot * Quaternion.Euler(0, 0, 90)); w.Kit.Collider(p + Vector3.up * 0.3f, new Vector3(0.9f, 0.6f, 0.6f), rot); break;
                    case 3: DressUtil.Bake(w, r.Id, Props.RopeCoil(0.4f), p, rot); break;
                    case 4: DressUtil.Bake(w, r.Id, Props.Planks(rng, rng.Range(3, 6), 2.4f), p, rot); break;
                    case 5: DressUtil.Bake(w, r.Id, Props.SeaChest(), p, rot, new Vector3(0.82f, 0.6f, 0.5f)); break;
                    default: DressUtil.Bake(w, r.Id, Props.Cannon(), p, rot, new Vector3(0.66f, 0.7f, 1.7f)); break;
                }
            }
        }

        static void Grate(World w, string chunk, Vector3 c, float wx, float wz)
        {
            var b = w.Kit.B(chunk, "wood_dark", 1f);
            for (float x = -wx * 0.5f + 0.1f; x < wx * 0.5f; x += 0.2f)
                b.Box(c + new Vector3(x, 0.03f, 0), new Vector3(0.05f, 0.05f, wz), Quaternion.identity);
            for (float z = -wz * 0.5f + 0.1f; z < wz * 0.5f; z += 0.2f)
                b.Box(c + new Vector3(0, 0.05f, z), new Vector3(wx, 0.02f, 0.05f), Quaternion.identity);
        }

        // ------------------------------------------------------------------ the tent wreck
        void Tent(World w)
        {
            var r = w.Act.Room("a2_tent");
            var rng = _rng;
            // an A-frame of sailcloth over a broken hull: it is the shape of a tent, and it is not an accident
            var cloth = w.Kit.B("a2_tent", "sail", 1.5f);
            float y0 = r.CeilH, ridge = r.CeilH + 1.4f;
            float cx = r.Rect.X + r.Rect.W * 0.5f;
            for (int s = -1; s <= 1; s += 2)
            {
                var a0 = new Vector3(cx + s * r.Rect.W * 0.5f, y0, r.Rect.Z);
                var a1 = new Vector3(cx + s * r.Rect.W * 0.5f, y0, r.Rect.MaxZ);
                var b1 = new Vector3(cx, ridge, r.Rect.MaxZ);
                var b0 = new Vector3(cx, ridge, r.Rect.Z);
                if (s < 0) { cloth.Quad(a1, a0, b0, b1); cloth.Quad(a0, a1, b1, b0); }
                else { cloth.Quad(a0, a1, b1, b0); cloth.Quad(a1, a0, b0, b1); }
            }
            w.Kit.B("a2_tent", "wood_dark").Cylinder(new Vector3(cx, ridge, r.Rect.Z - 0.4f), Quaternion.Euler(90, 0, 0), r.Rect.D + 0.8f, 0.08f, 0.08f, 8);
            // the sleeping bag, the camp lamp that went out, the headphones
            var bag = new PropMesh();
            bag["cloth_blue"].Box(new Vector3(0, 0.06f, 0), new Vector3(0.75f, 0.12f, 1.9f), Quaternion.identity);
            bag["cloth_blue"].Box(new Vector3(0, 0.13f, -0.75f), new Vector3(0.6f, 0.1f, 0.35f), Quaternion.identity);
            DressUtil.Bake(w, "a2_tent", bag, new Vector3(9.4f, 0f, 87.4f), Yaw(4), new Vector3(0.75f, 0.2f, 1.9f));
            var box = Props.Crate(new Vector3(0.5f, 0.4f, 0.45f));
            DressUtil.Bake(w, "a2_tent", box, new Vector3(11.6f, 0f, 89.6f), Yaw(10), new Vector3(0.5f, 0.4f, 0.45f));
            var lamp = new PropMesh();
            lamp["darkmetal"].Cylinder(Vector3.zero, Quaternion.identity, 0.04f, 0.07f, 0.07f, 10);
            lamp["glass"].Cylinder(new Vector3(0, 0.04f, 0), Quaternion.identity, 0.14f, 0.055f, 0.055f, 10, false);
            lamp["darkmetal"].Cylinder(new Vector3(0, 0.18f, 0), Quaternion.identity, 0.03f, 0.065f, 0.04f, 10);
            lamp.BakeInto(w.Kit, "a2_tent", new Vector3(12.9f, 0f, 86.0f), Quaternion.Euler(0, 0, 80));
            var shoe = new PropMesh();
            shoe["cloth_red"].Box(new Vector3(0, 0.02f, 0), new Vector3(0.08f, 0.03f, 0.18f), Quaternion.identity);
            shoe["cloth_red"].Box(new Vector3(0, 0.06f, 0.05f), new Vector3(0.07f, 0.01f, 0.012f), Quaternion.identity);
            shoe.BakeInto(w.Kit, "a2_tent", new Vector3(10.2f, 0f, 90.9f), Yaw(140));
            for (int i = 0; i < 6; i++)
                w.Kit.Box("a2_tent", "wood_dark", new Vector3(r.Rect.X + 0.3f + i * 1.1f, 0.6f, r.Rect.MaxZ - 0.15f), new Vector3(0.14f, 1.2f + rng.Range(0, 0.6f), 0.14f), Quaternion.Euler(0, 0, rng.Range(-12f, 12f)));
        }

        // ------------------------------------------------------------------ inside the great hulk
        void HulkInterior(World w)
        {
            var r = w.Act.Room("a2_e");
            var rng = _rng;
            var rib = w.Kit.B("a2_e", "wood_dark", 1f);
            for (float z = r.Rect.Z + 0.8f; z < r.Rect.MaxZ; z += 1.6f)
            {
                rib.Box(new Vector3(r.Rect.X + 0.12f, r.CeilH * 0.5f, z), new Vector3(0.24f, r.CeilH, 0.26f), Quaternion.identity);
                rib.Box(new Vector3(r.Rect.MaxX - 0.12f, r.CeilH * 0.5f, z), new Vector3(0.24f, r.CeilH, 0.26f), Quaternion.identity);
            }
            DressUtil.CeilingBeams(w, r, 1.6f);
            // the second bell hangs from the beams on a chain
            float pv = Pivot(1.8f, 1);
            w.Kit.B("a2_e", "iron").Cylinder(new Vector3(8f, pv, 224f), Quaternion.identity, r.CeilH - pv, 0.016f, 0.016f, 6, false);
            // two lanterns: two lights, the second note
            LanternsE = new[]
            {
                DressUtil.Lantern(w, "count_e1", new Vector3(6.6f, 3.3f, 224.8f), true, 280f, 9f, true, false, 2f),
                DressUtil.Lantern(w, "count_e2", new Vector3(9.4f, 3.3f, 224.8f), true, 280f, 9f, false, false, 2f),
            };
            foreach (float x in new[] { 6.6f, 9.4f })
                w.Kit.B("a2_e", "iron").Cylinder(new Vector3(x, 3.42f, 224.8f), Quaternion.identity, r.CeilH - 3.42f, 0.01f, 0.01f, 4, false);
            // cargo of a ship that went down a long time ago
            foreach (var (p, s) in new[] { (new Vector3(-3.0f, 0, 217.2f), 0.9f), (new Vector3(-2.0f, 0, 217.3f), 0.8f), (new Vector3(14.9f, 0, 217.3f), 1.0f), (new Vector3(15.0f, 0, 227.0f), 0.9f), (new Vector3(-3.1f, 0, 225.5f), 0.85f) })
                DressUtil.Bake(w, "a2_e", Props.Crate(new Vector3(s, s * 0.9f, s)), p, Yaw(rng.Range(-8, 8)), new Vector3(s, s * 0.9f, s));
            foreach (float z in new[] { 219.5f, 220.2f, 221.0f })
                DressUtil.Bake(w, "a2_e", Props.Barrel(0.92f, 0.31f), new Vector3(15.2f, 0, z), Yaw(rng.Range(0, 360)), new Vector3(0.62f, 0.92f, 0.62f));
            for (int i = 0; i < 6; i++)
                DressUtil.Sack(w, "a2_e", new Vector3(rng.Range(10.5f, 14.5f), 0, rng.Range(228.6f, 231.2f)), Yaw(rng.Range(0, 180)), rng);
            DressUtil.Puddles(w, r, 6, rng);
            // daylight never reaches here: only cracks of moon through the planking above
            for (int i = 0; i < 5; i++)
                w.Kit.Box("a2_e", "glow_cold", new Vector3(rng.Range(-2f, 14f), r.CeilH - 0.005f, rng.Range(218f, 231f)), new Vector3(rng.Range(0.6f, 1.8f), 0.004f, 0.05f), Yaw(rng.Range(0, 180)));
        }

        Light[] LanternsE;

        // ------------------------------------------------------------------ the bell hulk
        void BellHulk(World w)
        {
            var r = w.Act.Room("a2_hulk");
            var rng = _rng;
            // one long frame for the three bells, biggest first
            var fr = w.Kit.B("a2_hulk", "wood_dark", 1f);
            float top = 2.45f;
            foreach (float x in new[] { -0.2f, 4.5f, 9.5f, 14.2f })
            {
                fr.Box(new Vector3(x, top * 0.5f, 276f), new Vector3(0.22f, top, 0.22f), Quaternion.identity);
                w.Kit.Collider(new Vector3(x, top * 0.5f, 276f), new Vector3(0.24f, top, 0.24f));
            }
            fr.Box(new Vector3(7f, top + 0.1f, 276f), new Vector3(14.8f, 0.2f, 0.24f), Quaternion.identity);
            float[] hs = { 1.6f, 1.75f, 1.9f };
            float[] xs = { 2f, 7f, 12f };
            for (int i = 0; i < 3; i++)
            {
                float pv = Pivot(hs[i], i);
                w.Kit.B("a2_hulk", "iron").Cylinder(new Vector3(xs[i], pv, 276f), Quaternion.identity, top - pv, 0.016f, 0.016f, 6, false);
                // brass plates with lantern marks: one, two, three
                var plate = new Vector3(xs[i], top - 0.18f, 275.86f);
                w.Kit.Box("a2_hulk", "brass", plate, new Vector3(0.36f, 0.16f, 0.01f));
                DressUtil.Dots(w, "a2_hulk", plate + Vector3.back * 0.006f, Vector3.back, i + 1, 0.08f, 0.022f);
            }
            // the hulk's broken bow rises behind the figurehead
            var bow = w.Kit.B("a2_hulk", "hull", 2.4f);
            for (int s = -1; s <= 1; s += 2)
            {
                var a0 = new Vector3(7f + s * 6f, -1.5f, 284.6f);
                var a1 = new Vector3(7f + s * 0.4f, -1.5f, 290.5f);
                var b1 = new Vector3(7f + s * 0.3f, 5.2f, 289.5f);
                var b0 = new Vector3(7f + s * 7.2f, 5.0f, 285.0f);
                bow.Quad(a0, a1, b1, b0);
                bow.Quad(a1, a0, b0, b1);
            }
            w.Kit.B("a2_hulk", "wood_dark").Cylinder(new Vector3(7f, 3.4f, 288.5f), Quaternion.Euler(-70, 0, 0), 9f, 0.3f, 0.14f, 10);
            DeckDebris(w, r, 9);
            // shells and bones heaped round the horn: what it has already called
            var heap = new PropMesh();
            for (int i = 0; i < 26; i++)
            {
                var p = new Vector3(rng.Range(-0.6f, 0.6f), 0.02f, rng.Range(-0.5f, 0.5f));
                float sz = rng.Range(0.04f, 0.1f);
                heap["bone"].Lathe(p, Quaternion.Euler(rng.Range(60, 120), rng.Range(0, 360), 0), new[] { new Vector2(0.001f, 0f), new Vector2(sz, sz * 0.6f), new Vector2(sz * 0.3f, sz * 1.6f), new Vector2(0.001f, sz * 1.8f) }, 6);
            }
            heap.BakeInto(w.Kit, "a2_hulk", new Vector3(4f, 0f, 272f), Quaternion.identity);
        }

        // ------------------------------------------------------------------ the fork: fog gate and cove
        void Gate(World w)
        {
            var rng = _rng;
            // two bows lean together over the gate like hands
            WreckKit.LeaningMast(w, "gate", new Vector3(1.0f, -0.5f, 296.5f), new Vector3(7.2f, 9.5f, 299.5f), 0.4f, rng, true);
            WreckKit.LeaningMast(w, "gate", new Vector3(13.0f, -0.5f, 296.0f), new Vector3(6.8f, 9.8f, 299.8f), 0.38f, rng, true);
            // the open side is nothing but fog
            var fogGo = new GameObject("gate_fog");
            fogGo.transform.SetParent(w.Root, false);
            fogGo.transform.position = new Vector3(7f, 2.4f, 303f);
            var f = fogGo.AddComponent<LocalVolumetricFog>();
            var p = f.parameters;
            p.albedo = new Color(0.72f, 0.78f, 0.8f);
            p.meanFreePath = 1.4f;
            p.size = new Vector3(16f, 7f, 9f);
            p.blendingMode = LocalVolumetricFogBlendingMode.Additive;
            f.parameters = p;
            // the walkway goes on into the white and stops
            for (int i = 0; i < 6; i++)
                w.Kit.Box("gate", "deck", new Vector3(7f, -0.04f, 299.2f + i * 0.34f), new Vector3(3.6f - i * 0.4f, 0.07f, 0.26f), Yaw(rng.Range(-3f, 3f)));
        }

        void Cove(World w)
        {
            var rng = _rng;
            // a ring of stones and a small fire someone kept going for him
            var ring = new PropMesh();
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                ring["stone"].Box(new Vector3(Mathf.Cos(a) * 0.5f, 0.07f, Mathf.Sin(a) * 0.5f), new Vector3(0.2f, 0.14f, 0.16f), Quaternion.Euler(rng.Range(-10, 10), a * Mathf.Rad2Deg, rng.Range(-10, 10)));
            }
            for (int i = 0; i < 5; i++) ring["wood_dark"].Cylinder(new Vector3(0, 0.05f, 0), Quaternion.Euler(70, i * 72f, 0), 0.5f, 0.04f, 0.03f, 6);
            ring["ember"].Disc(new Vector3(0, 0.06f, 0), Quaternion.identity, 0.3f, 12);
            ring.BakeInto(w.Kit, "a2_cove", new Vector3(16.2f, 0f, 292.4f), Quaternion.identity);
            w.Kit.Collider(new Vector3(16.2f, 0.15f, 292.4f), new Vector3(1.1f, 0.3f, 1.1f));
            var fm = new PropMesh();
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                fm["flame"].Lathe(new Vector3(Mathf.Cos(a) * 0.1f, 0f, Mathf.Sin(a) * 0.1f), Quaternion.identity, new[] { new Vector2(0f, 0f), new Vector2(0.08f, 0.07f), new Vector2(0.045f, 0.26f), new Vector2(0f, 0.4f) }, 6);
            }
            var fire = fm.Spawn("cove_fire", w.Root, new Vector3(16.2f, 0.06f, 292.4f), Quaternion.identity, false);
            var l = LightKit.Point("cove_fire_light", fire.transform, new Vector3(0, 0.5f, 0), LightKit.Candle, 750f, 11f, true, true, 1f, 0.2f);
            var fl = fire.AddComponent<Flicker>();
            fl.Target = l; fl.Base = 750f; fl.Amount = 0.22f; fl.Speed = 8f;
            fl.Flame = fire.GetComponentInChildren<Renderer>();
            w.FlickerLights.Add(l);
            w.LightZones.Add(new LightZone { Pos = new Vector3(16.2f, 1f, 292.4f), Radius = 6f, Safe = true });
            // a child's shell collection laid out in a row on the sand
            var shells = new PropMesh();
            for (int i = 0; i < 9; i++)
                shells["porcelain"].Lathe(new Vector3(i * 0.16f, 0, rng.Range(-0.03f, 0.03f)), Quaternion.Euler(rng.Range(-15, 15), rng.Range(0, 360), rng.Range(-15, 15)), new[] { new Vector2(0f, 0f), new Vector2(0.045f, 0.015f), new Vector2(0.05f, 0.03f), new Vector2(0f, 0.045f) }, 7);
            shells.BakeInto(w.Kit, "a2_cove", new Vector3(17.6f, 0f, 289.4f), Yaw(-20));
        }

        // ------------------------------------------------------------------ light
        public void Light(World w, GameState state)
        {
            var act = w.Act;
            w.Atmos = Atmos.Create(w.Root, new AtmosPreset
            {
                FogDistance = act.FogDistance,
                FogTint = new Color(act.FogColor[0], act.FogColor[1], act.FogColor[2]),
                FogAlbedo = new Color(0.58f, 0.66f, 0.72f),
                FogHeight = 7f,
                SkyTop = new Color(0.02f, 0.03f, 0.05f),
                SkyMid = new Color(0.05f, 0.07f, 0.1f),
                SkyBottom = new Color(0.08f, 0.1f, 0.12f),
                SkyExposure = 1.5f,
                Exposure = 2.0f,
                Saturation = -20f,
                Contrast = 14f,
                Filter = new Color(0.88f, 0.95f, 1f),
                Bloom = 0.26f,
                Vignette = 0.3f,
            });
            w.MoonLux = 4f * act.Moon;
            w.Moon = LightKit.Directional("Moon", w.Root, Quaternion.Euler(34f, 168f, 0f), LightKit.Moon, w.MoonLux, true);
            foreach (var r in act.Rooms)
                if (r.Ceil) w.Atmos.Interior(r.Id, new Vector3(r.Rect.X + r.Rect.W * 0.5f, r.Floor + r.CeilH * 0.5f, r.Rect.Z + r.Rect.D * 0.5f), new Vector3(r.Rect.W, r.CeilH, r.Rect.D), r.Ambient, 30f);
            // the boat's own lamp on the beach: the first light of the act
            var boat = DressUtil.Lantern(w, "boat_lamp", BoatLamp, true, 260f, 8f, true, false, 3f);
            w.LightZones.Add(new LightZone { Pos = BoatLamp + Vector3.down * 1.6f, Radius = 3.5f });
            foreach (var l in new[] { LanternB }) if (l != null) w.LightZones.Add(new LightZone { Pos = l.transform.position + Vector3.down * 1f, Radius = 3.4f });
            if (LanternsD != null) foreach (var l in LanternsD) if (l != null) w.LightZones.Add(new LightZone { Pos = l.transform.position + Vector3.down * 1f, Radius = 3f });
            if (LanternsE != null) foreach (var l in LanternsE) if (l != null) w.LightZones.Add(new LightZone { Pos = l.transform.position + Vector3.down * 1.4f, Radius = 3.2f });
            // something pale glows behind the fog gate, too cold to be a lamp
            LightKit.Point("gate_glow", w.Root, new Vector3(7f, 1.6f, 302.5f), LightKit.Sea, 420f, 12f, false, false, 2.2f, 0.4f);
            // the debris falls away when the crate puzzle is solved
            w.On("a2_clear", rt => rt.Player.Shake(0.35f, 0.8f));
            w.On("a2_bells", rt =>
            {
                // the hulk answers the bells: the figurehead's eyes catch the moon, the fog thins
                Game.Audio.PlayAt("sfx_hull_groan_big", new Vector3(7f, 1f, 282f), Bus.Effects, 0.8f, 1.1f, 4f, 60f);
                rt.Player.Shake(0.3f, 1.5f);
            });
        }
    }
}
