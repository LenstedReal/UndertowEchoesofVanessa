using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public enum EchoPath { None = 0, Echo = 1, Mimic = 2 }
    public enum AltarChoice { None = 0, Echo = 1, Mimic = 2 }
    public enum EndingId { None = 0, Answer = 1, Surface = 2, Undertow = 3 }

    /// <summary>
    /// The whole mutable state of a playthrough. Saved at checkpoints, restored on load.
    /// Pure data: no Unity types.
    /// </summary>
    public sealed class GameState
    {
        public const int CurrentVersion = 3;

        public int Version = CurrentVersion;
        public int Act = 1;
        public string Checkpoint = "start";
        public readonly HashSet<string> Flags = new HashSet<string>(StringComparer.Ordinal);
        public readonly List<string> NotesRead = new List<string>();
        public readonly List<int> Drawings = new List<int>();
        public readonly Dictionary<string, int> Ints = new Dictionary<string, int>(StringComparer.Ordinal);

        public float Oil = Balance.StartOil;
        public float Sanity = Balance.SanityStart;
        public bool LanternLit = true;
        public double PlaySeconds;

        public EchoPath Act2Path = EchoPath.None;
        public int Answered = -1;           // -1 not yet, 0 stayed silent, 1 answered
        public AltarChoice Altar = AltarChoice.None;
        public EndingId Ending = EndingId.None;

        // Stats for achievements and the end screen.
        public int Listens;
        public int Examines;
        public int BellsRung;
        public int OilRunOuts;
        public int WorstTier;               // highest SanityTier reached (0..3)
        public int ActWorstTier;            // reset at every act start
        public int ActOilRunOuts;           // reset at every act start
        public int BraziersLit;

        public bool Has(string flag) => !string.IsNullOrEmpty(flag) && Flags.Contains(flag);

        public bool HasAll(IEnumerable<string> flags)
        {
            if (flags == null) return true;
            foreach (var f in flags) if (!Has(f)) return false;
            return true;
        }

        public bool Set(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return false;
            return Flags.Add(flag);
        }

        public void Clear(string flag)
        {
            if (!string.IsNullOrEmpty(flag)) Flags.Remove(flag);
        }

        public bool MarkNote(string noteId)
        {
            if (string.IsNullOrEmpty(noteId) || NotesRead.Contains(noteId)) return false;
            NotesRead.Add(noteId);
            return true;
        }

        public bool AddDrawing(int index)
        {
            if (index < 1 || index > 6 || Drawings.Contains(index)) return false;
            Drawings.Add(index);
            Drawings.Sort();
            return true;
        }

        public int DrawingCount => Drawings.Count;

        public int GetInt(string key, int fallback = 0) => Ints.TryGetValue(key, out var v) ? v : fallback;
        public void SetInt(string key, int value) => Ints[key] = value;

        public bool AnsweredInAct3 => Answered == 1;

        public void BeginAct(int act)
        {
            Act = act;
            ActWorstTier = 0;
            ActOilRunOuts = 0;
        }

        public GameState Clone()
        {
            var c = new GameState
            {
                Version = Version,
                Act = Act,
                Checkpoint = Checkpoint,
                Oil = Oil,
                Sanity = Sanity,
                LanternLit = LanternLit,
                PlaySeconds = PlaySeconds,
                Act2Path = Act2Path,
                Answered = Answered,
                Altar = Altar,
                Ending = Ending,
                Listens = Listens,
                Examines = Examines,
                BellsRung = BellsRung,
                OilRunOuts = OilRunOuts,
                WorstTier = WorstTier,
                ActWorstTier = ActWorstTier,
                ActOilRunOuts = ActOilRunOuts,
                BraziersLit = BraziersLit,
            };
            foreach (var f in Flags) c.Flags.Add(f);
            c.NotesRead.AddRange(NotesRead);
            c.Drawings.AddRange(Drawings);
            foreach (var kv in Ints) c.Ints[kv.Key] = kv.Value;
            return c;
        }

        public override string ToString() =>
            $"Act {Act} cp={Checkpoint} oil={Oil:0.0} sanity={Sanity:0.0} flags=[{string.Join(",", Flags.OrderBy(x => x))}]";
    }
}
