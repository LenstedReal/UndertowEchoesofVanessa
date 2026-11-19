using System;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Puzzles
{
    public class ClockTowerPuzzle : BasePuzzleController
    {
        private readonly string[] currentDialTimes = new string[3] { "12:00", "12:00", "12:00" };

        private void Awake() { puzzleId = "ACT3_PORT_SAINT_CLAIR_CLOCK_TOWER"; }
        private void Start()
        {
            if (ActProgressionManager.Instance != null && ActProgressionManager.Instance.State.ActIIIClockTowerSolved)
            {
                isSolved = true;
                for (int i = 0; i < 3; i++) currentDialTimes[i] = CanonicalGameData.ActIIIClockTowerCanonicalTimes[i];
            }
        }

        public void SetDialTime(int dialIndex, int hours, int minutes)
        {
            if (dialIndex < 0 || dialIndex >= 3)
                throw new ArgumentOutOfRangeException(nameof(dialIndex));

            int h = Mathf.Clamp(hours, 0, 23);
            int m = Mathf.Clamp(minutes, 0, 59);
            currentDialTimes[dialIndex] = $"{h:D2}:{m:D2}";
        }

        public string GetDialTime(int dialIndex) => currentDialTimes[dialIndex];

        public bool SubmitClockConfiguration()
        {
            if (isSolved) return true;

            for (int i = 0; i < 3; i++)
            {
                if (!string.Equals(currentDialTimes[i], CanonicalGameData.ActIIIClockTowerCanonicalTimes[i], StringComparison.Ordinal))
                {
                    RecordFailure();
                    return false;
                }
            }

            MarkSolved();
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.State.ActIIIClockTowerSolved = true;
            }
            return true;
        }

        public override void ResetPuzzle()
        {
            currentDialTimes[0] = "12:00";
            currentDialTimes[1] = "12:00";
            currentDialTimes[2] = "12:00";
            base.ResetPuzzle();
        }
    }
}
