using System;
using UnityEngine;

namespace Undertow.Audio
{
    public class ListenMechanicController : MonoBehaviour
    {
        [SerializeField] private float environmentalDuckDb = -14.0f;
        [SerializeField] private float environmentalLowPassCutoffHz = 320.0f;
        [SerializeField] private float normalLowPassCutoffHz = 22000.0f;
        [SerializeField] private float voiceDirectionalGainDb = 5.5f;
        [SerializeField] private float transitionSpeed = 6.0f;

        private bool isListening;
        private float listenBlend;

        public event Action<bool, float> OnListenBlendChanged;

        public bool IsListening => isListening;
        public float ListenBlend => listenBlend;
        public float CurrentEnvironmentLowPassHz => Mathf.Lerp(normalLowPassCutoffHz, environmentalLowPassCutoffHz, listenBlend);
        public float CurrentEnvironmentGainDb => Mathf.Lerp(0.0f, environmentalDuckDb, listenBlend);
        public float CurrentVoiceClarityGainDb => Mathf.Lerp(0.0f, voiceDirectionalGainDb, listenBlend);

        private void OnDisable()
        {
            SetListening(false, 0f);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) { SetListening(false, 0f); return; }
            bool held = Input.GetKey(KeyCode.Q) || Input.GetMouseButton(1);
            SetListening(held, Time.deltaTime);
        }

        public void SetListening(bool active, float deltaTime)
        {
            isListening = active;
            float target = isListening ? 1.0f : 0.0f;
            listenBlend = deltaTime > 0f
                ? Mathf.MoveTowards(listenBlend, target, transitionSpeed * deltaTime)
                : target;
            OnListenBlendChanged?.Invoke(isListening, listenBlend);
        }
    }
}
