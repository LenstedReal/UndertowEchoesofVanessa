using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>A door, grate, debris pile, fog wall or gate that opens when its flag is set.</summary>
    public sealed class Blocker : MonoBehaviour
    {
        public BlockerDef Def;
        public Transform Moving;
        public Collider Solid;
        public LocalVolumetricFog Fog;
        public bool Open { get; private set; }

        public void SetOpen(bool open, bool instant)
        {
            if (Open == open) return;
            Open = open;
            if (Solid != null) Solid.enabled = !open;
            if (!open) return;
            if (instant)
            {
                Finish();
                return;
            }
            StartCoroutine(Animate());
        }

        void Finish()
        {
            switch (Def.Style)
            {
                case "door":
                case "gate":
                    if (Moving != null) Moving.localRotation = Quaternion.Euler(0, 105f, 0);
                    break;
                case "grate":
                    if (Moving != null) Moving.localPosition = new Vector3(0, 2.4f, 0);
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }

        IEnumerator Animate()
        {
            var pos = transform.position + Vector3.up;
            switch (Def.Style)
            {
                case "door":
                case "gate":
                {
                    Game.Audio.PlayAt(Def.Style == "gate" ? "sfx_gate_open" : "sfx_door_unlock", pos, Bus.Effects, 0.9f);
                    yield return new WaitForSeconds(0.5f);
                    Game.Audio.PlayAt("sfx_door_creak", pos, Bus.Effects, 0.8f);
                    var from = Moving.localRotation;
                    var to = Quaternion.Euler(0, 105f, 0);
                    for (float t = 0; t < 1f; t += Time.deltaTime / 2.2f)
                    {
                        Moving.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0, 1, t));
                        yield return null;
                    }
                    break;
                }
                case "grate":
                {
                    Game.Audio.PlayAt("sfx_grate_lift", pos, Bus.Effects, 0.9f);
                    var from = Moving.localPosition;
                    for (float t = 0; t < 1f; t += Time.deltaTime / 2.6f)
                    {
                        Moving.localPosition = Vector3.Lerp(from, new Vector3(0, 2.4f, 0), Mathf.SmoothStep(0, 1, t));
                        yield return null;
                    }
                    break;
                }
                case "debris":
                {
                    Game.Audio.PlayAt("sfx_debris_fall", pos, Bus.Effects, 1f);
                    Game.Act?.Player?.Shake(0.35f, 0.6f);
                    var start = transform.position;
                    for (float t = 0; t < 1f; t += Time.deltaTime / 1.2f)
                    {
                        transform.position = start + Vector3.down * (t * t * 2.2f);
                        yield return null;
                    }
                    gameObject.SetActive(false);
                    break;
                }
                case "fog":
                {
                    Game.Audio.PlayAt("sfx_fog_part", pos, Bus.Effects, 0.9f);
                    if (Fog != null)
                    {
                        float d0 = Fog.parameters.meanFreePath;
                        for (float t = 0; t < 1f; t += Time.deltaTime / 3f)
                        {
                            var p = Fog.parameters;
                            p.meanFreePath = Mathf.Lerp(d0, 80f, t);
                            Fog.parameters = p;
                            yield return null;
                        }
                    }
                    gameObject.SetActive(false);
                    break;
                }
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }

    public static class BlockerFactory
    {
        public static void SpawnAll(World w, GameState state)
        {
            var parent = new GameObject("Blockers").transform;
            parent.SetParent(w.Root, false);
            foreach (var b in w.Act.Blockers)
            {
                var bl = Spawn(w, b, parent);
                w.Blockers[b.Id] = bl;
                if (state.Has(b.Until)) bl.SetOpen(true, true);
            }
        }

        static Blocker Spawn(World w, BlockerDef b, Transform parent)
        {
            var r = b.Rect;
            var center = new Vector3(r.X + r.W * 0.5f, 0, r.Z + r.D * 0.5f);
            var room = w.RoomAt(new Vector3(center.x, 50f, center.z)) ?? w.Act.RoomAt(center.x, center.z);
            float floor = FloorFor(w, b, center);
            center.y = floor;
            var go = new GameObject("blocker_" + b.Id);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var bl = go.AddComponent<Blocker>();
            bl.Def = b;
            bool alongX = r.W >= r.D;     // the opening line runs along the longer side
            float span = alongX ? r.W : r.D;
            var rot = alongX ? Quaternion.identity : Quaternion.Euler(0, 90, 0);
            var col = new GameObject("solid");
            col.transform.SetParent(go.transform, false);
            col.transform.localRotation = rot;
            var bc = col.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 1.3f, 0);
            bc.size = new Vector3(span + 0.1f, 2.6f, Mathf.Max(0.3f, alongX ? r.D : r.W));
            bl.Solid = bc;
            switch (b.Style)
            {
                case "door":
                case "gate":
                {
                    var hinge = new GameObject("hinge").transform;
                    hinge.SetParent(go.transform, false);
                    hinge.localRotation = rot;
                    var hingeLocal = new GameObject("hingePivot").transform;
                    hingeLocal.SetParent(hinge, false);
                    hingeLocal.localPosition = new Vector3(-span * 0.5f + 0.05f, 0, 0);
                    var pm = new PropMesh();
                    if (b.Style == "door")
                    {
                        pm["wood_dark"].Box(new Vector3(span * 0.5f - 0.05f, 1.05f, 0), new Vector3(span - 0.1f, 2.1f, 0.06f), Quaternion.identity);
                        for (int i = 0; i < 3; i++) pm["iron"].Box(new Vector3(span * 0.5f, 0.3f + i * 0.75f, 0.035f), new Vector3(span - 0.2f, 0.06f, 0.01f), Quaternion.identity);
                        pm["iron"].Cylinder(new Vector3(span - 0.22f, 1.0f, 0.05f), Quaternion.Euler(90, 0, 0), 0.05f, 0.03f, 0.03f, 8);
                    }
                    else
                    {
                        for (float x = 0.08f; x < span - 0.05f; x += 0.16f)
                            pm["iron"].Box(new Vector3(x, 1.15f, 0), new Vector3(0.03f, 2.3f, 0.03f), Quaternion.identity);
                        pm["iron"].Box(new Vector3(span * 0.5f, 2.25f, 0), new Vector3(span, 0.06f, 0.05f), Quaternion.identity);
                        pm["iron"].Box(new Vector3(span * 0.5f, 0.15f, 0), new Vector3(span, 0.05f, 0.05f), Quaternion.identity);
                        pm["iron"].Box(new Vector3(span * 0.5f, 1.2f, 0), new Vector3(span, 0.05f, 0.05f), Quaternion.identity);
                    }
                    pm.Spawn("leaf", hingeLocal, Vector3.zero, Quaternion.identity);
                    bl.Moving = hingeLocal;
                    break;
                }
                case "grate":
                {
                    var slide = new GameObject("slide").transform;
                    slide.SetParent(go.transform, false);
                    slide.localRotation = rot;
                    var pm = new PropMesh();
                    for (float x = -span * 0.5f + 0.1f; x < span * 0.5f; x += 0.18f)
                        pm["iron"].Box(new Vector3(x, 1.2f, 0), new Vector3(0.04f, 2.4f, 0.04f), Quaternion.identity);
                    for (int i = 0; i < 4; i++) pm["iron"].Box(new Vector3(0, 0.2f + i * 0.7f, 0), new Vector3(span, 0.05f, 0.05f), Quaternion.identity);
                    pm.Spawn("grate", slide, Vector3.zero, Quaternion.identity);
                    bl.Moving = slide;
                    break;
                }
                case "debris":
                {
                    var rng = new Rng(Rng.Hash(b.Id));
                    var pm = Props.Planks(rng, 9, span + 0.6f);
                    foreach (var kv in Props.Rubble(rng, 6, span * 0.4f, "wood_dark").Parts) pm[kv.Key].Append(kv.Value, Matrix4x4.identity);
                    var beam = new PropMesh();
                    pm["wood_dark"].Box(new Vector3(0, 0.9f, 0), new Vector3(0.3f, 0.3f, span + 1f), Quaternion.Euler(0, 90, 28));
                    pm["hull"].Box(new Vector3(0.3f, 0.5f, 0.1f), new Vector3(span * 0.8f, 1.0f, 0.12f), Quaternion.Euler(0, 0, -12));
                    pm.Spawn("debris", go.transform, Vector3.zero, rot);
                    break;
                }
                case "fog":
                {
                    var fogGo = new GameObject("fogwall");
                    fogGo.transform.SetParent(go.transform, false);
                    fogGo.transform.localPosition = new Vector3(0, 2f, 0);
                    var f = fogGo.AddComponent<LocalVolumetricFog>();
                    var p = f.parameters;
                    p.albedo = new Color(0.7f, 0.76f, 0.78f);
                    p.meanFreePath = 0.6f;
                    p.size = new Vector3(alongX ? span + 1f : 4f, 5f, alongX ? 4f : span + 1f);
                    p.blendingMode = LocalVolumetricFogBlendingMode.Additive;
                    f.parameters = p;
                    bl.Fog = f;
                    break;
                }
            }
            return bl;
        }

        /// <summary>
        /// The floor a blocker stands on. Decks overlap in plan, so "the room at this x,z" is not enough:
        /// a door belongs to the doorway it closes and stands on that doorway's floor (the cabin door of the
        /// gun deck must not end up on the open deck above it).
        /// </summary>
        static float FloorFor(World w, BlockerDef b, Vector3 center)
        {
            float best = float.NaN, bestDist = float.MaxValue;
            float reach = Mathf.Max(b.Rect.W, b.Rect.D) * 0.5f + 0.6f;
            if (w.Level != null)
            {
                foreach (var o in w.Level.Openings)
                {
                    float d = new Vector2(o.Center.X - center.x, o.Center.Z - center.z).magnitude;
                    if (d > reach || d >= bestDist) continue;
                    bestDist = d;
                    best = o.Y;
                }
            }
            return float.IsNaN(best) ? FloorBelow(w, center) : best;
        }

        static float FloorBelow(World w, Vector3 p)
        {
            float best = float.MinValue;
            foreach (var r in w.Act.Rooms)
            {
                if (!r.Rect.Contains(p.x, p.z)) continue;
                float y = r.FloorAt(p.x, p.z);
                if (y > best) best = y;
            }
            return best == float.MinValue ? 0f : best;
        }
    }

    /// <summary>
    /// Water: the open sea (low-poly grid with slow swell) or a flat flooded floor. Normal maps scroll;
    /// the level can animate (draining streets, the flooding mirror hall).
    /// </summary>
    public sealed class WaterSurface : MonoBehaviour
    {
        public float Level;
        public float Swell = 0.25f;
        public bool Sea;
        public RectXZ Area;
        public bool HasArea;
        Mesh _mesh;
        Vector3[] _base, _verts;
        Material _mat;
        float _target;
        float _speed;

        public static WaterSurface CreateSea(Transform parent, Vector3 center, float size, float level, int res = 72)
        {
            var go = new GameObject("Sea");
            go.transform.SetParent(parent, false);
            var w = go.AddComponent<WaterSurface>();
            w.Sea = true;
            w.Level = level;
            w._target = level;
            var mb = new MeshBuilder { TexScale = 6f };
            mb.Grid(new Vector3(center.x - size * 0.5f, 0, center.z - size * 0.5f), new Vector2(size, size), res, res, null, 6f);
            w._mesh = mb.Build("sea");
            w._mesh.MarkDynamic();
            w._base = w._mesh.vertices;
            w._verts = (Vector3[])w._base.Clone();
            go.AddComponent<MeshFilter>().sharedMesh = w._mesh;
            var mr = go.AddComponent<MeshRenderer>();
            w._mat = MatLib.Instance("water");
            mr.sharedMaterial = w._mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.position = new Vector3(0, level, 0);
            return w;
        }

        public static WaterSurface CreateFlat(Transform parent, string name, RectXZ rect, float level, bool shallow = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var w = go.AddComponent<WaterSurface>();
            w.Level = level;
            w._target = level;
            w.Area = rect;
            w.HasArea = true;
            var mb = new MeshBuilder { TexScale = 3f };
            mb.Quad(new Vector3(rect.X, 0, rect.Z), new Vector3(rect.MaxX, 0, rect.Z), new Vector3(rect.MaxX, 0, rect.MaxZ), new Vector3(rect.X, 0, rect.MaxZ));
            w._mesh = mb.Build(name);
            go.AddComponent<MeshFilter>().sharedMesh = w._mesh;
            var mr = go.AddComponent<MeshRenderer>();
            w._mat = MatLib.Instance(shallow ? "water_shallow" : "water");
            mr.sharedMaterial = w._mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.transform.position = new Vector3(0, level, 0);
            return w;
        }

        public bool Covers(Vector3 p) => HasArea && Area.Contains(p.x, p.z);

        /// <summary>Moves the surface to a new height over time (drain / flood).</summary>
        public void AnimateTo(float level, float seconds)
        {
            _target = level;
            _speed = Mathf.Abs(level - Level) / Mathf.Max(0.01f, seconds);
        }

        void Update()
        {
            float t = Time.time;
            if (!Mathf.Approximately(Level, _target)) Level = Mathf.MoveTowards(Level, _target, _speed * Time.deltaTime);
            transform.position = new Vector3(0, Level, 0);
            if (_mat != null && _mat.HasProperty("_BaseColorMap"))
                _mat.SetTextureOffset("_BaseColorMap", new Vector2(t * 0.012f, t * 0.007f));
            if (!Sea || _base == null) return;
            for (int i = 0; i < _base.Length; i++)
            {
                var b = _base[i];
                float h = Mathf.Sin(b.x * 0.11f + t * 0.6f) * 0.5f + Mathf.Sin(b.z * 0.07f - t * 0.45f) * 0.35f + Mathf.Sin((b.x + b.z) * 0.23f + t * 1.1f) * 0.15f;
                _verts[i] = new Vector3(b.x, h * Swell, b.z);
            }
            _mesh.vertices = _verts;
            _mesh.RecalculateNormals();
        }

        void OnDestroy()
        {
            if (_mat != null) Destroy(_mat);
        }
    }

    /// <summary>A dressing object that may shift while unseen (hidden sanity). Quest objects never are.</summary>
    public sealed class Distortable : MonoBehaviour
    {
        public Vector3 HomePos;
        public Quaternion HomeRot;
        public Vector3 AltPos;
        public Quaternion AltRot;
        public Bounds Bounds;
        public int Level;   // 0 home, 1 nudged, 2 shifted, 3 relocated

        public void Init(Vector3 alt, Quaternion altRot)
        {
            HomePos = transform.position;
            HomeRot = transform.rotation;
            AltPos = alt;
            AltRot = altRot;
            var r = GetComponentInChildren<Renderer>();
            Bounds = r != null ? r.bounds : new Bounds(HomePos, Vector3.one);
        }
    }
}
