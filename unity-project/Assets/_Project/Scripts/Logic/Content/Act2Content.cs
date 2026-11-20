namespace Undertow.Logic
{
    /// <summary>
    /// Act II — The Ship Graveyard. A chain of wreck clearings joined by narrow plank passages
    /// between towering hulls, all along +z. Moonlight, black water on both sides.
    /// Lantern counts give the notes (one = La, two = Do, three = Mi); the bell hulk wants them
    /// smallest to largest. The ending splits: Mimic calls from the fog gate, Echo from a cove.
    /// </summary>
    public static class Act2Content
    {
        public static ActDef Build()
        {
            var a = new ActDef
            {
                Id = 2,
                SceneName = "ActII_ShipGraveyard",
                Spawn = new V2(6.0f, 4.5f),
                SpawnYaw = 0f,
                FogDistance = 22f,
                FogColor = new[] { 0.22f, 0.28f, 0.33f },
                Moon = 0.75f,
                Ambience = "amb_graveyard",
                EndFlag = "a2_left",
            };

            a.AddRoom("a2_boat", 0, 0, 12, 16, 0, RoomKind.Shore, r => { r.Surface = Surface.Sand; r.Ambient = 0.42f; r.SeaView = true; });
            a.AddRoom("a2_g1", 4, 15, 4, 40, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.36f; });
            a.AddRoom("a2_b", -2, 54, 16, 16, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.34f; r.Landmark = "lanterns1"; });
            a.AddRoom("a2_g2", 4, 69, 4, 40, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.32f; });
            a.AddRoom("a2_tent", 8, 84, 6, 8, 0, RoomKind.Hold, r => { r.Ceil = true; r.CeilH = 2.3f; r.Surface = Surface.Wood; r.Ambient = 0.1f; r.Landmark = "tent"; });
            a.AddRoom("a2_c", -2, 108, 18, 16, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.3f; });
            a.AddRoom("a2_g3", 4, 123, 4, 40, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.3f; });
            a.AddRoom("a2_d", -2, 162, 16, 16, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.33f; r.Landmark = "lanterns3"; });
            a.AddRoom("a2_g4", 4, 177, 4, 40, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.14f; r.Landmark = "arches"; });
            a.AddRoom("a2_e", -4, 216, 20, 16, 0, RoomKind.Hold, r => { r.Ceil = true; r.CeilH = 4.2f; r.Surface = Surface.Wood; r.Ambient = 0.06f; r.Landmark = "lanterns2"; });
            a.AddRoom("a2_g5", 4, 231, 4, 36, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.3f; });
            a.AddRoom("a2_hulk", -2, 266, 18, 18, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.28f; r.Landmark = "figurehead"; });
            a.AddRoom("a2_gate", 2, 283, 10, 16, 0, RoomKind.Grave, r => { r.Surface = Surface.WetWood; r.Ambient = 0.4f; r.SeaView = true; r.Landmark = "foggate"; });
            a.AddRoom("a2_cove", 12, 288, 12, 12, 0, RoomKind.Cove, r => { r.Surface = Surface.Sand; r.Ambient = 0.45f; r.SeaView = true; r.Safe = true; r.Landmark = "cove"; });

            a.AddDoor("a2_tent", "a2_g2", 8, 88, 1.3f, "gap");
            a.AddDoor("a2_e", "a2_g4", 6, 216, 3.0f, "arch");
            a.AddDoor("a2_e", "a2_g5", 6, 232, 3.0f, "arch");
            a.AddDoor("a2_gate", "a2_cove", 12, 293.5f, 1.6f, "gap");

            a.AddBlocker("b_g3", 3.6f, 122.6f, 4.8f, 1.6f, "a2_clear", "debris");
            a.AddBlocker("b_gate", 2.0f, 282.6f, 10f, 1.6f, "a2_nail", "fog");

            a.AddItem("a2_map", "a2_boat", 3.0f, 7.0f, ItemKind.Note, i => { i.Flag = "a2_map"; i.Height = 0.75f; i.Prop = "map"; i.NoteStyle = "document"; });
            a.AddItem("a2_oil_boat", "a2_boat", 9.0f, 8.5f, ItemKind.Oil, i => { i.Oil = 38f; i.Flag = "a2_oil1"; i.Once = true; i.Height = 0.6f; i.Prop = "oilflask"; });
            a.AddItem("a2_carving", "a2_g1", 4.02f, 36.0f, ItemKind.Examine, i => { i.Flag = "a2_carving"; i.Height = 1.35f; i.Prop = "carving"; i.Achievement = "carving"; i.Optional = true; i.Yaw = 90f; });
            a.AddItem("a2_t0", "a2_b", 6.0f, 62.0f, ItemKind.Bell, i => { i.Puzzle = "a2c"; i.Index = 0; i.Flag = "a2_heard0"; i.Height = 1.8f; i.Prop = "bell"; });
            a.AddItem("a2_log_b", "a2_b", 2.0f, 66.0f, ItemKind.Note, i => { i.Flag = "a2_read0"; i.Height = 1.3f; i.Prop = "nameboard"; i.NoteStyle = "carving"; });
            a.AddItem("a2_headphones", "a2_tent", 11.6f, 89.6f, ItemKind.Examine, i => { i.Flag = "a2_headphones"; i.Height = 0.45f; i.Prop = "headphones"; i.Sanity = -Balance.ExamineCost; i.Once = true; i.Optional = true; });
            a.AddItem("a2_crate", "a2_c", 3.0f, 114.0f, ItemKind.Crate, i =>
            {
                i.Puzzle = "a2x"; i.Height = 0.6f; i.Prop = "debriscrate"; i.Goal = 2;
                i.Slots = new[] { new V2(3.0f, 114.0f), new V2(8.0f, 114.0f), new V2(12.5f, 118.5f) };
            });
            a.AddItem("a2_log_c", "a2_c", 10.0f, 111.5f, ItemKind.Note, i => { i.Flag = "a2_readc"; i.Height = 1.2f; i.Prop = "nameboard"; i.NoteStyle = "carving"; });
            a.AddItem("a2_t2", "a2_d", 6.0f, 170.0f, ItemKind.Bell, i => { i.Puzzle = "a2c"; i.Index = 2; i.Flag = "a2_heard2"; i.Height = 1.8f; i.Prop = "bell"; });
            a.AddItem("a2_log_d", "a2_d", 2.0f, 172.0f, ItemKind.Note, i => { i.Flag = "a2_read2"; i.Height = 1.3f; i.Prop = "nameboard"; i.NoteStyle = "carving"; });
            a.AddItem("a2_oil_e", "a2_e", 3.0f, 222.0f, ItemKind.Oil, i => { i.Oil = 40f; i.Flag = "a2_oil2"; i.Once = true; i.Height = 0.8f; i.Prop = "oilflask"; });
            a.AddItem("a2_t1", "a2_e", 8.0f, 224.0f, ItemKind.Bell, i => { i.Puzzle = "a2c"; i.Index = 1; i.Flag = "a2_heard1"; i.Height = 1.8f; i.Prop = "bell"; });
            a.AddItem("a2_log_e", "a2_e", 12.0f, 226.0f, ItemKind.Note, i => { i.Flag = "a2_read1"; i.Height = 1.3f; i.Prop = "nameboard"; i.NoteStyle = "carving"; });
            a.AddItem("a2_ruth_letter", "a2_e", -2.5f, 229.5f, ItemKind.Note, i => { i.Flag = "a2_ruth"; i.Height = 0.8f; i.Prop = "sealedletter"; i.Speaker = "ruth"; i.NoteStyle = "letter"; });
            a.AddItem("a2_drawing3", "a2_e", 15.98f, 230.4f, ItemKind.Drawing, i => { i.Drawing = 3; i.Flag = "a2_drawing3"; i.Once = true; i.Height = 1.2f; i.Prop = "drawing"; i.Yaw = -90f; });
            a.AddItem("a2_listen", "a2_hulk", 4.0f, 272.0f, ItemKind.Listen, i => { i.Puzzle = "a2"; i.Need = new[] { "a2_heard0", "a2_heard1", "a2_heard2" }; i.Height = 1.2f; i.Prop = "shellhorn"; });
            a.AddItem("a2_h0", "a2_hulk", 2.0f, 276.0f, ItemKind.Bell, i => { i.Puzzle = "a2"; i.Index = 0; i.Height = 1.6f; i.Prop = "bell"; });
            a.AddItem("a2_h1", "a2_hulk", 7.0f, 276.0f, ItemKind.Bell, i => { i.Puzzle = "a2"; i.Index = 1; i.Height = 1.75f; i.Prop = "bell"; });
            a.AddItem("a2_h2", "a2_hulk", 12.0f, 276.0f, ItemKind.Bell, i => { i.Puzzle = "a2"; i.Index = 2; i.Height = 1.9f; i.Prop = "bell"; });
            a.AddItem("a2_nail", "a2_hulk", 7.0f, 282.4f, ItemKind.Pickup, i => { i.Flag = "a2_nail"; i.Need = new[] { "a2_bells" }; i.Height = 1.4f; i.Prop = "nail"; i.Once = true; i.Yaw = 180f; });
            a.AddItem("a2_exit_gate", "a2_gate", 7.0f, 297.0f, ItemKind.Exit, i => { i.Flag = "a2_left"; i.Height = 1.2f; i.Prop = "foggate"; i.Achievement = "follow_mimic"; });
            a.AddItem("a2_exit_cove", "a2_cove", 20.0f, 297.0f, ItemKind.Boat, i => { i.Flag = "a2_left"; i.Height = 0.7f; i.Prop = "rowboat"; i.Achievement = "follow_echo"; });

            a.AddVoice("a2_echo_lanterns", VoiceKind.Echo, 6.0f, 52.0f, v => { v.TriggerRoom = "a2_g1"; v.Delay = 2f; v.Leads = true; });
            a.AddVoice("a2_adrian_tent", VoiceKind.Adrian, 11.0f, 88.0f, v => { v.TriggerRoom = "a2_tent"; v.Delay = 0.6f; });
            a.AddVoice("a2_mimic_water", VoiceKind.Mimic, 1.5f, 140.0f, v => { v.TriggerRoom = "a2_g3"; v.Height = -0.6f; v.Delay = 4f; });
            a.AddVoice("a2_echo_dark", VoiceKind.Echo, 6.0f, 214.0f, v => { v.TriggerRoom = "a2_g4"; v.Delay = 3f; v.Leads = true; });
            a.AddVoice("a2_echo_cove", VoiceKind.Echo, 19.0f, 296.0f, v => { v.WhenFlag = "a2_nail"; v.TriggerRoom = "a2_gate"; v.Delay = 1.5f; v.Leads = true; v.SetsFlag = "a2_echo_heard"; });
            a.AddVoice("a2_mimic_gate", VoiceKind.Mimic, 7.0f, 298.5f, v => { v.WhenFlag = "a2_nail"; v.TriggerRoom = "a2_gate"; v.Delay = 4.5f; v.Leads = true; v.SetsFlag = "a2_mimic_heard"; });

            a.AddGoal("a2_map", 3.0f, 7.0f);
            a.AddGoal("a2_heard0", 6.0f, 62.0f);
            a.AddGoal("a2_clear", 12.5f, 118.5f);
            a.AddGoal("a2_heard2", 6.0f, 170.0f);
            a.AddGoal("a2_heard1", 8.0f, 224.0f);
            a.AddGoal("a2_bells", 7.0f, 276.0f);
            a.AddGoal("a2_nail", 7.0f, 282.4f);
            a.AddGoal("a2_left", 11.0f, 293.5f);

            a.CheckpointFlags.AddRange(new[] { "a2_map", "a2_heard0", "a2_clear", "a2_heard2", "a2_heard1", "a2_bells", "a2_nail" });
            return a;
        }
    }
}
