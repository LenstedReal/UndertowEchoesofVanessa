using System.Collections.Generic;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    public class LanternCountBellPuzzle : BasePuzzleController
    {
        [SerializeField] private int[] shipwreckLanternCounts = new int[] { 1, 2, 3, 1 };
        private readonly List<BellNote> enteredNotes = new List<BellNote>();

        public bool EnvironmentalTrapTriggered { get; private set; }
        public bool SafeEchoRouteUnlocked { get; private set; }
        public bool RouteChoiceCommitted { get; private set; }

        private void Awake() { puzzleId = "ACT2_LANTERN_COUNT_BELL_PUZZLE"; }

        private void Start()
        {
            if (ActProgressionManager.Instance == null) return;
            CampaignState state = ActProgressionManager.Instance.State;
            isSolved = state.ActIILanternBellSolved;
            SafeEchoRouteUnlocked = state.FollowedEchoSafeRouteInActII;
            EnvironmentalTrapTriggered = state.TriggeredMimicTrapInActII;
            RouteChoiceCommitted = SafeEchoRouteUnlocked || EnvironmentalTrapTriggered;
            if (isSolved && shipwreckLanternCounts != null)
                for (int i = 0; i < shipwreckLanternCounts.Length; i++) enteredNotes.Add(CanonicalGameData.LanternCountToBellNote(shipwreckLanternCounts[i]));
        }

        public void SetLanternCountPattern(int[] counts)
        {
            if (counts != null && counts.Length > 0)
                shipwreckLanternCounts = counts;
            enteredNotes.Clear();
        }

        public bool FollowVoiceRoute(bool followedEchoTrueVanessa)
        {
            if (!isSolved || RouteChoiceCommitted) return false;
            RouteChoiceCommitted = true;
            if (followedEchoTrueVanessa)
            {
                SafeEchoRouteUnlocked = true;
                if (ActProgressionManager.Instance != null)
                    ActProgressionManager.Instance.State.FollowedEchoSafeRouteInActII = true;
                return true;
            }
            else
            {
                EnvironmentalTrapTriggered = true;
                RecordFailure();
                if (ActProgressionManager.Instance != null)
                    ActProgressionManager.Instance.State.TriggeredMimicTrapInActII = true;
                return false;
            }
        }

        public bool StrikeBellForStage(BellNote note)
        {
            if (isSolved) return true;

            int stageIndex = enteredNotes.Count;
            if (shipwreckLanternCounts == null || shipwreckLanternCounts.Length == 0) return false;
            if (stageIndex >= shipwreckLanternCounts.Length) return isSolved;
            BellNote expected = CanonicalGameData.LanternCountToBellNote(shipwreckLanternCounts[stageIndex]);
            if (note != expected)
            {
                RecordFailure();
                enteredNotes.Clear();
                return false;
            }

            enteredNotes.Add(note);
            if (enteredNotes.Count == shipwreckLanternCounts.Length)
            {
                MarkSolved();
                if (ActProgressionManager.Instance != null)
                    ActProgressionManager.Instance.State.ActIILanternBellSolved = true;
                return true;
            }
            return false;
        }

        public override void ResetPuzzle()
        {
            enteredNotes.Clear();
            EnvironmentalTrapTriggered = false;
            SafeEchoRouteUnlocked = false;
            RouteChoiceCommitted = false;
            base.ResetPuzzle();
        }
    }
}
