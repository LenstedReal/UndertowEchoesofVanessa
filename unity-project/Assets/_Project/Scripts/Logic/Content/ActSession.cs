using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public enum Outcome
    {
        None, NeedMissing, AlreadyDone,
        Note, Examine, Pickup, Drawing, Tape,
        Oil, OilFull,
        BellRung, BellWrong, BellSolved,
        ListenPlay,
        CrateMoved, CrateSolved, Reset,
        DialTurned, DialSolved,
        MirrorTurned, MirrorAligned, MirrorLocked,
        HatchOpened, Exit, Altar, Choice,
        BrazierLit, BrazierNoOil,
    }

    /// <summary>What happened when the player pressed Interact. The runtime turns it into sound, UI and animation.</summary>
    public sealed class InteractResult
    {
        public Outcome Outcome;
        public ItemDef Item;
        public string MessageKey;
        public float OilAdded;
        public float SanityDelta;
        public int Bell = -1;
        public int[] Sequence;
        public int Slot = -1;
        public int Dial = -1;
        public int BeamPrefix = -1;
        public bool Checkpoint;
        public bool Solved;
        public readonly List<string> FlagsSet = new List<string>();
        public readonly List<string> Achievements = new List<string>();

        public bool Success => Outcome != Outcome.None && Outcome != Outcome.NeedMissing && Outcome != Outcome.AlreadyDone
                               && Outcome != Outcome.OilFull && Outcome != Outcome.BrazierNoOil && Outcome != Outcome.MirrorLocked;
        public override string ToString() => $"{Outcome} {Item?.Id} flags=[{string.Join(",", FlagsSet)}]";
    }

    public enum PuzzleKind { Sequence, Single, Crates, Dials, Mirrors, Listen, Vision, Altar }

    public sealed class PuzzleDef
    {
        public string Id;
        public PuzzleKind Kind;
        public int[] Sequence;
        public string Flag;
        public float Reward;
        public string Achievement;
        public string Room;
    }

    /// <summary>
    /// Rules of one act: interactions, puzzles, goals, hints, checkpoints. Pure logic, deterministic,
    /// driven by the runtime and by the walkthrough simulation in the tests.
    /// </summary>
    public sealed class ActSession
    {
        public static readonly Dictionary<string, PuzzleDef> Puzzles = new Dictionary<string, PuzzleDef>
        {
            ["a1"] = new PuzzleDef { Id = "a1", Kind = PuzzleKind.Sequence, Sequence = new[] { 0, 2, 1, 2, 4 }, Flag = "a1_music", Reward = Balance.MusicReward, Achievement = "lullaby_bells", Room = "a1_music" },
            ["a1c"] = new PuzzleDef { Id = "a1c", Kind = PuzzleKind.Crates, Flag = "a1_crates", Achievement = "starboard_ring", Room = "a1_cargo" },
            ["a2c"] = new PuzzleDef { Id = "a2c", Kind = PuzzleKind.Single },
            ["a2x"] = new PuzzleDef { Id = "a2x", Kind = PuzzleKind.Crates, Flag = "a2_clear", Room = "a2_c" },
            ["a2"] = new PuzzleDef { Id = "a2", Kind = PuzzleKind.Sequence, Sequence = new[] { 0, 1, 2 }, Flag = "a2_bells", Reward = 6f, Room = "a2_hulk" },
            ["a3"] = new PuzzleDef { Id = "a3", Kind = PuzzleKind.Dials, Flag = "a3_clocks", Achievement = "three_clocks", Room = "a3_tower" },
            ["a3_mirror"] = new PuzzleDef { Id = "a3_mirror", Kind = PuzzleKind.Vision, Flag = "a3_choice" },
            ["a4"] = new PuzzleDef { Id = "a4", Kind = PuzzleKind.Mirrors, Flag = "a4_m4", Achievement = "beam", Room = "a4_cross" },
            ["a4v"] = new PuzzleDef { Id = "a4v", Kind = PuzzleKind.Listen },
            ["altar"] = new PuzzleDef { Id = "altar", Kind = PuzzleKind.Altar, Flag = "a4_end" },
        };

        /// <summary>Note frequencies (Hz) for bell indices, per puzzle. La Do Mi Sol La / La Do Mi.</summary>
        public static readonly float[] Act1BellHz = { 440f, 523.25f, 659.25f, 783.99f, 880f };
        public static readonly float[] Act2BellHz = { 440f, 523.25f, 659.25f };
        public static readonly string[] Act1BellNames = { "La", "Do", "Mi", "Sol", "La" };
        public static readonly string[] Act2BellNames = { "La", "Do", "Mi" };

        public readonly ActDef Act;
        public readonly GameState State;
        public readonly LanternModel Lantern;
        public readonly SanityModel Sanity;

        readonly Dictionary<string, int> _seqProgress = new Dictionary<string, int>();
        readonly Dictionary<string, float> _puzzleTime = new Dictionary<string, float>();

        public ActSession(ActDef act, GameState state)
        {
            Act = act ?? throw new ArgumentNullException(nameof(act));
            State = state ?? throw new ArgumentNullException(nameof(state));
            Lantern = new LanternModel(state.Oil, state.LanternLit);
            Sanity = new SanityModel(state.Sanity);
        }

        public void SyncToState()
        {
            State.Oil = Lantern.Oil;
            State.LanternLit = Lantern.Lit;
            State.Sanity = Sanity.Value;
            State.WorstTier = Math.Max(State.WorstTier, (int)Sanity.Tier);
            State.ActWorstTier = Math.Max(State.ActWorstTier, (int)Sanity.Tier);
        }

        // ------------------------------------------------------------ queries
        public bool IsConsumed(ItemDef i) => i.Once && State.Has("used:" + i.Id);
        public bool BlockerOpen(BlockerDef b) => b.Until == null || State.Has(b.Until);
        public bool RoomOpen(RoomDef r) => r.OpenFlag == null || State.Has(r.OpenFlag);
        public GoalDef CurrentGoal => Act.Goals.FirstOrDefault(g => !State.Has(g.Flag));
        public bool ActComplete => Act.EndFlag != null && State.Has(Act.EndFlag);

        public int CrateSlot(ItemDef crate) => State.GetInt("slot:" + crate.Id, 0);
        public int DialValue(ItemDef dial) => State.GetInt("dial:" + dial.Id, 0);
        public int MirrorTurns(ItemDef m) => State.GetInt("mirror:" + m.Id, 0);
        public int SequenceProgress(string puzzle) => _seqProgress.TryGetValue(puzzle, out var p) ? p : 0;
        public bool PuzzleSolved(string puzzle) => Puzzles.TryGetValue(puzzle, out var p) && p.Flag != null && State.Has(p.Flag);
        public bool NeedsMet(ItemDef i) => i.Need == null || State.HasAll(i.Need);

        public IEnumerable<ItemDef> PuzzleItems(string puzzle, ItemKind kind) => Act.Items.Where(x => x.Puzzle == puzzle && x.Kind == kind);

        public int BeamPrefix
        {
            get
            {
                var mirrors = PuzzleItems("a4", ItemKind.Mirror).OrderBy(m => m.Index).ToList();
                int n = 0;
                foreach (var m in mirrors)
                {
                    if (MirrorTurns(m) % 4 == m.Answer % 4) n++;
                    else break;
                }
                return n;
            }
        }

        /// <summary>True when the item would show a highlighted prompt (consumed items and solved puzzle parts go quiet).</summary>
        public bool IsInteractable(ItemDef i)
        {
            if (IsConsumed(i)) return false;
            switch (i.Kind)
            {
                case ItemKind.Crate:
                case ItemKind.Lever:
                    return !PuzzleSolved(i.Puzzle);
                case ItemKind.Clock:
                    return !PuzzleSolved("a3");
                case ItemKind.Mirror:
                    return !State.Has("a4_lens");
                case ItemKind.Brazier:
                    return !State.Has(i.Flag);
                case ItemKind.Hatch:
                case ItemKind.Altar:
                    return !State.Has(i.Flag);
                case ItemKind.Choice:
                    return !State.Has(i.Flag);
                case ItemKind.Exit:
                case ItemKind.Boat:
                    return !State.Has(i.Flag);
            }
            return true;
        }

        // ------------------------------------------------------------ hints
        public void TickPuzzleTime(string roomId, float dt)
        {
            var goal = CurrentGoal;
            if (goal == null) return;
            foreach (var p in Puzzles.Values)
            {
                if (p.Room == null || p.Room != roomId || p.Flag == null || State.Has(p.Flag)) continue;
                _puzzleTime.TryGetValue(p.Id, out var t);
                _puzzleTime[p.Id] = t + dt;
            }
        }

        public float PuzzleTime(string puzzle) => _puzzleTime.TryGetValue(puzzle, out var t) ? t : 0f;

        /// <summary>0 = no hint, 1..3 = increasingly direct diegetic hints (3 shows the answer).</summary>
        public int HintLevel(string puzzle)
        {
            int fails = State.GetInt("fail:" + puzzle);
            float t = PuzzleTime(puzzle);
            int byTime = t < Balance.HintFirstSeconds ? 0 : 1 + (int)((t - Balance.HintFirstSeconds) / Balance.HintStepSeconds);
            int byFail = fails / Balance.HintFailuresPerStep;
            return MathX.Clamp(Math.Max(byTime, byFail), 0, 3);
        }

        // ------------------------------------------------------------ interaction
        public InteractResult Interact(string itemId)
        {
            var item = Act.Item(itemId);
            var r = new InteractResult { Item = item };
            if (item == null) return r;

            if (IsConsumed(item))
            {
                r.Outcome = Outcome.AlreadyDone;
                return r;
            }
            if (item.Need != null && !State.HasAll(item.Need) && item.Kind != ItemKind.Bell && item.Kind != ItemKind.Crate)
            {
                r.Outcome = Outcome.NeedMissing;
                r.MessageKey = item.NeedKey;
                return r;
            }

            switch (item.Kind)
            {
                case ItemKind.Note:
                    State.MarkNote(item.Id);
                    SetFlag(r, item.Flag);
                    r.Outcome = Outcome.Note;
                    break;
                case ItemKind.Examine:
                {
                    bool first = !State.Has(item.Flag);
                    SetFlag(r, item.Flag);
                    State.MarkNote(item.Id);
                    if (first)
                    {
                        State.Examines++;
                        if (item.Sanity != 0f)
                        {
                            Sanity.Apply(item.Sanity);
                            r.SanityDelta = item.Sanity;
                        }
                    }
                    if (item.Once) State.Set("used:" + item.Id);
                    r.Outcome = Outcome.Examine;
                    break;
                }
                case ItemKind.Pickup:
                    SetFlag(r, item.Flag);
                    State.MarkNote(item.Id);
                    State.Set("used:" + item.Id);
                    r.Outcome = Outcome.Pickup;
                    break;
                case ItemKind.Drawing:
                    State.AddDrawing(item.Drawing);
                    SetFlag(r, item.Flag);
                    State.Set("used:" + item.Id);
                    if (State.DrawingCount >= Endings.DrawingsForAnswer) r.Achievements.Add("four_drawings");
                    if (State.DrawingCount >= 6) r.Achievements.Add("all_drawings");
                    r.Outcome = Outcome.Drawing;
                    break;
                case ItemKind.Tape:
                    SetFlag(r, item.Flag);
                    State.MarkNote(item.Id);
                    r.Outcome = Outcome.Tape;
                    break;
                case ItemKind.Oil:
                {
                    float added = Lantern.AddOil(item.Oil);
                    if (added < 0.5f)
                    {
                        r.Outcome = Outcome.OilFull;
                        r.MessageKey = "msg.oil_full";
                        // The flask stays for later, but an objective that asks for it must not stall on a full lantern.
                        if (item.Flag != null && Act.Goals.Any(g => g.Flag == item.Flag)) SetFlag(r, item.Flag);
                        return r;
                    }
                    r.OilAdded = added;
                    SetFlag(r, item.Flag);
                    State.Set("used:" + item.Id);
                    if (!Lantern.Lit) Lantern.TryLight();
                    r.Outcome = Outcome.Oil;
                    break;
                }
                case ItemKind.Bell:
                    InteractBell(item, r);
                    break;
                case ItemKind.Listen:
                {
                    State.Listens++;
                    SetFlag(r, item.Flag);
                    if (Puzzles.TryGetValue(item.Puzzle ?? "", out var p) && p.Sequence != null) r.Sequence = p.Sequence;
                    r.Outcome = Outcome.ListenPlay;
                    break;
                }
                case ItemKind.Crate:
                    InteractCrate(item, r);
                    break;
                case ItemKind.Lever:
                    InteractLever(item, r);
                    break;
                case ItemKind.Clock:
                    InteractDial(item, r);
                    break;
                case ItemKind.Mirror:
                    InteractMirror(item, r);
                    break;
                case ItemKind.Hatch:
                    SetFlag(r, item.Flag);
                    State.Set("used:" + item.Id);
                    r.Outcome = Outcome.HatchOpened;
                    break;
                case ItemKind.Brazier:
                    if (State.Has(item.Flag))
                    {
                        r.Outcome = Outcome.AlreadyDone;
                        break;
                    }
                    if (!Lantern.Spend(item.Oil))
                    {
                        r.Outcome = Outcome.BrazierNoOil;
                        r.MessageKey = "msg.brazier_no_oil";
                        break;
                    }
                    SetFlag(r, item.Flag);
                    State.BraziersLit++;
                    if (Act.Items.Where(x => x.Kind == ItemKind.Brazier).All(x => State.Has(x.Flag))) r.Achievements.Add("braziers");
                    r.Outcome = Outcome.BrazierLit;
                    break;
                case ItemKind.Exit:
                case ItemKind.Boat:
                    if (State.Has(item.Flag))
                    {
                        r.Outcome = Outcome.AlreadyDone;
                        break;
                    }
                    if (Act.Id == 2) State.Act2Path = item.Kind == ItemKind.Boat ? EchoPath.Echo : EchoPath.Mimic;
                    SetFlag(r, item.Flag);
                    if (Act.Id == 2) r.Achievements.Add("act2");
                    r.Outcome = Outcome.Exit;
                    break;
                case ItemKind.Altar:
                    if (State.Has(item.Flag))
                    {
                        r.Outcome = Outcome.AlreadyDone;
                        break;
                    }
                    SetFlag(r, item.Flag);
                    r.Outcome = Outcome.Altar;
                    break;
                case ItemKind.Choice:
                    InteractChoice(item, r);
                    break;
            }

            if (r.Success && item.Achievement != null && !r.Achievements.Contains(item.Achievement)) r.Achievements.Add(item.Achievement);
            if (r.FlagsSet.Any(f => Act.CheckpointFlags.Contains(f))) r.Checkpoint = true;
            SyncToState();
            return r;
        }

        void SetFlag(InteractResult r, string flag)
        {
            if (flag != null && State.Set(flag)) r.FlagsSet.Add(flag);
        }

        void InteractBell(ItemDef item, InteractResult r)
        {
            r.Bell = item.Index;
            State.BellsRung++;
            var p = Puzzles[item.Puzzle];
            if (p.Kind == PuzzleKind.Single)
            {
                SetFlag(r, item.Flag);
                r.Outcome = Outcome.BellRung;
                return;
            }
            if (State.Has(p.Flag))
            {
                r.Outcome = Outcome.BellRung;
                return;
            }
            int prog = SequenceProgress(p.Id);
            if (p.Sequence[prog] == item.Index)
            {
                prog++;
                if (prog >= p.Sequence.Length)
                {
                    _seqProgress[p.Id] = 0;
                    SetFlag(r, p.Flag);
                    if (p.Reward != 0f)
                    {
                        Sanity.Apply(p.Reward);
                        r.SanityDelta = p.Reward;
                    }
                    if (p.Achievement != null) r.Achievements.Add(p.Achievement);
                    r.Solved = true;
                    r.Outcome = Outcome.BellSolved;
                    return;
                }
                _seqProgress[p.Id] = prog;
                r.Outcome = Outcome.BellRung;
                return;
            }
            State.SetInt("fail:" + p.Id, State.GetInt("fail:" + p.Id) + 1);
            _seqProgress[p.Id] = item.Index == p.Sequence[0] ? 1 : 0;
            r.Outcome = Outcome.BellWrong;
        }

        void InteractCrate(ItemDef item, InteractResult r)
        {
            var p = Puzzles[item.Puzzle];
            if (State.Has(p.Flag))
            {
                r.Outcome = Outcome.AlreadyDone;
                return;
            }
            int slot = (CrateSlot(item) + 1) % item.Slots.Length;
            State.SetInt("slot:" + item.Id, slot);
            r.Slot = slot;
            bool all = PuzzleItems(p.Id, ItemKind.Crate).All(c => CrateSlot(c) == c.Goal);
            if (all)
            {
                SetFlag(r, p.Flag);
                if (p.Achievement != null && !State.Has("reset:" + p.Id)) r.Achievements.Add(p.Achievement);
                r.Solved = true;
                r.Outcome = Outcome.CrateSolved;
                return;
            }
            if (slot == 0) State.SetInt("fail:" + p.Id, State.GetInt("fail:" + p.Id) + 1);
            r.Outcome = Outcome.CrateMoved;
        }

        void InteractLever(ItemDef item, InteractResult r)
        {
            var p = Puzzles[item.Puzzle];
            if (State.Has(p.Flag))
            {
                r.Outcome = Outcome.AlreadyDone;
                return;
            }
            foreach (var c in PuzzleItems(p.Id, ItemKind.Crate)) State.SetInt("slot:" + c.Id, 0);
            State.Set("reset:" + p.Id);
            State.SetInt("fail:" + p.Id, State.GetInt("fail:" + p.Id) + 1);
            r.Outcome = Outcome.Reset;
        }

        void InteractDial(ItemDef item, InteractResult r)
        {
            var p = Puzzles[item.Puzzle];
            if (State.Has(p.Flag))
            {
                r.Outcome = Outcome.AlreadyDone;
                return;
            }
            int v = (DialValue(item) + 1) % item.Options.Length;
            State.SetInt("dial:" + item.Id, v);
            r.Dial = v;
            var dials = PuzzleItems(p.Id, ItemKind.Clock).ToList();
            if (dials.All(d => DialValue(d) == d.Answer))
            {
                SetFlag(r, p.Flag);
                if (p.Achievement != null && State.GetInt("fail:" + p.Id) == 0) r.Achievements.Add(p.Achievement);
                r.Solved = true;
                r.Outcome = Outcome.DialSolved;
                return;
            }
            // a full turn past the answer counts as a mistake (for hints)
            if (v == 0) State.SetInt("fail:" + p.Id, State.GetInt("fail:" + p.Id) + 1);
            r.Outcome = Outcome.DialTurned;
        }

        void InteractMirror(ItemDef item, InteractResult r)
        {
            if (State.Has("a4_lens"))
            {
                r.Outcome = Outcome.MirrorLocked;
                r.MessageKey = "msg.mirror_locked";
                return;
            }
            int before = BeamPrefix;
            int t = (MirrorTurns(item) + 1) % 4;
            State.SetInt("mirror:" + item.Id, t);
            int after = BeamPrefix;
            r.BeamPrefix = after;
            for (int k = 0; k < 4; k++)
            {
                string f = "a4_m" + (k + 1);
                if (k < after) SetFlag(r, f);
                else State.Clear(f);
            }
            if (after < before) State.SetInt("fail:a4", State.GetInt("fail:a4") + 1);
            if (after == 4 && before < 4) r.Achievements.Add("beam");
            r.Solved = after == 4;
            r.Outcome = after > before && after > item.Index ? Outcome.MirrorAligned : Outcome.MirrorTurned;
        }

        void InteractChoice(ItemDef item, InteractResult r)
        {
            if (State.Has(item.Flag))
            {
                r.Outcome = Outcome.AlreadyDone;
                return;
            }
            if (item.Puzzle == "altar")
            {
                State.Altar = item.Index == 1 ? AltarChoice.Echo : AltarChoice.Mimic;
                State.Ending = Endings.Resolve(State);
                SetFlag(r, item.Flag);
                r.Achievements.Add("act4");
                r.Achievements.Add("ending_" + Endings.Key(State.Ending));
            }
            else
            {
                SetFlag(r, item.Flag);
            }
            r.Outcome = Outcome.Choice;
        }

        /// <summary>Act III mirror hall: did Adrian answer this time?</summary>
        public InteractResult RecordAnswer(bool answered)
        {
            var r = new InteractResult { Outcome = Outcome.Choice };
            State.Answered = answered ? 1 : 0;
            if (answered)
            {
                SetFlag(r, "a3_sea_open");
                r.Achievements.Add("answered");
            }
            else
            {
                SetFlag(r, "a3_left");
                r.Achievements.Add("silent");
                r.Achievements.Add("act3");
            }
            r.Checkpoint = answered;
            SyncToState();
            return r;
        }

        /// <summary>Called by the runtime when a leading voice reached its destination.</summary>
        public bool MarkFlag(string flag)
        {
            if (!State.Set(flag)) return false;
            return Act.CheckpointFlags.Contains(flag);
        }

        /// <summary>Achievements decided at the end of an act from the act statistics.</summary>
        public List<string> ActEndAchievements()
        {
            var l = new List<string>();
            if (State.ActWorstTier < (int)SanityTier.Disturbed) l.Add("steady");
            if (State.ActOilRunOuts == 0 && Lantern.RunOuts == 0) l.Add("never_dark");
            return l;
        }
    }
}
