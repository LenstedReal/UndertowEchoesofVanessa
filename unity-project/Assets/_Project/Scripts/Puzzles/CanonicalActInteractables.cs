using System;
using UnityEngine;
using Undertow.Core;
using Undertow.Player;
using Undertow.Lantern;

namespace Undertow.Puzzles
{
    public sealed class LanternCountBellInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private LanternCountBellPuzzle puzzle;
        [SerializeField] private BellNote note = BellNote.La;
        public string InteractionPromptKey => "PROMPT_GRAVEYARD_BELL_" + note.ToString().ToUpperInvariant();
        public bool CanInteract => puzzle != null && !puzzle.IsSolved;
        public void Configure(LanternCountBellPuzzle target, BellNote value) { puzzle = target; note = value; }
        public void OnInteract(PlayerInteractionSystem interactor) { if (CanInteract) puzzle.StrikeBellForStage(note); }
    }

    public sealed class VoiceRouteChoiceInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private LanternCountBellPuzzle puzzle;
        [SerializeField] private bool followsEcho = true;
        private bool used;
        public string InteractionPromptKey => followsEcho ? "PROMPT_FOLLOW_ECHO" : "PROMPT_FOLLOW_MIMIC";
        public bool CanInteract => puzzle != null && puzzle.IsSolved && !puzzle.RouteChoiceCommitted && !used;
        public void Configure(LanternCountBellPuzzle target, bool echo) { puzzle = target; followsEcho = echo; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            puzzle.FollowVoiceRoute(followsEcho);
            used = true;
            gameObject.SetActive(false);
        }
    }

    public sealed class ActExitInteractable : MonoBehaviour, IInteractable
    {
        public string InteractionPromptKey => "PROMPT_CONTINUE_TO_NEXT_ACT";
        public bool CanInteract => ActProgressionManager.Instance != null && ActProgressionManager.Instance.CanAdvanceFromCurrentAct();
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (CanInteract) ActProgressionManager.Instance.TryAdvanceAct(true);
        }
    }

    public sealed class ClockDialInteractable : MonoBehaviour, IInteractable
    {
        private static readonly string[] Options = { "12:00", "11:40", "03:17", "06:05" };
        [SerializeField] private ClockTowerPuzzle puzzle;
        [SerializeField, Range(0, 2)] private int dialIndex;
        private int optionIndex;
        public string InteractionPromptKey => "PROMPT_CLOCK_DIAL_" + (dialIndex + 1);
        public bool CanInteract => puzzle != null && !puzzle.IsSolved;
        public void Configure(ClockTowerPuzzle target, int index) { puzzle = target; dialIndex = Mathf.Clamp(index, 0, 2); }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            optionIndex = (optionIndex + 1) % Options.Length;
            string value = Options[optionIndex];
            string[] parts = value.Split(':');
            puzzle.SetDialTime(dialIndex, int.Parse(parts[0]), int.Parse(parts[1]));
            transform.Rotate(Vector3.forward, -90f, Space.Self);
            bool allCorrect = true;
            string[] canonical = CanonicalGameData.ActIIIClockTowerCanonicalTimes;
            for (int i = 0; i < 3; i++) allCorrect &= puzzle.GetDialTime(i) == canonical[i];
            if (allCorrect) puzzle.SubmitClockConfiguration();
        }
    }

    public sealed class CraneLedgerInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private MirrorRoomChoiceSequence storySequence;
        private bool read;
        public string InteractionPromptKey => "PROMPT_READ_CRANE_LEDGER";
        public bool CanInteract => storySequence != null && !read;
        public void Configure(MirrorRoomChoiceSequence target) { storySequence = target; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            storySequence.DiscoverCraneFamilyLedger();
            read = true;
            gameObject.SetActive(false);
        }
    }

    public sealed class MirrorRoomDecisionInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private MirrorRoomChoiceSequence storySequence;
        [SerializeField] private bool answerCall;
        private bool committed;
        public string InteractionPromptKey => answerCall ? "PROMPT_ANSWER_VANESSA" : "PROMPT_REFUSE_VANESSA";
        public bool CanInteract => storySequence != null && storySequence.CanCommitChoice && !committed;
        public void Configure(MirrorRoomChoiceSequence target, bool answer) { storySequence = target; answerCall = answer; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            storySequence.CommitMirrorRoomDecision(answerCall);
            committed = true;
            gameObject.SetActive(false);
        }
    }

    public sealed class TempleMirrorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private SunkenTempleMirrorPrismPuzzle puzzle;
        [SerializeField, Range(0, 3)] private int mirrorIndex;
        private int turns;
        public string InteractionPromptKey => "PROMPT_ROTATE_TEMPLE_MIRROR_" + (mirrorIndex + 1);
        public bool CanInteract => puzzle != null && !puzzle.IsSolved;
        public void Configure(SunkenTempleMirrorPrismPuzzle target, int index) { puzzle = target; mirrorIndex = Mathf.Clamp(index, 0, 3); }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            turns = (turns + 1) % 4;
            puzzle.RotateMirror(mirrorIndex, 1);
            transform.Rotate(Vector3.up, 90f, Space.Self);
        }
    }

    public sealed class TempleComponentPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private SunkenTempleMirrorPrismPuzzle puzzle;
        [SerializeField] private bool isPrism;
        private bool collected;
        public string InteractionPromptKey => isPrism ? "PROMPT_PICKUP_GLASS_PRISM" : "PROMPT_PICKUP_FOCUSING_LENS";
        public bool CanInteract => puzzle != null && !collected;
        public void Configure(SunkenTempleMirrorPrismPuzzle target, bool prism) { puzzle = target; isPrism = prism; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            if (isPrism) puzzle.InstallGlassPrism(); else puzzle.InstallFocusingLens();
            collected = true;
            gameObject.SetActive(false);
        }
    }

    public sealed class FinalAltarInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private SunkenTempleMirrorPrismPuzzle puzzle;
        [SerializeField] private LullabyChoice choice = LullabyChoice.EchoIncorrect;
        private bool committed;
        public string InteractionPromptKey => choice == LullabyChoice.EchoIncorrect ? "PROMPT_CHOOSE_ECHO_LULLABY" : "PROMPT_CHOOSE_MIMIC_LULLABY";
        public bool CanInteract => puzzle != null && !committed;
        public void Configure(SunkenTempleMirrorPrismPuzzle target, LullabyChoice value) { puzzle = target; choice = value; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (!CanInteract) return;
            LanternController lantern = interactor != null ? interactor.GetComponentInChildren<LanternController>() : FindObjectOfType<LanternController>();
            if (!puzzle.TryChannelLanternBeamThroughAltar(lantern != null && lantern.IsLit)) return;
            EndingType ending = puzzle.CommitFinalAltarLullabyChoice(choice);
            committed = true;
            Debug.Log("[Undertow] Final lullaby choice committed. Ending=" + ending);
            if (ActProgressionManager.Instance != null) ActProgressionManager.Instance.TryAdvanceAct(true);
        }
    }

    public sealed class StoryFlagPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string collectibleId = "VANESSA_DRAWING_01";
        [SerializeField] private bool isDrawing = true;
        private bool collected;
        public string InteractionPromptKey => isDrawing ? "PROMPT_INSPECT_VANESSA_DRAWING" : "PROMPT_READ_JOURNAL";
        public bool CanInteract => !collected;
        public void Configure(string id, bool drawing) { collectibleId = id; isDrawing = drawing; }
        public void OnInteract(PlayerInteractionSystem interactor)
        {
            if (collected || ActProgressionManager.Instance == null) return;
            bool registered = isDrawing
                ? ActProgressionManager.Instance.RegisterVanessaDrawing(collectibleId)
                : ActProgressionManager.Instance.RegisterJournalEntry(collectibleId);
            if (registered) { collected = true; gameObject.SetActive(false); }
        }
    }
}
