using UnityEngine;
using UnityEngine.Rendering;

namespace Undertow.World
{
    /// <summary>
    /// Lightweight, deterministic multi-band ocean mesh for the storm scenes.
    /// It is a visual water surface (not a replacement for HDRP's full Water System).
    /// Keep the grid moderate and enable HDRP reflections on the assigned material.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class UndertowOceanSurface : MonoBehaviour
    {
        [SerializeField, Range(24, 180)] private int resolution = 112;
        [SerializeField] private float size = 240f;
        [SerializeField] private float waveAmplitude = 0.34f;
        [SerializeField] private float waveSpeed = 1f;
        [SerializeField] private float updateRate = 30f;
        [SerializeField] private float normalStrength = 1f;

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] displacedVertices;
        private float updateAccumulator;

        private void OnEnable() { if (mesh == null) BuildMesh(); }
        private void Awake() { if (mesh == null) BuildMesh(); }

        public void Configure(int gridResolution, float worldSize, float amplitude)
        {
            resolution = Mathf.Clamp(gridResolution, 24, 180);
            size = Mathf.Max(20f, worldSize);
            waveAmplitude = Mathf.Clamp(amplitude, 0.02f, 2.5f);
            if (Application.isPlaying) BuildMesh();
        }

        private void BuildMesh()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (mesh != null) { if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh); }

            int side = resolution + 1;
            baseVertices = new Vector3[side * side];
            displacedVertices = new Vector3[baseVertices.Length];
            Vector2[] uv = new Vector2[baseVertices.Length];
            int[] triangles = new int[resolution * resolution * 6];
            int index = 0;
            for (int z = 0; z < side; z++)
            {
                for (int x = 0; x < side; x++)
                {
                    int i = z * side + x;
                    float px = ((float)x / resolution - 0.5f) * size;
                    float pz = ((float)z / resolution - 0.5f) * size;
                    baseVertices[i] = new Vector3(px, 0f, pz);
                    uv[i] = new Vector2((float)x / resolution * size / 12f, (float)z / resolution * size / 12f);
                    if (x < resolution && z < resolution)
                    {
                        int a = i, b = i + side, c = i + 1, d = i + side + 1;
                        triangles[index++] = a; triangles[index++] = b; triangles[index++] = c;
                        triangles[index++] = c; triangles[index++] = b; triangles[index++] = d;
                    }
                }
            }
            mesh = new Mesh { name = "Undertow_Dynamic_Ocean_Mesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.vertices = baseVertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;
            UpdateWaveGeometry(Time.time, true);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            updateAccumulator += Time.deltaTime;
            float interval = 1f / Mathf.Max(5f, updateRate);
            if (updateAccumulator < interval) return;
            float dt = updateAccumulator;
            updateAccumulator = 0f;
            UpdateWaveGeometry(Time.time, false);
        }

        private void UpdateWaveGeometry(float time, bool immediate)
        {
            if (mesh == null || baseVertices == null) return;
            float t = time * waveSpeed;
            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 p = baseVertices[i];
                float a = Mathf.Sin(p.x * 0.105f + p.z * 0.034f + t * 1.15f) * 0.52f;
                float b = Mathf.Sin(p.x * -0.047f + p.z * 0.082f + t * 0.83f + 1.7f) * 0.30f;
                float c = Mathf.Sin((p.x + p.z) * 0.19f - t * 1.65f) * 0.13f;
                float d = Mathf.Sin(p.x * 0.29f - p.z * 0.21f + t * 2.1f) * 0.05f;
                displacedVertices[i] = new Vector3(p.x, (a + b + c + d) * waveAmplitude, p.z);
            }
            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh);
            }
        }
    }
}
