using System.Collections.Generic;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    public class FiveBellPuzzle : BasePuzzleController
    {
        private readonly List<BellNote> inputSequence = new List<BellNote>(5);

        public IReadOnlyList<BellNote> CurrentInputSequence => inputSequence;

        private void Awake()
        {
            puzzleId = "ACT1_FIVE_BELL_PUZZLE";
        }

        private void Start()
        {
            if (ActProgressionManager.Instance != null && ActProgressionManager.Instance.State.ActIFiveBellSolved)
                isSolved = true;
        }

        public bool StrikeBell(BellNote struckNote)
        {
            if (isSolved) return true;

            // Ignore extra strikes after the expected sequence length. This prevents an
            // out-of-range read if input is spammed while the puzzle is resolving.
            if (inputSequence.Count >= CanonicalGameData.ActIFiveBellSequence.Length)
                return isSolved;

            inputSequence.Add(struckNote);
            int idx = inputSequence.Count - 1;

            if (inputSequence[idx] != CanonicalGameData.ActIFiveBellSequence[idx])
            {
                RecordFailure();
                inputSequence.Clear();
                return false;
            }

            if (inputSequence.Count == CanonicalGameData.ActIFiveBellSequence.Length)
            {
                MarkSolved();
                if (ActProgressionManager.Instance != null)
                {
                    ActProgressionManager.Instance.State.ActIFiveBellSolved = true;
                }
                return true;
            }

            return false;
        }

        public override void ResetPuzzle()
        {
            inputSequence.Clear();
            base.ResetPuzzle();
        }
    }
}
