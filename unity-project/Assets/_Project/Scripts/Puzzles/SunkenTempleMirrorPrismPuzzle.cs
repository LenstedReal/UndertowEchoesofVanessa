using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    public class SunkenTempleMirrorPrismPuzzle : BasePuzzleController
    {
        private readonly int[] currentMirrorRotations = new int[4] { 0, 0, 0, 0 };

        public bool HasGlassPrism { get; private set; }
        public bool HasFocusingLens { get; private set; }
        public bool IsAltarBeamActive => isSolved && HasGlassPrism && HasFocusingLens;

        private void Awake() { puzzleId = "ACT4_SUNKEN_TEMPLE_FOUR_MIRRORS"; }
        private void Start()
        {
            if (ActProgressionManager.Instance == null) return;
            CampaignState state = ActProgressionManager.Instance.State;
            HasGlassPrism = state.HasGlassPrism;
            HasFocusingLens = state.HasFocusingLens;
            isSolved = state.ActIVMirrorsAligned;
            if (isSolved)
                for (int i = 0; i < 4; i++) currentMirrorRotations[i] = CanonicalGameData.ActIVMirrorRotationCounts[i];
        }

        public void RotateMirror(int mirrorIndex, int steps = 1)
        {
            if (mirrorIndex < 0 || mirrorIndex >= 4)
                throw new ArgumentOutOfRangeException(nameof(mirrorIndex));

            currentMirrorRotations[mirrorIndex] = (currentMirrorRotations[mirrorIndex] + steps) % 4;
            if (currentMirrorRotations[mirrorIndex] < 0)
                currentMirrorRotations[mirrorIndex] += 4;
        }

        public int GetMirrorRotationCount(int mirrorIndex) => currentMirrorRotations[mirrorIndex];

        public void InstallGlassPrism()
        {
            HasGlassPrism = true;
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.State.HasGlassPrism = true;
        }

        public void InstallFocusingLens()
        {
            HasFocusingLens = true;
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.State.HasFocusingLens = true;
        }

        public bool TryChannelLanternBeamThroughAltar(bool isLanternLit)
        {
            if (!isLanternLit || !HasGlassPrism || !HasFocusingLens)
            {
                RecordFailure();
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                if (currentMirrorRotations[i] != CanonicalGameData.ActIVMirrorRotationCounts[i])
                {
                    RecordFailure();
                    return false;
                }
            }

            MarkSolved();
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.State.ActIVMirrorsAligned = true;
            }
            return true;
        }

        public EndingType CommitFinalAltarLullabyChoice(LullabyChoice choice)
        {
            if (ActProgressionManager.Instance != null)
            {
                if (ActProgressionManager.Instance.State.FinalAltarLullabyChoice == LullabyChoice.None)
                    ActProgressionManager.Instance.State.FinalAltarLullabyChoice = choice;
                return EndingEvaluator.EvaluateEnding(ActProgressionManager.Instance.State);
            }
            return EndingType.Undertow;
        }

        public override void ResetPuzzle()
        {
            for (int i = 0; i < 4; i++) currentMirrorRotations[i] = 0;
            base.ResetPuzzle();
        }
    }
}
