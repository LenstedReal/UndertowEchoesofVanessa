using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    public class MirrorRoomChoiceSequence : MonoBehaviour
    {
        public bool CraneBloodlineRevealed { get; private set; }
        public bool ChoiceCommitted { get; private set; }
        public bool AnsweredVanessaCall { get; private set; }
        [SerializeField] private ClockTowerPuzzle clockTowerPuzzle;
        public bool CanCommitChoice => CraneBloodlineRevealed && clockTowerPuzzle != null && clockTowerPuzzle.IsSolved && !ChoiceCommitted;
        public void Configure(ClockTowerPuzzle clockPuzzle) { clockTowerPuzzle = clockPuzzle; }
        private void Start()
        {
            if (ActProgressionManager.Instance == null) return;
            CampaignState state = ActProgressionManager.Instance.State;
            CraneBloodlineRevealed = state.DiscoveredCraneBloodline;
            ChoiceCommitted = state.MirrorRoomChoiceMade;
            AnsweredVanessaCall = state.AnsweredVanessaInMirrorRoom;
        }

        public event Action<bool> OnMirrorChoiceCommitted;

        public void DiscoverCraneFamilyLedger()
        {
            CraneBloodlineRevealed = true;
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.State.DiscoveredCraneBloodline = true;
        }

        public void CommitMirrorRoomDecision(bool answerVanessaCall)
        {
            if (!CanCommitChoice) return;
            ChoiceCommitted = true;
            AnsweredVanessaCall = answerVanessaCall;

            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.State.MirrorRoomChoiceMade = true;
                ActProgressionManager.Instance.State.AnsweredVanessaInMirrorRoom = answerVanessaCall;
            }

            OnMirrorChoiceCommitted?.Invoke(answerVanessaCall);
        }
    }
}
