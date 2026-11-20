namespace Undertow.Logic
{
    /// <summary>
    /// Act III — The Forsaken Port, Port Saint-Clair, 1724. Knee-deep water in the streets.
    /// Five buildings give three moments; the tower's three dials want 11:40 (the town froze),
    /// 03:17 (Vanessa's last call) and 06:05 (Adrian woke to an empty tent). The water drains,
    /// the governor's house opens: the pact, Marisol's room, the family tree ("Ruth Crane"),
    /// and the mirror hall where Adrian finally hears his name — and may answer.
    /// </summary>
    public static class Act3Content
    {
        public const float WaterLevel = 0.42f;
        public static readonly string[] DialOptions = { "12:00", "11:40", "03:17", "06:05" };
        public static readonly string[] ClueFlags = { "a3_customs", "a3_tavern", "a3_ware", "a3_chapel", "a3_house" };

        public static ActDef Build()
        {
            var a = new ActDef
            {
                Id = 3,
                SceneName = "ActIII_ForsakenPort",
                Spawn = new V2(8.0f, 4.0f),
                SpawnYaw = 0f,
                FogDistance = 20f,
                FogColor = new[] { 0.27f, 0.32f, 0.35f },
                Moon = 0.6f,
                Ambience = "amb_port",
                EndFlag = "a3_left",
            };
            float w = WaterLevel;

            a.AddRoom("a3_dock", 0, 0, 18, 18, 0, RoomKind.Street, r => { r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.4f; r.SeaView = true; r.Landmark = "pier"; });
            a.AddRoom("a3_street", 5, 16, 6, 80, 0, RoomKind.Street, r => { r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.34f; });
            a.AddRoom("a3_customs", -9, 22, 14, 14, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 3.2f; r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.16f; });
            a.AddRoom("a3_tavern", 11, 24, 14, 14, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 3.0f; r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.14f; });
            a.AddRoom("a3_ware", -11, 44, 16, 14, 0, RoomKind.Hold, r => { r.Ceil = true; r.CeilH = 4.2f; r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.1f; });
            a.AddRoom("a3_chapel", 11, 46, 16, 14, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 5.5f; r.Surface = Surface.Stone; r.Ambient = 0.2f; r.Safe = true; r.Landmark = "altarcandles"; });
            a.AddRoom("a3_square", -4, 62, 24, 16, 0, RoomKind.Street, r => { r.Surface = Surface.Shallow; r.Water = w; r.Ambient = 0.36f; r.Landmark = "statue"; });
            a.AddRoom("a3_house", -14, 66, 10, 14, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.7f; r.Surface = Surface.Wood; r.Ambient = 0.12f; });
            a.AddRoom("a3_tower", 2, 96, 12, 16, 0, RoomKind.Hall, r => { r.Ceil = true; r.CeilH = 8.0f; r.Surface = Surface.Stone; r.Ambient = 0.15f; r.Landmark = "clockwork"; });

            // The governor's house (opens when the water drains).
            a.AddRoom("a3_mhall", 20, 64, 10, 14, 0, RoomKind.Hall, r => { r.Ceil = true; r.CeilH = 4.2f; r.Surface = Surface.Stone; r.Ambient = 0.1f; r.Landmark = "chandelier"; });
            a.AddRoom("a3_study", 30, 64, 10, 8, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 3.4f; r.Surface = Surface.Rug; r.Ambient = 0.1f; });
            a.AddRoom("a3_marisol", 30, 72, 10, 8, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 3.4f; r.Surface = Surface.Rug; r.Ambient = 0.12f; });
            a.AddRoom("a3_gallery", 20, 78, 12, 10, 0, RoomKind.Hall, r => { r.Ceil = true; r.CeilH = 4.2f; r.Surface = Surface.Wood; r.Ambient = 0.1f; r.Landmark = "familytree"; });
            a.AddRoom("a3_mirror", 18, 88, 18, 16, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 6.0f; r.Surface = Surface.Stone; r.Ambient = 0.16f; r.Landmark = "mirrors"; });
            a.AddRoom("a3_sea", 36, 92, 10, 8, 0, RoomKind.Street, r => { r.FloorTo = -2.4f; r.RampAlongX = true; r.Surface = Surface.Stone; r.Ambient = 0.3f; r.SeaView = true; r.OpenFlag = "a3_sea_open"; r.Landmark = "seastairs"; });

            a.AddDoor("a3_customs", "a3_street", 5, 29, 1.4f);
            a.AddDoor("a3_tavern", "a3_street", 11, 31, 1.4f);
            a.AddDoor("a3_ware", "a3_street", 5, 51, 2.4f, "arch");
            a.AddDoor("a3_chapel", "a3_street", 11, 53, 1.8f, "arch");
            a.AddDoor("a3_house", "a3_square", -4, 72, 1.2f);
            a.AddDoor("a3_tower", "a3_street", 8, 96, 1.8f, "arch");
            a.AddDoor("a3_mhall", "a3_square", 20, 70, 2.2f, "arch");
            a.AddDoor("a3_study", "a3_mhall", 30, 68, 1.2f);
            a.AddDoor("a3_marisol", "a3_mhall", 30, 75, 1.2f);
            a.AddDoor("a3_gallery", "a3_mhall", 25, 78, 1.6f, "arch");
            a.AddDoor("a3_mirror", "a3_gallery", 26, 88, 2.0f, "arch");
            a.AddDoor("a3_sea", "a3_mirror", 36, 96, 2.2f, "arch");

            a.AddBlocker("b_mansion", 19.5f, 68.6f, 1.0f, 2.8f, "a3_clocks", "gate");

            a.AddItem("a3_oil_dock", "a3_dock", 4.0f, 8.0f, ItemKind.Oil, i => { i.Oil = 36f; i.Flag = "a3_oil1"; i.Once = true; i.Height = 0.9f; i.Prop = "oilflask"; });
            a.AddItem("a3_board", "a3_dock", 12.0f, 8.0f, ItemKind.Note, i => { i.Flag = "a3_board"; i.Height = 1.4f; i.Prop = "board"; i.NoteStyle = "board"; });
            a.AddItem("a3_customs_log", "a3_customs", -5.0f, 29.0f, ItemKind.Note, i => { i.Flag = "a3_customs"; i.Height = 1.0f; i.Prop = "ledger"; i.NoteStyle = "document"; });
            a.AddItem("a3_tavern_note", "a3_tavern", 21.0f, 33.0f, ItemKind.Note, i => { i.Flag = "a3_tavern"; i.Height = 1.1f; i.Prop = "counternote"; i.NoteStyle = "letter"; });
            a.AddItem("a3_oil_ware", "a3_ware", -7.0f, 47.0f, ItemKind.Oil, i => { i.Oil = 38f; i.Flag = "a3_oil2"; i.Once = true; i.Height = 0.9f; i.Prop = "oilflask"; });
            a.AddItem("a3_chest", "a3_ware", -3.0f, 54.0f, ItemKind.Examine, i => { i.Flag = "a3_ware"; i.Height = 0.75f; i.Prop = "sealedchest"; i.Sanity = -Balance.ExamineCost; i.Once = true; });
            a.AddItem("a3_chapel_note", "a3_chapel", 23.0f, 53.0f, ItemKind.Note, i => { i.Flag = "a3_chapel"; i.Height = 1.05f; i.Prop = "lectern"; i.NoteStyle = "document"; });
            a.AddItem("a3_statue", "a3_square", 8.0f, 70.0f, ItemKind.Examine, i => { i.Flag = "a3_statue"; i.Height = 1.5f; i.Prop = "statue"; i.Sanity = -2f; i.Once = true; i.Optional = true; });
            a.AddItem("a3_house_watch", "a3_house", -10.0f, 73.0f, ItemKind.Note, i => { i.Flag = "a3_house"; i.Height = 0.85f; i.Prop = "wristwatch"; i.NoteStyle = "letter"; });
            a.AddItem("a3_drawing4", "a3_house", -13.98f, 77.0f, ItemKind.Drawing, i => { i.Drawing = 4; i.Flag = "a3_drawing4"; i.Once = true; i.Height = 1.3f; i.Prop = "drawing"; i.Yaw = 90f; });
            string[] dialNames = { "a3_c1", "a3_c2", "a3_c3" };
            float[] dx = { 5.0f, 8.0f, 11.0f };
            float[] dz = { 104.5f, 106.0f, 104.5f };
            int[] ans = { 1, 2, 3 };
            for (int d = 0; d < 3; d++)
            {
                int k = d;
                a.AddItem(dialNames[d], "a3_tower", dx[d], dz[d], ItemKind.Clock, i =>
                {
                    i.Puzzle = "a3"; i.Index = k; i.Options = DialOptions; i.Answer = ans[k]; i.Need = ClueFlags;
                    i.Height = 1.5f; i.Prop = "dial"; i.Yaw = 180f;
                });
            }
            a.AddItem("a3_drawing5", "a3_tower", 13.98f, 110.6f, ItemKind.Drawing, i => { i.Drawing = 5; i.Flag = "a3_drawing5"; i.Once = true; i.Height = 1.1f; i.Prop = "drawing"; i.Yaw = -90f; });
            a.AddItem("a3_pact", "a3_study", 36.0f, 68.0f, ItemKind.Note, i => { i.Flag = "a3_pact"; i.Height = 0.95f; i.Prop = "pact"; i.NoteStyle = "document"; i.Speaker = "crane"; });
            a.AddItem("a3_marisol_room", "a3_marisol", 37.5f, 77.0f, ItemKind.Examine, i => { i.Flag = "a3_marisol"; i.Height = 0.7f; i.Prop = "doll"; i.Sanity = -Balance.ExamineCost; i.Once = true; });
            a.AddItem("a3_tree", "a3_gallery", 31.97f, 83.0f, ItemKind.Note, i => { i.Flag = "a3_tree"; i.Height = 1.6f; i.Prop = "familytree"; i.NoteStyle = "document"; i.Achievement = "family_tree"; i.Yaw = -90f; });
            a.AddItem("a3_mirror_main", "a3_mirror", 27.0f, 103.95f, ItemKind.Choice, i => { i.Puzzle = "a3_mirror"; i.Flag = "a3_choice"; i.Need = new[] { "a3_tree" }; i.Height = 1.6f; i.Prop = "greatmirror"; i.Yaw = 180f; });
            a.AddItem("a3_exit", "a3_sea", 44.5f, 96.0f, ItemKind.Exit, i => { i.Flag = "a3_left"; i.Need = new[] { "a3_sea_open" }; i.Height = -1.0f; i.Prop = "seastairs"; i.Achievement = "act3"; });

            a.AddVoice("a3_echo_feet", VoiceKind.Echo, 8.0f, 22.0f, v => { v.TriggerRoom = "a3_dock"; v.Delay = 5f; v.Leads = true; });
            a.AddVoice("a3_mimic_square", VoiceKind.Mimic, -2.5f, 63.0f, v => { v.TriggerRoom = "a3_square"; v.Height = 0.2f; v.Delay = 3f; });
            a.AddVoice("a3_adrian_drain", VoiceKind.Adrian, 8.0f, 104.0f, v => { v.WhenFlag = "a3_clocks"; v.TriggerRoom = "a3_tower"; v.Delay = 6.5f; });
            a.AddVoice("a3_adrian_tree", VoiceKind.Adrian, 26.0f, 86.0f, v => { v.WhenFlag = "a3_tree"; v.TriggerRoom = "a3_gallery"; v.Delay = 1.2f; });
            a.AddVoice("a3_echo_mirror", VoiceKind.Echo, 27.0f, 100.0f, v => { v.WhenFlag = "a3_tree"; v.TriggerRoom = "a3_mirror"; v.Delay = 1.5f; v.Leads = true; });

            a.AddGoal("a3_board", 12.0f, 8.0f);
            a.AddGoal("a3_customs", -5.0f, 29.0f);
            a.AddGoal("a3_tavern", 21.0f, 33.0f);
            a.AddGoal("a3_ware", -3.0f, 54.0f);
            a.AddGoal("a3_chapel", 23.0f, 53.0f);
            a.AddGoal("a3_house", -10.0f, 73.0f);
            a.AddGoal("a3_clocks", 8.0f, 106.0f);
            a.AddGoal("a3_tree", 31.97f, 83.0f);
            a.AddGoal("a3_choice", 27.0f, 102.6f);
            a.AddGoal("a3_left", 44.5f, 96.0f);

            a.CheckpointFlags.AddRange(new[] { "a3_board", "a3_chapel", "a3_house", "a3_clocks", "a3_pact", "a3_tree", "a3_choice" });
            return a;
        }
    }
}
