using System;

namespace Undertow.Logic
{
    public enum SanityTier { Stable = 0, Uneasy = 1, Disturbed = 2, Breaking = 3 }

    /// <summary>What the world looks like to the player this frame.</summary>
    public struct SanityInput
    {
        public bool InSafeLight;    // safe room, chapel, lit brazier, music box glow
        public bool Dark;           // no light at all (lantern out, nothing nearby)
        public bool Moonlit;        // lantern out, but outdoors under the moon
        public bool StaringAtSea;   // camera is looking at open water
        public float Dt;
    }

    /// <summary>
    /// Hidden sanity, 0..100. Never shown on screen; it drives post effects, audio and distortion.
    /// Four tiers with a hysteresis buffer so effects never flicker at a boundary.
    /// </summary>
    public sealed class SanityModel
    {
        static readonly float[] Thresholds = { Balance.UneasyBelow, Balance.DisturbedBelow, Balance.BreakingBelow };

        public float Value { get; private set; }
        public SanityTier Tier { get; private set; }
        public float SeaTimer { get; private set; }

        public SanityModel(float start = Balance.SanityStart)
        {
            Value = MathX.Clamp(start, 0f, Balance.SanityMax);
            Tier = TierFor(Value, SanityTier.Stable, true);
        }

        /// <summary>Per-second rate for the given input (positive = recovering).</summary>
        public static float RateFor(in SanityInput i, float seaTimer)
        {
            float rate = LightRate(i);
            if (i.StaringAtSea && seaTimer >= Balance.SeaDelay) rate -= Balance.SeaDrain;
            return rate;
        }

        static float LightRate(in SanityInput i)
        {
            if (i.InSafeLight) return Balance.SafeRegen;
            if (i.Dark) return -Balance.DarkDrain;
            if (i.Moonlit) return -Balance.MoonDrain;
            return 0f;
        }

        /// <summary>Advances the model. Returns true when the tier changed.</summary>
        public bool Tick(in SanityInput i)
        {
            if (i.Dt <= 0f) return false;
            float before = SeaTimer;
            if (i.StaringAtSea) SeaTimer += i.Dt;
            else SeaTimer = Math.Max(0f, SeaTimer - i.Dt * 2f);
            // only the part of this tick that lies beyond the delay drains
            float seaSeconds = i.StaringAtSea ? Math.Max(0f, SeaTimer - Math.Max(before, Balance.SeaDelay)) : 0f;
            return Apply(LightRate(i) * i.Dt - Balance.SeaDrain * seaSeconds);
        }

        /// <summary>Instant change (examine a disturbing object, music box). Returns true when the tier changed.</summary>
        public bool Apply(float delta)
        {
            Value = MathX.Clamp(Value + delta, 0f, Balance.SanityMax);
            var t = TierFor(Value, Tier, false);
            if (t == Tier) return false;
            Tier = t;
            return true;
        }

        public void Set(float value)
        {
            Value = MathX.Clamp(value, 0f, Balance.SanityMax);
            Tier = TierFor(Value, SanityTier.Stable, true);
        }

        /// <summary>
        /// Tier with hysteresis: you drop a tier as soon as you fall under its threshold,
        /// but climb back only after recovering Hysteresis points above it.
        /// </summary>
        public static SanityTier TierFor(float value, SanityTier current, bool fresh)
        {
            if (fresh)
            {
                int t = 0;
                while (t < Thresholds.Length && value < Thresholds[t]) t++;
                return (SanityTier)t;
            }
            int tier = (int)current;
            while (tier < Thresholds.Length && value < Thresholds[tier]) tier++;
            while (tier > 0 && value >= Thresholds[tier - 1] + Balance.Hysteresis) tier--;
            return (SanityTier)tier;
        }

        /// <summary>0 at full sanity, 1 at zero. Smooth input for post effects.</summary>
        public float Stress01 => 1f - Value / Balance.SanityMax;
    }
}
