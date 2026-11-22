using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// One playable act: owns the session (rules), the built world, the player, and turns interaction
    /// results into sound, text, animation, checkpoints and achievements.
    /// </summary>
    public sealed partial class ActRuntime : MonoBehaviour
    {
        public ActDef Act;
        public ActSession Session;
        public World World;
        public PlayerRig Player;
        public GameState CheckpointState;
        public GameState State => Session.State;
        public RoomDef Room { get; private set; }
        public bool Listening { get; private set; }
        public float Tremor { get; private set; }
        public float KillY { get; private set; } = -40f;
        public bool Finishing { get; private set; }
        public float Presence => _presence;
        /// <summary>0..1, lowers the lantern out of view (photo mode).</summary>
        public float PhotoHide;

        bool _started;
        float _saveCooldown;

        public static ActRuntime Create(ActDef act, GameState state)
        {
            var go = new GameObject("[Act " + act.Id + "]");
            var rt = go.AddComponent<ActRuntime>();
            rt.Init(act, state);
            return rt;
        }

        void Init(ActDef act, GameState state)
        {
            Act = act;
            state.Act = act.Id;
            Session = new ActSession(act, state);
            Game.Act = this;
            World = WorldBuilder.Build(act, state);
            World.Root.SetParent(transform, true);
            float minFloor = act.Rooms.Count > 0 ? act.Rooms.Min(r => r.MinFloor) : 0f;
            KillY = minFloor - 14f;
            foreach (var ia in World.Items.Values) if (ia != null) ia.Sync(Session);

            var spawn = new Vector3(act.Spawn.X, 0f, act.Spawn.Z);
            float yaw = act.SpawnYaw;
            spawn = World.Ground(spawn.x, spawn.z, 50f);
            if (state.Checkpoint != "start" && state.Ints.ContainsKey("cp.x"))
            {
                spawn = new Vector3(state.GetInt("cp.x") / 100f, state.GetInt("cp.y") / 100f + 0.05f, state.GetInt("cp.z") / 100f);
                yaw = state.GetInt("cp.yaw") / 100f;
            }
            Player = PlayerRig.Create(this, spawn, yaw);
            _lastLight = spawn;
            _lastLightYaw = yaw;
            CheckpointState = state.Clone();

            Game.Audio.SetAmbience(act.Ambience, 3f);
            Game.UI.InGame = true;
            Game.UI.RefreshMode();
            InitSystems();
            Room = World.RoomAt(spawn);
            OnRoomChanged(Room);
            _started = true;
            StartCoroutine(Opening());
        }

        IEnumerator Opening()
        {
            yield return new WaitForSeconds(2.2f);
            var g = Session.CurrentGoal;
            if (g != null && Game.Settings.ObjectiveHints) Game.UI.Hud.Objective(Game.Loc[g.TextKey]);
            _goalFlag = g?.Flag;
            if (State.Checkpoint == "start" && Act.Id == 1 && !State.Has("tut_lantern"))
            {
                yield return new WaitForSeconds(3.5f);
                Game.UI.Hud.Message(Game.Loc.F("msg.tut_lantern", Game.Input.Tag(Game.Input.Lantern)), 5f);
                yield return new WaitForSeconds(6f);
                Game.UI.Hud.Message(Game.Loc.F("msg.tut_listen", Game.Input.Tag(Game.Input.Listen)), 5f);
                State.Set("tut_lantern");
            }
        }

        void OnDestroy()
        {
            if (Game.Act == this) Game.Act = null;
            if (Game.Audio != null) Game.Audio.Duck = 0f;
            Game.Input?.StopRumble();
        }

        // ------------------------------------------------------------------ frame
        void Update()
        {
            if (!_started || Game.Paused || Player == null) return;
            float dt = Time.deltaTime;
            State.PlaySeconds += dt;
            _saveCooldown -= dt;
            var feet = Player.Feet;
            var room = World.RoomAt(feet) ?? Room;
            if (room != Room) OnRoomChanged(room);

            bool control = !Player.InputLocked && Game.UI.ScreenCount == 0 && !Game.UI.Cinematic && !Finishing && !Game.Flow.Busy;
            if (control && (Game.Input.Lantern.WasPressedThisFrame() || TouchControls.TakeLantern())) ToggleLantern();
            ListenTick(control, dt);
            // with the touch test controls a tap is a left click: the overlay decides what it means, not the click binding
            bool interact = Game.Input.Interact.WasPressedThisFrame() &&
                            !(TouchControls.Active && UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame);
            if (control && (interact || TouchControls.TakeInteract()) && Player.Focus != null) Use(Player.Focus);

            if (control) WalkInExits(feet);
            if (Session.Lantern.Tick(dt, Player.Sprinting)) OnLanternOut();
            Player.Lantern.Sync(Session.Lantern, Room != null && Room.Outdoor ? 0.6f : 0.1f, PhotoHide);
            SystemsTick(dt, feet);
            var hud = Game.UI.Hud;
            hud.Oil01 = Session.Lantern.Oil / Balance.OilMax;
            hud.OilLow = Session.Lantern.Low;
        }

        /// <summary>Some exits are places, not objects: walking into the fog or down the sea stairs is the choice.</summary>
        void WalkInExits(Vector3 feet)
        {
            foreach (var it in Act.Items)
            {
                if (it.Kind != ItemKind.Exit || (it.Prop != "seastairs" && it.Prop != "foggate")) continue;
                if (State.Has(it.Flag) || !Session.NeedsMet(it)) continue;
                var p = World.ItemPoint(it);
                float floor = p.y - it.Height;
                if (new Vector2(feet.x - p.x, feet.z - p.z).magnitude > 1.9f || Mathf.Abs(feet.y - floor) > 1.6f) continue;
                if (World.Items.TryGetValue(it.Id, out var ia) && ia != null) Use(ia);
                return;
            }
        }

        void OnRoomChanged(RoomDef room)
        {
            Room = room;
            if (room == null) return;
            AudioReverbPreset p;
            switch (room.Kind)
            {
                case RoomKind.Cabin: p = AudioReverbPreset.Room; break;
                case RoomKind.Hold: p = AudioReverbPreset.Hangar; break;
                case RoomKind.Hall: p = room.Ceil ? AudioReverbPreset.Hallway : AudioReverbPreset.StoneCorridor; break;
                case RoomKind.Temple: p = AudioReverbPreset.Cave; break;
                case RoomKind.Street: p = AudioReverbPreset.City; break;
                case RoomKind.Grave: p = AudioReverbPreset.Quarry; break;
                default: p = AudioReverbPreset.Plain; break;
            }
            if (Act.Id == 4 && room.Kind != RoomKind.Temple) p = AudioReverbPreset.Cave;
            Game.Audio.SetReverb(p);
            if (!State.Has("seen:" + room.Id))
            {
                State.Set("seen:" + room.Id);
                if (Game.Loc.Has(room.NameKey) && room.Landmark != null) Game.UI.Hud.Message(Game.Loc[room.NameKey], 2.6f);
            }
        }

        // ------------------------------------------------------------------ lantern
        void ToggleLantern()
        {
            var l = Session.Lantern;
            if (l.Lit)
            {
                l.Extinguish();
                Game.UI.Hud.ShowOil(2.5f);
            }
            else if (!l.TryLight())
            {
                Game.UI.Hud.Message(Game.Loc["msg.no_oil"]);
                Game.Audio.PlayAt("sfx_lantern_empty", Player.EyePos, Bus.Effects, 0.6f);
            }
            else Game.UI.Hud.ShowOil(2.5f);
            Session.SyncToState();
        }

        void OnLanternOut()
        {
            State.OilRunOuts++;
            State.ActOilRunOuts++;
            Game.UI.Hud.Message(Game.Loc["msg.lantern_out"], 4f);
            Game.UI.Hud.ShowOil(5f);
            Game.Audio.PlayAt("sfx_lantern_empty", Player.EyePos, Bus.Effects, 0.8f);
            Session.SyncToState();
        }

        // ------------------------------------------------------------------ interaction
        public void Use(Interactable ia)
        {
            if (ia == null || Finishing) return;
            var r = Session.Interact(ia.Item.Id);
            ia.Feedback(r);
            Handle(ia, r);
        }

        void Handle(Interactable ia, InteractResult r)
        {
            var it = r.Item;
            var loc = Game.Loc;
            var hud = Game.UI.Hud;
            var at = ia != null ? ia.Point : Player.EyePos;
            switch (r.Outcome)
            {
                case Outcome.NeedMissing:
                    hud.Message(loc[r.MessageKey ?? it.NeedKey]);
                    Game.Audio.PlayAt("sfx_locked", at, Bus.Effects, 0.7f);
                    break;
                case Outcome.Note:
                case Outcome.Examine:
                case Outcome.Pickup:
                case Outcome.Drawing:
                case Outcome.Tape:
                    Game.Audio.PlayAt(r.Outcome == Outcome.Tape ? "sfx_tape_play" : (r.Outcome == Outcome.Pickup ? "sfx_pickup" : "sfx_paper"), at, Bus.Effects, 0.8f);
                    if (r.SanityDelta < 0f)
                    {
                        Game.Audio.Play2D("sfx_sting_low", Bus.Effects, 0.55f);
                        World.Atmos.Flash(0.25f);
                        Player.Shake(0.25f, 0.5f);
                    }
                    if (r.Outcome == Outcome.Pickup) hud.Message(loc.F("msg.picked_up", loc[it.TitleKey]));
                    if (r.Outcome == Outcome.Drawing) hud.Message(loc.F("msg.drawing_found", State.DrawingCount, 6), 4f);
                    Game.UI.Push(new NoteReader(it, r.Outcome == Outcome.Drawing ? State.DrawingCount : 0));
                    break;
                case Outcome.Oil:
                    Game.Audio.PlayAt("sfx_oil_pour", at, Bus.Effects, 0.9f);
                    hud.Message(loc.F("msg.oil_added", Mathf.RoundToInt(r.OilAdded)));
                    hud.ShowOil(4f);
                    break;
                case Outcome.OilFull:
                    hud.Message(loc["msg.oil_full"]);
                    break;
                case Outcome.BellRung:
                case Outcome.BellWrong:
                case Outcome.BellSolved:
                    RingBell(it, r, at);
                    break;
                case Outcome.ListenPlay:
                    StartCoroutine(PlayMelody(ia, r.Sequence));
                    break;
                case Outcome.Reset:
                    Game.Audio.PlayAt("sfx_lever", at, Bus.Effects, 0.9f);
                    hud.Message(loc["msg.crates_reset"]);
                    StartCoroutine(SyncCratesLater());
                    break;
                case Outcome.CrateSolved:
                    StartCoroutine(Delayed(1.3f, () =>
                    {
                        Game.Audio.PlayAt("sfx_mechanism", at, Bus.Effects, 1f);
                        Player.Shake(0.2f, 0.6f);
                    }));
                    break;
                case Outcome.DialTurned:
                    Game.Audio.PlayAt("sfx_dial_turn", at, Bus.Effects, 0.8f);
                    break;
                case Outcome.DialSolved:
                    Game.Audio.PlayAt("sfx_dial_turn", at, Bus.Effects, 0.8f);
                    StartCoroutine(Delayed(1.0f, () =>
                    {
                        Game.Audio.PlayAt("sfx_clock_chime", at + Vector3.up * 6f, Bus.Effects, 1f, 1f, 4f, 80f);
                        Player.Shake(0.3f, 1.4f);
                    }));
                    break;
                case Outcome.MirrorTurned:
                case Outcome.MirrorAligned:
                    Game.Audio.PlayAt("sfx_mirror_turn", at, Bus.Effects, 0.8f);
                    FlagSet("turn:" + it.Id);   // beams after this mirror may have gone out
                    break;
                case Outcome.MirrorLocked:
                    hud.Message(loc[r.MessageKey]);
                    break;
                case Outcome.HatchOpened:
                    Game.Audio.PlayAt("sfx_hatch_open", at, Bus.Effects, 1f);
                    Player.Shake(0.12f, 0.5f);
                    break;
                case Outcome.BrazierLit:
                    hud.ShowOil(3f);
                    break;
                case Outcome.BrazierNoOil:
                    hud.Message(loc[r.MessageKey]);
                    hud.ShowOil(3f);
                    break;
                case Outcome.Exit:
                    StartCoroutine(Moments.ActExit(this, it));
                    break;
                case Outcome.Altar:
                    StartCoroutine(Moments.Altar(this, it));
                    break;
                case Outcome.Choice:
                    if (it != null && it.Puzzle == "a3_mirror") StartCoroutine(Moments.MirrorHall(this, it));
                    else if (it != null && it.Puzzle == "altar") StartCoroutine(Moments.AltarChoice(this, it));
                    break;
            }
            if (r.Achievements.Count > 0) Game.Achievements.UnlockAll(r.Achievements);
            foreach (var f in r.FlagsSet) FlagSet(f);
            if (r.Outcome == Outcome.Note || r.Outcome == Outcome.Examine || r.Outcome == Outcome.Tape || r.Outcome == Outcome.Pickup) CheckAllNotes();
            if (r.Checkpoint) SaveCheckpointNow(r.FlagsSet.FirstOrDefault(f => Act.CheckpointFlags.Contains(f)) ?? "cp");
        }

        IEnumerator SyncCratesLater()
        {
            yield return new WaitForSeconds(0.5f);
            foreach (var c in Session.PuzzleItems("a1c", ItemKind.Crate).Concat(Session.PuzzleItems("a2c", ItemKind.Crate)))
                if (World.Items.TryGetValue(c.Id, out var ia) && ia != null) ia.Sync(Session);
        }

        static IEnumerator Delayed(float s, System.Action a)
        {
            yield return new WaitForSeconds(s);
            a();
        }

        void RingBell(ItemDef it, InteractResult r, Vector3 at)
        {
            string id = (Act.Id == 1 ? "bell_a1_" : "bell_a2_") + it.Index;
            Game.Audio.PlayAt(id, at, Bus.Effects, 1f, 1f, 2f, 40f, true);
            Game.Input.Rumble(0.1f, 0.3f, 0.15f);
            if (r.Outcome == Outcome.BellWrong)
            {
                StartCoroutine(Delayed(0.55f, () => Game.Audio.PlayAt("sfx_bell_wrong", at, Bus.Effects, 0.7f)));
                if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.bell_wrong"], 2.2f, at);
            }
            else if (Game.Settings.SoundCaptions)
            {
                var names = Act.Id == 1 ? ActSession.Act1BellNames : ActSession.Act2BellNames;
                Game.UI.Subtitles.Caption(Game.Loc.F("caption.bell", names[Mathf.Clamp(it.Index, 0, names.Length - 1)]), 1.6f, at);
            }
            if (r.Outcome == Outcome.BellSolved)
            {
                StartCoroutine(Delayed(1.4f, () =>
                {
                    var box = Act.Items.FirstOrDefault(x => x.Kind == ItemKind.Listen && x.Puzzle == it.Puzzle);
                    var pos = box != null ? World.ItemPoint(box) : at;
                    Game.Audio.PlayAt("mb_lullaby_echo", pos, Bus.Music, 0.9f, 1f, 2f, 30f, true);
                    Game.UI.Hud.Message(Game.Loc["msg.bells_solved_" + Act.Id], 4.5f);
                    World.Atmos.Flash(0.18f);
                }));
            }
        }

        IEnumerator PlayMelody(Interactable ia, int[] seq)
        {
            if (seq == null) yield break;
            var at = ia != null ? ia.Point : Player.EyePos;
            Game.Audio.Duck = 0.6f;
            if (Game.Settings.SoundCaptions) Game.UI.Subtitles.Caption(Game.Loc["caption.musicbox"], 3f, at);
            yield return new WaitForSeconds(0.6f);
            foreach (int n in seq)
            {
                string id = "mb_note_" + (Act.Id == 1 ? "a1_" : "a2_") + n;
                Game.Audio.PlayAt(id, at, Bus.Effects, 1f, 1f, 1.5f, 25f, true);
                yield return new WaitForSeconds(0.62f);
            }
            yield return new WaitForSeconds(0.8f);
            Game.Audio.Duck = Listening ? 0.55f : 0f;
        }

        /// <summary>Reacts to a flag that the session already set (doors, dressing events).</summary>
        public void RaiseFlagEvent(string flag) => FlagSet(flag);

        void FlagSet(string flag)
        {
            foreach (var b in World.Blockers.Values)
                if (b != null && b.Def.Until == flag) b.SetOpen(true, false);
            if (World.OnFlag.TryGetValue(flag, out var h))
            {
                try { h(this); }
                catch (System.Exception e) { Debug.LogException(e); }
            }
            if (flag == Act.EndFlag && !Finishing && Act.Id != 4) { /* exits drive their own moment */ }
        }

        /// <summary>Sets a flag from a runtime event (voice reached, moment done) and reacts like an interaction would.</summary>
        public void RaiseFlag(string flag)
        {
            if (string.IsNullOrEmpty(flag) || State.Has(flag)) return;
            bool cp = Session.MarkFlag(flag);
            FlagSet(flag);
            if (cp) SaveCheckpointNow(flag);
        }

        void CheckAllNotes()
        {
            var all = ContentDb.Acts.SelectMany(a => a.Items).Where(i => i.Kind == ItemKind.Note || i.Kind == ItemKind.Tape).Select(i => i.Id);
            if (all.All(id => State.NotesRead.Contains(id))) Game.Achievements.Unlock("all_notes");
        }

        // ------------------------------------------------------------------ saving
        public void SaveCheckpointNow(string why)
        {
            if (Player == null) return;
            Session.SyncToState();
            var p = Player.Grounded && !Player.Wading ? Player.Feet : _lastLight;
            State.SetInt("cp.x", Mathf.RoundToInt(p.x * 100f));
            State.SetInt("cp.y", Mathf.RoundToInt(p.y * 100f));
            State.SetInt("cp.z", Mathf.RoundToInt(p.z * 100f));
            State.SetInt("cp.yaw", Mathf.RoundToInt(Player.Yaw * 100f));
            if (why != "quit") State.Checkpoint = why;
            else if (State.Checkpoint == "start") State.Checkpoint = "resume";
            CheckpointState = State.Clone();
            int slot = Game.Saves.ActiveSlot;
            if (slot >= 0)
            {
                Game.Saves.Save(slot, State);
                if (why != "quit" && _saveCooldown <= 0f)
                {
                    Game.UI.Hud.Saving();
                    _saveCooldown = 4f;
                }
            }
        }

        /// <summary>Ends the act: achievements for the act, then the next chapter (or the endings).</summary>
        public void FinishAct()
        {
            if (Finishing) return;
            Finishing = true;
            Session.SyncToState();
            Game.Achievements.UnlockAll(Session.ActEndAchievements());
            Game.Audio.Duck = 0f;
            Game.Flow.ActFinished(State);
        }

        public void BeginMoment() { Finishing = true; Game.UI.Cinematic = true; Player.Lock(); Game.UI.RefreshMode(); }
        public void EndMoment() { Finishing = false; Game.UI.Cinematic = false; Player.Unlock(); Game.UI.RefreshMode(); }

        /// <summary>Water depth at the feet (0 when dry): shallow flooding in Act III, the cove in Act II.</summary>
        public float WaterDepthAt(Vector3 feet)
        {
            float best = 0f;
            foreach (var w in World.Waters)
            {
                if (w == null || w.Sea) continue;
                if (!w.Covers(feet)) continue;
                best = Mathf.Max(best, w.Level - feet.y);
            }
            return best;
        }
    }
}
