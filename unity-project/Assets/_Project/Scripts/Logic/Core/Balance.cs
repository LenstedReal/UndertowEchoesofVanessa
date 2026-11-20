namespace Undertow.Logic
{
    /// <summary>
    /// Every tunable number of the design in one place (design doc "Sistemler" table).
    /// Runtime code must read these values; tests check the design values.
    /// </summary>
    public static class Balance
    {
        // Hidden sanity (0..100), never shown on screen.
        public const float SanityMax = 100f;
        public const float SanityStart = 82f;
        public const float DarkDrain = 0.8f;        // per second, no light at all
        public const float MoonDrain = 0.22f;       // per second, only moonlight / weak light
        public const float SeaDrain = 1.2f;         // per second, after staring at the sea longer than SeaDelay
        public const float SeaDelay = 6f;           // seconds
        public const float SafeRegen = 1f;          // per second inside safe light
        public const float ExamineCost = 4f;        // one disturbing object
        public const float MusicReward = 15f;       // the music box
        public const float Hysteresis = 3f;         // points of buffer between tiers

        // Tier thresholds: value >= threshold keeps (or reaches) the tier above.
        public const float UneasyBelow = 70f;
        public const float DisturbedBelow = 45f;
        public const float BreakingBelow = 20f;

        // Lantern oil (0..100).
        public const float OilMax = 100f;
        public const float StartOil = 64f;
        public const float OilBurn = 0.075f;        // per second while lit
        public const float OilSprint = 0.05f;       // extra per second while sprinting with the lantern lit
        public const float OilFlask = 42f;          // standard flask
        public const float OilLowWarning = 22f;
        public const float OilCritical = 8f;
        public const float BrazierCost = 20f;       // Act IV: oil poured into a temple brazier

        // Environment distortion (only while the object is out of view).
        public const float DistortUneasy = 0.05f;   // metres
        public const float DistortDisturbed = 0.35f;

        // Player.
        public const float WalkSpeed = 2.6f;
        public const float SprintSpeed = 4.3f;
        public const float CrouchSpeed = 1.4f;
        public const float WadeFactor = 0.72f;      // knee-deep water in Act III
        public const float InteractRange = 2.3f;
        public const float PlayerRadius = 0.3f;
        public const float PlayerHeight = 1.75f;
        public const float StepHeight = 0.36f;

        // Puzzles.
        public const float HintFirstSeconds = 150f; // first diegetic hint after this long on one puzzle
        public const float HintStepSeconds = 120f;  // each further hint
        public const int HintFailuresPerStep = 3;   // or after this many failed attempts

        // Undertow presence (Act IV): it approaches in darkness, it never strikes.
        public const float PresenceRiseDark = 0.09f;   // per second in darkness
        public const float PresenceFallLight = 0.25f;  // per second in light
        public const float PresenceDisplace = 1f;      // at this level the player is pulled back to the last light

        // Audio dynamics.
        public const float SilenceBeforeKeyMoment = 2f; // seconds of ambience cut before a key moment
    }
}
