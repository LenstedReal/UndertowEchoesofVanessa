using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Core;
using Undertow.Lantern;
using Undertow.Sanity;
using Undertow.Player;

namespace Undertow.World
{
    /// <summary>Periodic, scene-aware save with a short interval and pause/quit flush.</summary>
    public sealed class UndertowCampaignAutosave : MonoBehaviour
    {
        [SerializeField, Min(5f)] private float intervalSeconds = 25f;
        private float timer;
        private ActProgressionManager campaign;
        private LanternController lantern;
        private HiddenSanitySystem sanity;
        private FirstPersonController player;

        private void Awake()
        {
            campaign = ActProgressionManager.Instance;
            lantern = FindObjectOfType<LanternController>();
            sanity = FindObjectOfType<HiddenSanitySystem>();
            player = FindObjectOfType<FirstPersonController>();
            timer = intervalSeconds;
        }

        private void Update()
        {
            if (campaign == null) campaign = ActProgressionManager.Instance;
            if (campaign == null || campaign.State == null) return;
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                CaptureState();
                campaign.PersistNow();
                timer = intervalSeconds;
            }
        }

        private void CaptureState()
        {
            if (lantern == null) lantern = FindObjectOfType<LanternController>();
            if (sanity == null) sanity = FindObjectOfType<HiddenSanitySystem>();
            if (player == null) player = FindObjectOfType<FirstPersonController>();
            CampaignState state = campaign.State;
            if (lantern != null) state.LanternFuelNormalized = lantern.FuelNormalized;
            if (sanity != null) state.SanityValue = sanity.CurrentSanity;
            if (player != null)
            {
                state.HasPlayerTransform = true;
                state.SavedSceneName = SceneManager.GetActiveScene().name;
                state.SavedPlayerPosition = player.transform.position;
                state.SavedPlayerYaw = player.transform.eulerAngles.y;
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) Flush(); }
        private void OnApplicationQuit() { Flush(); }
        private void OnDisable() { Flush(); }
        public void Flush()
        {
            if (campaign == null) campaign = ActProgressionManager.Instance;
            if (campaign == null) return;
            CaptureState();
            campaign.PersistNow();
        }
    }
}
