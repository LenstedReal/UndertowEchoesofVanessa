using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Undertow
{
    /// <summary>
    /// Procedural mesh assembly with world-scale UVs (textures tile at a fixed size in metres,
    /// so planks and stones never stretch), boxes, cylinders, lathes, tubes and grids.
    /// </summary>
    public sealed class MeshBuilder
    {
        public readonly List<Vector3> V = new List<Vector3>(1024);
        public readonly List<Vector3> N = new List<Vector3>(1024);
        public readonly List<Vector2> UV = new List<Vector2>(1024);
        public readonly List<Color32> C = new List<Color32>(1024);
        public readonly List<int> T = new List<int>(2048);
        public float TexScale = 1f;         // metres per texture repeat
        public Color32 Tint = new Color32(255, 255, 255, 255);

        public int VertexCount => V.Count;
        public bool Empty => T.Count == 0;

        int Add(Vector3 p, Vector3 n, Vector2 uv)
        {
            V.Add(p);
            N.Add(n);
            UV.Add(uv);
            C.Add(Tint);
            return V.Count - 1;
        }

        /// <summary>Quad a-b-c-d (counter-clockwise seen from the front). UVs projected in metres.</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float uvScale = 0f, Vector2 uvOffset = default)
        {
            var n = Vector3.Cross(d - a, b - a).normalized;
            if (n.sqrMagnitude < 1e-8f) n = Vector3.up;
            float s = uvScale > 0f ? uvScale : TexScale;
            // tangent frame on the quad
            var u = (b - a).normalized;
            if (u.sqrMagnitude < 1e-8f) u = Vector3.right;
            var v = Vector3.Cross(u, n);
            Vector2 P(Vector3 p) => new Vector2(Vector3.Dot(p, u) / s, Vector3.Dot(p, v) / s) + uvOffset;
            int i0 = Add(a, n, P(a)), i1 = Add(b, n, P(b)), i2 = Add(c, n, P(c)), i3 = Add(d, n, P(d));
            T.Add(i0); T.Add(i3); T.Add(i2);
            T.Add(i0); T.Add(i2); T.Add(i1);
        }

        /// <summary>Quad with explicit UVs.</summary>
        public void QuadUV(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
        {
            var n = Vector3.Cross(d - a, b - a).normalized;
            int i0 = Add(a, n, ua), i1 = Add(b, n, ub), i2 = Add(c, n, uc), i3 = Add(d, n, ud);
            T.Add(i0); T.Add(i3); T.Add(i2);
            T.Add(i0); T.Add(i2); T.Add(i1);
        }

        public void Tri(Vector3 a, Vector3 b, Vector3 c)
        {
            var n = Vector3.Cross(c - a, b - a).normalized;
            var u = (b - a).normalized;
            var v = Vector3.Cross(u, n);
            float s = TexScale;
            Vector2 P(Vector3 p) => new Vector2(Vector3.Dot(p, u) / s, Vector3.Dot(p, v) / s);
            int i0 = Add(a, n, P(a)), i1 = Add(b, n, P(b)), i2 = Add(c, n, P(c));
            T.Add(i0); T.Add(i2); T.Add(i1);
        }

        /// <summary>Oriented box. Optional faces mask: 1 +X, 2 -X, 4 +Y, 8 -Y, 16 +Z, 32 -Z.</summary>
        public void Box(Vector3 center, Vector3 size, Quaternion rot, int faces = 63, float uvScale = 0f)
        {
            var h = size * 0.5f;
            Vector3 P(float x, float y, float z) => center + rot * new Vector3(x * h.x, y * h.y, z * h.z);
            var p000 = P(-1, -1, -1); var p100 = P(1, -1, -1); var p010 = P(-1, 1, -1); var p110 = P(1, 1, -1);
            var p001 = P(-1, -1, 1); var p101 = P(1, -1, 1); var p011 = P(-1, 1, 1); var p111 = P(1, 1, 1);
            if ((faces & 16) != 0) Quad(p101, p001, p011, p111, uvScale);   // +Z
            if ((faces & 32) != 0) Quad(p000, p100, p110, p010, uvScale);   // -Z
            if ((faces & 1) != 0) Quad(p100, p101, p111, p110, uvScale);    // +X
            if ((faces & 2) != 0) Quad(p001, p000, p010, p011, uvScale);    // -X
            if ((faces & 4) != 0) Quad(p010, p110, p111, p011, uvScale);    // +Y
            if ((faces & 8) != 0) Quad(p001, p101, p100, p000, uvScale);    // -Y
        }

        public void Box(Vector3 center, Vector3 size) => Box(center, size, Quaternion.identity);

        public void BoxMinMax(Vector3 min, Vector3 max, int faces = 63) => Box((min + max) * 0.5f, max - min, Quaternion.identity, faces);

        /// <summary>Cylinder along local Y from base, with optional caps.</summary>
        public void Cylinder(Vector3 bottom, Quaternion rot, float height, float r0, float r1, int seg = 12, bool caps = true, float uvScale = 0f)
        {
            float s = uvScale > 0f ? uvScale : TexScale;
            float circ = Mathf.PI * 2f * Mathf.Max(r0, r1);
            int start = V.Count;
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var slope = (r0 - r1) / Mathf.Max(0.0001f, height);
                var n = rot * new Vector3(dir.x, slope, dir.z).normalized;
                float u = i / (float)seg * circ / s;
                Add(bottom + rot * (dir * r0), n, new Vector2(u, 0f));
                Add(bottom + rot * (dir * r1 + Vector3.up * height), n, new Vector2(u, height / s));
            }
            for (int i = 0; i < seg; i++)
            {
                int a = start + i * 2, b = a + 1, c = a + 2, d = a + 3;
                T.Add(a); T.Add(b); T.Add(d);
                T.Add(a); T.Add(d); T.Add(c);
            }
            if (!caps) return;
            Disc(bottom, rot * Quaternion.Euler(180f, 0f, 0f), r0, seg);
            Disc(bottom + rot * Vector3.up * height, rot, r1, seg);
        }

        public void Disc(Vector3 center, Quaternion rot, float r, int seg = 12)
        {
            if (r <= 0f) return;
            var n = rot * Vector3.up;
            int c = Add(center, n, new Vector2(0.5f, 0.5f));
            int first = V.Count;
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Add(center + rot * (d * r), n, new Vector2(0.5f + d.x * 0.5f, 0.5f + d.z * 0.5f));
            }
            for (int i = 0; i < seg; i++)
            {
                T.Add(c); T.Add(first + i + 1); T.Add(first + i);
            }
        }

        /// <summary>Surface of revolution around local Y. profile: (radius, height) from bottom to top.</summary>
        public void Lathe(Vector3 origin, Quaternion rot, IList<Vector2> profile, int seg = 16, float uvScale = 0f)
        {
            float s = uvScale > 0f ? uvScale : TexScale;
            int rows = profile.Count;
            int start = V.Count;
            float vAcc = 0f;
            var vCoord = new float[rows];
            for (int r = 1; r < rows; r++)
            {
                vAcc += Vector2.Distance(profile[r], profile[r - 1]);
                vCoord[r] = vAcc / s;
            }
            for (int r = 0; r < rows; r++)
            {
                var prev = profile[Mathf.Max(0, r - 1)];
                var next = profile[Mathf.Min(rows - 1, r + 1)];
                var tangent = next - prev;
                var n2 = new Vector2(tangent.y, -tangent.x).normalized; // outward
                for (int i = 0; i <= seg; i++)
                {
                    float a = i / (float)seg * Mathf.PI * 2f;
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    var p = dir * profile[r].x + Vector3.up * profile[r].y;
                    var n = (dir * n2.x + Vector3.up * n2.y).normalized;
                    Add(origin + rot * p, rot * n, new Vector2(i / (float)seg * 2f, vCoord[r]));
                }
            }
            int stride = seg + 1;
            for (int r = 0; r < rows - 1; r++)
            {
                for (int i = 0; i < seg; i++)
                {
                    int a = start + r * stride + i, b = a + 1, c = a + stride, d = c + 1;
                    T.Add(a); T.Add(c); T.Add(d);
                    T.Add(a); T.Add(d); T.Add(b);
                }
            }
        }

        /// <summary>Round tube along a polyline (ropes, chains, pipes, rigging).</summary>
        public void Tube(IList<Vector3> path, float radius, int seg = 6, float uvScale = 0f)
        {
            if (path.Count < 2) return;
            float s = uvScale > 0f ? uvScale : TexScale;
            int start = V.Count;
            float len = 0f;
            Vector3 up = Vector3.up;
            for (int k = 0; k < path.Count; k++)
            {
                var fwd = (k < path.Count - 1 ? path[k + 1] - path[k] : path[k] - path[k - 1]).normalized;
                if (k > 0) len += Vector3.Distance(path[k], path[k - 1]);
                if (Mathf.Abs(Vector3.Dot(fwd, up)) > 0.95f) up = Vector3.right;
                var side = Vector3.Cross(fwd, up).normalized;
                var up2 = Vector3.Cross(side, fwd);
                for (int i = 0; i <= seg; i++)
                {
                    float a = i / (float)seg * Mathf.PI * 2f;
                    var n = side * Mathf.Cos(a) + up2 * Mathf.Sin(a);
                    Add(path[k] + n * radius, n, new Vector2(i / (float)seg, len / s));
                }
            }
            int stride = seg + 1;
            for (int k = 0; k < path.Count - 1; k++)
                for (int i = 0; i < seg; i++)
                {
                    int a = start + k * stride + i, b = a + 1, c = a + stride, d = c + 1;
                    T.Add(a); T.Add(c); T.Add(d);
                    T.Add(a); T.Add(d); T.Add(b);
                }
        }

        /// <summary>Sagging rope between two points.</summary>
        public void Rope(Vector3 a, Vector3 b, float sag, float radius = 0.025f, int steps = 10)
        {
            var pts = new List<Vector3>(steps + 1);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                var p = Vector3.Lerp(a, b, t);
                p.y -= sag * 4f * t * (1f - t);
                pts.Add(p);
            }
            Tube(pts, radius, 5, 0.5f);
        }

        /// <summary>Height-field grid on XZ (used for water, sand and cloth).</summary>
        public void Grid(Vector3 origin, Vector2 size, int nx, int nz, Func<float, float, float> height, float uvScale = 0f, Func<float, float, bool> keep = null)
        {
            float s = uvScale > 0f ? uvScale : TexScale;
            int start = V.Count;
            for (int z = 0; z <= nz; z++)
                for (int x = 0; x <= nx; x++)
                {
                    float fx = x / (float)nx, fz = z / (float)nz;
                    float px = fx * size.x, pz = fz * size.y;
                    float y = height != null ? height(px, pz) : 0f;
                    Add(origin + new Vector3(px, y, pz), Vector3.up, new Vector2((origin.x + px) / s, (origin.z + pz) / s));
                }
            int stride = nx + 1;
            for (int z = 0; z < nz; z++)
                for (int x = 0; x < nx; x++)
                {
                    if (keep != null && !keep((x + 0.5f) / nx * size.x, (z + 0.5f) / nz * size.y)) continue;
                    int a = start + z * stride + x, b = a + 1, c = a + stride, d = c + 1;
                    T.Add(a); T.Add(c); T.Add(d);
                    T.Add(a); T.Add(d); T.Add(b);
                }
            // smooth normals for the grid
            for (int z = 0; z <= nz; z++)
                for (int x = 0; x <= nx; x++)
                {
                    int i = start + z * stride + x;
                    var l = V[start + z * stride + Mathf.Max(0, x - 1)];
                    var r = V[start + z * stride + Mathf.Min(nx, x + 1)];
                    var dn = V[start + Mathf.Max(0, z - 1) * stride + x];
                    var up = V[start + Mathf.Min(nz, z + 1) * stride + x];
                    var n = Vector3.Cross(up - dn, r - l).normalized;
                    N[i] = n.y < 0 ? -n : n;
                }
        }

        public void Append(MeshBuilder o, Matrix4x4 m)
        {
            int start = V.Count;
            for (int i = 0; i < o.V.Count; i++)
            {
                V.Add(m.MultiplyPoint3x4(o.V[i]));
                N.Add(m.MultiplyVector(o.N[i]).normalized);
                UV.Add(o.UV[i]);
                C.Add(o.C[i]);
            }
            for (int i = 0; i < o.T.Count; i++) T.Add(start + o.T[i]);
        }

        public Mesh Build(string name, bool tangents = true)
        {
            var m = new Mesh { name = name };
            if (V.Count > 65000) m.indexFormat = IndexFormat.UInt32;
            m.SetVertices(V);
            m.SetNormals(N);
            m.SetUVs(0, UV);
            m.SetColors(C);
            m.SetTriangles(T, 0, true);
            if (tangents) m.RecalculateTangents();
            m.RecalculateBounds();
            return m;
        }

        public void Clear()
        {
            V.Clear(); N.Clear(); UV.Clear(); C.Clear(); T.Clear();
        }
    }
}
