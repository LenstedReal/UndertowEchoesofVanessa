using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Procedural props. Every prop is built in local space: origin at the base centre, +Z is its front.
    /// </summary>
    public static class Props
    {
        static readonly Quaternion Id = Quaternion.identity;

        static Vector2 P(float r, float h) => new Vector2(r, h);

        // ------------------------------------------------------------------ furniture & cargo
        public static PropMesh Barrel(float h = 0.9f, float r = 0.3f)
        {
            var p = new PropMesh();
            var prof = new List<Vector2>();
            for (int i = 0; i <= 8; i++)
            {
                float t = i / 8f;
                float bulge = Mathf.Sin(t * Mathf.PI) * 0.12f * r;
                prof.Add(P(r * 0.86f + bulge, t * h));
            }
            p["wood"].Lathe(Vector3.zero, Id, prof, 14, 0.5f);
            p["wood"].Disc(new Vector3(0, h - 0.02f, 0), Id, r * 0.84f, 14);
            p["wood"].Disc(Vector3.up * 0.02f, Quaternion.Euler(180, 0, 0), r * 0.84f, 14);
            foreach (float hy in new[] { 0.12f, 0.36f, 0.64f, 0.88f })
            {
                float t = hy;
                float rr = r * 0.86f + Mathf.Sin(t * Mathf.PI) * 0.12f * r + 0.008f;
                p["iron"].Cylinder(new Vector3(0, h * t - 0.025f, 0), Id, 0.05f, rr, rr, 14, false);
            }
            return p;
        }

        public static PropMesh Crate(Vector3 size, bool lid = true)
        {
            var p = new PropMesh();
            p["wood"].Box(Vector3.up * size.y * 0.5f, size * 0.98f, Id);
            float e = 0.05f;
            var frame = p["wood_dark"];
            // vertical edges
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    frame.Box(new Vector3(sx * (size.x * 0.5f - e * 0.4f), size.y * 0.5f, sz * (size.z * 0.5f - e * 0.4f)), new Vector3(e, size.y, e), Id);
            // horizontal bands
            foreach (float y in new[] { e * 0.5f, size.y - e * 0.5f })
            {
                frame.Box(new Vector3(0, y, size.z * 0.5f), new Vector3(size.x, e, e), Id);
                frame.Box(new Vector3(0, y, -size.z * 0.5f), new Vector3(size.x, e, e), Id);
                frame.Box(new Vector3(size.x * 0.5f, y, 0), new Vector3(e, e, size.z), Id);
                frame.Box(new Vector3(-size.x * 0.5f, y, 0), new Vector3(e, e, size.z), Id);
            }
            // diagonal brace on the front
            var diag = Quaternion.Euler(0, 0, Mathf.Atan2(size.y, size.x) * Mathf.Rad2Deg);
            frame.Box(new Vector3(0, size.y * 0.5f, size.z * 0.5f + 0.01f), new Vector3(Mathf.Sqrt(size.x * size.x + size.y * size.y) * 0.9f, e, 0.02f), diag);
            return p;
        }

        public static PropMesh Table(float w = 1.4f, float d = 0.8f, float h = 0.78f, string mat = "wood")
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(0, h - 0.03f, 0), new Vector3(w, 0.06f, d), Id);
            foreach (var s in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                p[mat].Box(new Vector3(s.x * (w * 0.5f - 0.07f), (h - 0.06f) * 0.5f, s.y * (d * 0.5f - 0.07f)), new Vector3(0.07f, h - 0.06f, 0.07f), Id);
            p[mat].Box(new Vector3(0, 0.18f, d * 0.5f - 0.07f), new Vector3(w - 0.14f, 0.04f, 0.03f), Id);
            p[mat].Box(new Vector3(0, 0.18f, -d * 0.5f + 0.07f), new Vector3(w - 0.14f, 0.04f, 0.03f), Id);
            return p;
        }

        public static PropMesh Chair(string mat = "wood")
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(0, 0.45f, 0), new Vector3(0.44f, 0.04f, 0.42f), Id);
            foreach (var s in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                p[mat].Box(new Vector3(s.x * 0.19f, 0.225f, s.y * 0.18f), new Vector3(0.04f, 0.45f, 0.04f), Id);
            p[mat].Box(new Vector3(-0.19f, 0.72f, -0.18f), new Vector3(0.04f, 0.5f, 0.04f), Id);
            p[mat].Box(new Vector3(0.19f, 0.72f, -0.18f), new Vector3(0.04f, 0.5f, 0.04f), Id);
            p[mat].Box(new Vector3(0, 0.9f, -0.18f), new Vector3(0.42f, 0.1f, 0.03f), Id);
            p[mat].Box(new Vector3(0, 0.68f, -0.18f), new Vector3(0.42f, 0.05f, 0.03f), Id);
            return p;
        }

        public static PropMesh Stool()
        {
            var p = new PropMesh();
            p["wood"].Cylinder(new Vector3(0, 0.44f, 0), Id, 0.04f, 0.17f, 0.17f, 10);
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                var rot = Quaternion.Euler(0, a, 8f);
                p["wood"].Cylinder(Quaternion.Euler(0, a, 0) * new Vector3(0.11f, 0, 0), rot, 0.45f, 0.022f, 0.02f, 6, false);
            }
            return p;
        }

        public static PropMesh Shelf(float w = 1.2f, float h = 1.8f, float d = 0.32f, Rng rng = null, string mat = "wood_dark")
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(-w * 0.5f + 0.02f, h * 0.5f, 0), new Vector3(0.04f, h, d), Id);
            p[mat].Box(new Vector3(w * 0.5f - 0.02f, h * 0.5f, 0), new Vector3(0.04f, h, d), Id);
            p[mat].Box(new Vector3(0, h * 0.5f, -d * 0.5f + 0.01f), new Vector3(w, h, 0.02f), Id);
            int shelves = 4;
            for (int i = 0; i <= shelves; i++)
            {
                float y = 0.06f + i * (h - 0.1f) / shelves;
                p[mat].Box(new Vector3(0, y, 0), new Vector3(w - 0.04f, 0.03f, d), Id);
                if (rng == null || i == shelves) continue;
                float x = -w * 0.5f + 0.08f;
                while (x < w * 0.5f - 0.1f)
                {
                    float r = rng.Value();
                    if (r < 0.45f)
                    {
                        float bw = rng.Range(0.03f, 0.06f), bh = rng.Range(0.18f, 0.26f);
                        string bm = rng.Chance(0.5f) ? "cloth_red" : (rng.Chance(0.5f) ? "cloth_blue" : "wood_dark");
                        p[bm].Box(new Vector3(x + bw * 0.5f, y + 0.015f + bh * 0.5f, 0.02f), new Vector3(bw, bh, d * 0.75f), Quaternion.Euler(0, 0, rng.Chance(0.15f) ? 12f : 0f));
                        x += bw + 0.005f;
                    }
                    else if (r < 0.7f)
                    {
                        p["glass"].Cylinder(new Vector3(x + 0.04f, y + 0.015f, 0.02f), Id, rng.Range(0.12f, 0.22f), 0.035f, 0.025f, 8);
                        x += 0.1f;
                    }
                    else x += rng.Range(0.05f, 0.18f);
                }
            }
            return p;
        }

        public static PropMesh Bunk(float l = 1.9f, float w = 0.8f, bool doubleBunk = true)
        {
            var p = new PropMesh();
            int levels = doubleBunk ? 2 : 1;
            for (int i = 0; i < levels; i++)
            {
                float y = 0.35f + i * 0.95f;
                p["wood_dark"].Box(new Vector3(0, y, 0), new Vector3(w, 0.06f, l), Id);
                p["cloth"].Box(new Vector3(0, y + 0.07f, 0.05f), new Vector3(w - 0.08f, 0.09f, l - 0.2f), Id);
                p["cloth"].Box(new Vector3(0, y + 0.12f, -l * 0.5f + 0.2f), new Vector3(w - 0.2f, 0.1f, 0.3f), Id);
            }
            float top = 0.35f + (levels - 1) * 0.95f + 0.4f;
            foreach (var s in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
                p["wood_dark"].Box(new Vector3(s.x * (w * 0.5f - 0.03f), top * 0.5f, s.y * (l * 0.5f - 0.03f)), new Vector3(0.06f, top, 0.06f), Id);
            return p;
        }

        public static PropMesh Hammock(float l = 2.2f)
        {
            var p = new PropMesh();
            var b = p["cloth"];
            var pts = new List<Vector3>();
            var a = new Vector3(0, 1.7f, -l * 0.5f);
            var c = new Vector3(0, 1.7f, l * 0.5f);
            // cloth as a curved strip
            int n = 10;
            for (int i = 0; i < n; i++)
            {
                float t0 = i / (float)n, t1 = (i + 1) / (float)n;
                float y0 = 1.7f - Mathf.Sin(t0 * Mathf.PI) * 0.55f, y1 = 1.7f - Mathf.Sin(t1 * Mathf.PI) * 0.55f;
                float z0 = Mathf.Lerp(-l * 0.4f, l * 0.4f, t0), z1 = Mathf.Lerp(-l * 0.4f, l * 0.4f, t1);
                float w0 = 0.35f * Mathf.Sin(t0 * Mathf.PI) + 0.05f, w1 = 0.35f * Mathf.Sin(t1 * Mathf.PI) + 0.05f;
                b.Quad(new Vector3(-w0, y0, z0), new Vector3(w0, y0, z0), new Vector3(w1, y1, z1), new Vector3(-w1, y1, z1));
            }
            p["rope"].Rope(a, new Vector3(0, 1.7f, -l * 0.4f), 0.02f, 0.012f);
            p["rope"].Rope(new Vector3(0, 1.7f, l * 0.4f), c, 0.02f, 0.012f);
            return p;
        }

        public static PropMesh SeaChest()
        {
            var p = new PropMesh();
            p["wood_dark"].Box(new Vector3(0, 0.22f, 0), new Vector3(0.8f, 0.44f, 0.46f), Id);
            p["wood_dark"].Cylinder(new Vector3(-0.4f, 0.44f, 0), Quaternion.Euler(0, 0, -90), 0.8f, 0.23f, 0.23f, 10, true);
            foreach (float x in new[] { -0.28f, 0.28f })
                p["iron"].Box(new Vector3(x, 0.33f, 0), new Vector3(0.05f, 0.68f, 0.5f), Id);
            p["brass"].Box(new Vector3(0, 0.42f, 0.24f), new Vector3(0.09f, 0.1f, 0.02f), Id);
            return p;
        }

        // ------------------------------------------------------------------ ship
        public static PropMesh Cannon()
        {
            var p = new PropMesh();
            p["wood_dark"].Box(new Vector3(0, 0.22f, -0.2f), new Vector3(0.62f, 0.3f, 1.1f), Id);
            foreach (float z in new[] { -0.6f, 0.25f })
                foreach (float x in new[] { -0.33f, 0.33f })
                    p["wood_dark"].Cylinder(new Vector3(x, 0.14f, z), Quaternion.Euler(0, 0, 90), 0.08f, 0.14f, 0.14f, 10);
            var prof = new List<Vector2> { P(0.17f, 0), P(0.19f, 0.15f), P(0.16f, 0.2f), P(0.15f, 0.9f), P(0.13f, 1.6f), P(0.15f, 1.65f), P(0.1f, 1.7f) };
            p["darkmetal"].Lathe(new Vector3(0, 0.5f, -0.75f), Quaternion.Euler(90, 0, 0), prof, 14);
            return p;
        }

        public static PropMesh Helm()
        {
            var p = new PropMesh();
            p["wood_dark"].Box(new Vector3(0, 0.55f, -0.12f), new Vector3(0.16f, 1.1f, 0.22f), Id);
            p["wood_dark"].Box(new Vector3(0, 0.05f, -0.12f), new Vector3(0.6f, 0.1f, 0.6f), Id);
            var center = new Vector3(0, 1.15f, 0.05f);
            var rot = Quaternion.Euler(90, 0, 0);
            p["wood"].Cylinder(center - Vector3.forward * 0.06f, rot, 0.12f, 0.1f, 0.1f, 12);
            var ring = new List<Vector3>();
            for (int i = 0; i <= 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2f;
                ring.Add(center + new Vector3(Mathf.Cos(a) * 0.55f, Mathf.Sin(a) * 0.55f, 0));
            }
            p["wood"].Tube(ring, 0.035f, 6, 0.4f);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * 360f;
                var dir = Quaternion.Euler(0, 0, a) * Vector3.up;
                p["wood"].Tube(new[] { center, center + dir * 0.75f }, 0.025f, 6, 0.4f);
            }
            return p;
        }

        public static PropMesh Mast(float height, float radius, float yardY, float yardLen)
        {
            var p = new PropMesh();
            p["wood_dark"].Cylinder(Vector3.zero, Id, height, radius, radius * 0.6f, 14, true, 2f);
            foreach (float k in new[] { yardY, yardY + (height - yardY) * 0.45f })
            {
                p["wood_dark"].Cylinder(new Vector3(-yardLen * 0.5f, k, 0), Quaternion.Euler(0, 0, -90), yardLen, radius * 0.45f, radius * 0.3f, 10, true, 2f);
            }
            p["iron"].Cylinder(new Vector3(0, 1.0f, 0), Id, 0.12f, radius + 0.02f, radius + 0.02f, 14, false);
            p["iron"].Cylinder(new Vector3(0, yardY - 0.3f, 0), Id, 0.1f, radius * 0.95f, radius * 0.95f, 14, false);
            // crow's nest
            float cy = yardY + (height - yardY) * 0.25f;
            p["wood_dark"].Cylinder(new Vector3(0, cy, 0), Id, 0.08f, radius * 4f, radius * 4f, 14, true);
            return p;
        }

        /// <summary>A torn sail hanging from a yard: subdivided cloth with holes and sag.</summary>
        public static PropMesh Sail(float width, float height, Rng rng, string mat = "sail", float tear = 0.25f)
        {
            var p = new PropMesh();
            var b = p[mat];
            int nx = 12, ny = 10;
            var pts = new Vector3[nx + 1, ny + 1];
            for (int y = 0; y <= ny; y++)
                for (int x = 0; x <= nx; x++)
                {
                    float fx = x / (float)nx, fy = y / (float)ny;
                    float belly = Mathf.Sin(fx * Mathf.PI) * Mathf.Sin(fy * Mathf.PI * 0.85f) * 0.6f;
                    float sag = fy * fy * 0.15f;
                    pts[x, y] = new Vector3((fx - 0.5f) * width * (1f - fy * 0.08f), -fy * height, belly + sag + (rng.Value() - 0.5f) * 0.04f);
                }
            for (int y = 0; y < ny; y++)
                for (int x = 0; x < nx; x++)
                {
                    float fy = y / (float)ny;
                    if (fy > 0.3f && rng.Chance(tear * fy)) continue;
                    var a = pts[x, y]; var c = pts[x + 1, y]; var d = pts[x + 1, y + 1]; var e = pts[x, y + 1];
                    b.QuadUV(a, c, d, e, new Vector2(x / (float)nx, 1 - y / (float)ny), new Vector2((x + 1) / (float)nx, 1 - y / (float)ny), new Vector2((x + 1) / (float)nx, 1 - (y + 1) / (float)ny), new Vector2(x / (float)nx, 1 - (y + 1) / (float)ny));
                }
            return p;
        }

        public static PropMesh RopeCoil(float r = 0.32f)
        {
            var p = new PropMesh();
            var pts = new List<Vector3>();
            for (int i = 0; i <= 80; i++)
            {
                float t = i / 80f;
                float a = t * Mathf.PI * 2f * 4f;
                float rr = r * (0.45f + 0.55f * (1f - Mathf.Floor(t * 4f) / 4f * 0.25f));
                pts.Add(new Vector3(Mathf.Cos(a) * rr, 0.035f + Mathf.Floor(t * 4f) * 0.045f, Mathf.Sin(a) * rr));
            }
            p["rope"].Tube(pts, 0.03f, 6, 0.3f);
            return p;
        }

        public static PropMesh HangingLantern(bool lit)
        {
            var p = new PropMesh();
            p["iron"].Cylinder(new Vector3(0, -0.06f, 0), Id, 0.04f, 0.1f, 0.1f, 8);
            p["iron"].Cylinder(new Vector3(0, -0.36f, 0), Id, 0.03f, 0.11f, 0.11f, 8);
            p["glass"].Cylinder(new Vector3(0, -0.33f, 0), Id, 0.27f, 0.085f, 0.085f, 8, false);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f + 45f;
                var o = Quaternion.Euler(0, a, 0) * new Vector3(0.095f, 0, 0);
                p["iron"].Box(o + new Vector3(0, -0.2f, 0), new Vector3(0.012f, 0.3f, 0.012f), Id);
            }
            p["iron"].Tube(new[] { new Vector3(0, -0.02f, 0), new Vector3(0, 0.12f, 0) }, 0.008f, 4, 0.2f);
            if (lit) p["flame"].Lathe(new Vector3(0, -0.28f, 0), Id, new[] { P(0.0f, 0f), P(0.018f, 0.02f), P(0.012f, 0.06f), P(0.0f, 0.09f) }, 6);
            return p;
        }

        public static PropMesh Bell(float r, string mat = "brass")
        {
            var p = new PropMesh();
            var prof = new List<Vector2>
            {
                P(r * 1.0f, 0f), P(r * 0.98f, r * 0.08f), P(r * 0.8f, r * 0.35f), P(r * 0.66f, r * 0.75f),
                P(r * 0.6f, r * 1.05f), P(r * 0.52f, r * 1.25f), P(r * 0.3f, r * 1.38f), P(0.001f, r * 1.42f),
            };
            p[mat].Lathe(new Vector3(0, -r * 1.42f, 0), Id, prof, 18, 0.3f);
            // inner surface (visible from below)
            var inner = new List<Vector2>();
            for (int i = prof.Count - 1; i >= 0; i--) inner.Add(new Vector2(Mathf.Max(0.001f, prof[i].x - r * 0.06f), prof[i].y + r * 0.03f));
            p["darkmetal"].Lathe(new Vector3(0, -r * 1.42f, 0), Id, inner, 18, 0.3f);
            p["darkmetal"].Tube(new[] { new Vector3(0, -r * 0.1f, 0), new Vector3(0, -r * 1.1f, 0) }, r * 0.05f, 5, 0.2f);
            p["darkmetal"].Cylinder(new Vector3(0, -r * 1.25f, 0), Id, r * 0.18f, r * 0.12f, r * 0.12f, 8);
            p["iron"].Box(new Vector3(0, r * 0.06f, 0), new Vector3(r * 0.5f, r * 0.12f, r * 0.12f), Id);
            return p;
        }

        // ------------------------------------------------------------------ story objects
        public static PropMesh MusicBox(float lidAngle = 105f, bool worn = false)
        {
            var p = new PropMesh();
            string wood = worn ? "wood" : "wood_dark";
            p[wood].Box(new Vector3(0, 0.07f, 0), new Vector3(0.26f, 0.14f, 0.18f), Id);
            var lidPivot = new Vector3(0, 0.14f, -0.09f);
            var lidRot = Quaternion.Euler(-lidAngle, 0, 0);
            p[wood].Box(lidPivot + lidRot * new Vector3(0, 0.015f, 0.09f), new Vector3(0.26f, 0.03f, 0.18f), lidRot);
            p["mirror_old"].Box(lidPivot + lidRot * new Vector3(0, -0.002f, 0.09f), new Vector3(0.2f, 0.004f, 0.13f), lidRot);
            p["brass"].Cylinder(new Vector3(-0.08f, 0.115f, 0.0f), Quaternion.Euler(0, 0, -90), 0.16f, 0.025f, 0.025f, 12);
            p["brass"].Box(new Vector3(0, 0.1f, 0.035f), new Vector3(0.16f, 0.02f, 0.03f), Id);
            p["brass"].Cylinder(new Vector3(0.13f, 0.07f, 0f), Quaternion.Euler(0, 0, -90), 0.04f, 0.012f, 0.012f, 6);
            p["brass"].Box(new Vector3(0.175f, 0.07f, 0f), new Vector3(0.01f, 0.06f, 0.015f), Id);
            foreach (var c in new[] { new Vector3(-0.125f, 0, -0.085f), new Vector3(0.125f, 0, -0.085f), new Vector3(-0.125f, 0, 0.085f), new Vector3(0.125f, 0, 0.085f) })
                p["brass"].Box(c + Vector3.up * 0.07f, new Vector3(0.02f, 0.145f, 0.02f), Id);
            return p;
        }

        public static PropMesh Book(bool open, string cover = "cloth_red")
        {
            var p = new PropMesh();
            if (!open)
            {
                p[cover].Box(new Vector3(0, 0.025f, 0), new Vector3(0.22f, 0.05f, 0.3f), Id);
                p["paper"].Box(new Vector3(0.006f, 0.025f, 0), new Vector3(0.2f, 0.04f, 0.285f), Id);
                return p;
            }
            p[cover].Box(new Vector3(-0.15f, 0.008f, 0), new Vector3(0.3f, 0.016f, 0.32f), Quaternion.Euler(0, 0, 4));
            p[cover].Box(new Vector3(0.15f, 0.008f, 0), new Vector3(0.3f, 0.016f, 0.32f), Quaternion.Euler(0, 0, -4));
            p["paper"].Box(new Vector3(-0.14f, 0.024f, 0), new Vector3(0.27f, 0.02f, 0.3f), Quaternion.Euler(0, 0, 5));
            p["paper"].Box(new Vector3(0.14f, 0.024f, 0), new Vector3(0.27f, 0.02f, 0.3f), Quaternion.Euler(0, 0, -5));
            return p;
        }

        public static PropMesh Lectern(string mat = "wood_dark")
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(0, 0.02f, 0), new Vector3(0.45f, 0.04f, 0.4f), Id);
            p[mat].Box(new Vector3(0, 0.5f, -0.05f), new Vector3(0.1f, 1.0f, 0.1f), Id);
            p[mat].Box(new Vector3(0, 1.02f, 0f), new Vector3(0.5f, 0.04f, 0.38f), Quaternion.Euler(18, 0, 0));
            var book = Book(true, "wood_dark");
            foreach (var kv in book.Parts) p[kv.Key].Append(kv.Value, Matrix4x4.TRS(new Vector3(0, 1.06f, 0.0f), Quaternion.Euler(18, 0, 0), Vector3.one));
            return p;
        }

        public static PropMesh Paper(string mat = "paper", float w = 0.21f, float h = 0.28f, float fold = 6f)
        {
            var p = new PropMesh();
            var b = p[mat];
            var r1 = Quaternion.Euler(0, 0, fold);
            var r2 = Quaternion.Euler(0, 0, -fold);
            Vector3 L(Vector3 v) => r1 * v;
            Vector3 R(Vector3 v) => r2 * v;
            b.QuadUV(L(new Vector3(-w * 0.5f, 0.003f, -h * 0.5f)), new Vector3(0, 0.003f, -h * 0.5f), new Vector3(0, 0.003f, h * 0.5f), L(new Vector3(-w * 0.5f, 0.003f, h * 0.5f)),
                new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, 1));
            b.QuadUV(new Vector3(0, 0.003f, -h * 0.5f), R(new Vector3(w * 0.5f, 0.003f, -h * 0.5f)), R(new Vector3(w * 0.5f, 0.003f, h * 0.5f)), new Vector3(0, 0.003f, h * 0.5f),
                new Vector2(0.5f, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0.5f, 1));
            return p;
        }

        /// <summary>Picture on a wall: frame + canvas facing +Z. Origin at the bottom centre of the frame.</summary>
        public static PropMesh Framed(string canvas, float w, float h, string frame = "wood_dark", float depth = 0.05f)
        {
            var p = new PropMesh();
            p[canvas].QuadUV(new Vector3(-w * 0.5f, 0, depth + 0.002f), new Vector3(w * 0.5f, 0, depth + 0.002f), new Vector3(w * 0.5f, h, depth + 0.002f), new Vector3(-w * 0.5f, h, depth + 0.002f),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            float t = 0.07f;
            p[frame].Box(new Vector3(0, -t * 0.5f, depth * 0.5f), new Vector3(w + 2 * t, t, depth + 0.02f), Id);
            p[frame].Box(new Vector3(0, h + t * 0.5f, depth * 0.5f), new Vector3(w + 2 * t, t, depth + 0.02f), Id);
            p[frame].Box(new Vector3(-w * 0.5f - t * 0.5f, h * 0.5f, depth * 0.5f), new Vector3(t, h, depth + 0.02f), Id);
            p[frame].Box(new Vector3(w * 0.5f + t * 0.5f, h * 0.5f, depth * 0.5f), new Vector3(t, h, depth + 0.02f), Id);
            p[frame].Box(new Vector3(0, h * 0.5f, 0.005f), new Vector3(w, h, 0.01f), Id);
            return p;
        }

        /// <summary>Paper pinned flat to a wall, facing +Z.</summary>
        public static PropMesh Pinned(string mat, float w, float h)
        {
            var p = new PropMesh();
            p[mat].QuadUV(new Vector3(-w * 0.5f, 0, 0.004f), new Vector3(w * 0.5f, 0, 0.004f), new Vector3(w * 0.5f, h, 0.012f), new Vector3(-w * 0.5f, h, 0.012f),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
            p["brass"].Cylinder(new Vector3(0, h - 0.03f, 0.012f), Quaternion.Euler(90, 0, 0), 0.008f, 0.007f, 0.007f, 6);
            return p;
        }

        public static PropMesh Hook()
        {
            var p = new PropMesh();
            var pts = new List<Vector3>();
            for (int i = 0; i <= 14; i++)
            {
                float t = i / 14f;
                float a = t * Mathf.PI * 1.25f;
                pts.Add(new Vector3(Mathf.Sin(a) * 0.09f, 0.1f - Mathf.Cos(a) * 0.09f, 0));
            }
            pts.Insert(0, new Vector3(0, 0.42f, 0));
            p["iron"].Tube(pts, 0.016f, 6, 0.2f);
            p["wood_dark"].Cylinder(new Vector3(0, 0.38f, 0), Id, 0.2f, 0.028f, 0.028f, 8);
            return p;
        }

        public static PropMesh Hatch(float w, float d, bool grate = true)
        {
            var p = new PropMesh();
            p["wood_dark"].Box(new Vector3(0, 0.06f, 0), new Vector3(w, 0.12f, d), Id);
            if (grate)
            {
                for (float x = -w * 0.5f + 0.15f; x < w * 0.5f; x += 0.22f) p["wood"].Box(new Vector3(x, 0.14f, 0), new Vector3(0.06f, 0.05f, d - 0.1f), Id);
                for (float z = -d * 0.5f + 0.15f; z < d * 0.5f; z += 0.22f) p["wood"].Box(new Vector3(0, 0.16f, z), new Vector3(w - 0.1f, 0.04f, 0.05f), Id);
            }
            p["iron"].Box(new Vector3(0, 0.2f, -d * 0.5f + 0.15f), new Vector3(0.18f, 0.03f, 0.05f), Id);
            var ring = new List<Vector3>();
            for (int i = 0; i <= 16; i++)
            {
                float a = i / 16f * Mathf.PI * 2f;
                ring.Add(new Vector3(Mathf.Cos(a) * 0.08f, 0.22f + Mathf.Sin(a) * 0.08f, -d * 0.5f + 0.15f));
            }
            p["iron"].Tube(ring, 0.012f, 5, 0.2f);
            return p;
        }

        public static PropMesh Lever()
        {
            var p = new PropMesh();
            p["iron"].Box(new Vector3(0, 0.6f, -0.02f), new Vector3(0.22f, 0.5f, 0.04f), Id);
            p["iron"].Tube(new[] { new Vector3(0, 0.6f, 0.02f), new Vector3(0, 1.05f, 0.18f) }, 0.022f, 6, 0.3f);
            p["wood_dark"].Cylinder(new Vector3(0, 1.05f, 0.18f), Quaternion.Euler(0, 0, 90), 0.16f, 0.03f, 0.03f, 8);
            return p;
        }

        public static PropMesh Locket()
        {
            var p = new PropMesh();
            p["brass"].Cylinder(Vector3.zero, Quaternion.Euler(90, 0, 0), 0.012f, 0.03f, 0.03f, 14);
            var chain = new List<Vector3>();
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                chain.Add(new Vector3(Mathf.Sin(t * Mathf.PI * 2f) * 0.07f, 0.005f, 0.03f + Mathf.Cos(t * Mathf.PI * 2f) * 0.06f + 0.06f));
            }
            p["brass"].Tube(chain, 0.003f, 4, 0.1f);
            return p;
        }

        public static PropMesh PocketWatch(bool open = true)
        {
            var p = new PropMesh();
            p["brass"].Cylinder(Vector3.zero, Id, 0.014f, 0.026f, 0.026f, 16);
            p["paper"].Disc(new Vector3(0, 0.0145f, 0), Id, 0.022f, 16);
            // stopped at 03:17, like every clock touched by the Undertow (12 o'clock = local +Z)
            float hourA = (3f + 17f / 60f) * 30f, minA = 17f * 6f;
            p["black"].Box(Quaternion.Euler(0, hourA, 0) * new Vector3(0, 0.016f, 0.006f), new Vector3(0.002f, 0.001f, 0.012f), Quaternion.Euler(0, hourA, 0));
            p["black"].Box(Quaternion.Euler(0, minA, 0) * new Vector3(0, 0.0165f, 0.009f), new Vector3(0.0015f, 0.001f, 0.018f), Quaternion.Euler(0, minA, 0));
            if (open) p["brass"].Cylinder(new Vector3(0, 0.01f, -0.026f), Quaternion.Euler(-80, 0, 0), 0.004f, 0.026f, 0.026f, 16);
            return p;
        }

        public static PropMesh OilFlask()
        {
            var p = new PropMesh();
            var prof = new List<Vector2> { P(0.07f, 0), P(0.085f, 0.03f), P(0.085f, 0.14f), P(0.06f, 0.2f), P(0.025f, 0.23f), P(0.022f, 0.28f) };
            p["brass"].Lathe(Vector3.zero, Id, prof, 12);
            p["wood_dark"].Cylinder(new Vector3(0, 0.275f, 0), Id, 0.035f, 0.02f, 0.017f, 8);
            p["rope"].Cylinder(new Vector3(0, 0.07f, 0), Id, 0.04f, 0.088f, 0.088f, 12, false);
            return p;
        }

        public static PropMesh Headphones()
        {
            var p = new PropMesh();
            var band = new List<Vector3>();
            for (int i = 0; i <= 14; i++)
            {
                float a = Mathf.Lerp(-80f, 80f, i / 14f) * Mathf.Deg2Rad;
                band.Add(new Vector3(Mathf.Sin(a) * 0.09f, 0.03f, Mathf.Cos(a) * 0.09f - 0.04f));
            }
            p["black"].Tube(band, 0.008f, 5, 0.1f);
            p["black"].Cylinder(new Vector3(-0.09f, 0.0f, -0.04f), Quaternion.Euler(0, 0, 90), 0.03f, 0.04f, 0.04f, 12);
            p["black"].Cylinder(new Vector3(0.12f, 0.0f, -0.04f), Quaternion.Euler(0, 0, 90), 0.03f, 0.04f, 0.04f, 12);
            var cable = new List<Vector3>();
            for (int i = 0; i <= 10; i++) cable.Add(new Vector3(0.12f + i * 0.04f, 0.005f, -0.04f - Mathf.Sin(i * 0.7f) * 0.05f));
            p["black"].Tube(cable, 0.003f, 4, 0.1f);
            return p;
        }

        public static PropMesh Chest(bool open)
        {
            var p = SeaChest();
            if (open)
            {
                p["temple"].Box(new Vector3(0, 0.03f, 0), new Vector3(0.9f, 0.06f, 0.56f), Id);
            }
            return p;
        }

        public static PropMesh Pedestal(float h = 1.0f, string mat = "temple")
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(0, 0.08f, 0), new Vector3(0.7f, 0.16f, 0.7f), Id);
            p[mat].Box(new Vector3(0, h * 0.5f, 0), new Vector3(0.48f, h - 0.1f, 0.48f), Id);
            p[mat].Box(new Vector3(0, h - 0.05f, 0), new Vector3(0.62f, 0.1f, 0.62f), Id);
            return p;
        }

        /// <summary>Temple mirror on a turning pedestal. The scratches on the base tell the number of turns.</summary>
        public static PropMesh MirrorBase(int scratches)
        {
            var p = Pedestal(0.7f);
            for (int i = 0; i < scratches; i++)
            {
                float x = (i - (scratches - 1) * 0.5f) * 0.07f;
                p["black"].Box(new Vector3(x, 0.705f, 0.22f), new Vector3(0.012f, 0.004f, 0.12f), Quaternion.Euler(0, 15f, 0));
            }
            return p;
        }

        public static PropMesh MirrorHead()
        {
            var p = new PropMesh();
            p["brass"].Cylinder(Vector3.zero, Id, 0.06f, 0.18f, 0.18f, 14);
            p["brass"].Box(new Vector3(-0.42f, 0.55f, 0), new Vector3(0.05f, 1.0f, 0.06f), Id);
            p["brass"].Box(new Vector3(0.42f, 0.55f, 0), new Vector3(0.05f, 1.0f, 0.06f), Id);
            p["brass"].Box(new Vector3(0, 1.06f, 0), new Vector3(0.9f, 0.05f, 0.06f), Id);
            p["brass"].Box(new Vector3(0, 0.07f, 0), new Vector3(0.9f, 0.05f, 0.06f), Id);
            p["mirror"].Box(new Vector3(0, 0.56f, 0.0f), new Vector3(0.8f, 0.94f, 0.02f), Id);
            return p;
        }

        public static PropMesh Brazier()
        {
            var p = new PropMesh();
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                var foot = Quaternion.Euler(0, a, 0) * new Vector3(0.32f, 0, 0);
                p["darkmetal"].Tube(new[] { foot, new Vector3(foot.x * 0.4f, 0.75f, foot.z * 0.4f) }, 0.025f, 5, 0.3f);
            }
            var bowl = new List<Vector2> { P(0.05f, 0f), P(0.24f, 0.06f), P(0.36f, 0.18f), P(0.4f, 0.24f) };
            p["darkmetal"].Lathe(new Vector3(0, 0.7f, 0), Id, bowl, 16);
            p["black"].Disc(new Vector3(0, 0.86f, 0), Id, 0.34f, 16);
            return p;
        }

        public static PropMesh TapeRecorder()
        {
            var p = new PropMesh();
            p["black"].Box(new Vector3(0, 0.05f, 0), new Vector3(0.32f, 0.1f, 0.2f), Id);
            p["darkmetal"].Box(new Vector3(0, 0.101f, 0.02f), new Vector3(0.28f, 0.004f, 0.12f), Id);
            p["wood_dark"].Cylinder(new Vector3(-0.07f, 0.1f, 0.02f), Id, 0.012f, 0.035f, 0.035f, 12);
            p["wood_dark"].Cylinder(new Vector3(0.07f, 0.1f, 0.02f), Id, 0.012f, 0.035f, 0.035f, 12);
            for (int i = 0; i < 5; i++) p["darkmetal"].Box(new Vector3(-0.1f + i * 0.05f, 0.1f, -0.075f), new Vector3(0.035f, 0.02f, 0.03f), Id);
            p["iron"].Tube(new[] { new Vector3(-0.15f, 0.1f, -0.1f), new Vector3(-0.15f, 0.2f, -0.1f), new Vector3(0.15f, 0.2f, -0.1f), new Vector3(0.15f, 0.1f, -0.1f) }, 0.006f, 5, 0.1f);
            return p;
        }

        public static PropMesh Doll()
        {
            var p = new PropMesh();
            p["velvet"].Lathe(Vector3.zero, Id, new[] { P(0.08f, 0f), P(0.07f, 0.06f), P(0.04f, 0.16f), P(0.03f, 0.2f) }, 10);
            p["porcelain"].Lathe(new Vector3(0, 0.2f, 0), Id, new[] { P(0.0f, 0f), P(0.035f, 0.01f), P(0.045f, 0.045f), P(0.035f, 0.08f), P(0.0f, 0.09f) }, 10);
            p["wood_dark"].Lathe(new Vector3(0, 0.25f, -0.005f), Id, new[] { P(0.05f, 0f), P(0.05f, 0.03f), P(0.02f, 0.05f), P(0f, 0.052f) }, 10);
            return p;
        }

        public static PropMesh Statue(bool turned)
        {
            var p = new PropMesh();
            p["stone"].Box(new Vector3(0, 0.45f, 0), new Vector3(1.1f, 0.9f, 1.1f), Id);
            p["stone"].Box(new Vector3(0, 0.95f, 0), new Vector3(1.25f, 0.1f, 1.25f), Id);
            var body = new List<Vector2> { P(0.36f, 0f), P(0.4f, 0.4f), P(0.3f, 1.0f), P(0.26f, 1.3f), P(0.33f, 1.45f), P(0.18f, 1.55f), P(0.06f, 1.62f) };
            p["stone"].Lathe(new Vector3(0, 1.0f, 0), Id, body, 12);
            p["stone"].Lathe(new Vector3(turned ? 0.04f : 0f, 2.62f, turned ? -0.04f : 0f), Quaternion.Euler(0, turned ? 160f : 0f, turned ? 8f : 0f),
                new[] { P(0.0f, 0f), P(0.1f, 0.04f), P(0.13f, 0.15f), P(0.11f, 0.26f), P(0.0f, 0.3f) }, 10);
            p["stone"].Tube(new[] { new Vector3(-0.3f, 2.35f, 0.05f), new Vector3(-0.45f, 1.95f, 0.2f), new Vector3(-0.35f, 1.65f, 0.3f) }, 0.07f, 6, 0.5f);
            p["stone"].Tube(new[] { new Vector3(0.3f, 2.35f, 0.05f), new Vector3(0.4f, 2.0f, -0.15f), new Vector3(0.2f, 1.8f, -0.3f) }, 0.07f, 6, 0.5f);
            return p;
        }

        public static PropMesh Column(float h, float r, string mat = "temple", bool broken = false)
        {
            var p = new PropMesh();
            p[mat].Box(new Vector3(0, 0.15f, 0), new Vector3(r * 2.6f, 0.3f, r * 2.6f), Id);
            float shaftH = broken ? h * 0.55f : h - 0.6f;
            var prof = new List<Vector2> { P(r * 1.1f, 0f), P(r, 0.1f), P(r * 0.92f, shaftH * 0.5f), P(r * 0.88f, shaftH) };
            p[mat].Lathe(new Vector3(0, 0.3f, 0), Id, prof, 16, 2f);
            if (!broken)
            {
                p[mat].Lathe(new Vector3(0, 0.3f + shaftH, 0), Id, new[] { P(r * 0.88f, 0f), P(r * 1.2f, 0.18f), P(r * 1.25f, 0.24f) }, 16);
                p[mat].Box(new Vector3(0, h - 0.03f, 0), new Vector3(r * 2.7f, 0.06f, r * 2.7f), Id);
            }
            else p[mat].Disc(new Vector3(0, 0.3f + shaftH, 0), Id, r * 0.88f, 16);
            return p;
        }

        public static PropMesh Altar()
        {
            var p = new PropMesh();
            p["temple"].Box(new Vector3(0, 0.2f, 0), new Vector3(3.2f, 0.4f, 2.2f), Id);
            p["temple"].Box(new Vector3(0, 0.75f, 0), new Vector3(2.2f, 0.7f, 1.2f), Id);
            p["temple"].Box(new Vector3(0, 1.14f, 0), new Vector3(2.5f, 0.1f, 1.45f), Id);
            p["black"].Box(new Vector3(0, 1.19f, 0), new Vector3(0.5f, 0.02f, 0.3f), Id);
            p["algae"].Box(new Vector3(-1.1f, 0.42f, 0.62f), new Vector3(0.6f, 0.04f, 0.04f), Id);
            return p;
        }

        public static PropMesh Rowboat()
        {
            var p = new PropMesh();
            var b = p["hull"];
            int n = 10;
            float len = 3.6f;
            for (int i = 0; i < n; i++)
            {
                float t0 = i / (float)n, t1 = (i + 1) / (float)n;
                float w0 = Mathf.Sin(t0 * Mathf.PI) * 0.75f + 0.04f, w1 = Mathf.Sin(t1 * Mathf.PI) * 0.75f + 0.04f;
                float z0 = (t0 - 0.5f) * len, z1 = (t1 - 0.5f) * len;
                float d0 = 0.45f * Mathf.Sin(t0 * Mathf.PI) + 0.1f, d1 = 0.45f * Mathf.Sin(t1 * Mathf.PI) + 0.1f;
                b.Quad(new Vector3(-w0, 0.5f, z0), new Vector3(-w1, 0.5f, z1), new Vector3(-w1 * 0.4f, 0.5f - d1, z1), new Vector3(-w0 * 0.4f, 0.5f - d0, z0));
                b.Quad(new Vector3(w0 * 0.4f, 0.5f - d0, z0), new Vector3(w1 * 0.4f, 0.5f - d1, z1), new Vector3(w1, 0.5f, z1), new Vector3(w0, 0.5f, z0));
                b.Quad(new Vector3(-w0 * 0.4f, 0.5f - d0, z0), new Vector3(-w1 * 0.4f, 0.5f - d1, z1), new Vector3(w1 * 0.4f, 0.5f - d1, z1), new Vector3(w0 * 0.4f, 0.5f - d0, z0));
            }
            foreach (float z in new[] { -0.5f, 0.6f }) p["wood"].Box(new Vector3(0, 0.35f, z), new Vector3(1.3f, 0.04f, 0.25f), Id);
            p["wood"].Cylinder(new Vector3(-0.6f, 0.45f, 0.1f), Quaternion.Euler(80, 20, 0), 2.2f, 0.025f, 0.02f, 6);
            return p;
        }

        public static PropMesh Board(float w = 1.2f, float h = 0.9f)
        {
            var p = new PropMesh();
            p["wood_dark"].Box(new Vector3(-w * 0.5f, 0.9f, 0), new Vector3(0.08f, 1.8f, 0.08f), Id);
            p["wood_dark"].Box(new Vector3(w * 0.5f, 0.9f, 0), new Vector3(0.08f, 1.8f, 0.08f), Id);
            p["wood"].Box(new Vector3(0, 1.3f, 0), new Vector3(w, h, 0.04f), Id);
            p["wood_dark"].Box(new Vector3(0, 1.3f + h * 0.5f + 0.06f, 0.0f), new Vector3(w + 0.2f, 0.06f, 0.2f), Quaternion.Euler(-10, 0, 0));
            var paper = Paper("parchment", 0.4f, 0.5f, 2f);
            foreach (var kv in paper.Parts) p[kv.Key].Append(kv.Value, Matrix4x4.TRS(new Vector3(0, 1.3f, 0.025f), Quaternion.Euler(-90, 0, 0), Vector3.one));
            return p;
        }

        public static PropMesh Dial(float r = 0.42f)
        {
            var p = new PropMesh();
            p["brass"].Cylinder(Vector3.zero, Quaternion.Euler(90, 0, 0), 0.06f, r + 0.05f, r + 0.05f, 24);
            p["paper"].Disc(new Vector3(0, 0, 0.061f), Quaternion.Euler(90, 0, 0), r, 24);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                p["black"].Box(new Vector3(Mathf.Sin(a) * r * 0.86f, Mathf.Cos(a) * r * 0.86f, 0.065f), new Vector3(0.02f, i % 3 == 0 ? 0.07f : 0.04f, 0.004f), Quaternion.Euler(0, 0, -a * Mathf.Rad2Deg));
            }
            return p;
        }

        /// <summary>A clock hand pointing up (12) from the origin, facing +Z.</summary>
        public static PropMesh ClockHand(float len, float w)
        {
            var p = new PropMesh();
            p["black"].Box(new Vector3(0, len * 0.4f, 0.075f), new Vector3(w, len, 0.006f), Id);
            p["black"].Cylinder(new Vector3(0, 0, 0.07f), Quaternion.Euler(90, 0, 0), 0.012f, 0.025f, 0.025f, 10);
            return p;
        }

        public static PropMesh ShellHorn()
        {
            var p = new PropMesh();
            var prof = new List<Vector2>();
            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                prof.Add(P(0.02f + t * t * 0.16f, t * 0.42f));
            }
            p["bone"].Lathe(Vector3.zero, Quaternion.Euler(70, 0, 0), prof, 12);
            p["wood_dark"].Box(new Vector3(0, 0.55f, -0.1f), new Vector3(0.08f, 1.1f, 0.08f), Id);
            return p;
        }

        public static PropMesh Figurehead()
        {
            var p = new PropMesh();
            p["wood"].Lathe(Vector3.zero, Id, new[] { P(0.25f, 0f), P(0.32f, 0.4f), P(0.26f, 0.9f), P(0.18f, 1.2f), P(0.2f, 1.3f), P(0.0f, 1.55f) }, 12);
            p["wood"].Lathe(new Vector3(0, 1.38f, 0.05f), Id, new[] { P(0f, 0f), P(0.11f, 0.05f), P(0.13f, 0.16f), P(0.1f, 0.26f), P(0f, 0.3f) }, 10);
            p["wood_dark"].Box(new Vector3(0, -0.3f, -0.4f), new Vector3(0.5f, 0.6f, 1.2f), Quaternion.Euler(20, 0, 0));
            return p;
        }

        public static PropMesh Nail()
        {
            var p = new PropMesh();
            p["iron"].Cylinder(Vector3.zero, Quaternion.Euler(90, 0, 0), 0.22f, 0.014f, 0.004f, 6);
            p["iron"].Cylinder(new Vector3(0, 0, -0.01f), Quaternion.Euler(90, 0, 0), 0.012f, 0.03f, 0.03f, 8);
            return p;
        }

        public static PropMesh Candles(Rng rng, int n = 5)
        {
            var p = new PropMesh();
            for (int i = 0; i < n; i++)
            {
                var o = new Vector3(rng.Range(-0.14f, 0.14f), 0, rng.Range(-0.1f, 0.1f));
                float h = rng.Range(0.08f, 0.26f);
                p["candle"].Cylinder(o, Id, h, 0.02f, 0.018f, 8);
                p["flame"].Lathe(o + Vector3.up * (h + 0.005f), Id, new[] { P(0f, 0f), P(0.008f, 0.01f), P(0.005f, 0.03f), P(0f, 0.04f) }, 5);
            }
            return p;
        }

        public static PropMesh Bottle(Rng rng)
        {
            var p = new PropMesh();
            float h = rng.Range(0.22f, 0.32f);
            p["glass"].Lathe(Vector3.zero, Id, new[] { P(0.04f, 0f), P(0.042f, h * 0.6f), P(0.014f, h * 0.8f), P(0.012f, h) }, 8);
            return p;
        }

        public static PropMesh Rubble(Rng rng, int n, float spread, string mat = "stone")
        {
            var p = new PropMesh();
            for (int i = 0; i < n; i++)
            {
                float s = rng.Range(0.08f, 0.35f);
                p[mat].Box(new Vector3(rng.Range(-spread, spread), s * 0.4f, rng.Range(-spread, spread)), new Vector3(s, s * rng.Range(0.5f, 0.9f), s * rng.Range(0.7f, 1.3f)), Quaternion.Euler(rng.Range(-15, 15), rng.Range(0, 180), rng.Range(-15, 15)));
            }
            return p;
        }

        public static PropMesh Planks(Rng rng, int n, float len = 2.2f)
        {
            var p = new PropMesh();
            for (int i = 0; i < n; i++)
                p["wood"].Box(new Vector3(rng.Range(-0.6f, 0.6f), 0.03f + i * 0.04f, rng.Range(-0.4f, 0.4f)), new Vector3(0.2f, 0.03f, len * rng.Range(0.6f, 1f)), Quaternion.Euler(rng.Range(-4, 4), rng.Range(-40, 40), rng.Range(-4, 4)));
            return p;
        }

        /// <summary>A small bare footprint (left or right), lying flat.</summary>
        public static MeshBuilder Footprint(bool left)
        {
            var b = new MeshBuilder();
            float s = left ? -1f : 1f;
            // sole as a fan of quads
            var outline = new List<Vector3>();
            for (int i = 0; i <= 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                float x = Mathf.Cos(a) * 0.035f;
                float z = Mathf.Sin(a) * 0.075f;
                if (z > 0) x *= 1.15f;
                outline.Add(new Vector3(x * s, 0.004f, z));
            }
            var c = new Vector3(0, 0.004f, 0);
            for (int i = 0; i < outline.Count - 1; i++)
            {
                // mirrored x flips the winding; keep the sole facing up either way
                if (left) b.Tri(c, outline[i + 1], outline[i]);
                else b.Tri(c, outline[i], outline[i + 1]);
            }
            for (int t = 0; t < 4; t++)
            {
                var tc = new Vector3((t - 1.5f) * 0.016f * s, 0.004f, 0.09f - Mathf.Abs(t - 1.2f) * 0.006f);
                b.Disc(tc, Quaternion.identity, 0.008f, 6);
            }
            return b;
        }
    }
}
