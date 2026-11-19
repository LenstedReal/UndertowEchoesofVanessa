using UnityEngine;

namespace Undertow.World
{
    /// <summary>Subtle bubbles and suspended particles for the underwater temple.</summary>
    public sealed class UndertowUnderwaterParticles : MonoBehaviour
    {
        [SerializeField] private int particleCount = 90;
        [SerializeField] private Vector3 volumeSize = new Vector3(34f, 18f, 54f);
        [SerializeField] private float riseSpeed = 0.18f;
        [SerializeField] private float maxScale = 0.055f;
        private Transform[] particles;
        private float[] phases;

        private void Start()
        {
            particles = new Transform[Mathf.Clamp(particleCount, 8, 250)];
            phases = new float[particles.Length];
            Material bubbleMat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (bubbleMat.HasProperty("_BaseColor")) bubbleMat.SetColor("_BaseColor", new Color(0.45f, 0.86f, 0.92f, 0.42f));
            if (bubbleMat.HasProperty("_Color")) bubbleMat.SetColor("_Color", new Color(0.45f, 0.86f, 0.92f, 0.42f));
            for (int i = 0; i < particles.Length; i++)
            {
                GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bubble.name = "Underwater_Suspended_Particle_" + i;
                bubble.transform.SetParent(transform, false);
                bubble.transform.localPosition = new Vector3(Random.Range(-volumeSize.x * .5f, volumeSize.x * .5f), Random.Range(-volumeSize.y * .5f, volumeSize.y * .5f), Random.Range(-volumeSize.z * .5f, volumeSize.z * .5f));
                float scale = Random.Range(maxScale * .22f, maxScale);
                bubble.transform.localScale = Vector3.one * scale;
                Renderer r = bubble.GetComponent<Renderer>(); r.sharedMaterial = bubbleMat;
                Collider c = bubble.GetComponent<Collider>(); if (c != null) Destroy(c);
                particles[i] = bubble.transform; phases[i] = Random.value * 6.28f;
            }
        }

        private void Update()
        {
            if (particles == null) return;
            for (int i = 0; i < particles.Length; i++)
            {
                Transform p = particles[i]; if (p == null) continue;
                Vector3 pos = p.localPosition;
                pos.y += riseSpeed * (0.55f + (i % 7) * 0.12f) * Time.deltaTime;
                pos.x += Mathf.Sin(Time.time * 0.42f + phases[i]) * 0.006f;
                if (pos.y > volumeSize.y * .5f) pos.y = -volumeSize.y * .5f;
                p.localPosition = pos;
            }
        }
    }
}
