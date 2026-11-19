using System;
using UnityEngine;

namespace Undertow.Puzzles
{
    public interface IPuzzleController
    {
        string PuzzleId { get; }
        bool IsSolved { get; }
        int FailureCount { get; }
        void ResetPuzzle();
    }

    public abstract class BasePuzzleController : MonoBehaviour, IPuzzleController
    {
        [SerializeField] protected string puzzleId = "PUZZLE_BASE";

        protected bool isSolved;
        protected int failureCount;

        public string PuzzleId => puzzleId;
        public bool IsSolved => isSolved;
        public int FailureCount => failureCount;

        public event Action<string> OnPuzzleSolved;
        public event Action<string, int> OnPuzzleFailed;
        public event Action<string> OnPuzzleReset;

        protected void MarkSolved()
        {
            if (isSolved) return;
            isSolved = true;
            OnPuzzleSolved?.Invoke(puzzleId);
        }

        protected void RecordFailure()
        {
            failureCount++;
            OnPuzzleFailed?.Invoke(puzzleId, failureCount);
        }

        public virtual void ResetPuzzle()
        {
            if (isSolved) isSolved = false;
            OnPuzzleReset?.Invoke(puzzleId);
        }
    }
}
