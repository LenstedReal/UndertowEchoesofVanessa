using System;
using UnityEngine;

namespace Undertow.Player
{
    public interface IInteractable
    {
        string InteractionPromptKey { get; }
        bool CanInteract { get; }
        void OnInteract(PlayerInteractionSystem interactor);
    }

    public class PlayerInteractionSystem : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private float maxInteractionDistance = 2.35f;
        [SerializeField] private LayerMask interactableMask = ~0;

        private IInteractable currentFocus;
        private bool isInspectingObject;
        private string activeInspectionItemId;

        public event Action<IInteractable> OnFocusChanged;
        public event Action<string, string> OnItemInspected;

        public IInteractable CurrentFocus => currentFocus;
        public bool IsInspectingObject => isInspectingObject;

        private void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = GetComponent<Camera>();
                if (viewCamera == null)
                    viewCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (isInspectingObject)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
                    EndInspection();
                return;
            }

            UpdateRaycastFocus();

            if (currentFocus != null && currentFocus.CanInteract && Input.GetKeyDown(KeyCode.E))
            {
                currentFocus.OnInteract(this);
            }
        }

        public void UpdateRaycastFocus()
        {
            if (viewCamera == null) return;

            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            IInteractable found = null;
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, interactableMask, QueryTriggerInteraction.Collide))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (!ReferenceEquals(found, currentFocus))
            {
                currentFocus = found;
                OnFocusChanged?.Invoke(currentFocus);
            }
        }

        public void BeginInspection(string itemId, string localizationTextKey)
        {
            isInspectingObject = true;
            activeInspectionItemId = itemId;
            OnItemInspected?.Invoke(itemId, localizationTextKey);
        }

        public void EndInspection()
        {
            isInspectingObject = false;
            activeInspectionItemId = null;
        }
    }
}
