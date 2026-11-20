using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Act IV — the sunken temple: pale stone under a ceiling of moving water, turquoise shafts of light,
    /// a beam carried by four mirrors and a lens to the altar, a whale's bones, and something vast outside.
    /// </summary>
    public sealed class Act4Dressing : IActDressing
    {
        Rng _rng;
        readonly List<(GameObject go, string flag)> _beams = new List<(GameObject, string)>();
        static Quaternion Yaw(float d) => Quaternion.Euler(0, d, 0);
        static bool WaterCeil(RoomDef r) => r.Id == "a4_cross" || r.Id == "a4_gal" || r.Id == "a4_altar" || r.Id == "a4_nave";

        public LevelStyle Style(World w)
        {
            var st = new LevelStyle();
            st.FloorMat = r => r.IsRamp ? "temple" : "temple_floor";
            st.CeilMat = r => WaterCeil(r) ? "water_ceiling" : "temple";
            st.FrameMat = o => "temple";
            st.Wall = wp =>
            {
                // the gallery's sea side is a great window: drawn by the dressing, the wall stays a collider
                if (wp.Room.Id == "a4_gal" && wp.Kind == EdgeKind.Void && !wp.AlongX && wp.A.X > 21.5f) return new WallLook(WallMode.Hidden, "temple");
                return new WallLook(WallMode.Solid, "temple");
            };
            st.Steps = r => r.IsRamp;
            return st;
        }

        public void Dress(World w, GameState state)
        {
            _rng = new Rng(4409);
            Columns(w);
            Nave(w);
            BurnedHall(w);
            CrossHall(w);
            Alcoves(w);
            Gallery(w);
            Ramp(w);
            AltarRoom(w);
            Beams(w, state);
            Outside(w);
        }

        // ------------------------------------------------------------------ architecture
        void Columns(World w)
        {
            var rng = _rng;
            foreach (var r in w.Act.Rooms)
            {
                if (r.Kind != RoomKind.Temple || r.Rect.W < 12f) continue;
                float h = r.CeilH;
                // two rows of columns, a little in from the walls, some broken
                float inset = r.Id == "a4_cross" ? 2.2f : 1.6f;
                for (float z = r.Rect.Z + 2.5f; z < r.Rect.MaxZ - 1.5f; z += r.Id == "a4_cross" ? 4.2f : 3.6f)
                {
                    foreach (float x in new[] { r.Rect.X + inset, r.Rect.MaxX - inset })
                    {
                        var p = new Vector3(x, r.Floor, z);
                        if (!DressUtil.Clear(w, p, 0.7f)) continue;
                        bool broken = rng.Chance(0.18f);
                        float ch = broken ? rng.Range(1.5f, h * 0.6f) : h;
                        Props.Column(ch, 0.42f, "temple", broken).BakeInto(w.Kit, r.Id, p, Yaw(rng.Range(0, 90)));
                        w.Kit.Collider(p + Vector3.up * (ch * 0.5f), new Vector3(0.75f, ch, 0.75f));
                        if (broken) Props.Rubble(rng, 7, 0.9f, "temple").BakeInto(w.Kit, r.Id, p + new Vector3(rng.Range(-1f, 1f), 0, rng.Range(-1f, 1f)), Yaw(rng.Range(0, 360)));
                        // kelp and shells grown up the shafts
                        if (rng.Chance(0.5f)) w.Kit.B(r.Id, "algae").Cylinder(p, Quaternion.identity, rng.Range(0.6f, 1.6f), 0.45f, 0.44f, 12, false);
                    }
                }
            }
        }

        void Nave(World w)
        {
            var r = w.Act.Room("a4_nave");
            var rng = _rng;
            // reliefs along the walls: children holding shells to their ears
            for (int i = 0; i < 4; i++)
            {
                Props.Framed("relief", 1.6f, 1.0f, "temple", 0.1f).BakeInto(w.Kit, "a4_nave", new Vector3(0.03f, 1.6f, 2.5f + i * 3.2f), Yaw(90));
                Props.Framed("relief", 1.6f, 1.0f, "temple", 0.1f).BakeInto(w.Kit, "a4_nave", new Vector3(17.97f, 1.6f, 2.5f + i * 3.2f), Yaw(-90));
            }
            // the safe light here comes from a ring of shells glowing on the floor
            var ring = new PropMesh();
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                ring["glow_cold"].Lathe(new Vector3(Mathf.Cos(a) * 1.6f, 0f, Mathf.Sin(a) * 1.6f), Quaternion.Euler(rng.Range(70, 110), rng.Range(0, 360), 0), new[] { new Vector2(0.001f, 0f), new Vector2(0.07f, 0.04f), new Vector2(0.02f, 0.11f), new Vector2(0.001f, 0.12f) }, 6);
            }
            ring.BakeInto(w.Kit, "a4_nave", new Vector3(9f, 0f, 6.5f), Quaternion.identity);
            NaveGlow = new Vector3(9f, 0.6f, 6.5f);
            for (int i = 0; i < 5; i++) Props.Rubble(rng, 5, 0.6f, "temple").BakeInto(w.Kit, "a4_nave", new Vector3(rng.Range(1f, 17f), 0f, rng.Range(1f, 13f)), Yaw(rng.Range(0, 360)));
        }

        Vector3 NaveGlow;

        void BurnedHall(World w)
        {
            var r = w.Act.Room("a4_hall");
            var rng = _rng;
            // somebody else came this far with a lamp: scorch marks, a melted lantern, burnt offerings
            for (int i = 0; i < 14; i++)
                w.Kit.Box("a4_hall", "black", new Vector3(rng.Range(6.4f, 11.6f), 0.004f, rng.Range(14.5f, 23.5f)), new Vector3(rng.Range(0.3f, 1.2f), 0.004f, rng.Range(0.3f, 1.2f)), Yaw(rng.Range(0, 180)));
            for (int i = 0; i < 6; i++)
                w.Kit.Box("a4_hall", "black", new Vector3(rng.Chance(0.5f) ? 6.03f : 11.97f, rng.Range(0.8f, 3.6f), rng.Range(15f, 23f)), new Vector3(0.02f, rng.Range(0.6f, 1.6f), rng.Range(0.4f, 1.4f)));
            Props.HangingLantern(false).BakeInto(w.Kit, "a4_hall", new Vector3(7.2f, 0.06f, 21.4f), Quaternion.Euler(0, 30, 90));
            Props.Candles(rng, 6).BakeInto(w.Kit, "a4_hall", new Vector3(10.9f, 0f, 16.3f), Yaw(0));
        }

        void CrossHall(World w)
        {
            var r = w.Act.Room("a4_cross");
            var rng = _rng;
            // the source: a crystal on a pedestal where the shaft from the ceiling lands
            var src = new PropMesh();
            src["temple"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.5f, 0f), new Vector2(0.45f, 0.15f), new Vector2(0.3f, 0.2f), new Vector2(0.28f, 1.1f), new Vector2(0.4f, 1.2f), new Vector2(0.4f, 1.28f) }, 14);
            src["glass"].Lathe(new Vector3(0, 1.28f, 0), Quaternion.identity, new[] { new Vector2(0.001f, 0f), new Vector2(0.14f, 0.1f), new Vector2(0.1f, 0.28f), new Vector2(0.001f, 0.36f) }, 6);
            DressUtil.Bake(w, "a4_cross", src, new Vector3(Act4Content.BeamSource.X, 0f, Act4Content.BeamSource.Z), Yaw(0), new Vector3(0.9f, 1.3f, 0.9f));
            w.Anchor("beam_source", new Vector3(Act4Content.BeamSource.X, 1.45f, Act4Content.BeamSource.Z));
            // a long floor mosaic of the tide, cracked across
            DressUtil.Rug(w, r, new Vector3(10f, 0, 41f), 3.2f, 26f, "temple", 0f);
            for (int i = 0; i < 10; i++) w.Kit.Box("a4_cross", "black", new Vector3(10f + rng.Range(-1.4f, 1.4f), 0.012f, 29f + i * 2.5f + rng.Range(-0.5f, 0.5f)), new Vector3(rng.Range(0.6f, 1.6f), 0.002f, 0.025f), Yaw(rng.Range(0, 180)));
            // fallen ceiling blocks along the sides
            for (int i = 0; i < 8; i++)
            {
                var p = new Vector3(rng.Chance(0.5f) ? rng.Range(0.8f, 1.6f) : rng.Range(18.4f, 19.2f), 0f, rng.Range(26f, 56f));
                if (!DressUtil.Clear(w, p, 0.8f)) continue;
                float s = rng.Range(0.6f, 1.2f);
                DressUtil.Bake(w, "a4_cross", Props.Crate(new Vector3(s, s * 0.8f, s)), p, Quaternion.Euler(rng.Range(-6, 6), rng.Range(0, 90), rng.Range(-6, 6)), new Vector3(s, s * 0.8f, s));
            }
        }

        void Alcoves(World w)
        {
            var rng = _rng;
            // each mirror alcove is carved with scratches: the number of turns its mirror needs
            string[] ids = { "a4_m1r", "a4_m2r", "a4_m3r", "a4_m4r" };
            for (int i = 0; i < 4; i++)
            {
                var r = w.Act.Room(ids[i]);
                bool west = r.Rect.X < 0f;
                float wx = west ? r.Rect.X + 0.03f : r.Rect.MaxX - 0.03f;
                var n = west ? Vector3.right : Vector3.left;
                var c = new Vector3(wx, 2.0f, r.Rect.Z + r.Rect.D * 0.5f);
                var rot = Quaternion.LookRotation(n);
                var scr = w.Kit.B(ids[i], "chalk", 0.5f);
                int turns = Act4Content.MirrorAnswers[i];
                for (int k = 0; k < turns; k++)
                    scr.Box(c + rot * new Vector3((k - (turns - 1) * 0.5f) * 0.14f, 0, 0.01f), new Vector3(0.025f, 0.5f, 0.004f), rot * Quaternion.Euler(0, 0, 8f));
                Props.Framed("relief", 1.2f, 0.8f, "temple", 0.08f).BakeInto(w.Kit, ids[i], c + Vector3.up * 0.7f - rot * Vector3.forward * 0.0f, rot);
                Props.Candles(rng, 5).BakeInto(w.Kit, ids[i], new Vector3(west ? r.Rect.X + 0.6f : r.Rect.MaxX - 0.6f, 0f, r.Rect.Z + 1.2f), Yaw(0));
            }
        }

        void Gallery(World w)
        {
            var r = w.Act.Room("a4_gal");
            var rng = _rng;
            // the whale: spine along the hall, ribs arching overhead, the skull resting by the ramp
            var bone = w.Kit.B("a4_gal", "bone", 1f);
            var spine = new List<Vector3>();
            for (int i = 0; i <= 16; i++)
            {
                float t = i / 16f;
                spine.Add(new Vector3(10f + Mathf.Sin(t * 3f) * 0.6f, 7.4f + Mathf.Sin(t * Mathf.PI) * 0.8f, 59.5f + t * 15f));
            }
            bone.Tube(spine, 0.22f, 8, 0.6f);
            for (int i = 1; i < 16; i += 1)
            {
                var top = spine[i];
                foreach (int s in new[] { -1, 1 })
                {
                    var rib = new List<Vector3>();
                    float span = 5.6f * Mathf.Sin((i / 16f) * Mathf.PI) + 1.2f;
                    for (int k = 0; k <= 8; k++)
                    {
                        float a = k / 8f * Mathf.PI * 0.55f;
                        rib.Add(top + new Vector3(s * Mathf.Sin(a) * span, -Mathf.Sin(a) * Mathf.Sin(a) * 4.2f, 0f));
                    }
                    bone.Tube(rib, 0.09f, 6, 0.4f);
                }
            }
            var skull = new PropMesh();
            skull["bone"].Lathe(Vector3.zero, Quaternion.Euler(90, 0, 0), new[] { new Vector2(0.3f, 0f), new Vector2(1.2f, 0.8f), new Vector2(1.4f, 2.4f), new Vector2(1.0f, 3.8f), new Vector2(0.3f, 4.4f) }, 12);
            DressUtil.Bake(w, "a4_gal", skull, new Vector3(2.4f, 0.9f, 73.2f), Yaw(-30), new Vector3(2.4f, 1.8f, 3.6f), 0.9f);
            // the great window on the sea side, with the stone around it
            float x = r.Rect.MaxX;
            var stone = w.Kit.B("a4_gal", "temple", 2f);
            stone.BoxMinMax(new Vector3(x, 0f, 58f), new Vector3(x + 0.6f, r.CeilH, 62f));
            stone.BoxMinMax(new Vector3(x, 0f, 74f), new Vector3(x + 0.6f, r.CeilH, 76f));
            stone.BoxMinMax(new Vector3(x, 0f, 62f), new Vector3(x + 0.6f, 1.0f, 74f));
            stone.BoxMinMax(new Vector3(x, 8.4f, 62f), new Vector3(x + 0.6f, r.CeilH, 74f));
            w.Kit.Box("a4_gal", "glass", new Vector3(x + 0.3f, 4.7f, 68f), new Vector3(0.03f, 7.4f, 12f));
            for (float z = 64f; z < 74f; z += 2f) w.Kit.Box("a4_gal", "temple", new Vector3(x + 0.3f, 4.7f, z), new Vector3(0.3f, 7.4f, 0.22f));
            w.Kit.Box("a4_gal", "temple", new Vector3(x + 0.3f, 4.7f, 68f), new Vector3(0.32f, 0.24f, 12f));
            // a wall of shells to listen to (the item draws the shells; a niche holds them)
            w.Kit.Box("a4_gal", "temple", new Vector3(16f, 1.25f, 75.7f), new Vector3(3.4f, 2.7f, 0.5f));
        }

        void Ramp(World w)
        {
            var r = w.Act.Room("a4_ramp");
            // drowned lamps hang along the descent
            for (float z = 78f; z < 89f; z += 3.5f)
            {
                DressUtil.Lantern(w, "ramp_lamp_" + z, new Vector3(7.35f, r.FloorAt(10f, z) + 3.4f, z), false, 0, 0, false, false, 0.6f);
                DressUtil.Lantern(w, "ramp_lamp_e" + z, new Vector3(12.65f, r.FloorAt(10f, z) + 3.4f, z + 1.7f), false, 0, 0, false, false, 0.6f);
            }
        }

        void AltarRoom(World w)
        {
            var r = w.Act.Room("a4_altar");
            var rng = _rng;
            float f = r.Floor;
            // steps up to the altar dais and a circle of standing stones
            w.Kit.Box("a4_altar", "temple", new Vector3(10f, f + 0.08f, 100f), new Vector3(6f, 0.16f, 4.4f), true);
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                var p = new Vector3(10f + Mathf.Cos(a) * 6.2f, f, 100f + Mathf.Sin(a) * 5.2f);
                if (!DressUtil.Clear(w, p, 0.6f)) continue;
                float h = rng.Range(1.8f, 3.4f);
                w.Kit.Box("a4_altar", "temple", p + Vector3.up * (h * 0.5f), new Vector3(0.7f, h, 0.5f), Quaternion.Euler(rng.Range(-4, 4), a * Mathf.Rad2Deg + 90f, rng.Range(-4, 4)), true);
            }
            // the choir: hundreds of small shells set into the far wall, each one a voice
            var choir = new PropMesh();
            for (int i = 0; i < 260; i++)
            {
                var p = new Vector3(rng.Range(-9f, 9f), rng.Range(0.6f, 7.5f), 0f);
                float s = rng.Range(0.04f, 0.1f);
                choir[rng.Chance(0.12f) ? "glow_cold" : "bone"].Lathe(p, Quaternion.Euler(-90 + rng.Range(-20, 20), rng.Range(0, 360), 0), new[] { new Vector2(0.001f, 0f), new Vector2(s, s * 0.6f), new Vector2(s * 0.3f, s * 1.6f), new Vector2(0.001f, s * 1.8f) }, 6);
            }
            choir.BakeInto(w.Kit, "a4_altar", new Vector3(10f, f, 107.95f), Quaternion.identity);
            w.Anchor("choir", new Vector3(10f, f + 4f, 107.5f));
            // the two tables for the music boxes are made by the items; candles at their feet
            foreach (float x in new[] { 3f, 17f })
                Props.Candles(rng, 6).BakeInto(w.Kit, "a4_altar", new Vector3(x + (x < 10f ? -0.9f : 0.9f), f, 104f), Yaw(0));
            for (int i = 0; i < 5; i++) Props.Rubble(rng, 5, 0.7f, "temple").BakeInto(w.Kit, "a4_altar", new Vector3(rng.Range(1f, 19f), f, rng.Range(91f, 107f)), Yaw(rng.Range(0, 360)));
        }

        // ------------------------------------------------------------------ the beam
        void Beams(World w, GameState state)
        {
            float y = 1.42f;
            var src = new Vector3(Act4Content.BeamSource.X, y, Act4Content.BeamSource.Z);
            var m = new Vector3[4];
            for (int i = 0; i < 4; i++) m[i] = new Vector3(Act4Content.MirrorPos[i].X, y + 0.1f, Act4Content.MirrorPos[i].Z);
            var lens = new Vector3(Act4Content.LensPos.X, 1.25f, Act4Content.LensPos.Z);
            var altar = new Vector3(Act4Content.AltarPos.X, -1.6f + 1.25f, Act4Content.AltarPos.Z);
            Segment(w, "beam0", src, m[0], null, state);
            Segment(w, "beam1", m[0], m[1], "a4_m1", state);
            Segment(w, "beam2", m[1], m[2], "a4_m2", state);
            Segment(w, "beam3", m[2], m[3], "a4_m3", state);
            Segment(w, "beam4", m[3], lens, "a4_m4", state);
            Segment(w, "beam5", lens + Vector3.forward * 0.3f, altar, "a4_altar", state);
            // a shaft from the water ceiling onto the source, always
            var shaft = new PropMesh();
            shaft["shaft"].Cylinder(Vector3.zero, Quaternion.identity, 7.6f, 0.45f, 1.2f, 16, false);
            shaft.Spawn("source_shaft", w.Root, new Vector3(src.x, 1.4f, src.z), Quaternion.identity, false);
            foreach (var f in new[] { "a4_m1", "a4_m2", "a4_m3", "a4_m4", "a4_altar" })
            {
                string flag = f;
                w.On(flag, rt => Refresh(rt.State));
            }
            // a mirror turned out of line clears the flags after it: refresh on every turn too
            foreach (var it in w.Act.Items)
                if (it.Kind == ItemKind.Mirror) w.On("turn:" + it.Id, rt => Refresh(rt.State));
        }

        void Segment(World w, string name, Vector3 a, Vector3 b, string flag, GameState state)
        {
            var d = b - a;
            var pm = new PropMesh();
            pm["beam"].Cylinder(Vector3.zero, Quaternion.FromToRotation(Vector3.up, d.normalized), d.magnitude, 0.05f, 0.05f, 8, false);
            pm["shaft"].Cylinder(Vector3.zero, Quaternion.FromToRotation(Vector3.up, d.normalized), d.magnitude, 0.16f, 0.16f, 10, false);
            var go = pm.Spawn(name, w.Root, a, Quaternion.identity, false);
            var l = LightKit.Point(name + "_glow", go.transform, d * 0.5f, LightKit.Sea, 220f, Mathf.Min(12f, d.magnitude * 0.6f), false, false, 0.6f, 0.2f);
            go.SetActive(flag == null || state.Has(flag));
            _beams.Add((go, flag));
        }

        public void Refresh(GameState s)
        {
            foreach (var (go, flag) in _beams)
            {
                if (go == null) continue;
                bool on = flag == null || s.Has(flag);
                if (go.activeSelf != on)
                {
                    go.SetActive(on);
                    if (on) Game.Audio.PlayAt("sfx_beam_on", go.transform.position, Bus.Effects, 0.7f);
                }
            }
        }

        // ------------------------------------------------------------------ outside the window
        void Outside(World w)
        {
            // the Undertow: a shape too big to be an animal, gliding through the murk, never close
            var body = new PropMesh();
            var prof = new List<Vector2>();
            for (int i = 0; i <= 14; i++)
            {
                float t = i / 14f;
                prof.Add(new Vector2(Mathf.Sin(t * Mathf.PI) * 9f * (1f - t * 0.35f) + 0.01f, t * 70f));
            }
            body["silhouette"].Lathe(Vector3.zero, Quaternion.Euler(90, 0, 0), prof, 16);
            for (int i = 0; i < 5; i++)
                body["silhouette"].Tube(new[] { new Vector3(0, -4f, 50f + i * 3f), new Vector3((i - 2) * 5f, -16f, 62f + i * 4f), new Vector3((i - 2) * 9f, -26f, 74f + i * 6f) }, 1.2f - i * 0.1f, 6, 4f);
            var go = body.Spawn("undertow", w.Root, new Vector3(95f, -18f, 30f), Yaw(-12), false);
            var drift = go.AddComponent<Drift>();
            drift.Speed = 0.012f;
            drift.Span = 60f;
            // sea floor beyond the window, and the glow of other ruins far off
            var floor = WaterSurface.CreateFlat(w.Root, "seabed", new RectXZ(22.6f, 20f, 160f, 110f), -6f, false);
            w.Waters.Add(floor);
            LightKit.Point("far_ruin_glow", w.Root, new Vector3(60f, -2f, 80f), LightKit.Sea, 6000f, 60f, false, false, 2f, 3f);
        }

        // ------------------------------------------------------------------ light
        public void Light(World w, GameState state)
        {
            var act = w.Act;
            w.Atmos = Atmos.Create(w.Root, new AtmosPreset
            {
                FogDistance = act.FogDistance,
                FogTint = new Color(act.FogColor[0], act.FogColor[1], act.FogColor[2]),
                FogAlbedo = new Color(0.3f, 0.75f, 0.78f),
                FogHeight = 14f,
                FogAnisotropy = 0.7f,
                SkyTop = new Color(0.02f, 0.12f, 0.14f),
                SkyMid = new Color(0.01f, 0.07f, 0.08f),
                SkyBottom = new Color(0.005f, 0.03f, 0.035f),
                SkyExposure = 1.2f,
                Exposure = 2.0f,
                Saturation = -6f,
                Contrast = 10f,
                Filter = new Color(0.82f, 1f, 0.98f),
                Bloom = 0.34f,
                Vignette = 0.32f,
            });
            w.MoonLux = 0f;
            foreach (var r in act.Rooms)
                if (r.Ceil) w.Atmos.Interior(r.Id, new Vector3(r.Rect.X + r.Rect.W * 0.5f, r.MinFloor + r.CeilH * 0.5f, r.Rect.Z + r.Rect.D * 0.5f), new Vector3(r.Rect.W, r.CeilH + (r.IsRamp ? 1.6f : 0f), r.Rect.D), Mathf.Max(0.15f, r.Ambient), 40f);
            // turquoise shafts falling through the water ceiling
            var shafts = new[]
            {
                (new Vector3(5f, 5.8f, 7f), 2600f, 34f), (new Vector3(13f, 5.8f, 4f), 2200f, 30f),
                (new Vector3(10f, 8.8f, 26f), 5200f, 26f), (new Vector3(4f, 8.8f, 38f), 3000f, 36f), (new Vector3(16f, 8.8f, 50f), 3000f, 36f),
                (new Vector3(6f, 9.8f, 64f), 3600f, 38f), (new Vector3(15f, 9.8f, 71f), 3200f, 34f),
                (new Vector3(10f, 8.2f, 100f), 4200f, 30f), (new Vector3(4f, 8.2f, 94f), 2000f, 40f),
            };
            int k = 0;
            foreach (var (pos, lm, ang) in shafts)
                LightKit.Spot("shaft_" + k++, w.Root, pos, Quaternion.Euler(86f, k * 37f, 0f), LightKit.Sea, lm, 14f, ang, k % 3 == 0, false, 1.8f);
            // the nave's shell ring: the safe light at the start
            LightKit.Point("nave_glow", w.Root, NaveGlow, LightKit.Sea, 600f, 9f, false, false, 0.8f, 1.2f);
            w.LightZones.Add(new LightZone { Pos = NaveGlow, Radius = 6f, Safe = true });
        }
    }

    /// <summary>Very slow back-and-forth drift (the silhouette outside the temple).</summary>
    public sealed class Drift : MonoBehaviour
    {
        public float Speed = 0.01f;
        public float Span = 50f;
        Vector3 _base;
        float _ph;

        void Start()
        {
            _base = transform.position;
            _ph = Random.value * 6f;
        }

        void Update()
        {
            float t = Time.time * Speed + _ph;
            transform.position = _base + new Vector3(Mathf.Sin(t * 0.7f) * 6f, Mathf.Sin(t * 1.3f) * 3f, Mathf.Sin(t) * Span);
            transform.rotation = Quaternion.Euler(Mathf.Sin(t * 1.1f) * 3f, -12f + Mathf.Cos(t) * 10f, 0f);
        }
    }
}
