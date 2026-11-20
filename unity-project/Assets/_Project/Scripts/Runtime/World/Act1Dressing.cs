using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Act I — the galleon "Merciful Requiem". Stern at z = 0, bow towards +z, centre line x = 8.
    /// Upper deck y = 0, gun deck y = -4, sea at y = -5.2. The upper deck between z 56 and 92 has
    /// collapsed into the cargo hold.
    /// </summary>
    public sealed partial class Act1Dressing : IActDressing
    {
        const float CX = 8f;
        const float Sea = -5.2f;
        const float CollapseA = 56f, CollapseB = 92f;
        static readonly float[] SecY = { 0f, -1.2f, -3f, -4.2f, -5.4f, -6.8f };
        static readonly float[] SecF = { 1f, 1.03f, 1.03f, 1.005f, 0.86f, 0.3f };

        Rng _rng;

        // ------------------------------------------------------------------ shape of the hull
        /// <summary>Half-width of the hull's outer skin at deck level (bulwarks sit just inside it).</summary>
        static float Hw(float z)
        {
            if (z <= 70f) return 8.2f;
            if (z <= 90f) return Mathf.Lerp(8.2f, 6.2f, (z - 70f) / 20f);
            if (z <= 108.2f) return 6.2f;
            float t = Mathf.Clamp01((z - 108.2f) / 6.3f);
            return Mathf.Max(0.12f, 6.2f * Mathf.Sqrt(1f - t * t));
        }

        static float F(float y)
        {
            if (y >= SecY[0]) return SecF[0];
            for (int i = 0; i < SecY.Length - 1; i++)
                if (y <= SecY[i] && y >= SecY[i + 1])
                    return Mathf.Lerp(SecF[i], SecF[i + 1], (SecY[i] - y) / (SecY[i] - SecY[i + 1]));
            return SecF[SecF.Length - 1];
        }

        static bool Collapsed(float z) => z > CollapseA + 0.3f && z < CollapseB - 0.3f;

        float TopAt(float z)
        {
            if (!Collapsed(z)) return 0f;
            // jagged broken sheer line where the deck tore away
            float k = Mathf.Sin(z * 1.7f) * 0.18f + Mathf.Sin(z * 4.3f + 1.1f) * 0.12f;
            float mid = 1f - Mathf.Abs((z - (CollapseA + CollapseB) * 0.5f) / ((CollapseB - CollapseA) * 0.5f));
            return -0.15f - mid * 0.55f + k;
        }

        static Vector3 L(float z, float y) => new Vector3(CX - Hw(z) * F(y), y, z);
        static Vector3 R(float z, float y) => new Vector3(CX + Hw(z) * F(y), y, z);

        // ------------------------------------------------------------------ style of the compiled level
        public LevelStyle Style(World w)
        {
            var st = new LevelStyle();
            st.FloorMat = r =>
            {
                if (r.Kind == RoomKind.Deck) return "deck";
                if (r.IsRamp) return "wood";
                if (r.Id == "a1_cargo") return "deck";
                return "deck_dry";
            };
            st.CeilMat = r => r.Id == "a1_capt" ? "panel" : "deck_dry";
            st.Steps = r => r.IsRamp;
            st.FrameMat = o => "wood_dark";
            st.Wall = wp =>
            {
                var r = wp.Room;
                bool alongX = wp.AlongX;
                float z = (wp.A.Z + wp.B.Z) * 0.5f;
                bool collapseEdge = alongX && (Mathf.Abs(wp.A.Z - CollapseA) < 0.05f || Mathf.Abs(wp.A.Z - CollapseB) < 0.05f);
                if (r.Kind == RoomKind.Deck)
                {
                    if (wp.Kind == EdgeKind.Void) return collapseEdge ? new WallLook(WallMode.Hidden, "hull") : new WallLook(WallMode.Bulwark, "hull", "wood_dark");
                    if (wp.Kind == EdgeKind.Drop) return collapseEdge ? new WallLook(WallMode.Hidden, "hull") : new WallLook(WallMode.Railing, "wood_dark", "wood_dark");
                    if (wp.Kind == EdgeKind.Partition && (wp.Other != null && wp.Other.Id == "a1_capt")) return new WallLook(WallMode.Solid, "wood_dark");
                    return new WallLook(WallMode.Solid, "panel");
                }
                if (r.Id == "a1_capt") return new WallLook(WallMode.Solid, "wood_dark");
                if (r.Id == "a1_stair" || r.Id == "a1_bowramp")
                {
                    // the open part of each companion way crosses the collapsed hold: railings, not walls
                    bool open = r.Id == "a1_stair" ? z > CollapseA + 0.2f : z < CollapseB - 0.2f;
                    if (open && (wp.Kind == EdgeKind.Void || wp.Kind == EdgeKind.Drop)) return new WallLook(WallMode.Railing, "wood_dark", "wood_dark");
                    return new WallLook(WallMode.Solid, "panel");
                }
                if (r.Id == "a1_cargo") return new WallLook(WallMode.Solid, "hull_dark");
                if (wp.Kind == EdgeKind.Void && !alongX)
                {
                    float x = wp.A.X;
                    if (x <= 0.6f || x >= 15.4f) return new WallLook(WallMode.Solid, "hull_dark");
                }
                return new WallLook(WallMode.Solid, "panel");
            };
            return st;
        }

        // ------------------------------------------------------------------ dressing
        public void Dress(World w, GameState state)
        {
            _rng = new Rng(1301);
            Hull(w);
            Transom(w);
            Prow(w);
            Masts(w);
            UpperDeck(w);
            CollapseVisuals(w);
            Spine(w);
            Crew(w);
            Music(w);
            Surgery(w);
            Store(w);
            Nessa(w);
            Cargo(w);
            BowDeck(w);
            Captain(w);
            SeaAndBilge(w);
            w.Anchor("beneath", new Vector3(-1.6f, -0.8f, 33f));
        }

        void Hull(World w)
        {
            var outer = w.Kit.B("hull", "hull", 2.4f);
            var inner = w.Kit.B("hull", "hull_dark", 2.4f);
            var zs = new List<float>();
            for (float z = -0.2f; z < 108.2f; z += 1f) zs.Add(z);
            for (float z = 108.2f; z < 114.5f; z += 0.45f) zs.Add(z);
            zs.Add(114.5f);
            for (int i = 0; i < zs.Count - 1; i++)
            {
                float z0 = zs[i], z1 = zs[i + 1];
                float t0 = TopAt(z0), t1 = TopAt(z1);
                for (int k = 0; k < SecY.Length - 1; k++)
                {
                    float yHi = SecY[k], yLo = SecY[k + 1];
                    float h0 = k == 0 ? t0 : yHi, h1 = k == 0 ? t1 : yHi;
                    // port (x < 8), outside faces -x
                    var a = new Vector3(L(z1, yLo).x, yLo, z1); var b = new Vector3(L(z0, yLo).x, yLo, z0);
                    var c = new Vector3(L(z0, h0).x, h0, z0); var d = new Vector3(L(z1, h1).x, h1, z1);
                    outer.Quad(a, b, c, d);
                    inner.Quad(b, a, d, c);
                    // starboard, outside faces +x
                    var e = new Vector3(R(z0, yLo).x, yLo, z0); var f = new Vector3(R(z1, yLo).x, yLo, z1);
                    var g = new Vector3(R(z1, h1).x, h1, z1); var hh = new Vector3(R(z0, h0).x, h0, z0);
                    outer.Quad(e, f, g, hh);
                    inner.Quad(f, e, hh, g);
                }
                // keel strip
                var k0 = L(z0, -6.8f); var k1 = L(z1, -6.8f); var k2 = R(z1, -6.8f); var k3 = R(z0, -6.8f);
                outer.Quad(k3, k0, k1, k2);
            }
            // wales (thick strakes) and closed gun ports along both sides
            var wale = w.Kit.B("hull", "wood_dark", 1f);
            foreach (float y in new[] { -0.28f, -1.35f, -3.4f })
            {
                foreach (int side in new[] { -1, 1 })
                {
                    var path = new List<Vector3>();
                    for (float z = -0.2f; z <= 113.6f; z += (z > 107f ? 0.6f : 2f))
                    {
                        if (y > -0.5f && Collapsed(z) && TopAt(z) < y + 0.1f) { if (path.Count > 1) wale.Tube(path, 0.075f, 6, 0.6f); path = new List<Vector3>(); continue; }
                        var p = side < 0 ? L(z, y) : R(z, y);
                        p.x += side * 0.05f;
                        path.Add(p);
                    }
                    if (path.Count > 1) wale.Tube(path, 0.075f, 6, 0.6f);
                }
            }
            foreach (int side in new[] { -1, 1 })
                for (float z = 21f; z <= 63f; z += 6f)
                {
                    var p = side < 0 ? L(z, -2.95f) : R(z, -2.95f);
                    p.x += side * 0.04f;
                    w.Kit.Box("hull", "wood_dark", p, new Vector3(0.08f, 0.72f, 0.78f));
                    w.Kit.Box("hull", "iron", p + new Vector3(side * 0.045f, 0.2f, 0f), new Vector3(0.02f, 0.05f, 0.7f));
                }
            // channels with deadeyes where the shrouds come down
            foreach (float mz in new[] { 7.5f, 29.4f, 47.5f })
                foreach (int side in new[] { -1, 1 })
                {
                    var p = side < 0 ? L(mz, -0.5f) : R(mz, -0.5f);
                    w.Kit.Box("hull", "wood_dark", p + new Vector3(side * 0.25f, 0, 0), new Vector3(0.5f, 0.1f, 4.2f));
                }
        }

        void Transom(World w)
        {
            var outer = w.Kit.B("hull", "hull", 2.4f);
            const float z = -0.2f;
            for (int k = 0; k < SecY.Length - 1; k++)
            {
                float y1 = SecY[k], y0 = SecY[k + 1];
                outer.Quad(L(z, y0), R(z, y0), R(z, y1), L(z, y1));
            }
            // stern gallery windows (seen when leaning over the taffrail)
            for (int i = 0; i < 5; i++)
            {
                float x = 2.6f + i * 2.7f;
                var c = new Vector3(x, -1.05f, z - 0.03f);
                w.Kit.Box("hull", "glass", c, new Vector3(1.5f, 0.85f, 0.02f));
                w.Kit.Box("hull", "wood_dark", c + new Vector3(0, 0.47f, -0.02f), new Vector3(1.7f, 0.1f, 0.08f));
                w.Kit.Box("hull", "wood_dark", c + new Vector3(0, -0.47f, -0.02f), new Vector3(1.7f, 0.1f, 0.08f));
                w.Kit.Box("hull", "wood_dark", c + new Vector3(0, 0, -0.02f), new Vector3(0.06f, 0.85f, 0.05f));
            }
            w.Kit.Box("hull", "wood_dark", new Vector3(CX, -1.65f, z - 0.25f), new Vector3(15.6f, 0.12f, 0.5f));
            // carved taffrail on top of the stern bulwark
            var trim = w.Kit.B("stern", "wood_dark", 1f);
            trim.Box(new Vector3(CX, 1.22f, -0.1f), new Vector3(16.4f, 0.16f, 0.3f), Quaternion.identity);
            for (int i = 0; i <= 10; i++)
            {
                float x = -0.1f + i * 1.62f;
                trim.Lathe(new Vector3(x, 1.06f, -0.1f), Quaternion.identity, new[] { new Vector2(0.06f, 0f), new Vector2(0.08f, 0.04f), new Vector2(0.05f, 0.1f), new Vector2(0.07f, 0.16f), new Vector2(0.05f, 0.2f) }, 8);
            }
            // stern lanterns (cold, cracked)
            foreach (float x in new[] { 0.1f, 15.9f })
            {
                var basePos = new Vector3(x, 1.3f, -0.1f);
                trim.Cylinder(basePos, Quaternion.identity, 0.5f, 0.05f, 0.04f, 8);
                var lan = Props.HangingLantern(false);
                lan.BakeInto(w.Kit, "stern", basePos + Vector3.up * 1.25f, Quaternion.Euler(0, 20, 0), 1.9f);
            }
            // flag staff with a rag of a flag
            trim.Cylinder(new Vector3(CX, 1.3f, -0.25f), Quaternion.Euler(-8, 0, 0), 5.2f, 0.06f, 0.035f, 8);
            Props.Sail(1.5f, 0.9f, _rng, "cloth_red", 0.55f).BakeInto(w.Kit, "stern", new Vector3(CX + 0.78f, 6.2f, -0.95f), Quaternion.Euler(0, 90, -6));
            // the ship's name on the inside of the stern bulwark: the first thing you read when you turn round
            var nb = new PropMesh();
            nb["wood_dark"].Box(new Vector3(0, 0, -0.02f), new Vector3(2.9f, 0.5f, 0.04f), Quaternion.identity);
            nb["nameboard_req"].QuadUV(new Vector3(-1.38f, -0.21f, 0.002f), new Vector3(1.38f, -0.21f, 0.002f), new Vector3(1.38f, 0.21f, 0.002f), new Vector3(-1.38f, 0.21f, 0.002f),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            nb.BakeInto(w.Kit, "stern", new Vector3(CX, 0.62f, 0.045f), Quaternion.identity);
        }

        void Prow(World w)
        {
            // deck cap over the beak, a low rail round it, the stem post and the bowsprit
            var cap = w.Kit.B("bow", "deck", 2f);
            var trim = w.Kit.B("bow", "wood_dark", 1f);
            var zs = new List<float>();
            for (float z = 108.2f; z < 114.5f; z += 0.45f) zs.Add(z);
            zs.Add(114.5f);
            for (int i = 0; i < zs.Count - 1; i++)
            {
                var a = L(zs[i], 0f); var b = R(zs[i], 0f); var c = R(zs[i + 1], 0f); var d = L(zs[i + 1], 0f);
                // viewed from above (+y), front = up: a(-x,z0) b(+x,z0) c(+x,z1) d(-x,z1) is counter-clockwise
                cap.Quad(a, b, c, d);
                foreach (int side in new[] { -1, 1 })
                {
                    var p0 = side < 0 ? L(zs[i], 0f) : R(zs[i], 0f);
                    var p1 = side < 0 ? L(zs[i + 1], 0f) : R(zs[i + 1], 0f);
                    var mid = (p0 + p1) * 0.5f + Vector3.up * 0.5f;
                    var dir = p1 - p0;
                    trim.Box(mid, new Vector3(0.14f, 1.0f, dir.magnitude + 0.04f), Quaternion.LookRotation(dir.normalized, Vector3.up));
                }
            }
            trim.Box(new Vector3(CX, -2.6f, 114.55f), new Vector3(0.3f, 8.4f, 0.3f), Quaternion.Euler(-12, 0, 0));
            var dirB = Quaternion.Euler(-16f, 0, 0) * Vector3.forward;
            var bsBase = new Vector3(CX, 0.75f, 106.8f);
            trim.Cylinder(bsBase, Quaternion.FromToRotation(Vector3.up, dirB), 15f, 0.32f, 0.13f, 12, true, 2f);
            for (int i = 0; i < 3; i++)
                w.Kit.B("bow", "iron").Cylinder(bsBase + dirB * (8.2f + i * 1.6f) - dirB * 0.06f, Quaternion.FromToRotation(Vector3.up, dirB), 0.12f, 0.3f - i * 0.03f, 0.3f - i * 0.03f, 10, false);
            BowspritTip = bsBase + dirB * 15f;
        }

        Vector3 BowspritTip;

        // ------------------------------------------------------------------ masts, sails, rigging
        void Masts(World w)
        {
            MastAt(w, new Vector3(11.5f, -4.3f, 7.5f), 21f, 0.38f, 11.5f, 8.5f, 6.5f, 4.4f, "mizzen");
            MastAt(w, new Vector3(CX, -4.3f, 29.4f), 29f, 0.54f, 14.5f, 15f, 12.5f, 7.2f, "main");
            MastAt(w, new Vector3(CX, -4.3f, 47.5f), 26f, 0.46f, 13.5f, 13f, 11f, 6.3f, "fore");
            var rope = w.Kit.B("rigging", "rope", 0.4f);
            // stays between the mast heads and out to the bowsprit
            rope.Rope(new Vector3(11.5f, 15.5f, 7.5f), new Vector3(CX, 20f, 29.4f), 0.6f, 0.03f, 14);
            rope.Rope(new Vector3(CX, 22.5f, 29.4f), new Vector3(CX, 18.5f, 47.5f), 0.5f, 0.035f, 14);
            rope.Rope(new Vector3(CX, 19.5f, 47.5f), BowspritTip, 1.2f, 0.035f, 18);
            rope.Rope(new Vector3(CX, 14f, 47.5f), BowspritTip + new Vector3(0, -1.2f, -3f), 1.0f, 0.03f, 16);
            rope.Rope(new Vector3(11.5f, 13f, 7.5f), new Vector3(8.6f, 1.2f, -0.1f), 0.4f, 0.025f, 12);
            // loose lines swinging off the yards
            for (int i = 0; i < 9; i++)
            {
                float mz = i < 3 ? 7.5f : (i < 6 ? 29.4f : 47.5f);
                float mx = i < 3 ? 11.5f : CX;
                float yy = i < 3 ? 7.2f : (i < 6 ? 10.2f : 9.2f);
                var top = new Vector3(mx + _rng.Range(-5f, 5f), yy, mz + _rng.Range(-0.2f, 0.2f));
                var bot = new Vector3(top.x + _rng.Range(-0.6f, 0.6f), _rng.Range(1.2f, 4.5f), top.z + _rng.Range(-1.5f, 1.5f));
                if (bot.x < 0.3f || bot.x > 15.7f) continue;
                rope.Rope(top, bot, 0.1f, 0.014f, 8);
            }
        }

        void MastAt(World w, Vector3 foot, float height, float radius, float yardY, float yardLen, float sailW, float sailH, string chunk)
        {
            var pm = Props.Mast(height, radius, yardY, yardLen);
            pm.BakeInto(w.Kit, "mast_" + chunk, foot, Quaternion.identity);
            // collider from the gun deck to well above head height on the upper deck
            w.Kit.Collider(new Vector3(foot.x, 0.0f, foot.z), new Vector3(radius * 1.7f, 8.6f, radius * 1.7f));
            // partners: the collar where the mast goes through each deck
            foreach (float y in new[] { 0.0f, -1.4f })
            {
                if (chunk == "mizzen" && y < -1f) continue;
                w.Kit.Box("mast_" + chunk, "wood_dark", new Vector3(foot.x, y + 0.07f, foot.z), new Vector3(radius * 2.8f, 0.14f, radius * 2.8f));
            }
            // fife rail with belaying pins round the foot of the mast on deck
            var rail = w.Kit.B("mast_" + chunk, "wood_dark", 1f);
            float fr = radius + 0.75f;
            foreach (var s in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) })
                rail.Box(new Vector3(foot.x + s.x * fr, 0.42f, foot.z + s.y * fr), new Vector3(0.1f, 0.84f, 0.1f), Quaternion.identity);
            rail.Box(new Vector3(foot.x, 0.8f, foot.z - fr), new Vector3(fr * 2f + 0.1f, 0.08f, 0.14f), Quaternion.identity);
            rail.Box(new Vector3(foot.x, 0.8f, foot.z + fr), new Vector3(fr * 2f + 0.1f, 0.08f, 0.14f), Quaternion.identity);
            for (int i = 0; i < 6; i++)
            {
                float x = foot.x - fr + 0.25f + i * (fr * 2f - 0.5f) / 5f;
                w.Kit.B("mast_" + chunk, "wood").Cylinder(new Vector3(x, 0.7f, foot.z - fr), Quaternion.identity, 0.3f, 0.018f, 0.014f, 6);
            }
            w.Kit.Collider(new Vector3(foot.x, 0.42f, foot.z - fr), new Vector3(fr * 2f + 0.1f, 0.84f, 0.14f));
            w.Kit.Collider(new Vector3(foot.x, 0.42f, foot.z + fr), new Vector3(fr * 2f + 0.1f, 0.84f, 0.14f));
            Props.RopeCoil(0.28f).BakeInto(w.Kit, "mast_" + chunk, new Vector3(foot.x + fr - 0.35f, 0f, foot.z + fr - 0.45f), Quaternion.identity);
            // torn sails under the lower yard and the topsail yard
            float upperY = yardY + (height - yardY) * 0.45f;
            Props.Sail(sailW, sailH, _rng, "sail", 0.32f).BakeInto(w.Kit, "sails", new Vector3(foot.x, foot.y + yardY - 0.15f, foot.z + 0.35f), Quaternion.identity);
            Props.Sail(sailW * 0.72f, sailH * 0.62f, _rng, "sail_light", 0.4f).BakeInto(w.Kit, "sails", new Vector3(foot.x, foot.y + upperY - 0.12f, foot.z + 0.3f), Quaternion.identity);
            // shrouds down to the channels outside the hull, with ratlines on the lower part
            var rope = w.Kit.B("rigging", "rope", 0.4f);
            float topY = foot.y + yardY - 0.6f;
            foreach (int side in new[] { -1, 1 })
            {
                var ends = new List<Vector3>();
                for (int i = 0; i < 4; i++)
                {
                    float z = foot.z - 1.5f + i * 1.0f;
                    var ch = side < 0 ? L(z, -0.45f) : R(z, -0.45f);
                    ch.x += side * 0.45f;
                    var head = new Vector3(foot.x + side * radius * 0.7f, topY, foot.z);
                    rope.Rope(head, ch, 0.05f, 0.022f, 6);
                    ends.Add(ch);
                    w.Kit.B("rigging", "wood_dark").Cylinder(ch + Vector3.up * 0.25f, Quaternion.Euler(0, 0, 90), 0.08f, 0.08f, 0.08f, 8);
                }
                if (chunk != "main") continue;
                var heads = new Vector3(foot.x + side * radius * 0.7f, topY, foot.z);
                for (float k = 0.12f; k < 0.55f; k += 0.05f)
                {
                    var a = Vector3.Lerp(ends[0], heads, k);
                    var b = Vector3.Lerp(ends[3], heads, k);
                    rope.Rope(a, b, 0.02f, 0.012f, 3);
                }
            }
        }

        // ------------------------------------------------------------------ upper deck
        void UpperDeck(World w)
        {
            var rng = _rng;
            // stern: helm with the stopped watch on its hub, binnacle, a lantern post with the only light
            Props.Helm().BakeInto(w.Kit, "a1_stern", new Vector3(CX, 0f, 2.18f), Quaternion.identity);
            w.Kit.Collider(new Vector3(CX, 0.6f, 2.06f), new Vector3(0.7f, 1.2f, 0.5f));
            var bin = new PropMesh();
            bin["wood_dark"].Box(new Vector3(0, 0.5f, 0), new Vector3(0.5f, 1.0f, 0.5f), Quaternion.identity);
            bin["brass"].Lathe(new Vector3(0, 1.0f, 0), Quaternion.identity, new[] { new Vector2(0.2f, 0f), new Vector2(0.2f, 0.08f), new Vector2(0.15f, 0.2f), new Vector2(0.06f, 0.27f), new Vector2(0f, 0.28f) }, 12);
            bin["glass"].Disc(new Vector3(0, 1.085f, 0), Quaternion.identity, 0.19f, 12);
            bin.BakeInto(w.Kit, "a1_stern", new Vector3(9.7f, 0f, 3.2f), Quaternion.Euler(0, 15, 0));
            w.Kit.Collider(new Vector3(9.7f, 0.6f, 3.2f), new Vector3(0.55f, 1.2f, 0.55f));
            var post = new PropMesh();
            post["wood_dark"].Box(new Vector3(0, 1.1f, 0), new Vector3(0.12f, 2.2f, 0.12f), Quaternion.identity);
            post["wood_dark"].Box(new Vector3(0.25f, 2.15f, 0), new Vector3(0.6f, 0.08f, 0.08f), Quaternion.identity);
            post["iron"].Box(new Vector3(0.5f, 2.08f, 0), new Vector3(0.02f, 0.1f, 0.02f), Quaternion.identity);
            post.BakeInto(w.Kit, "a1_stern", new Vector3(5.9f, 0f, 2.5f), Quaternion.identity);
            w.Kit.Collider(new Vector3(5.9f, 1.1f, 2.5f), new Vector3(0.2f, 2.2f, 0.2f));
            HelmLantern = new Vector3(6.4f, 2.03f, 2.5f);
            Props.HangingLantern(true).BakeInto(w.Kit, "a1_stern", HelmLantern, Quaternion.identity);
            Props.SeaChest().BakeInto(w.Kit, "a1_stern", new Vector3(2.0f, 0f, 1.0f), Quaternion.Euler(0, 8, 0));
            w.Kit.Collider(new Vector3(2.0f, 0.35f, 1.0f), new Vector3(0.85f, 0.7f, 0.5f), Quaternion.Euler(0, 8, 0));
            foreach (var p in new[] { new Vector3(14.9f, 0, 12.4f), new Vector3(14.85f, 0, 13.2f), new Vector3(14.2f, 0, 12.8f), new Vector3(14.9f, 0, 14.0f) })
            {
                DressUtil.Bake(w, "a1_stern", Props.Barrel(0.92f, 0.31f), p, Quaternion.Euler(0, rng.Range(0, 360), 0), new Vector3(0.62f, 0.92f, 0.62f));
            }
            Props.RopeCoil(0.36f).BakeInto(w.Kit, "a1_stern", new Vector3(13.0f, 0f, 9.3f), Quaternion.identity);
            Props.Crate(new Vector3(0.8f, 0.7f, 0.8f)).BakeInto(w.Kit, "a1_stern", new Vector3(0.9f, 0f, 14.6f), Quaternion.Euler(0, 6, 0));
            w.Kit.Collider(new Vector3(0.9f, 0.35f, 14.6f), new Vector3(0.82f, 0.7f, 0.82f), Quaternion.Euler(0, 6, 0));
            Props.Crate(new Vector3(0.6f, 0.55f, 0.6f)).BakeInto(w.Kit, "a1_stern", new Vector3(0.9f, 0.7f, 14.6f), Quaternion.Euler(0, 31, 0));
            // a child's wet footprints from the helm towards the companion way
            var fpL = Props.Footprint(true); var fpR = Props.Footprint(false);
            var fb = w.Kit.B("a1_stern", "footprint", 1f);
            Vector3 from = new Vector3(6.6f, 0f, 4.2f), to = new Vector3(7.6f, 0f, 9.6f);
            int steps = 12;
            for (int i = 0; i < steps; i++)
            {
                float t = i / (float)(steps - 1);
                var p = Vector3.Lerp(from, to, t);
                var dir = (to - from).normalized;
                var side = Vector3.Cross(Vector3.up, dir) * ((i % 2 == 0) ? -0.07f : 0.07f);
                var rot = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(0, rng.Range(-8f, 8f), 0);
                fb.Append(i % 2 == 0 ? fpL : fpR, Matrix4x4.TRS(p + side + Vector3.up * 0.002f, rot, Vector3.one * 0.85f));
            }

            // main deck: guns with their ports shut, capstan, an overturned boat, gratings
            foreach (float z in new[] { 20f, 25.5f, 31f, 36.5f })
            {
                foreach (int side in new[] { -1, 1 })
                {
                    float x = side < 0 ? 1.1f : 14.9f;
                    var rot = Quaternion.Euler(0, side < 0 ? -90f : 90f, 0);
                    Props.Cannon().BakeInto(w.Kit, "a1_main", new Vector3(x, 0f, z), rot);
                    w.Kit.Collider(new Vector3(x - side * 0.05f, 0.35f, z), new Vector3(1.7f, 0.7f, 0.66f));
                    // the closed port lid seen from inside the bulwark
                    float wx = side < 0 ? 0.012f : 15.988f;
                    var lid = new PropMesh();
                    lid["wood_dark"].Box(new Vector3(0, 0.5f, 0.01f), new Vector3(0.78f, 0.62f, 0.02f), Quaternion.identity);
                    lid["iron"].Box(new Vector3(0, 0.74f, 0.025f), new Vector3(0.7f, 0.04f, 0.012f), Quaternion.identity);
                    lid["iron"].Box(new Vector3(0, 0.26f, 0.025f), new Vector3(0.7f, 0.04f, 0.012f), Quaternion.identity);
                    lid.BakeInto(w.Kit, "a1_main", new Vector3(wx, 0f, z), Quaternion.Euler(0, side < 0 ? 90f : -90f, 0));
                    // breeching ropes
                    var rp = w.Kit.B("a1_main", "rope", 0.3f);
                    rp.Rope(new Vector3(wx + (side < 0 ? 0.02f : -0.02f), 0.55f, z - 0.55f), new Vector3(x + (side < 0 ? 0.35f : -0.35f), 0.45f, z - 0.3f), 0.08f, 0.02f, 6);
                    rp.Rope(new Vector3(wx + (side < 0 ? 0.02f : -0.02f), 0.55f, z + 0.55f), new Vector3(x + (side < 0 ? 0.35f : -0.35f), 0.45f, z + 0.3f), 0.08f, 0.02f, 6);
                }
                // shot garland
                var g = new PropMesh();
                g["wood_dark"].Box(new Vector3(0, 0.08f, 0), new Vector3(0.25f, 0.16f, 0.9f), Quaternion.identity);
                for (int k = 0; k < 4; k++) g["darkmetal"].Cylinder(new Vector3(0, 0.16f, -0.3f + k * 0.2f), Quaternion.identity, 0.18f, 0.085f, 0.085f, 8);
                g.BakeInto(w.Kit, "a1_main", new Vector3(2.35f, 0f, z + 2.4f), Quaternion.identity);
                g.BakeInto(w.Kit, "a1_main", new Vector3(13.65f, 0f, z + 2.4f), Quaternion.identity);
            }
            // the carving is cut into a board strapped to the main mast
            var board = new PropMesh();
            board["wood"].Box(new Vector3(0, 0, -0.02f), new Vector3(0.04f, 0.48f, 1.0f), Quaternion.identity);
            board.BakeInto(w.Kit, "a1_main", new Vector3(8.55f, 1.45f, 29.4f), Quaternion.identity);
            foreach (float dy in new[] { -0.18f, 0.18f })
                w.Kit.B("a1_main", "iron").Cylinder(new Vector3(CX, 1.45f + dy - 0.02f, 29.4f), Quaternion.identity, 0.04f, 0.565f, 0.565f, 18, false);
            // capstan
            var cap = new PropMesh();
            cap["wood_dark"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.62f, 0f), new Vector2(0.62f, 0.14f), new Vector2(0.42f, 0.2f), new Vector2(0.36f, 0.7f), new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.92f), new Vector2(0f, 0.95f) }, 16);
            for (int i = 0; i < 4; i++)
                cap["wood"].Box(new Vector3(0, 0.85f, 0), new Vector3(2.4f, 0.07f, 0.07f), Quaternion.Euler(0, i * 45f, 0));
            cap.BakeInto(w.Kit, "a1_main", new Vector3(CX, 0f, 37.6f), Quaternion.identity);
            w.Kit.Collider(new Vector3(CX, 0.48f, 37.6f), new Vector3(1.2f, 0.95f, 1.2f));
            // overturned jolly boat on chocks
            var boat = Props.Rowboat();
            boat.BakeInto(w.Kit, "a1_main", new Vector3(12.3f, 0.74f, 40.4f), Quaternion.Euler(0, 82, 180));
            w.Kit.Box("a1_main", "wood_dark", new Vector3(11.5f, 0.1f, 40.4f), new Vector3(0.2f, 0.2f, 1.4f));
            w.Kit.Box("a1_main", "wood_dark", new Vector3(13.1f, 0.1f, 40.4f), new Vector3(0.2f, 0.2f, 1.4f));
            w.Kit.Collider(new Vector3(12.3f, 0.4f, 40.4f), new Vector3(3.6f, 0.8f, 1.6f), Quaternion.Euler(0, 82, 0));
            Grating(w, "a1_main", new Vector3(3.4f, 0f, 24.4f), 1.6f, 1.6f);
            Grating(w, "a1_main", new Vector3(12.4f, 0f, 33.6f), 1.4f, 1.8f);
            Props.RopeCoil(0.4f).BakeInto(w.Kit, "a1_main", new Vector3(2.2f, 0f, 41.6f), Quaternion.identity);
            Props.Planks(rng, 4, 2.0f).BakeInto(w.Kit, "a1_main", new Vector3(4.5f, 0f, 39.7f), Quaternion.Euler(0, 20, 0));

            // fore deck: the watch bell in its belfry, coils and a split barrel
            var belfry = new PropMesh();
            belfry["wood_dark"].Box(new Vector3(-0.45f, 0.9f, 0), new Vector3(0.12f, 1.8f, 0.12f), Quaternion.identity);
            belfry["wood_dark"].Box(new Vector3(0.45f, 0.9f, 0), new Vector3(0.12f, 1.8f, 0.12f), Quaternion.identity);
            belfry["wood_dark"].Box(new Vector3(0, 1.84f, 0), new Vector3(1.2f, 0.12f, 0.2f), Quaternion.identity);
            belfry.BakeInto(w.Kit, "a1_fore", new Vector3(3.2f, 0f, 53.4f), Quaternion.identity);
            w.Kit.Collider(new Vector3(3.2f, 0.9f, 53.4f), new Vector3(1.1f, 1.8f, 0.3f));
            var shipBell = Props.Bell(0.22f, "brass");
            var bellGo = shipBell.Spawn("ShipBell", w.Root, new Vector3(3.2f, 1.42f, 53.4f), Quaternion.identity);
            w.Anchors["ship_bell"] = bellGo.transform;
            Props.RopeCoil(0.33f).BakeInto(w.Kit, "a1_fore", new Vector3(1.3f, 0f, 44.5f), Quaternion.identity);
            Props.RopeCoil(0.3f).BakeInto(w.Kit, "a1_fore", new Vector3(14.7f, 0f, 43.2f), Quaternion.identity);
            var split = Props.Barrel(0.6f, 0.3f);
            split.BakeInto(w.Kit, "a1_fore", new Vector3(1.4f, 0.28f, 50.2f), Quaternion.Euler(84, 30, 0));
            Props.Planks(rng, 3, 1.4f).BakeInto(w.Kit, "a1_fore", new Vector3(2.4f, 0f, 49.4f), Quaternion.Euler(0, -30, 0));

            foreach (var id in new[] { "a1_stern", "a1_main", "a1_fore", "a1_bowdeck" })
                DressUtil.Puddles(w, w.Act.Room(id), 7, rng);
        }

        Vector3 HelmLantern;

        static void Grating(World w, string chunk, Vector3 c, float wx, float wz)
        {
            var b = w.Kit.B(chunk, "wood_dark", 1f);
            b.Box(c + new Vector3(0, 0.03f, 0), new Vector3(wx + 0.16f, 0.06f, wz + 0.16f), Quaternion.identity, 55);
            for (float x = -wx * 0.5f + 0.1f; x < wx * 0.5f; x += 0.2f)
                b.Box(c + new Vector3(x, 0.07f, 0), new Vector3(0.05f, 0.03f, wz), Quaternion.identity);
            for (float z = -wz * 0.5f + 0.1f; z < wz * 0.5f; z += 0.2f)
                b.Box(c + new Vector3(0, 0.09f, z), new Vector3(wx, 0.02f, 0.05f), Quaternion.identity);
            w.Kit.B(chunk, "black").Box(c + new Vector3(0, 0.061f, 0), new Vector3(wx, 0.002f, wz), Quaternion.identity, 4);
        }

        // ------------------------------------------------------------------ the collapse
        void CollapseVisuals(World w)
        {
            var rng = _rng;
            var planks = w.Kit.B("collapse", "deck", 2f);
            var dark = w.Kit.B("collapse", "wood_dark", 1f);
            // splintered plank ends hanging over both broken edges
            void Edge(float z0, float dirZ, float xFrom, float xTo, string skipRoom)
            {
                for (float x = xFrom + 0.12f; x < xTo - 0.1f; x += 0.24f)
                {
                    if (skipRoom != null)
                    {
                        var r = w.Act.Room(skipRoom);
                        if (r != null && x > r.Rect.X - 0.15f && x < r.Rect.MaxX + 0.15f) continue;
                    }
                    float len = rng.Range(0.15f, 1.5f);
                    float droop = rng.Range(4f, 38f) * (len > 0.8f ? 1f : 0.4f);
                    var rot = Quaternion.Euler(dirZ * droop, rng.Range(-6f, 6f), rng.Range(-3f, 3f));
                    var start = new Vector3(x, -0.06f, z0);
                    var c = start + rot * new Vector3(0, 0, dirZ * len * 0.5f);
                    planks.Box(c, new Vector3(0.22f, 0.06f, len), rot);
                }
                // broken deck beam ends under the planking
                for (float x = xFrom + 0.6f; x < xTo; x += 2.2f)
                    dark.Box(new Vector3(x, -0.3f, z0 + dirZ * 0.35f), new Vector3(0.26f, 0.24f, 0.7f), Quaternion.Euler(dirZ * rng.Range(0f, 12f), 0, 0));
            }
            Edge(CollapseA, 1f, 0f, 16f, "a1_stair");
            Edge(CollapseB, -1f, 2f, 14f, "a1_bowramp");

            // what is left of the deck beams across the hold, broken and sagging; spans over the
            // companion ways are left out where the headroom would be too low
            foreach (float bz in new[] { 64f, 70f, 76f, 82f, 88f })
            {
                float hw = Hw(bz) * F(-0.35f) - 0.06f;
                float brk = CX + rng.Range(-2.5f, 2.5f);
                float sag = bz < 66f ? rng.Range(0.2f, 0.6f) : rng.Range(0.3f, 1.1f);
                var a = new Vector3(CX - hw, -0.35f, bz);
                var b = new Vector3(CX + hw, -0.35f, bz);
                var ma = new Vector3(brk - 0.1f, -0.35f - sag, bz + rng.Range(-0.3f, 0.3f));
                var mb = new Vector3(brk + 0.1f, -0.35f - sag * rng.Range(0.3f, 1.2f), bz + rng.Range(-0.3f, 0.3f));
                BeamSeg(w, dark, a, ma, bz);
                BeamSeg(w, dark, mb, b, bz);
            }
            // hanging ropes and torn planks dangling into the hold
            var rope = w.Kit.B("collapse", "rope", 0.3f);
            for (int i = 0; i < 10; i++)
            {
                float z = rng.Range(58f, 90f);
                float x = rng.Range(2f, 14f);
                var top = new Vector3(x, -0.4f, z);
                var bot = top + new Vector3(rng.Range(-0.3f, 0.3f), -rng.Range(1.2f, 2.6f), rng.Range(-0.3f, 0.3f));
                if (bot.y < -2.2f && w.Act.Room("a1_cargo").Rect.Contains(bot.x, bot.z)) bot.y = -2.2f;
                rope.Rope(top, bot, 0.04f, 0.016f, 6);
            }
            // jagged stumps of the bulwark where the hull top tore away
            var hullB = w.Kit.B("collapse", "hull", 2f);
            for (float z = CollapseA + 0.6f; z < CollapseB - 0.5f; z += 0.32f)
            {
                foreach (int side in new[] { -1, 1 })
                {
                    if (!rng.Chance(0.45f)) continue;
                    float top = TopAt(z);
                    float h = rng.Range(0.1f, 0.9f);
                    var p = side < 0 ? L(z, top) : R(z, top);
                    p.x -= side * 0.1f;
                    hullB.Box(p + Vector3.up * (h * 0.5f), new Vector3(0.12f, h, 0.24f), Quaternion.Euler(rng.Range(-8f, 8f), 0, side * rng.Range(-14f, 4f)));
                }
            }
        }

        void BeamSeg(World w, MeshBuilder b, Vector3 a, Vector3 c, float bz)
        {
            // drop the beam where it would hang into a companion way's head room
            foreach (var id in new[] { "a1_stair", "a1_bowramp" })
            {
                var r = w.Act.Room(id);
                if (r == null) continue;
                for (float t = 0f; t <= 1f; t += 0.1f)
                {
                    var p = Vector3.Lerp(a, c, t);
                    if (r.Rect.Expanded(0.3f).Contains(p.x, p.z) && p.y - 0.13f - r.FloorAt(Mathf.Clamp(p.x, r.Rect.X, r.Rect.MaxX), Mathf.Clamp(p.z, r.Rect.Z, r.Rect.MaxZ)) < 2.2f)
                        return;
                }
            }
            var cargo = w.Act.Room("a1_cargo");
            for (float t = 0f; t <= 1f; t += 0.1f)
            {
                var p = Vector3.Lerp(a, c, t);
                if (cargo.Rect.Contains(p.x, p.z) && p.y - 0.13f < cargo.Floor + 2.2f) return;
            }
            var mid = (a + c) * 0.5f;
            var dir = c - a;
            b.Box(mid, new Vector3(0.24f, 0.26f, dir.magnitude), Quaternion.LookRotation(dir.normalized, Vector3.up));
        }
    }
}
