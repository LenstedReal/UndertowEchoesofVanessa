using System;

namespace Undertow.Logic
{
    /// <summary>The only warm light (#FF9227). Burns oil while lit; goes out when empty.</summary>
    public sealed class LanternModel
    {
        public float Oil { get; private set; }
        public bool Lit { get; private set; }
        public int RunOuts { get; private set; }

        public LanternModel(float oil = Balance.StartOil, bool lit = true)
        {
            Oil = MathX.Clamp(oil, 0f, Balance.OilMax);
            Lit = lit && Oil > 0f;
        }

        /// <summary>Returns true if the flame died this tick because the oil ran out.</summary>
        public bool Tick(float dt, bool sprinting)
        {
            if (!Lit || dt <= 0f) return false;
            float burn = Balance.OilBurn + (sprinting ? Balance.OilSprint : 0f);
            Oil = Math.Max(0f, Oil - burn * dt);
            if (Oil <= 0f)
            {
                Lit = false;
                RunOuts++;
                return true;
            }
            return false;
        }

        public bool TryLight()
        {
            if (Oil <= 0f) return false;
            Lit = true;
            return true;
        }

        public void Extinguish() => Lit = false;

        /// <summary>Adds oil, capped at OilMax. Returns the amount actually added.</summary>
        public float AddOil(float amount)
        {
            if (amount <= 0f) return 0f;
            float before = Oil;
            Oil = Math.Min(Balance.OilMax, Oil + amount);
            return Oil - before;
        }

        /// <summary>Spends oil for something other than burning (Act IV braziers). False if not enough.</summary>
        public bool Spend(float amount)
        {
            if (amount <= 0f) return true;
            if (Oil < amount) return false;
            Oil -= amount;
            if (Oil <= 0f) { Oil = 0f; Lit = false; }
            return true;
        }

        public bool Low => Oil < Balance.OilLowWarning;
        public bool Critical => Oil < Balance.OilCritical;

        /// <summary>Flame strength multiplier: full until critical, then sputters down.</summary>
        public float Strength01 => !Lit ? 0f : (Oil >= Balance.OilCritical ? 1f : 0.35f + 0.65f * (Oil / Balance.OilCritical));

        /// <summary>Seconds of light left at walking pace.</summary>
        public float SecondsLeft => Oil / Balance.OilBurn;

        public void Restore(float oil, bool lit)
        {
            Oil = MathX.Clamp(oil, 0f, Balance.OilMax);
            Lit = lit && Oil > 0f;
        }
    }
}
