using UnityEngine;
using Undertow.Player;

namespace Undertow.Lantern
{
    public class LanternFuelPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string pickupId = "OIL_FLASK_ACT1_01";
        [SerializeField] private float fuelAmount = 35.0f;
        [SerializeField] private LanternController targetLantern;

        private bool isCollected;

        public string PickupId => pickupId;
        public string InteractionPromptKey => "PROMPT_PICKUP_WHALE_OIL";
        public bool CanInteract => !isCollected;

        public bool TryConsume(LanternController lantern)
        {
            if (isCollected || lantern == null) return false;
            if (lantern.FuelNormalized >= 0.999f) return false;

            lantern.AddFuel(fuelAmount);
            isCollected = true;
            if (gameObject != null) gameObject.SetActive(false);
            return true;
        }

        public void OnInteract(PlayerInteractionSystem interactor)
        {
            LanternController lantern = targetLantern != null
                ? targetLantern
                : (interactor != null ? interactor.GetComponentInChildren<LanternController>() : null);
            TryConsume(lantern);
        }
    }
}
