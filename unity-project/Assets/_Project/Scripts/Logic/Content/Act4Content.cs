namespace Undertow.Logic
{
    /// <summary>
    /// Act IV — The Sunken Temple. Turquoise shafts of light fall through the water ceiling.
    /// A shaft in the great cross hall is caught by four mirrors (turned 1-2-3-1 times, as their
    /// scratches show), then by the lens in the whale gallery, then by the altar. The Undertow is
    /// never an enemy: in darkness it comes closer, it never strikes. Oil is a choice here:
    /// the lantern, or the three braziers that make safe light for good.
    /// </summary>
    public static class Act4Content
    {
        public static readonly int[] MirrorAnswers = { 1, 2, 3, 1 };
        public static readonly V2 BeamSource = new V2(10f, 26f);
        public static readonly V2[] MirrorPos = { new V2(-3f, 32f), new V2(23f, 40f), new V2(-3f, 48f), new V2(23f, 53.5f) };
        public static readonly V2 LensPos = new V2(10f, 66f);
        public static readonly V2 AltarPos = new V2(10f, 100f);

        public static ActDef Build()
        {
            var a = new ActDef
            {
                Id = 4,
                SceneName = "ActIV_SunkenTemple",
                Spawn = new V2(9.0f, 3.0f),
                SpawnYaw = 0f,
                FogDistance = 26f,
                FogColor = new[] { 0.05f, 0.24f, 0.27f },
                Moon = 0f,
                Ambience = "amb_temple",
                EndFlag = "a4_end",
            };

            a.AddRoom("a4_nave", 0, 0, 18, 14, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 6f; r.Surface = Surface.Stone; r.Ambient = 0.2f; r.Safe = true; r.Landmark = "columns"; });
            a.AddRoom("a4_hall", 6, 14, 6, 10, 0, RoomKind.Hall, r => { r.Ceil = true; r.CeilH = 4.6f; r.Surface = Surface.Stone; r.Ambient = 0.1f; r.Landmark = "burned"; });
            a.AddRoom("a4_cross", 0, 24, 20, 34, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 9f; r.Surface = Surface.Stone; r.Ambient = 0.12f; r.Landmark = "columns"; });
            a.AddRoom("a4_m1r", -6, 27, 6, 10, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 5f; r.Surface = Surface.Stone; r.Ambient = 0.08f; });
            a.AddRoom("a4_m2r", 20, 35, 6, 10, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 5f; r.Surface = Surface.Stone; r.Ambient = 0.08f; });
            a.AddRoom("a4_m3r", -6, 43, 6, 10, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 5f; r.Surface = Surface.Stone; r.Ambient = 0.08f; });
            a.AddRoom("a4_m4r", 20, 48, 6, 10, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 5f; r.Surface = Surface.Stone; r.Ambient = 0.08f; });
            a.AddRoom("a4_gal", -2, 58, 24, 18, 0, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 10f; r.Surface = Surface.Stone; r.Ambient = 0.16f; r.SeaView = true; r.Landmark = "whale"; });
            a.AddRoom("a4_ramp", 7, 76, 6, 14, 0, RoomKind.Hall, r => { r.FloorTo = -1.6f; r.Ceil = true; r.CeilH = 4.8f; r.Surface = Surface.Stone; r.Ambient = 0.14f; });
            a.AddRoom("a4_altar", 0, 90, 20, 18, -1.6f, RoomKind.Temple, r => { r.Ceil = true; r.CeilH = 10f; r.Surface = Surface.Stone; r.Ambient = 0.22f; r.Landmark = "altar"; });

            a.AddDoor("a4_nave", "a4_hall", 9, 14, 2.6f, "arch");
            a.AddDoor("a4_hall", "a4_cross", 9, 24, 3.2f, "arch");
            a.AddDoor("a4_m1r", "a4_cross", 0, 32, 8f, "arch");
            a.AddDoor("a4_m2r", "a4_cross", 20, 40, 8f, "arch");
            a.AddDoor("a4_m3r", "a4_cross", 0, 48, 8f, "arch");
            a.AddDoor("a4_m4r", "a4_cross", 20, 53, 9f, "arch");
            a.AddDoor("a4_gal", "a4_cross", 10, 58, 18f, "arch");

            a.AddItem("a4_chest", "a4_nave", 4.0f, 6.0f, ItemKind.Pickup, i => { i.Flag = "a4_prism"; i.Height = 0.8f; i.Prop = "chest"; i.Once = true; });
            a.AddItem("a4_raynote", "a4_nave", 14.0f, 8.0f, ItemKind.Note, i => { i.Flag = "a4_raynote"; i.Height = 1.05f; i.Prop = "pedestal"; i.NoteStyle = "carving"; });
            string[] mid = { "a4_m1", "a4_m2", "a4_m3", "a4_m4" };
            string[] mroom = { "a4_m1r", "a4_m2r", "a4_m3r", "a4_m4r" };
            for (int m = 0; m < 4; m++)
            {
                int k = m;
                a.AddItem(mid[m], mroom[m], MirrorPos[m].X, MirrorPos[m].Z, ItemKind.Mirror, i =>
                {
                    i.Puzzle = "a4"; i.Index = k; i.Answer = MirrorAnswers[k]; i.Height = 1.4f; i.Prop = "mirror";
                });
            }
            a.AddItem("a4_brazier1", "a4_cross", 10.0f, 44.0f, ItemKind.Brazier, i => { i.Flag = "a4_lit1"; i.Oil = Balance.BrazierCost; i.Height = 0.9f; i.Prop = "brazier"; i.Optional = true; });
            a.AddItem("a4_oil", "a4_gal", 1.0f, 61.0f, ItemKind.Oil, i => { i.Oil = 40f; i.Flag = "a4_oil"; i.Once = true; i.Height = 0.8f; i.Prop = "oilflask"; });
            a.AddItem("a4_lens", "a4_gal", LensPos.X, LensPos.Z, ItemKind.Pickup, i => { i.Flag = "a4_lens"; i.Need = new[] { "a4_m4" }; i.Height = 1.25f; i.Prop = "lens"; i.Once = true; });
            a.AddItem("a4_whale_note", "a4_gal", 21.98f, 70.0f, ItemKind.Note, i => { i.Flag = "a4_whale_note"; i.Height = 1.3f; i.Prop = "windowplaque"; i.NoteStyle = "carving"; i.Yaw = -90f; });
            a.AddItem("a4_voices", "a4_gal", 16.0f, 74.6f, ItemKind.Listen, i => { i.Puzzle = "a4v"; i.Flag = "a4_voices"; i.Height = 1.2f; i.Prop = "shellwall"; i.Optional = true; });
            a.AddItem("a4_brazier2", "a4_gal", 4.0f, 72.0f, ItemKind.Brazier, i => { i.Flag = "a4_lit2"; i.Oil = Balance.BrazierCost; i.Height = 0.9f; i.Prop = "brazier"; i.Optional = true; });
            a.AddItem("a4_drawing6", "a4_altar", 19.98f, 92.4f, ItemKind.Drawing, i => { i.Drawing = 6; i.Flag = "a4_drawing6"; i.Once = true; i.Height = 1.0f; i.Prop = "drawing"; i.Yaw = -90f; });
            a.AddItem("a4_tape", "a4_altar", 14.0f, 91.6f, ItemKind.Tape, i => { i.Flag = "a4_tape"; i.Height = 0.85f; i.Prop = "taperecorder"; i.Achievement = "cassette"; i.Speaker = "vanessa"; i.NoteStyle = "tape"; });
            a.AddItem("a4_brazier3", "a4_altar", 3.0f, 94.0f, ItemKind.Brazier, i => { i.Flag = "a4_lit3"; i.Oil = Balance.BrazierCost; i.Height = 0.9f; i.Prop = "brazier"; i.Optional = true; });
            a.AddItem("a4_altarstone", "a4_altar", AltarPos.X, AltarPos.Z, ItemKind.Altar, i =>
            {
                i.Flag = "a4_altar"; i.Height = 1.0f; i.Prop = "altar";
                i.Need = new[] { "a4_prism", "a4_lens", "a4_m1", "a4_m2", "a4_m3", "a4_m4", "a1_locket" };
            });
            a.AddItem("a4_choice_echo", "a4_altar", 3.0f, 104.0f, ItemKind.Choice, i => { i.Puzzle = "altar"; i.Index = 1; i.Flag = "a4_end"; i.Need = new[] { "a4_altar" }; i.Height = 1.0f; i.Prop = "echobox"; });
            a.AddItem("a4_choice_mimic", "a4_altar", 17.0f, 104.0f, ItemKind.Choice, i => { i.Puzzle = "altar"; i.Index = 2; i.Flag = "a4_end"; i.Need = new[] { "a4_altar" }; i.Height = 1.0f; i.Prop = "mimicbox"; });

            a.AddVoice("a4_echo_bright", VoiceKind.Echo, 9.0f, 12.0f, v => { v.TriggerRoom = "a4_nave"; v.Delay = 6f; });
            a.AddVoice("a4_mimic_window", VoiceKind.Mimic, 23.5f, 68.0f, v => { v.TriggerRoom = "a4_gal"; v.Delay = 5f; v.Height = 2.5f; });
            a.AddVoice("a4_tape_auto", VoiceKind.Echo, 14.0f, 91.6f, v => { v.TriggerRoom = "a4_altar"; v.Delay = 1.0f; v.SetsFlag = "a4_tape_heard"; });
            a.AddVoice("a4_echo_choice", VoiceKind.Echo, 3.0f, 104.0f, v => { v.WhenFlag = "a4_altar"; v.TriggerRoom = "a4_altar"; v.Delay = 9f; v.Leads = true; });
            a.AddVoice("a4_mimic_choice", VoiceKind.Mimic, 17.0f, 104.0f, v => { v.WhenFlag = "a4_altar"; v.TriggerRoom = "a4_altar"; v.Delay = 13f; v.Leads = true; });

            a.AddGoal("a4_prism", 4.0f, 6.0f);
            a.AddGoal("a4_raynote", 14.0f, 8.0f);
            a.AddGoal("a4_m1", MirrorPos[0].X, MirrorPos[0].Z);
            a.AddGoal("a4_m2", MirrorPos[1].X, MirrorPos[1].Z);
            a.AddGoal("a4_m3", MirrorPos[2].X, MirrorPos[2].Z);
            a.AddGoal("a4_m4", MirrorPos[3].X, MirrorPos[3].Z);
            a.AddGoal("a4_lens", LensPos.X, LensPos.Z);
            a.AddGoal("a4_altar", AltarPos.X, AltarPos.Z);
            a.AddGoal("a4_end", 10.0f, 104.0f);

            a.CheckpointFlags.AddRange(new[] { "a4_prism", "a4_m2", "a4_m4", "a4_lens", "a4_altar" });
            return a;
        }
    }
}
