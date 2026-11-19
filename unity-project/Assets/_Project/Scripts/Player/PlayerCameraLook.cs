using UnityEngine;

namespace Undertow.Player
{
    public class PlayerCameraLook : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private float mouseSensitivity = 1.65f;
        [SerializeField] private float smoothing = 18.0f;
        [SerializeField] private float minPitch = -82.0f;
        [SerializeField] private float maxPitch = 80.0f;

        [Header("Ocean Roll & Sanity Tremor")]
        [SerializeField] private float oceanRollAmplitudeDeg = 0.85f;
        [SerializeField] private float oceanRollFrequency = 0.22f;

        private float targetPitch;
        private float currentPitch;
        private float currentYawDelta;
        private float sanityTremorIntensity;
        private bool lookLocked;

        public float MouseSensitivity
        {
            get => mouseSensitivity;
            set => mouseSensitivity = Mathf.Clamp(value, 0.1f, 10.0f);
        }

        private void Awake()
        {
            if (playerBody == null && transform.parent != null)
            {
                playerBody = transform.parent;
            }

            float initialEulerX = transform.localEulerAngles.x;
            if (initialEulerX > 180f) initialEulerX -= 360f;
            currentPitch = targetPitch = Mathf.Clamp(initialEulerX, minPitch, maxPitch);
        }

        private void Start()
        {
            if (Application.isFocused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && !lookLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void SetSanityTremor(float normalizedIntensity)
        {
            sanityTremorIntensity = Mathf.Clamp01(normalizedIntensity);
        }

        public void SetLookLocked(bool locked)
        {
            lookLocked = locked;
            if (Application.isPlaying)
            {
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
        }

        private void Update()
        {
            if (lookLocked || Time.timeScale <= 0f) return;

            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            ApplyLookInput(mouseX, mouseY, Time.deltaTime);
        }

        public void ApplyLookInput(float deltaX, float deltaY, float deltaTime)
        {
            targetPitch = Mathf.Clamp(targetPitch - deltaY, minPitch, maxPitch);
            float t = deltaTime > 0f ? Mathf.Clamp01(smoothing * deltaTime) : 1f;
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, t);
            currentYawDelta = Mathf.Lerp(currentYawDelta, deltaX, t);

            float waveRoll = Mathf.Sin(Time.time * oceanRollFrequency * Mathf.PI * 2f) * oceanRollAmplitudeDeg;
            float tremorPitch = (Mathf.PerlinNoise(Time.time * 4.7f, 1.3f) - 0.5f) * 1.4f * sanityTremorIntensity;
            float tremorRoll = (Mathf.PerlinNoise(2.9f, Time.time * 5.1f) - 0.5f) * 1.8f * sanityTremorIntensity;

            transform.localRotation = Quaternion.Euler(currentPitch + tremorPitch, 0f, waveRoll + tremorRoll);
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * currentYawDelta);
            }
        }
    }
}
