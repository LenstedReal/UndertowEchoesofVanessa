using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>Placement helpers shared by the act dressings: keep paths, doors and story items clear.</summary>
    public static class DressUtil
    {
        public static bool Clear(World w, Vector3 p, float r)
        {
            var spawn = new Vector3(w.Act.Spawn.X, p.y, w.Act.Spawn.Z);
            if (Flat(p - spawn) < r + 1.6f) return false;
            foreach (var it in w.Act.Items)
            {
                var ip = new Vector3(it.Pos.X, p.y, it.Pos.Z);
                float need = r + (it.Kind == ItemKind.Crate ? 1.4f : 0.9f);
                if (Flat(p - ip) < need) return false;
                if (it.Slots != null)
                    foreach (var s in it.Slots)
                        if (Flat(p - new Vector3(s.X, p.y, s.Z)) < r + 1.0f) return false;
            }
            foreach (var o in w.Level.Openings)
            {
                var op = new Vector3(o.Center.X, p.y, o.Center.Z);
                if (Flat(p - op) < r + o.Width * 0.5f + 0.9f) return false;
            }
            foreach (var b in w.Act.Blockers)
                if (b.Rect.Expanded(r + 0.6f).Contains(p.x, p.z)) return false;
            foreach (var room in w.Act.Rooms)
            {
                if (!room.IsRamp) continue;
                if (room.Rect.Expanded(r + 0.4f).Contains(p.x, p.z)) return false;
            }
            return true;
        }

        static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;

        /// <summary>Spots along the inside of a room's walls, facing into the room.</summary>
        public static List<(Vector3 pos, Quaternion rot)> WallSpots(World w, RoomDef room, float spacing, float inset, Rng rng)
        {
            var res = new List<(Vector3, Quaternion)>();
            foreach (var wp in w.Level.Walls)
            {
                if (wp.Room != room) continue;
                if (wp.Kind == EdgeKind.Drop) continue;
                var a = new Vector3(wp.A.X, 0, wp.A.Z);
                var b = new Vector3(wp.B.X, 0, wp.B.Z);
                float len = wp.Length;
                var n = new Vector3(wp.Normal.X, 0, wp.Normal.Z);
                int count = Mathf.FloorToInt(len / spacing);
                for (int i = 0; i < count; i++)
                {
                    float t = (i + 0.5f + rng.Range(-0.25f, 0.25f)) / Mathf.Max(1, count);
                    var p = Vector3.Lerp(a, b, t) + n * inset;
                    p.y = room.FloorAt(p.x, p.z);
                    res.Add((p, Quaternion.LookRotation(n, Vector3.up)));
                }
            }
            return res;
        }

        /// <summary>Static, baked prop (no own GameObject). Optional box collider.</summary>
        public static void Bake(World w, string chunk, PropMesh pm, Vector3 pos, Quaternion rot, Vector3? col = null, float colY = -1f)
        {
            pm.BakeInto(w.Kit, chunk, pos, rot);
            if (col.HasValue)
            {
                var c = col.Value;
                w.Kit.Collider(pos + rot * new Vector3(0, colY >= 0 ? colY : c.y * 0.5f, 0), c, rot);
            }
        }

        /// <summary>Separate object that hidden sanity may nudge while unseen.</summary>
        public static GameObject Shifty(World w, string name, PropMesh pm, Vector3 pos, Quaternion rot, Vector3? col, Rng rng, Transform parent = null)
        {
            var go = pm.Spawn(name, parent ?? w.Root, pos, rot);
            if (col.HasValue)
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = new Vector3(0, col.Value.y * 0.5f, 0);
                bc.size = col.Value;
            }
            var d = go.AddComponent<Distortable>();
            var alt = pos + rot * new Vector3(rng.Range(-0.6f, 0.6f), 0, rng.Range(-0.3f, 0.3f));
            d.Init(alt, rot * Quaternion.Euler(0, rng.Range(-60f, 60f), 0));
            w.Distortables.Add(d);
            return go;
        }

        public static void Puddles(World w, RoomDef room, int count, Rng rng)
        {
            var b = w.Kit.B(room.Id, "water", 3f);
            for (int i = 0; i < count; i++)
            {
                float x = rng.Range(room.Rect.X + 0.8f, room.Rect.MaxX - 0.8f);
                float z = rng.Range(room.Rect.Z + 0.8f, room.Rect.MaxZ - 0.8f);
                float y = room.FloorAt(x, z) + 0.006f;
                float r = rng.Range(0.3f, 1.1f);
                int seg = 9;
                var c = new Vector3(x, y, z);
                var pts = new List<Vector3>();
                for (int k = 0; k <= seg; k++)
                {
                    float a = k / (float)seg * Mathf.PI * 2f;
                    float rr = r * rng.Range(0.65f, 1.15f);
                    pts.Add(c + new Vector3(Mathf.Cos(a) * rr, 0, Mathf.Sin(a) * rr * 0.7f));
                }
                pts[seg] = pts[0];
                for (int k = 0; k < seg; k++) b.Tri(c, pts[k], pts[k + 1]);
            }
        }

        /// <summary>Beams under a flat ceiling, every 'step' metres across the short side.</summary>
        public static void CeilingBeams(World w, RoomDef room, float step, string mat = "wood_dark")
        {
            if (!room.Ceil || room.IsRamp) return;
            var r = room.Rect;
            float y = room.Floor + room.CeilH - 0.09f;
            bool alongX = r.W <= r.D; // beams span the short side
            float len = alongX ? r.W : r.D;
            float from = alongX ? r.Z : r.X, to = alongX ? r.MaxZ : r.MaxX;
            for (float s = from + step * 0.5f; s < to - 0.2f; s += step)
            {
                var c = alongX ? new Vector3(r.X + r.W * 0.5f, y, s) : new Vector3(s, y, r.Z + r.D * 0.5f);
                var size = alongX ? new Vector3(len, 0.18f, 0.16f) : new Vector3(0.16f, 0.18f, len);
                w.Kit.Box(room.Id, mat, c, size);
            }
        }

        public static void Rug(World w, RoomDef room, Vector3 center, float wdt, float dpt, string mat = "cloth_red", float yaw = 0f)
        {
            var b = w.Kit.B(room.Id, mat, 1.2f);
            var rot = Quaternion.Euler(0, yaw, 0);
            float y = room.FloorAt(center.x, center.z) + 0.008f;
            var c = new Vector3(center.x, y, center.z);
            b.QuadUV(c + rot * new Vector3(-wdt * 0.5f, 0, -dpt * 0.5f), c + rot * new Vector3(wdt * 0.5f, 0, -dpt * 0.5f), c + rot * new Vector3(wdt * 0.5f, 0, dpt * 0.5f), c + rot * new Vector3(-wdt * 0.5f, 0, dpt * 0.5f),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
        }

        /// <summary>Static scatter of small clutter along walls (bottles, ropes, planks), avoiding everything important.</summary>
        public static void Clutter(World w, RoomDef room, float density, Rng rng, string[] kinds)
        {
            foreach (var (pos, rot) in WallSpots(w, room, 1.6f / Mathf.Max(0.1f, density), 0.45f, rng))
            {
                if (!rng.Chance(0.75f) || !Clear(w, pos, 0.4f)) continue;
                string k = kinds[rng.Range(0, kinds.Length)];
                var yaw = rot * Quaternion.Euler(0, rng.Range(-25f, 25f), 0);
                switch (k)
                {
                    case "barrel": Bake(w, room.Id, Props.Barrel(rng.Range(0.8f, 0.95f), rng.Range(0.28f, 0.33f)), pos, yaw, new Vector3(0.62f, 0.95f, 0.62f)); break;
                    case "crate": { float s = rng.Range(0.55f, 0.85f); Bake(w, room.Id, Props.Crate(new Vector3(s, s * rng.Range(0.8f, 1f), s)), pos, yaw, new Vector3(s, s, s)); break; }
                    case "crates":
                    {
                        float s = rng.Range(0.6f, 0.8f);
                        Bake(w, room.Id, Props.Crate(new Vector3(s, s, s)), pos, yaw, new Vector3(s, s * 1.9f, s));
                        Bake(w, room.Id, Props.Crate(new Vector3(s * 0.8f, s * 0.8f, s * 0.8f)), pos + Vector3.up * s, yaw * Quaternion.Euler(0, rng.Range(-20, 20), 0));
                        break;
                    }
                    case "coil": Bake(w, room.Id, Props.RopeCoil(rng.Range(0.25f, 0.35f)), pos, yaw); break;
                    case "bottle": Bake(w, room.Id, Props.Bottle(rng), pos + rot * Vector3.back * 0.2f, yaw); break;
                    case "planks": Bake(w, room.Id, Props.Planks(rng, rng.Range(2, 5), 1.8f), pos, yaw); break;
                    case "rubble": Bake(w, room.Id, Props.Rubble(rng, rng.Range(4, 9), 0.5f, room.Surface == Surface.Stone ? "stone" : "wood_dark"), pos, yaw); break;
                    case "chest": Bake(w, room.Id, Props.SeaChest(), pos, yaw, new Vector3(0.82f, 0.6f, 0.5f)); break;
                    case "chair": Bake(w, room.Id, Props.Chair(), pos, yaw * Quaternion.Euler(0, 180, 0), new Vector3(0.45f, 0.9f, 0.45f)); break;
                    case "stool": Bake(w, room.Id, Props.Stool(), pos, yaw); break;
                    case "candles": Bake(w, room.Id, Props.Candles(rng, rng.Range(3, 6)), pos, yaw); break;
                }
            }
        }

        public static void Sack(World w, string chunk, Vector3 pos, Quaternion rot, Rng rng)
        {
            var pm = new PropMesh();
            pm["cloth"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.24f, 0f), new Vector2(0.3f, 0.15f), new Vector2(0.27f, 0.45f), new Vector2(0.12f, 0.62f), new Vector2(0.05f, 0.68f) }, 9);
            Bake(w, chunk, pm, pos, rot);
        }
    
        /// <summary>
        /// Hanging lantern as its own object: sways with the ship, optional flame and light (as children, so
        /// shadows swing too). Returns the light, or null when unlit.
        /// </summary>
        public static Light Lantern(World w, string name, Vector3 pos, bool lit, float lumens, float range, bool shadows, bool important, float sway = 2.5f, float scale = 1f)
        {
            var root = new GameObject(name).transform;
            root.SetParent(w.Root, false);
            root.position = pos;
            var body = Props.HangingLantern(false).Spawn("body", root, Vector3.zero, Quaternion.identity);
            body.transform.localScale = Vector3.one * scale;
            if (sway > 0f)
            {
                var s = root.gameObject.AddComponent<Sway>();
                s.Amp = sway;
            }
            if (!lit) return null;
            var fm = new PropMesh();
            fm["flame"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.0f, 0f), new Vector2(0.018f, 0.02f), new Vector2(0.012f, 0.06f), new Vector2(0.0f, 0.09f) }, 6);
            var flame = fm.Spawn("flame", root, new Vector3(0, -0.28f, 0) * scale, Quaternion.identity, false);
            flame.transform.localScale = Vector3.one * scale;
            var l = LightKit.Point(name + "_light", root, new Vector3(0, -0.24f, 0) * scale, LightKit.Lantern, lumens, range, shadows, important, 1f, 0.04f);
            var fl = root.gameObject.AddComponent<Flicker>();
            fl.Target = l;
            fl.Base = lumens;
            fl.Amount = 0.1f;
            fl.Speed = 7f;
            var fr = flame.GetComponentInChildren<Renderer>();
            fl.Flame = fr;
            w.FlickerLights.Add(l);
            return l;
        }

        /// <summary>Small chalk dots on a wall, facing along 'normal'.</summary>
        public static void Dots(World w, string chunk, Vector3 center, Vector3 normal, int count, float spacing = 0.07f, float r = 0.018f)
        {
            var b = w.Kit.B(chunk, "chalk", 0.5f);
            var rot = Quaternion.FromToRotation(Vector3.up, normal);
            var side = Vector3.Cross(Vector3.up, normal).normalized;
            for (int i = 0; i < count; i++)
            {
                var c = center + side * ((i - (count - 1) * 0.5f) * spacing) + normal * 0.004f;
                b.Disc(c, rot, r, 8);
            }
        }
    }

    /// <summary>Slow pendulum motion for hanging things: the ship is still rolling.</summary>
    public sealed class Sway : MonoBehaviour
    {
        public float Amp = 2.5f;
        public float Speed = 0.55f;
        float _ph;
        Quaternion _base;

        void Start()
        {
            _base = transform.localRotation;
            _ph = Random.value * 10f;
        }

        void Update()
        {
            float t = Time.time * Speed + _ph;
            transform.localRotation = _base * Quaternion.Euler(Mathf.Sin(t) * Amp, 0f, Mathf.Sin(t * 0.77f + 1.3f) * Amp * 0.7f);
        }
    }
}
