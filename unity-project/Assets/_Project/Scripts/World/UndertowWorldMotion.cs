using UnityEngine;

namespace Undertow.World
{
    /// <summary>Small procedural motion for sails, suspended props, and the distant leviathan.</summary>
    public sealed class UndertowSailSway : MonoBehaviour
    {
        [SerializeField] private float yawDegrees = 2.2f;
        [SerializeField] private float rollDegrees = 1.1f;
        [SerializeField] private float frequency = 0.22f;
        private Quaternion initial;
        private float phase;
        private void Awake() { initial = transform.localRotation; phase = Random.value * 10f; }
        private void Update()
        {
            float t = (Time.time + phase) * frequency * Mathf.PI * 2f;
            transform.localRotation = initial * Quaternion.Euler(Mathf.Sin(t * 0.72f) * rollDegrees, Mathf.Sin(t) * yawDegrees, Mathf.Cos(t * 0.83f) * rollDegrees);
        }
    }

    public sealed class UndertowLeviathanSwim : MonoBehaviour
    {
        [SerializeField] private float driftRadius = 2.6f;
        [SerializeField] private float verticalBob = 0.55f;
        [SerializeField] private float swimSpeed = 0.08f;
        private Vector3 origin;
        private Quaternion initial;
        private float phase;
        private void Awake() { origin = transform.position; initial = transform.rotation; phase = Random.value * 6f; }
        private void Update()
        {
            float t = Time.time * swimSpeed + phase;
            transform.position = origin + new Vector3(Mathf.Sin(t) * driftRadius, Mathf.Sin(t * 1.7f) * verticalBob, Mathf.Cos(t * 0.72f) * driftRadius * 0.7f);
            transform.rotation = initial * Quaternion.Euler(Mathf.Sin(t * 1.2f) * 1.8f, Mathf.Sin(t * 0.7f) * 3.5f, Mathf.Sin(t * 1.5f) * 1.2f);
        }
    }

    public sealed class UndertowFloatingMotion : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.08f;
        [SerializeField] private float frequency = 0.4f;
        private Vector3 origin;
        private float phase;
        private void Awake() { origin = transform.position; phase = Random.value * 10f; }
        private void Update()
        {
            float t = (Time.time + phase) * frequency;
            transform.position = origin + Vector3.up * (Mathf.Sin(t) * amplitude);
            transform.Rotate(Vector3.up, Mathf.Sin(t * 0.6f) * 0.025f, Space.Self);
        }
    }

    public sealed class UndertowStormFlash : MonoBehaviour
    {
        [SerializeField] private Light stormLight;
        [SerializeField] private Vector2 intervalSeconds = new Vector2(14f, 32f);
        [SerializeField] private float flashIntensity = 3.6f;
        private float timer;
        private float flash;
        private float baseIntensity;
        private void Awake()
        {
            if (stormLight == null) stormLight = GetComponent<Light>();
            if (stormLight != null) baseIntensity = stormLight.intensity;
            timer = Random.Range(intervalSeconds.x, intervalSeconds.y);
        }
        private void Update()
        {
            if (stormLight == null) return;
            timer -= Time.deltaTime;
            if (timer <= 0f) { flash = Random.Range(0.08f, 0.22f); timer = Random.Range(intervalSeconds.x, intervalSeconds.y); }
            if (flash > 0f)
            {
                flash -= Time.deltaTime;
                stormLight.intensity = flash > 0.04f ? flashIntensity : baseIntensity;
            }
            else stormLight.intensity = baseIntensity;
        }
    }

    public sealed class UndertowFirstPersonLanternMotion : MonoBehaviour
    {
        [SerializeField] private float sway = 0.018f;
        [SerializeField] private float rotation = 1.6f;
        [SerializeField] private float frequency = 1.7f;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private void Awake() { startPosition = transform.localPosition; startRotation = transform.localRotation; }
        private void LateUpdate()
        {
            float t = Time.time * frequency;
            float gait = Mathf.Sin(t) * 0.55f + Mathf.Sin(t * 0.5f) * 0.45f;
            transform.localPosition = startPosition + new Vector3(gait * sway, Mathf.Abs(Mathf.Cos(t)) * sway * 0.35f, 0f);
            transform.localRotation = startRotation * Quaternion.Euler(Mathf.Sin(t * .5f) * rotation, 0f, gait * rotation * 0.8f);
        }
    }
}
