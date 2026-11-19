using System.Collections.Generic;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    [DefaultExecutionOrder(-100)]
    public class CrateRingPuzzle : BasePuzzleController
    {
        [SerializeField] private int requiredCratesInRings = 2;
        [SerializeField] private bool requiresSurgeonsHookForHatch = true;

        private bool hasSurgeonsHook;
        private bool isCargoHatchOpen;
        private readonly HashSet<int> occupiedRingIndices = new HashSet<int>();

        public bool HasSurgeonsHook => hasSurgeonsHook;
        public bool IsCargoHatchOpen => isCargoHatchOpen;
        public int AlignedCrateCount => occupiedRingIndices.Count;

        public void ConfigureForAct(GameAct act)
        {
            if (act == GameAct.ActI_CursedGalleon)
            {
                puzzleId = "ACT1_SURGEON_HOOK_AND_STARBOARD_CRATES";
                requiredCratesInRings = 2;
                requiresSurgeonsHookForHatch = true;
            }
            else if (act == GameAct.ActII_ShipGraveyard)
            {
                puzzleId = "ACT2_GRAVEYARD_CRATE_RING";
                requiredCratesInRings = 1;
                requiresSurgeonsHookForHatch = false;
                isCargoHatchOpen = true;
            }
        }

        private void Start()
        {
            if (ActProgressionManager.Instance != null)
            {
                CampaignState state = ActProgressionManager.Instance.State;
                hasSurgeonsHook = state.HasSurgeonsHook;
                isCargoHatchOpen = !requiresSurgeonsHookForHatch || state.ActICargoHatchOpened;
                int restoredCount = requiredCratesInRings == 2 ? Mathf.Clamp(state.ActIStarboardCratesAligned, 0, 2) : (state.ActIICrateRingAligned ? 1 : 0);
                occupiedRingIndices.Clear();
                for (int i = 0; i < restoredCount; i++) occupiedRingIndices.Add(i);
                if (isCargoHatchOpen && restoredCount >= requiredCratesInRings) isSolved = true;
            }
        }

        public void AcquireSurgeonsHook()
        {
            hasSurgeonsHook = true;
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.State.HasSurgeonsHook = true;
        }

        public bool TryOpenCargoHatch()
        {
            if (requiresSurgeonsHookForHatch && !hasSurgeonsHook)
            {
                RecordFailure();
                return false;
            }

            isCargoHatchOpen = true;
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.State.ActICargoHatchOpened = true;
            EvaluateCompletion();
            return true;
        }

        public bool MoveCrateIntoRing(int ringIndex, bool isCorrectRing = true)
        {
            if (ringIndex < 0 || ringIndex >= requiredCratesInRings)
            {
                RecordFailure();
                return false;
            }
            if (requiresSurgeonsHookForHatch && !isCargoHatchOpen)
            {
                RecordFailure();
                return false;
            }
            if (!isCorrectRing)
            {
                RecordFailure();
                return false;
            }

            occupiedRingIndices.Add(ringIndex);
            if (ActProgressionManager.Instance != null)
            {
                if (requiredCratesInRings == 2)
                    ActProgressionManager.Instance.State.ActIStarboardCratesAligned = occupiedRingIndices.Count;
                else
                    ActProgressionManager.Instance.State.ActIICrateRingAligned = occupiedRingIndices.Count >= requiredCratesInRings;
            }
            return EvaluateCompletion();
        }

        private bool EvaluateCompletion()
        {
            bool hatchReady = !requiresSurgeonsHookForHatch || isCargoHatchOpen;
            if (hatchReady && occupiedRingIndices.Count >= requiredCratesInRings)
            {
                MarkSolved();
                return true;
            }
            return false;
        }

        public override void ResetPuzzle()
        {
            occupiedRingIndices.Clear();
            if (requiresSurgeonsHookForHatch)
                isCargoHatchOpen = false;
            base.ResetPuzzle();
        }
    }
}
