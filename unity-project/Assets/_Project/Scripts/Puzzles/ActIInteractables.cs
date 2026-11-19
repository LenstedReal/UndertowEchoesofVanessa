using System.Collections;
using UnityEngine;
using Undertow.Core;
using Undertow.Player;
using Undertow.Lantern;
using Undertow.Sanity;
using Undertow.Audio;
using Undertow.Narrative;

namespace Undertow.Puzzles
{
    public sealed class BellStrikeInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private FiveBellPuzzle puzzle;
        [SerializeField] private BellNote note;
        public string InteractionPromptKey => "PROMPT_STRIKE_BELL_" + note.ToString().ToUpperInvariant();
        public bool CanInteract => puzzle != null && !puzzle.IsSolved;
        public void Configure(FiveBellPuzzle target, BellNote bellNote) { puzzle = target; note = bellNote; }
        public void OnInteract(PlayerInteractionSystem interactor) { if (CanInteract) puzzle.StrikeBell(note); }
    }

    public sealed class SurgeonsHookPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private CrateRingPuzzle puzzle;
        private bool collected;
        public string InteractionPromptKey => "PROMPT_PICKUP_SURGEONS_HOOK";
        public bool CanInteract => !collected && puzzle != null;
        public void Configure(CrateRingPuzzle target) { puzzle = target; }
        private void Start() { if (puzzle != null && puzzle.HasSurgeonsHook) { collected = true; gameObject.SetActive(false); } }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            collected = true;
            puzzle.AcquireSurgeonsHook();
            gameObject.SetActive(false);
        }
    }

    public sealed class CargoHatchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CrateRingPuzzle puzzle;
        public string InteractionPromptKey => "PROMPT_OPEN_CARGO_HATCH";
        public bool CanInteract => puzzle != null && !puzzle.IsCargoHatchOpen;
        public void Configure(CrateRingPuzzle target) { puzzle = target; }
        private void Start() { if (puzzle != null && puzzle.IsCargoHatchOpen) transform.localRotation = Quaternion.Euler(-78f, 0f, 0f); }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (CanInteract && puzzle.TryOpenCargoHatch())
                transform.localRotation = Quaternion.Euler(-78f, 0f, 0f);
        }
    }

    public sealed class CrateRingInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private CrateRingPuzzle puzzle;
        [SerializeField] private int ringIndex;
        [SerializeField] private bool correctRing = true;
        [SerializeField] private Vector3 targetPosition;
        private bool used;
        private Vector3 originalPosition;
        private bool hasOriginalPosition;
        public string InteractionPromptKey => "PROMPT_ALIGN_CARGO_CRATE";
        public bool CanInteract => puzzle != null && !used;
        public void Configure(CrateRingPuzzle target, int index, Vector3 destination, bool isCorrect = true) { puzzle = target; ringIndex = index; targetPosition = destination; correctRing = isCorrect; }
        private void Awake() { originalPosition = transform.position; hasOriginalPosition = true; }
        private void Start()
        {
            if (puzzle == null || ActProgressionManager.Instance == null) return;
            int restored = puzzle.AlignedCrateCount;
            if (ringIndex < restored)
            {
                used = true;
                transform.position = targetPosition;
            }
        }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            if (!hasOriginalPosition) originalPosition = transform.position;
            int failuresBefore = puzzle.FailureCount;
            int alignedBefore = puzzle.AlignedCrateCount;
            bool solved = puzzle.MoveCrateIntoRing(ringIndex, correctRing);
            if (!correctRing || puzzle.FailureCount > failuresBefore || puzzle.AlignedCrateCount == alignedBefore)
            {
                // Failed alignment remains retryable; never consume a required crate on a mistake.
                return;
            }
            // A valid first crate is not necessarily the whole puzzle solution; it still counts as a valid move.
            used = true;
            StartCoroutine(SlideIntoRing());
        }
        private IEnumerator SlideIntoRing()
        {
            Vector3 from = transform.position;
            float t = 0f;
            while (t < 0.42f)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.42f));
                transform.position = Vector3.Lerp(from, targetPosition, k);
                yield return null;
            }
            transform.position = targetPosition;
        }
    }

    public sealed class UndertowBasicHUD : MonoBehaviour
    {
        [SerializeField] private PlayerInteractionSystem interaction;
        [SerializeField] private LanternController lantern;
        [SerializeField] private HiddenSanitySystem sanity;
        [SerializeField] private ListenMechanicController listen;
        [SerializeField] private ObjectiveJournalSystem objectives;
        [SerializeField] private DialogueSubtitleSystem dialogue;
        private GUIStyle labelStyle;
        private GUIStyle promptStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private string activeSubtitle;
        private float subtitleExpiry;

        private void Awake()
        {
            if (interaction == null) interaction = GetComponentInChildren<PlayerInteractionSystem>();
            if (lantern == null) lantern = GetComponentInChildren<LanternController>();
            if (sanity == null) sanity = FindObjectOfType<HiddenSanitySystem>();
            if (listen == null) listen = FindObjectOfType<ListenMechanicController>();
            if (objectives == null) objectives = FindObjectOfType<ObjectiveJournalSystem>();
            if (dialogue == null) dialogue = FindObjectOfType<DialogueSubtitleSystem>();
        }

        private void OnEnable()
        {
            if (dialogue != null) dialogue.OnSubtitleDisplayed += HandleSubtitle;
        }

        private void OnDisable()
        {
            if (dialogue != null) dialogue.OnSubtitleDisplayed -= HandleSubtitle;
        }

        private void HandleSubtitle(string speaker, string text, float duration)
        {
            activeSubtitle = string.IsNullOrWhiteSpace(speaker) ? text : speaker + ":  " + text;
            subtitleExpiry = Time.unscaledTime + Mathf.Clamp(duration, 0.5f, 12f);
        }

        private void Update()
        {
            if (sanity != null && lantern != null)
                sanity.TickSanity(Time.deltaTime, lantern.IsLit, false, false);
            if (subtitleExpiry > 0f && Time.unscaledTime >= subtitleExpiry) activeSubtitle = null;
        }

        private void EnsureStyles(int scale)
        {
            if (labelStyle != null && labelStyle.fontSize == 14 * scale) return;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 * scale, normal = { textColor = new Color(0.78f, 0.88f, 0.91f) } };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 * scale, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.88f, 0.82f, 0.68f) } };
            promptStyle = new GUIStyle(GUI.skin.label) { fontSize = 17 * scale, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.78f, 0.43f) } };
            subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18 * scale, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = Color.white, background = null } };
        }

        private void OnGUI()
        {
            int scale = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 1080f), 1, 3);
            EnsureStyles(scale);
            float pad = 24f * scale;
            GUI.Label(new Rect(pad, pad * .55f, 540f * scale, 34f * scale), "UNDERTOW  /  ECHOES OF VANESSA", titleStyle);
            CampaignState state = ActProgressionManager.Instance != null ? ActProgressionManager.Instance.State : null;
            string act = state != null ? state.CurrentAct.ToString().Replace("_", " ") : "THE CURSED GALLEON";
            GUI.Label(new Rect(pad, pad * 1.85f, 500f * scale, 26f * scale), act.ToUpperInvariant(), labelStyle);

            if (lantern != null)
            {
                float barWidth = 235f * scale;
                GUI.Label(new Rect(pad, pad * 3.05f, barWidth, 24f * scale), "LANTERN OIL  " + Mathf.CeilToInt(lantern.FuelNormalized * 100f) + "%", labelStyle);
                GUI.color = new Color(0.12f, 0.14f, 0.15f, 0.88f);
                GUI.Box(new Rect(pad, pad * 4.05f, barWidth, 8f * scale), GUIContent.none);
                GUI.color = new Color(1f, 0.48f, 0.17f, 0.95f);
                GUI.Box(new Rect(pad, pad * 4.05f, barWidth * lantern.FuelNormalized, 8f * scale), GUIContent.none);
                GUI.color = Color.white;
            }
            if (objectives != null && dialogue != null)
            {
                string objective = dialogue.Localize(objectives.CurrentObjectiveKey);
                GUI.Label(new Rect(pad, pad * 5.15f, 760f * scale, 52f * scale), objective, labelStyle);
            }
            if (listen != null && listen.IsListening)
            {
                GUI.color = new Color(0.36f, 0.88f, 0.91f);
                GUI.Label(new Rect(Screen.width - 205f * scale, pad, 180f * scale, 30f * scale), "◉  LISTENING", titleStyle);
                GUI.color = Color.white;
            }
            if (interaction != null && interaction.CurrentFocus != null && interaction.CurrentFocus.CanInteract)
                GUI.Label(new Rect(Screen.width * 0.5f - 260f * scale, Screen.height * 0.68f, 520f * scale, 42f * scale), "[ E ]  " + (dialogue != null ? dialogue.Localize(interaction.CurrentFocus.InteractionPromptKey) : interaction.CurrentFocus.InteractionPromptKey), promptStyle);
            GUI.Label(new Rect(Screen.width * 0.5f - 12f * scale, Screen.height * 0.5f - 14f * scale, 24f * scale, 28f * scale), "+", promptStyle);
            if (!string.IsNullOrEmpty(activeSubtitle))
            {
                Rect subtitleRect = new Rect(Screen.width * 0.18f, Screen.height * 0.79f, Screen.width * 0.64f, 76f * scale);
                GUI.color = new Color(0.015f, 0.025f, 0.035f, 0.86f);
                GUI.Box(subtitleRect, GUIContent.none);
                GUI.color = Color.white;
                GUI.Label(new Rect(subtitleRect.x + 12f * scale, subtitleRect.y + 4f * scale, subtitleRect.width - 24f * scale, subtitleRect.height - 8f * scale), activeSubtitle, subtitleStyle);
            }
            GUI.Label(new Rect(pad, Screen.height - 38f * scale, Screen.width - pad * 2f, 28f * scale), "WASD MOVE     MOUSE LOOK     E INTERACT     F LANTERN     Q LISTEN     SHIFT CAUTIOUS     C CROUCH     ESC MENU", labelStyle);
        }
    }
}
