using UnityEngine;
using Undertow.Lantern;

namespace Undertow.Puzzles
{
    public class UndertowPresenceDirector : MonoBehaviour
    {
        [SerializeField] private float maxSafeDistanceMeters = 32.0f;
        [SerializeField] private float minEnvelopeDistanceMeters = 2.5f;
        [SerializeField] private float approachSpeedWhenDark = 3.8f;
        [SerializeField] private float recedeSpeedWhenLit = 6.2f;

        [SerializeField] private Transform leviathan;
        private LanternController lantern;
        private Transform player;

        public float CurrentPresenceDistanceMeters { get; private set; } = 32.0f;
        public float ChorusResonanceIntensity => 1.0f - Mathf.InverseLerp(minEnvelopeDistanceMeters, maxSafeDistanceMeters, CurrentPresenceDistanceMeters);

        private void Start()
        {
            lantern = FindObjectOfType<LanternController>();
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        public void SetLeviathan(Transform target) { leviathan = target; }

        private void Update()
        {
            if (lantern == null) lantern = FindObjectOfType<LanternController>();
            TickPresence(Time.deltaTime, lantern != null && lantern.IsLit);
            if (leviathan != null && player != null)
            {
                Vector3 away = (leviathan.position - player.position);
                away.y = 0f;
                if (away.sqrMagnitude < 0.01f) away = Vector3.forward;
                away.Normalize();
                Vector3 target = player.position + away * CurrentPresenceDistanceMeters;
                target.y = leviathan.position.y;
                leviathan.position = Vector3.Lerp(leviathan.position, target, Time.deltaTime * 0.12f);
            }
        }

        public void TickPresence(float deltaTime, bool isLanternLit)
        {
            if (deltaTime <= 0f) return;

            if (!isLanternLit)
            {
                CurrentPresenceDistanceMeters = Mathf.Max(
                    minEnvelopeDistanceMeters,
                    CurrentPresenceDistanceMeters - approachSpeedWhenDark * deltaTime
                );
            }
            else
            {
                CurrentPresenceDistanceMeters = Mathf.Min(
                    maxSafeDistanceMeters,
                    CurrentPresenceDistanceMeters + recedeSpeedWhenLit * deltaTime
                );
            }
        }
    }
}
