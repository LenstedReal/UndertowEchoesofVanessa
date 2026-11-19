using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Kinematics")]
        [SerializeField] private float walkSpeed = 2.65f;
        [SerializeField] private float cautiousSpeed = 1.45f;
        [SerializeField] private float crouchSpeed = 1.35f;
        [SerializeField] private float acceleration = 9.5f;
        [SerializeField] private float gravity = -16.8f;

        [Header("Posture & Collision")]
        [SerializeField] private float standingHeight = 1.78f;
        [SerializeField] private float crouchingHeight = 1.05f;
        [SerializeField] private Transform cameraRoot;

        [Header("Headbob & Footsteps")]
        [SerializeField] private float bobFrequency = 1.65f;
        [SerializeField] private float bobAmplitude = 0.028f;
        [SerializeField] private float stepIntervalMeters = 1.42f;

        private CharacterController controller;
        private Vector3 currentHorizontalVelocity;
        private float verticalVelocity;
        private float stepDistanceAccumulator;
        private float headbobTimer;
        private bool isCrouching;
        private bool isMovementLocked;
        private SurfaceMaterialType currentSurface = SurfaceMaterialType.WetDeckWood;

        public event Action<SurfaceMaterialType, float> OnFootstepTriggered;

        public bool IsCrouching => isCrouching;
        public Vector3 Velocity => currentHorizontalVelocity + Vector3.up * verticalVelocity;
        public Transform CameraRoot => cameraRoot;

        private void Awake()
        {
            InitializeController();
            var campaign = ActProgressionManager.Instance;
            if (campaign != null && campaign.State != null && campaign.State.HasPlayerTransform &&
                string.Equals(campaign.State.SavedSceneName, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, StringComparison.Ordinal))
            {
                transform.position = campaign.State.SavedPlayerPosition;
                transform.rotation = Quaternion.Euler(0f, campaign.State.SavedPlayerYaw, 0f);
            }
        }

        public void InitializeController()
        {
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.height = standingHeight;
                controller.center = new Vector3(0f, standingHeight * 0.5f, 0f);
            }

            if (cameraRoot == null)
            {
                Camera childCam = GetComponentInChildren<Camera>();
                if (childCam != null)
                    cameraRoot = childCam.transform;
            }
        }

        private void Update()
        {
            if (isMovementLocked) return;

            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            bool toggleCrouch = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C);
            if (toggleCrouch) SetCrouch(!isCrouching);

            SimulateStep(moveInput, Input.GetKey(KeyCode.LeftShift), Time.deltaTime);
        }

        public void SetMovementLocked(bool locked)
        {
            isMovementLocked = locked;
            if (locked) currentHorizontalVelocity = Vector3.zero;
        }

        public void SetCrouch(bool crouch)
        {
            if (!crouch && !CanStandUp()) return;
            isCrouching = crouch;
            float targetH = isCrouching ? crouchingHeight : standingHeight;
            if (controller != null)
            {
                controller.height = targetH;
                controller.center = new Vector3(0f, targetH * 0.5f, 0f);
            }
        }

        public bool CanStandUp()
        {
            Vector3 origin = transform.position + Vector3.up * crouchingHeight;
            float checkDist = standingHeight - crouchingHeight + 0.1f;
            return !Physics.SphereCast(origin, 0.25f, Vector3.up, out _, checkDist, ~0, QueryTriggerInteraction.Ignore);
        }

        public void SimulateStep(Vector2 rawInput, bool cautiousFocusHeld, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (controller == null) InitializeController();

            Vector2 clamped = Vector2.ClampMagnitude(rawInput, 1f);
            float targetSpeed = isCrouching ? crouchSpeed : (cautiousFocusHeld ? cautiousSpeed : walkSpeed);
            Vector3 desiredWorld = (transform.right * clamped.x + transform.forward * clamped.y) * targetSpeed;

            currentHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                desiredWorld,
                acceleration * deltaTime
            );

            if (controller != null && controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2.0f;
            else
                verticalVelocity += gravity * deltaTime;

            Vector3 displacement = (currentHorizontalVelocity + Vector3.up * verticalVelocity) * deltaTime;
            if (controller != null && controller.enabled)
                controller.Move(displacement);
            else
                transform.position += displacement;

            if (cameraRoot != null)
            {
                float targetEye = (isCrouching ? crouchingHeight : standingHeight) - 0.14f;
                Vector3 lp = cameraRoot.localPosition;
                lp.y = Mathf.Lerp(lp.y, targetEye, 1f - Mathf.Exp(-12f * deltaTime));
                cameraRoot.localPosition = new Vector3(0f, lp.y, 0f);
            }

            float planarStep = currentHorizontalVelocity.magnitude * deltaTime;
            if (planarStep > 0.0001f)
            {
                stepDistanceAccumulator += planarStep;
                headbobTimer += deltaTime * bobFrequency * (currentHorizontalVelocity.magnitude / Mathf.Max(0.1f, walkSpeed));
                if (cameraRoot != null)
                {
                    float baseEyeHeight = (isCrouching ? crouchingHeight : standingHeight) - 0.14f;
                    float offsetY = Mathf.Sin(headbobTimer * Mathf.PI * 2f) * bobAmplitude;
                    cameraRoot.localPosition = new Vector3(0f, cameraRoot.localPosition.y + offsetY * deltaTime * 6f, 0f);
                }

                if (stepDistanceAccumulator >= stepIntervalMeters)
                {
                    stepDistanceAccumulator -= stepIntervalMeters;
                    DetectSurfaceUnderfoot();
                    OnFootstepTriggered?.Invoke(currentSurface, currentHorizontalVelocity.magnitude / Mathf.Max(0.1f, walkSpeed));
                }
            }
        }

        private void DetectSurfaceUnderfoot()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 0.6f))
            {
                string tagOrName = (hit.collider.tag + "_" + hit.collider.name).ToLowerInvariant();
                if (tagOrName.Contains("grate") || tagOrName.Contains("iron"))
                    currentSurface = SurfaceMaterialType.IronGrating;
                else if (tagOrName.Contains("water") || tagOrName.Contains("bilge"))
                    currentSurface = SurfaceMaterialType.ShallowBilgeWater;
                else if (tagOrName.Contains("cobble") || tagOrName.Contains("port"))
                    currentSurface = SurfaceMaterialType.PortCobblestone;
                else if (tagOrName.Contains("temple") || tagOrName.Contains("basalt"))
                    currentSurface = SurfaceMaterialType.SunkenTempleBasalt;
                else
                    currentSurface = SurfaceMaterialType.WetDeckWood;
            }
        }
    }
}
