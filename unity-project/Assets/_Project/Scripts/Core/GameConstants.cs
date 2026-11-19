using System;
using System.Collections.Generic;

namespace Undertow.Core
{
    public enum GameAct
    {
        ActI_CursedGalleon = 1,
        ActII_ShipGraveyard = 2,
        ActIII_ForsakenPort = 3,
        ActIV_SunkenTemple = 4
    }

    public enum BellNote
    {
        Do = 0,
        Mi = 1,
        La = 2
    }

    public enum SanityState
    {
        Stable = 0,
        Uneasy = 1,
        Disturbed = 2,
        Breaking = 3
    }

    public enum LullabyChoice
    {
        None = 0,
        EchoIncorrect = 1, // "Hush, little side." - Vanessa's real surviving 9-year-old echo
        MimicCorrect = 2   // Undertow's older, polished imitation singing the lullaby correctly
    }

    public enum EndingType
    {
        None = 0,
        Answer = 1,
        Surface = 2,
        Undertow = 3
    }

    public enum SurfaceMaterialType
    {
        WetDeckWood = 0,
        IronGrating = 1,
        ShallowBilgeWater = 2,
        PortCobblestone = 3,
        SunkenTempleBasalt = 4
    }

    public static class CanonicalGameData
    {
        public const string ProtagonistName = "Adrian Crane";
        public const int ProtagonistAge = 32;
        public const string SisterName = "Vanessa Crane";
        public const int SisterDisappearanceAge = 9;
        public const int YearsSinceDisappearance = 15;
        public const int TotalVanessaDrawings = 6;
        public const int RequiredDrawingsForAnswerEnding = 4;

        public static readonly string[] CanonicalVanessaDrawingIds = new string[]
        {
            "DRAWING_01_GALLEON",
            "DRAWING_02_BELLS",
            "DRAWING_03_GRAVEYARD",
            "DRAWING_04_PORT",
            "DRAWING_05_CLOCKTOWER",
            "DRAWING_06_TEMPLE"
        };

        // Act I: Merciful Requiem Five-Bell Sequence (La - Mi - Do - Mi - La)
        public static readonly BellNote[] ActIFiveBellSequence = new BellNote[]
        {
            BellNote.La,
            BellNote.Mi,
            BellNote.Do,
            BellNote.Mi,
            BellNote.La
        };

        // Act II: Ship Graveyard Lantern-Count to Bell-Note Rule (1=La, 2=Do, 3=Mi)
        public static BellNote LanternCountToBellNote(int litLanternCount)
        {
            switch (litLanternCount)
            {
                case 1: return BellNote.La;
                case 2: return BellNote.Do;
                case 3: return BellNote.Mi;
                default:
                    throw new ArgumentOutOfRangeException(nameof(litLanternCount), "Lantern count must be 1, 2, or 3.");
            }
        }

        // Act III: Port Saint-Clair (1724) Canonical Clock Tower Values (03:17 canonical; 02:17 rejected)
        public static readonly string[] ActIIIClockTowerCanonicalTimes = new string[]
        {
            "11:40",
            "03:17",
            "06:05"
        };

        // Act IV: Sunken Temple Four-Mirror Rotation Counts (1 - 2 - 3 - 1)
        public static readonly int[] ActIVMirrorRotationCounts = new int[]
        {
            1, 2, 3, 1
        };

        public const string EchoLullabyLyric = "Hush, little side, don't say a word... the tide remembers what you heard.";
        public const string MimicLullabyLyric = "Hush, little child, don't say a word... come to the water and be heard.";

        public static bool TryNormalizeCollectibleId(string rawId, out string normalizedId)
        {
            normalizedId = string.Empty;
            if (string.IsNullOrWhiteSpace(rawId)) return false;
            normalizedId = rawId.Trim().ToUpperInvariant();
            return normalizedId.Length > 0;
        }

        public static int DeduplicateCollectibleList(List<string> list, int maxAllowed = int.MaxValue)
        {
            if (list == null) return 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cleaned = new List<string>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (TryNormalizeCollectibleId(list[i], out string norm) && seen.Add(norm))
                {
                    cleaned.Add(norm);
                    if (cleaned.Count >= maxAllowed) break;
                }
            }
            list.Clear();
            list.AddRange(cleaned);
            return list.Count;
        }
    }
}
