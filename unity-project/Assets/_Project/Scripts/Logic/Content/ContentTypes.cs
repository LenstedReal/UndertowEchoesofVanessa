using System;
using System.Collections.Generic;
using System.Linq;

namespace Undertow.Logic
{
    public enum RoomKind { Deck, Cabin, Hold, Hall, Grave, Street, Temple, Cove, Shore }
    public enum Surface { Wood, WetWood, Stone, Sand, Shallow, Metal, Rug }

    /// <summary>
    /// A walkable space. Rect is the floor footprint (min corner + size). A ramp rises or falls
    /// linearly from Floor to FloorTo along +Z (or +X when RampAlongX).
    /// </summary>
    public sealed class RoomDef
    {
        public string Id;
        public int Act;
        public RectXZ Rect;
        public float Floor;
        public float FloorTo = float.NaN;
        public bool RampAlongX;
        public bool Ceil;
        public float CeilH = 2.6f;
        public RoomKind Kind;
        public Surface Surface = Surface.Wood;
        public float Ambient = 0.2f;     // 0..1 how much fill light the space has
        public bool Safe;                // safe light: sanity regenerates
        public bool Outdoor;             // under the moon
        public bool SeaView;             // open water visible from here (sea-staring counts)
        public float Water = float.NaN;  // absolute water surface height when the floor is flooded
        public string OpenFlag;          // room is sealed until this flag is set
        public string Landmark;          // mast, columns, clock, whale, statue ...

        public string NameKey => "room." + Id;
        public bool IsRamp => !float.IsNaN(FloorTo);
        public bool Enclosed => Kind == RoomKind.Cabin || Kind == RoomKind.Hold || Kind == RoomKind.Hall || Kind == RoomKind.Temple;

        public float FloorAt(float x, float z)
        {
            if (!IsRamp) return Floor;
            float t = RampAlongX ? (x - Rect.X) / Rect.W : (z - Rect.Z) / Rect.D;
            return Floor + (FloorTo - Floor) * MathX.Clamp01(t);
        }

        public float MinFloor => IsRamp ? Math.Min(Floor, FloorTo) : Floor;
        public float MaxFloor => IsRamp ? Math.Max(Floor, FloorTo) : Floor;

        /// <summary>Slope in degrees (0 for flat rooms).</summary>
        public float SlopeDegrees
        {
            get
            {
                if (!IsRamp) return 0f;
                float run = RampAlongX ? Rect.W : Rect.D;
                return (float)(Math.Atan2(Math.Abs(FloorTo - Floor), run) * 180.0 / Math.PI);
            }
        }
    }

    /// <summary>An opening in the wall between two rooms (otherwise the compiler picks the middle of the shared edge).</summary>
    public sealed class DoorDef
    {
        public string A;
        public string B;
        public V2 At;               // a point on the shared edge
        public float Width = 1.4f;
        public float Height = 2.15f;
        public string Style = "door"; // door, arch, gap, hatch, grate
    }

    /// <summary>Something that blocks the way until a flag is set (locked door, debris, fog wall, bolt).</summary>
    public sealed class BlockerDef
    {
        public string Id;
        public RectXZ Rect;
        public string Until;
        public string Style = "door";
        public float Height = 2.4f;
    }

    public enum ItemKind
    {
        Note, Oil, Bell, Listen, Crate, Clock, Mirror, Pickup, Examine, Hatch, Exit,
        Altar, Drawing, Lever, Brazier, Choice, Tape, Boat
    }

    /// <summary>Everything the player can look at and press Interact on.</summary>
    public sealed class ItemDef
    {
        public string Id;
        public int Act;
        public string Room;
        public V2 Pos;
        public float Height = 1.0f;     // interaction point above the floor
        public float Yaw;               // direction the prop's front faces, degrees around Y (0 = +Z, 90 = +X)
        public ItemKind Kind;
        public string Flag;
        public string[] Need;
        public float Sanity;
        public float Oil;
        public bool Once;
        public string Puzzle;
        public int Index;
        public V2[] Slots;
        public int Goal;
        public string[] Options;
        public int Answer;
        public int Drawing;
        public string Prop;             // visual prop id built by the runtime
        public string NoteStyle = "log";// log, child, letter, carving, document, board, tape
        public string Speaker;          // who wrote / says it
        public string Voice;            // narration clip id (optional)
        public string Achievement;
        public bool Optional;           // not needed to finish the act

        public string PromptKey => "item." + Id + ".prompt";
        public string TitleKey => "item." + Id + ".title";
        public string BodyKey => "item." + Id + ".body";
        public string NeedKey => "item." + Id + ".need";
        public bool HasText => Kind == ItemKind.Note || Kind == ItemKind.Examine || Kind == ItemKind.Pickup || Kind == ItemKind.Drawing || Kind == ItemKind.Tape;
    }

