using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>Big procedural set pieces shared by the later acts: wreck hulls, house facades, roofs, arches.</summary>
    public static class WreckKit
    {
        public sealed class HullLook
        {
            public string Mat = "hull";
            public string Trim = "wood_dark";
            public float MinH = 4.5f, MaxH = 8.5f;
            public float Water = -0.7f;
            public float MinLen = 7f, MaxLen = 15f;
        }

        /// <summary>
        /// Lines a wall piece with the curved sides of wrecked ships (the piece itself stays an invisible collider).
        /// The surfaces always stay behind the wall line, so nothing intrudes into walkable space.
        /// </summary>
        public static void HullWall(World w, string chunk, WallPiece wp, Rng rng, HullLook look)
        {
            var a = new Vector3(wp.A.X, 0, wp.A.Z);
            var b = new Vector3(wp.B.X, 0, wp.B.Z);
            float len = Vector3.Distance(a, b);
            if (len < 1f) return;
            var u = (b - a) / len;
            var n = new Vector3(wp.Normal.X, 0, wp.Normal.Z);
            float baseY = Mathf.Min(wp.Bot0, wp.Bot1);
            float s = -rng.Range(0.5f, 3f);
            while (s < len)
            {
                float shipLen = rng.Range(look.MinLen, look.MaxLen);
                float s0 = s, s1 = s + shipLen;
                Ship(w, chunk, a, u, n, s0, s1, baseY, rng, look);
                s = s1 + rng.Range(0.2f, 0.7f);
            }
        }

        static void Ship(World w, string chunk, Vector3 a, Vector3 u, Vector3 n, float s0, float s1, float baseY, Rng rng, HullLook look)
        {
            string mat = rng.Chance(0.35f) ? "hull_dark" : look.Mat;
            var skin = w.Kit.B(chunk, mat, 2.4f);
            var trim = w.Kit.B(chunk, look.Trim, 1f);
            float H = rng.Range(look.MinH, look.MaxH);
            float belly = rng.Range(1.2f, 2.4f);
            float lean = rng.Range(-0.25f, 0.35f);                 // listing wrecks lean in or away
            float yLo = look.Water - 0.6f;
            int nt = Mathf.Max(4, Mathf.CeilToInt((s1 - s0) / 1.1f));
            int ny = 7;
            var pts = new Vector3[nt + 1, ny + 1];
            for (int i = 0; i <= nt; i++)
            {
                float t = i / (float)nt;
                float e = 2f * t - 1f;
                float top = baseY + H + 0.9f * e * e;
                float endBack = 1.4f * e * e * e * e;
                for (int j = 0; j <= ny; j++)
                {
                    float y = Mathf.Lerp(yLo, top, j / (float)ny);
                    float d = 0.06f + 0.085f * (y - baseY - belly) * (y - baseY - belly) + endBack + Mathf.Max(0f, y - baseY) * lean * 0.12f;
                    d = Mathf.Max(0.05f, d);
                    pts[i, j] = a + u * Mathf.Lerp(s0, s1, t) - n * d + Vector3.up * y;
                }
            }
            // faces toward the room (+n): seen from the room, 'u' runs to the right or left depending on the side
            bool flip = Vector3.Dot(Vector3.Cross(Vector3.up, u), n) > 0f;
            for (int i = 0; i < nt; i++)
                for (int j = 0; j < ny; j++)
                {
                    var p00 = pts[i, j]; var p10 = pts[i + 1, j]; var p11 = pts[i + 1, j + 1]; var p01 = pts[i, j + 1];
                    // flip: 'u' runs to the viewer's right when looking at the wall from the room
                    if (flip) skin.Quad(p00, p10, p11, p01);
                    else skin.Quad(p10, p00, p01, p11);
                }
            // rail along the sheer, wales, algae at the waterline
            var rail = new List<Vector3>();
            for (int i = 0; i <= nt; i++) rail.Add(pts[i, ny] + n * 0.03f);
            trim.Tube(rail, 0.07f, 6, 0.6f);
            foreach (int j in new[] { 2, 4 })
            {
                var wl = new List<Vector3>();
                for (int i = 0; i <= nt; i++) wl.Add(pts[i, j] + n * 0.04f);
                trim.Tube(wl, 0.06f, 5, 0.6f);
            }
            var alg = w.Kit.B(chunk, "algae", 1.5f);
            for (int i = 0; i < nt; i++)
            {
                var p0 = Vector3.Lerp(pts[i, 0], pts[i, 1], 0.55f) + n * 0.02f;
                var p1 = Vector3.Lerp(pts[i + 1, 0], pts[i + 1, 1], 0.55f) + n * 0.02f;
                float h = rng.Range(0.3f, 0.9f);
                var q0 = p0 + Vector3.up * h; var q1 = p1 + Vector3.up * h;
                if (flip) alg.Quad(p0, p1, q1, q0);
                else alg.Quad(p1, p0, q0, q1);
            }
            // ports: gun ports on warships, round lights on the rest
            bool war = rng.Chance(0.5f);
            float portY = baseY + (war ? rng.Range(2.4f, 3.2f) : rng.Range(2.6f, 3.6f));
            for (float sp = s0 + 1.5f; sp < s1 - 1.2f; sp += war ? 2.2f : 1.7f)
            {
                if (rng.Chance(0.2f)) continue;
                var c = a + u * sp + Vector3.up * portY;
                float d = 0.06f + 0.085f * (portY - baseY - belly) * (portY - baseY - belly) + Mathf.Max(0f, portY - baseY) * lean * 0.12f;
                c -= n * (d - 0.03f);
                var rot = Quaternion.LookRotation(n, Vector3.up);
                if (war)
                {
                    w.Kit.Box(chunk, "black", c, new Vector3(0.62f, 0.55f, 0.02f), rot);
                    if (rng.Chance(0.6f)) w.Kit.Box(chunk, look.Trim, c + Vector3.up * 0.42f + n * 0.12f, new Vector3(0.7f, 0.06f, 0.5f), rot * Quaternion.Euler(-60, 0, 0));
                }
                else
                {
                    w.Kit.B(chunk, "black").Disc(c + n * 0.005f, Quaternion.FromToRotation(Vector3.up, n), 0.16f, 12);
                    var ring = new List<Vector3>();
                    for (int k = 0; k <= 14; k++)
                    {
                        float ang = k / 14f * Mathf.PI * 2f;
                        ring.Add(c + n * 0.02f + (Quaternion.LookRotation(n) * new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0)) * 0.18f);
                    }
                    w.Kit.B(chunk, "brass").Tube(ring, 0.02f, 4, 0.2f);
                }
            }
            // a breach: torn planks round a black hole
            if (rng.Chance(0.4f))
            {
                float sp = Mathf.Lerp(s0, s1, rng.Range(0.3f, 0.7f));
                float hy = baseY + rng.Range(0.6f, 2.2f);
                float d = 0.06f + 0.085f * (hy - baseY - belly) * (hy - baseY - belly);
                var c = a + u * sp + Vector3.up * hy - n * (d - 0.02f);
                var rot = Quaternion.LookRotation(n, Vector3.up);
                float bw = rng.Range(0.6f, 1.4f), bh = rng.Range(0.5f, 1.0f);
                w.Kit.Box(chunk, "black", c, new Vector3(bw, bh, 0.02f), rot * Quaternion.Euler(0, 0, rng.Range(-20f, 20f)));
                for (int k = 0; k < 6; k++)
                {
                    var off = new Vector3(rng.Range(-bw * 0.6f, bw * 0.6f), rng.Range(-bh * 0.6f, bh * 0.6f), 0.05f);
                    w.Kit.Box(chunk, mat, c + rot * off, new Vector3(0.2f, rng.Range(0.2f, 0.6f), 0.04f), rot * Quaternion.Euler(rng.Range(-30, 30), 0, rng.Range(-60, 60)));
                }
            }
            // nets, ladders, chains hanging down the side
            if (rng.Chance(0.35f))
            {
                float sp = Mathf.Lerp(s0, s1, rng.Range(0.2f, 0.8f));
                var top = a + u * sp + Vector3.up * (baseY + H - 0.3f) + n * 0.15f;
                var rope = w.Kit.B(chunk, "rope", 0.3f);
                for (int k = 0; k < 2; k++)
                {
                    var t0 = top + u * (k * 0.45f);
                    rope.Rope(t0, t0 + Vector3.down * (H - 0.8f) + n * 0.15f, 0.05f, 0.016f, 6);
                }
                for (float y = 0.4f; y < H - 1f; y += 0.36f)
                    rope.Rope(top + Vector3.down * y, top + Vector3.down * y + u * 0.45f, 0.02f, 0.012f, 3);
            }
        }

        // ------------------------------------------------------------------ houses (Act III)
        public sealed class HouseLook
        {
            public string Wall = "plaster";
            public string Timber = "wood_dark";
            public string Roof = "roof";
            public string Stone = "stone";
        }

        /// <summary>
        /// A town house standing behind a street edge: facade with windows and doors, timber framing,
        /// a pitched roof and a chimney. 'front' is the street side line; the house extends away from n.
        /// </summary>
        public static void House(World w, string chunk, Vector3 frontA, Vector3 frontB, Vector3 n, float baseY, float height, float depth, Rng rng, HouseLook look, bool lowerFacade = true, float lowerTop = 0f)
        {
            float width = Vector3.Distance(frontA, frontB);
            if (width < 1.5f) return;
            var u = (frontB - frontA) / width;
            var rot = Quaternion.LookRotation(n, Vector3.up);        // +Z faces the street
            var center = (frontA + frontB) * 0.5f;
            string wallMat = rng.Chance(0.3f) ? look.Stone : look.Wall;
            float yFrom = lowerFacade ? baseY - 0.6f : lowerTop;
            float hBody = baseY + height - yFrom;
            // body: front face only + side returns; the back is never seen
            var bodyC = center - n * (depth * 0.5f) + Vector3.up * (yFrom + hBody * 0.5f);
            w.Kit.Box(chunk, wallMat, bodyC, new Vector3(width, hBody, depth), rot, false, 16 | 1 | 2, 2f);
            // jettied upper storey on timber houses
            bool timber = wallMat != look.Stone;
            float storey = 2.9f;
            if (timber)
            {
                for (float y = baseY + storey; y < baseY + height - 0.4f; y += storey)
                    w.Kit.Box(chunk, look.Timber, center + n * 0.04f + Vector3.up * y, new Vector3(width + 0.1f, 0.2f, 0.16f), rot);
                for (float x = -width * 0.5f + 0.1f; x <= width * 0.5f; x += width / Mathf.Max(1, Mathf.Round(width / 1.6f)))
                    w.Kit.Box(chunk, look.Timber, center + u * x + n * 0.03f + Vector3.up * (Mathf.Max(yFrom, baseY + 0.2f) + (baseY + height - Mathf.Max(yFrom, baseY + 0.2f)) * 0.5f), new Vector3(0.16f, baseY + height - Mathf.Max(yFrom, baseY + 0.2f), 0.1f), rot);
            }
            // windows per storey
            int cols = Mathf.Max(1, Mathf.FloorToInt(width / 1.8f));
            for (float y = (lowerFacade ? baseY + 1.0f : lowerTop + 0.9f); y < baseY + height - 1.2f; y += storey)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = (c + 0.5f) / cols * width - width * 0.5f;
                    if (lowerFacade && y < baseY + 2f && rng.Chance(0.35f)) continue;
                    var wc = center + u * x + n * 0.035f + Vector3.up * (y + 0.6f);
                    bool lit = rng.Chance(0.06f);
                    w.Kit.Box(chunk, lit ? "glow_warm" : "glass", wc, new Vector3(0.7f, 1.0f, 0.02f), rot);
                    w.Kit.Box(chunk, look.Timber, wc + n * 0.02f + Vector3.up * 0.53f, new Vector3(0.86f, 0.08f, 0.08f), rot);
                    w.Kit.Box(chunk, look.Timber, wc + n * 0.03f + Vector3.down * 0.53f, new Vector3(0.9f, 0.07f, 0.14f), rot);
                    w.Kit.Box(chunk, look.Timber, wc + n * 0.02f, new Vector3(0.04f, 1.0f, 0.04f), rot);
                    if (rng.Chance(0.5f))
                    {
                        // a shutter hanging off one hinge
                        float side = rng.Chance(0.5f) ? -1f : 1f;
                        w.Kit.Box(chunk, look.Timber, wc + u * side * 0.56f + n * 0.08f, new Vector3(0.36f, 1.0f, 0.03f), rot * Quaternion.Euler(0, side * rng.Range(10f, 70f), rng.Range(-4f, 8f)));
                    }
                }
            }
            // roof: two slopes running along the street
            float ridgeH = Mathf.Clamp(depth * 0.45f, 1.6f, 3.2f);
            float roofY = baseY + height;
            var eaveF = center + n * 0.45f + Vector3.up * (roofY - 0.1f);
            var eaveB = center - n * (depth + 0.45f) + Vector3.up * (roofY - 0.1f);
            var ridge = center - n * (depth * 0.5f) + Vector3.up * (roofY + ridgeH);
            var rb = w.Kit.B(chunk, look.Roof, 1.4f);
            var hu = u * (width * 0.5f + 0.35f);
            Quad2(rb, eaveF - hu, eaveF + hu, ridge + hu, ridge - hu, n);
            Quad2(rb, ridge - hu, ridge + hu, eaveB + hu, eaveB - hu, -n);
            // gable ends
            var gb = w.Kit.B(chunk, wallMat, 2f);
            foreach (float sgn in new[] { -1f, 1f })
            {
                var g0 = center + hu * 0.98f * sgn + Vector3.up * roofY;
                var g1 = center + hu * 0.98f * sgn - n * depth + Vector3.up * roofY;
                var g2 = ridge + hu * 0.98f * sgn;
                gb.Tri(g0, g1, g2);
                gb.Tri(g1, g0, g2);   // both sides: gables are seen from either end of the street
            }
            if (rng.Chance(0.6f))
            {
                var ch = center + u * rng.Range(-width * 0.3f, width * 0.3f) - n * (depth * rng.Range(0.3f, 0.7f)) + Vector3.up * (roofY + ridgeH * 0.5f);
                w.Kit.Box(chunk, look.Stone, ch + Vector3.up * 0.8f, new Vector3(0.55f, 2.2f, 0.55f), rot);
            }
            // a door at street level, and sometimes a shop sign
            if (lowerFacade && width > 3f && rng.Chance(0.7f))
            {
                float dx = rng.Range(-width * 0.3f, width * 0.3f);
                var dc = center + u * dx + n * 0.03f + Vector3.up * (baseY + 1.05f);
                w.Kit.Box(chunk, look.Timber, dc, new Vector3(1.1f, 2.1f, 0.05f), rot);
                w.Kit.Box(chunk, "iron", dc + u * 0.38f + n * 0.04f, new Vector3(0.05f, 0.05f, 0.05f), rot);
                w.Kit.Box(chunk, look.Stone, dc + Vector3.up * 1.12f + n * 0.03f, new Vector3(1.35f, 0.16f, 0.1f), rot);
                if (rng.Chance(0.45f))
                {
                    var sc = dc + Vector3.up * 1.6f + u * 0.9f + n * 0.55f;
                    w.Kit.Box(chunk, "iron", sc + Vector3.up * 0.35f - n * 0.25f, new Vector3(0.03f, 0.03f, 0.7f), rot);
                    w.Kit.Box(chunk, "plaque", sc, new Vector3(0.04f, 0.5f, 0.7f), rot * Quaternion.Euler(0, 90, 0));
                }
            }
        }

        static void Quad2(MeshBuilder b, Vector3 a, Vector3 c, Vector3 d, Vector3 e, Vector3 outward)
        {
            var nrm = Vector3.Cross(e - a, c - a);
            if (Vector3.Dot(nrm, Vector3.up + outward * 0.5f) >= 0f) b.Quad(a, c, d, e);
            else b.Quad(c, a, e, d);
        }

        // ------------------------------------------------------------------ masts across corridors
        /// <summary>A broken mast leaning across from one side to the other, high overhead.</summary>
        public static void LeaningMast(World w, string chunk, Vector3 foot, Vector3 tip, float radius, Rng rng, bool sail)
        {
            var dir = tip - foot;
            float len = dir.magnitude;
            var rot = Quaternion.FromToRotation(Vector3.up, dir / len);
            w.Kit.B(chunk, "wood_dark", 2f).Cylinder(foot, rot, len, radius, radius * 0.7f, 12, true, 2f);
            // yard hanging from it
            var yp = Vector3.Lerp(foot, tip, rng.Range(0.55f, 0.8f));
            var yd = Quaternion.Euler(0, rng.Range(-30f, 30f), rng.Range(-20f, 20f)) * Vector3.Cross(dir, Vector3.up).normalized;
            w.Kit.B(chunk, "wood_dark").Cylinder(yp - yd * 3f, Quaternion.FromToRotation(Vector3.up, yd), 6f, radius * 0.45f, radius * 0.35f, 8);
            var rope = w.Kit.B(chunk, "rope", 0.3f);
            for (int i = 0; i < 4; i++)
            {
                var p = Vector3.Lerp(foot, tip, rng.Range(0.3f, 0.95f));
                rope.Rope(p, p + Vector3.down * rng.Range(1.2f, 3f) + new Vector3(rng.Range(-0.3f, 0.3f), 0, rng.Range(-0.3f, 0.3f)), 0.05f, 0.014f, 6);
            }
            if (sail)
            {
                var sp = Props.Sail(rng.Range(3f, 5f), rng.Range(2f, 3.5f), rng, "sail_light", 0.5f);
                sp.BakeInto(w.Kit, chunk, yp + Vector3.down * 0.1f, Quaternion.LookRotation(Vector3.Cross(yd, Vector3.up), Vector3.up));
            }
        }

        /// <summary>A lantern on a crooked post (the Act II counting lanterns).</summary>
        public static Light LanternPost(World w, string name, Vector3 foot, float height, bool lit, float lumens, Rng rng)
        {
            var rot = Quaternion.Euler(rng.Range(-4f, 4f), rng.Range(0f, 360f), rng.Range(-4f, 4f));
            var post = new PropMesh();
            post["wood_dark"].Box(new Vector3(0, height * 0.5f, 0), new Vector3(0.14f, height, 0.14f), Quaternion.identity);
            post["wood_dark"].Box(new Vector3(0.3f, height - 0.05f, 0), new Vector3(0.7f, 0.09f, 0.09f), Quaternion.identity);
            post.BakeInto(w.Kit, "props", foot, rot);
            w.Kit.Collider(foot + Vector3.up * (height * 0.5f), new Vector3(0.2f, height, 0.2f));
            var hang = foot + rot * new Vector3(0.6f, height - 0.1f, 0);
            return DressUtil.Lantern(w, name, hang, lit, lumens, 9f, lit, false, 2.2f);
        }
    }
}
