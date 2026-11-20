using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Collects procedural geometry per chunk (usually per room) and per material, then builds one
    /// mesh per chunk and material. Colliders are separate invisible boxes. Few draw calls, fast loads.
    /// </summary>
    public sealed class Kit
    {
        public readonly Transform Root;
        public readonly Rng Rng;
        readonly Dictionary<string, Dictionary<string, MeshBuilder>> _chunks = new Dictionary<string, Dictionary<string, MeshBuilder>>();
        readonly Transform _colliders;
        readonly List<Renderer> _renderers = new List<Renderer>();
        public int ColliderCount { get; private set; }

        public Kit(Transform root, int seed)
        {
            Root = root;
            Rng = new Rng(seed);
            _colliders = new GameObject("Colliders").transform;
            _colliders.SetParent(root, false);
        }

        public MeshBuilder B(string chunk, string mat, float texScale = 1f)
        {
            if (!_chunks.TryGetValue(chunk, out var mats)) _chunks[chunk] = mats = new Dictionary<string, MeshBuilder>();
            if (!mats.TryGetValue(mat, out var b))
            {
                b = new MeshBuilder { TexScale = texScale };
                mats[mat] = b;
            }
            return b;
        }

        public void Box(string chunk, string mat, Vector3 center, Vector3 size, Quaternion rot, bool collide = false, int faces = 63, float uv = 0f)
        {
            B(chunk, mat).Box(center, size, rot, faces, uv);
            if (collide) Collider(center, size, rot);
        }

        public void Box(string chunk, string mat, Vector3 center, Vector3 size, bool collide = false)
            => Box(chunk, mat, center, size, Quaternion.identity, collide);

        public BoxCollider Collider(Vector3 center, Vector3 size, Quaternion rot, string name = "c")
        {
            var go = new GameObject(name);
            go.transform.SetParent(_colliders, false);
            go.transform.localPosition = center;
            go.transform.localRotation = rot;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y), Mathf.Max(0.01f, size.z));
            ColliderCount++;
            return bc;
        }

        public BoxCollider Collider(Vector3 center, Vector3 size) => Collider(center, size, Quaternion.identity);

        /// <summary>Builds every chunk. Call once when the act is assembled.</summary>
        public void Finish(bool shadows = true)
        {
            foreach (var chunk in _chunks)
            {
                var cgo = new GameObject(chunk.Key);
                cgo.transform.SetParent(Root, false);
                cgo.isStatic = true;
                foreach (var mb in chunk.Value)
                {
                    if (mb.Value.Empty) continue;
                    var go = new GameObject(mb.Key);
                    go.transform.SetParent(cgo.transform, false);
                    go.isStatic = true;
                    var mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = mb.Value.Build(chunk.Key + "_" + mb.Key);
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = MatLib.Get(mb.Key);
                    var def = MatLib.Def(mb.Key);
                    bool transparent = def != null && def.Transparent;
                    mr.shadowCastingMode = shadows && !transparent ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    mr.receiveShadows = true;
                    mr.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
                    _renderers.Add(mr);
                }
            }
            _chunks.Clear();
        }

        /// <summary>A standalone object (movable prop) from a builder.</summary>
        public static GameObject Object(string name, Transform parent, Vector3 pos, Quaternion rot, MeshBuilder mb, string mat, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mb.Build(name);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = MatLib.Get(mat);
            mr.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Multi-material standalone object: one child per material.</summary>
        public static GameObject Object(string name, Transform parent, Vector3 pos, Quaternion rot, Dictionary<string, MeshBuilder> parts, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            foreach (var p in parts)
            {
                if (p.Value.Empty) continue;
                var child = new GameObject(p.Key);
                child.transform.SetParent(go.transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = p.Value.Build(name + "_" + p.Key);
                var mr = child.AddComponent<MeshRenderer>();
                mr.sharedMaterial = MatLib.Get(p.Key);
                var def = MatLib.Def(p.Key);
                mr.shadowCastingMode = shadows && (def == null || !def.Transparent) ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
            return go;
        }
    }

    /// <summary>Multi-material builder for one prop (local space).</summary>
    public sealed class PropMesh
    {
        public readonly Dictionary<string, MeshBuilder> Parts = new Dictionary<string, MeshBuilder>();

        public MeshBuilder this[string mat]
        {
            get
            {
                if (!Parts.TryGetValue(mat, out var b)) Parts[mat] = b = new MeshBuilder { TexScale = 0.5f };
                return b;
            }
        }

        public GameObject Spawn(string name, Transform parent, Vector3 pos, Quaternion rot, bool shadows = true)
            => Kit.Object(name, parent, pos, rot, Parts, shadows);

        /// <summary>Bakes the prop into a kit chunk (static, no own GameObject).</summary>
        public void BakeInto(Kit kit, string chunk, Vector3 pos, Quaternion rot, float scale = 1f)
        {
            var m = Matrix4x4.TRS(pos, rot, Vector3.one * scale);
            foreach (var p in Parts) kit.B(chunk, p.Key).Append(p.Value, m);
        }
    }
}