    public sealed class GoalDef
    {
        public string Flag;
        public V2 Pos;
        public string TextKey => "goal." + Flag;
    }

    public enum VoiceKind { Echo, Mimic, Adrian, Thorne, Ruth, Choir, Whisper }

    /// <summary>A line that plays once when its conditions hold (spatial, with a subtitle).</summary>
    public sealed class VoiceCue
    {
        public string Id;
        public VoiceKind Kind;
        public string Clip;             // audio clip id
        public V2 From;                 // where the voice comes from
        public float Height = 1.2f;
        public string TriggerRoom;      // player enters this room ...
        public V2 TriggerAt;            // ... or comes within TriggerRadius of this point
        public float TriggerRadius;
        public string WhenFlag;
        public string UnlessFlag;
        public float Delay;
        public bool Leads;              // Echo/Mimic beacon: keeps singing from From until the player gets there
        public string SetsFlag;         // set when the player reaches From (following the voice)

        public string LineKey => "voice." + Id;
    }

    public sealed class ActDef
    {
        public int Id;
        public string SceneName;
        public V2 Spawn;
        public float SpawnYaw;
        public float FogDistance = 18f;     // HDRP fog mean free path (metres)
        public float[] FogColor = { 0.3f, 0.36f, 0.38f };
        public float Moon = 0.5f;           // moonlight strength 0..1 (0 = underground)
        public string Ambience;
        public string EndFlag;
        public readonly List<RoomDef> Rooms = new List<RoomDef>();
        public readonly List<DoorDef> Doors = new List<DoorDef>();
        public readonly List<BlockerDef> Blockers = new List<BlockerDef>();
        public readonly List<ItemDef> Items = new List<ItemDef>();
        public readonly List<GoalDef> Goals = new List<GoalDef>();
        public readonly List<VoiceCue> Voices = new List<VoiceCue>();
        public readonly List<string> CheckpointFlags = new List<string>();

        public string NameKey => "act." + Id + ".name";
        public string LineKey => "act." + Id + ".line";

        public RoomDef Room(string id) => Rooms.FirstOrDefault(r => r.Id == id);
        public ItemDef Item(string id) => Items.FirstOrDefault(i => i.Id == id);

        /// <summary>The most specific (smallest) open room containing the point, as the prototype did.</summary>
        public RoomDef RoomAt(float x, float z, GameState state = null)
        {
            RoomDef best = null;
            foreach (var r in Rooms)
            {
                if (!r.Rect.Contains(x, z)) continue;
                if (state != null && r.OpenFlag != null && !state.Has(r.OpenFlag)) continue;
                if (best == null || r.Rect.Area < best.Rect.Area) best = r;
            }
            return best;
        }

        // ---------- small builders used by the act files
        public RoomDef AddRoom(string id, float x, float z, float w, float d, float floor, RoomKind kind, Action<RoomDef> more = null)
        {
            var r = new RoomDef { Id = id, Act = Id, Rect = new RectXZ(x, z, w, d), Floor = floor, Kind = kind };
            if (kind == RoomKind.Deck || kind == RoomKind.Grave || kind == RoomKind.Street || kind == RoomKind.Cove || kind == RoomKind.Shore)
                r.Outdoor = true;
            more?.Invoke(r);
            Rooms.Add(r);
            return r;
        }

        public ItemDef AddItem(string id, string room, float x, float z, ItemKind kind, Action<ItemDef> more = null)
        {
            var it = new ItemDef { Id = id, Act = Id, Room = room, Pos = new V2(x, z), Kind = kind };
            more?.Invoke(it);
            Items.Add(it);
            return it;
        }

        public void AddDoor(string a, string b, float x, float z, float width = 1.4f, string style = "door")
            => Doors.Add(new DoorDef { A = a, B = b, At = new V2(x, z), Width = width, Style = style });

        public void AddBlocker(string id, float x, float z, float w, float d, string until, string style)
            => Blockers.Add(new BlockerDef { Id = id, Rect = new RectXZ(x, z, w, d), Until = until, Style = style });

        public void AddGoal(string flag, float x, float z) => Goals.Add(new GoalDef { Flag = flag, Pos = new V2(x, z) });

        public VoiceCue AddVoice(string id, VoiceKind kind, float x, float z, Action<VoiceCue> more = null)
        {
            var v = new VoiceCue { Id = id, Kind = kind, From = new V2(x, z), Clip = id };
            more?.Invoke(v);
            Voices.Add(v);
            return v;
        }
    }
}
