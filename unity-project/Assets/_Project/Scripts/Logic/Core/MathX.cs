using System;

namespace Undertow.Logic
{
    /// <summary>Point on the horizontal plane (x = right, z = forward), in metres.</summary>
    [Serializable]
    public struct V2 : IEquatable<V2>
    {
        public float X;
        public float Z;

        public V2(float x, float z)
        {
            X = x;
            Z = z;
        }

        public static V2 operator +(V2 a, V2 b) => new V2(a.X + b.X, a.Z + b.Z);
        public static V2 operator -(V2 a, V2 b) => new V2(a.X - b.X, a.Z - b.Z);
        public static V2 operator *(V2 a, float s) => new V2(a.X * s, a.Z * s);
        public float Length => MathF.Sqrt(X * X + Z * Z);
        public static float Distance(V2 a, V2 b) => (a - b).Length;
        public bool Equals(V2 o) => X == o.X && Z == o.Z;
        public override bool Equals(object obj) => obj is V2 o && Equals(o);
        public override int GetHashCode() => HashCode.Combine(X, Z);
        public override string ToString() => "(" + X.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ", " + Z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>Axis-aligned rectangle on the horizontal plane. (X, Z) is the minimum corner.</summary>
    [Serializable]
    public struct RectXZ
    {
        public float X;
        public float Z;
        public float W;
        public float D;

        public RectXZ(float x, float z, float w, float d)
        {
            X = x;
            Z = z;
            W = w;
            D = d;
        }

        public float MaxX => X + W;
        public float MaxZ => Z + D;
        public V2 Center => new V2(X + W * 0.5f, Z + D * 0.5f);
        public float Area => W * D;

        public bool Contains(float x, float z) => x >= X && x <= X + W && z >= Z && z <= Z + D;
        public bool Contains(V2 p) => Contains(p.X, p.Z);

        public bool ContainsStrict(float x, float z) => x > X && x < X + W && z > Z && z < Z + D;

        public bool Overlaps(RectXZ o) => X < o.MaxX && o.X < MaxX && Z < o.MaxZ && o.Z < MaxZ;

        public RectXZ Expanded(float m) => new RectXZ(X - m, Z - m, W + 2 * m, D + 2 * m);

        public override string ToString() => $"[{X},{Z} {W}x{D}]";
    }

    public static class MathX
    {
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);
        public static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float InverseLerp(float a, float b, float v) => Math.Abs(b - a) < 1e-6f ? 0f : Clamp01((v - a) / (b - a));
        public static float MoveTowards(float cur, float target, float maxDelta)
        {
            if (Math.Abs(target - cur) <= maxDelta) return target;
            return cur + Math.Sign(target - cur) * maxDelta;
        }
        public static float SmoothStep(float t)
        {
            t = Clamp01(t);
            return t * t * (3f - 2f * t);
        }
        public static bool Near(float a, float b, float eps = 1e-3f) => Math.Abs(a - b) <= eps;
        public static int Mod(int a, int n) => ((a % n) + n) % n;
    }

    /// <summary>Small deterministic PRNG (xorshift32) so generated content is identical on every run.</summary>
    public sealed class Rng
    {
        uint _s;

        public Rng(int seed)
        {
            _s = (uint)seed * 2654435761u + 0x9E3779B9u;
            if (_s == 0) _s = 0x12345678u;
            for (int i = 0; i < 4; i++) Next();
        }

        public uint Next()
        {
            uint x = _s;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _s = x;
            return x;
        }

        /// <summary>Uniform in [0, 1).</summary>
        public float Value() => (Next() >> 8) * (1f / 16777216f);
        public float Range(float lo, float hi) => lo + (hi - lo) * Value();
        public int Range(int lo, int hiExclusive) => hiExclusive <= lo ? lo : lo + (int)(Next() % (uint)(hiExclusive - lo));
        public bool Chance(float p) => Value() < p;

        public static int Hash(string s)
        {
            unchecked
            {
                int h = (int)2166136261;
                foreach (char c in s) h = (h ^ c) * 16777619;
                return h;
            }
        }
    }
}
