namespace Undertow.Logic
{
    /// <summary>
    /// Act I — The Cursed Galleon (Merciful Requiem). Stern at z = 0, bow towards +z.
    /// Upper deck at y = 0, gun deck at y = -4. The middle of the upper deck (z 56–92) has collapsed
    /// into the cargo hold; the bow is reached from below.
    /// Route: stern → stairwell → gun deck (crew note, five bells, Vanessa's cabin, surgeon) →
    /// fore deck hatch (hook) → cargo hold (two crates) → bow ramp → locket → captain's cabin (clip moment).
    /// </summary>
    public static class Act1Content
    {
        public static ActDef Build()
        {
            var a = new ActDef
            {
                Id = 1,
                SceneName = "ActI_MercifulRequiem",
                Spawn = new V2(5.0f, 6.5f),
                SpawnYaw = 18f,
                FogDistance = 16f,
                FogColor = new[] { 0.30f, 0.36f, 0.38f },
                Moon = 0.55f,
                Ambience = "amb_galleon",
                EndFlag = "a1_left",
            };

            // ---- upper deck (open, wet planks, sea all around)
            a.AddRoom("a1_stern", 0, 0, 16, 18, 0, RoomKind.Deck, r => { r.Surface = Surface.WetWood; r.Ambient = 0.55f; r.SeaView = true; r.Landmark = "helm"; });
            a.AddRoom("a1_main", 0, 17, 16, 26, 0, RoomKind.Deck, r => { r.Surface = Surface.WetWood; r.Ambient = 0.5f; r.SeaView = true; r.Landmark = "mast"; });
            a.AddRoom("a1_fore", 0, 40, 16, 16, 0, RoomKind.Deck, r => { r.Surface = Surface.WetWood; r.Ambient = 0.48f; r.SeaView = true; r.Landmark = "foremast"; });
            a.AddRoom("a1_bowdeck", 2, 92, 12, 16, 0, RoomKind.Deck, r => { r.Surface = Surface.WetWood; r.Ambient = 0.5f; r.SeaView = true; r.Landmark = "bowsprit"; });
            a.AddRoom("a1_capt", 4, 99, 8, 8, 0, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.5f; r.Ambient = 0.14f; r.Surface = Surface.Rug; });

            // ---- stairwells
            a.AddRoom("a1_well", 6, 10, 4, 12, 0, RoomKind.Hall, r => { r.FloorTo = -4f; r.Ambient = 0.3f; });
            a.AddRoom("a1_stair", 10.5f, 50, 4, 18, 0, RoomKind.Hall, r => { r.FloorTo = -4f; r.Ambient = 0.12f; r.OpenFlag = "a1_hatch"; r.Outdoor = true; });
            a.AddRoom("a1_bowramp", 7, 84, 4, 10, -4, RoomKind.Hall, r => { r.FloorTo = 0f; r.Ambient = 0.22f; r.Outdoor = true; });

            // ---- gun deck (y = -4): central corridor, cabins port (x < 6) and starboard (x > 10)
            a.AddRoom("a1_spine", 6, 20, 4, 46, -4, RoomKind.Hall, r => { r.Ceil = true; r.CeilH = 2.6f; r.Ambient = 0.07f; });
            a.AddRoom("a1_crew", 0.5f, 26, 5.5f, 12, -4, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.4f; r.Ambient = 0.1f; });
            a.AddRoom("a1_music", 0.5f, 40, 5.5f, 12, -4, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.4f; r.Ambient = 0.14f; r.Safe = true; r.Surface = Surface.Rug; });
            a.AddRoom("a1_surg", 0.5f, 54, 5.5f, 10, -4, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.4f; r.Ambient = 0.06f; });
            a.AddRoom("a1_store", 10, 24, 5.5f, 12, -4, RoomKind.Hold, r => { r.Ceil = true; r.CeilH = 2.4f; r.Ambient = 0.06f; });
            a.AddRoom("a1_nessa", 10, 38, 5.5f, 11, -4, RoomKind.Cabin, r => { r.Ceil = true; r.CeilH = 2.4f; r.Ambient = 0.1f; r.Surface = Surface.Rug; });

            // ---- cargo hold, open to the collapsed deck above
            a.AddRoom("a1_cargo", 1.5f, 68, 13, 16, -4, RoomKind.Hold, r => { r.Ambient = 0.12f; r.Outdoor = true; r.Landmark = "brokendeck"; });

            a.AddDoor("a1_crew", "a1_spine", 6, 32, 1.2f);
            a.AddDoor("a1_music", "a1_spine", 6, 46, 1.2f);
            a.AddDoor("a1_surg", "a1_spine", 6, 59, 1.2f);
            a.AddDoor("a1_store", "a1_spine", 10, 30, 1.2f);
            a.AddDoor("a1_nessa", "a1_spine", 10, 43.5f, 1.2f);
            a.AddDoor("a1_capt", "a1_bowdeck", 8, 99, 1.2f);

            a.AddBlocker("b_nessa", 9.7f, 42.8f, 0.6f, 1.4f, "a1_music", "door");
            a.AddBlocker("b_surg", 5.7f, 58.2f, 0.6f, 1.6f, "a1_letter", "door");
            a.AddBlocker("b_bow", 7, 84, 4, 1.0f, "a1_crates", "grate");

            // ---- items
            a.AddItem("a1_oil_stern", "a1_stern", 3.0f, 6.0f, ItemKind.Oil, i => { i.Oil = Balance.OilFlask; i.Flag = "a1_oil_stern"; i.Once = true; i.Height = 0.95f; i.Prop = "oilskin"; });
            a.AddItem("a1_log_stern", "a1_stern", 12.6f, 5.5f, ItemKind.Note, i => { i.Flag = "a1_log_stern"; i.Height = 1.1f; i.Prop = "logstand"; i.Speaker = "thorne"; i.NoteStyle = "log"; });
            a.AddItem("a1_watch", "a1_stern", 8.0f, 2.3f, ItemKind.Examine, i => { i.Flag = "a1_watch"; i.Height = 1.15f; i.Prop = "pocketwatch"; i.Optional = true; });
            a.AddItem("a1_carving", "a1_main", 8.55f, 29.4f, ItemKind.Examine, i => { i.Flag = "a1_carving"; i.Height = 1.45f; i.Prop = "carving"; i.Sanity = -Balance.ExamineCost; i.Once = true; i.Optional = true; i.Yaw = 90f; });
            a.AddItem("a1_oil_spine", "a1_spine", 9.25f, 33f, ItemKind.Oil, i => { i.Oil = 36f; i.Flag = "a1_oil_spine"; i.Once = true; i.Height = 0.7f; i.Prop = "oilflask"; });
            a.AddItem("a1_crewnote", "a1_crew", 1.3f, 30.2f, ItemKind.Note, i => { i.Flag = "a1_crewnote"; i.Height = 0.48f; i.Prop = "bunknote"; i.Speaker = "crew"; i.NoteStyle = "letter"; });
            a.AddItem("a1_store_kegs", "a1_store", 12.6f, 26.4f, ItemKind.Examine, i => { i.Flag = "a1_kegs"; i.Height = 0.9f; i.Prop = "kegs"; i.Optional = true; });
            a.AddItem("a1_drawing2", "a1_store", 15.48f, 34.2f, ItemKind.Drawing, i => { i.Drawing = 2; i.Flag = "a1_drawing2"; i.Once = true; i.Height = 1.2f; i.Prop = "drawing"; i.Optional = true; i.Yaw = -90f; });
            a.AddItem("a1_portrait", "a1_music", 0.52f, 47.0f, ItemKind.Examine, i => { i.Flag = "a1_portrait"; i.Height = 1.6f; i.Prop = "portrait"; i.Sanity = -Balance.ExamineCost; i.Once = true; i.Optional = true; i.Yaw = 90f; });
            a.AddItem("a1_listen", "a1_music", 3.4f, 43.5f, ItemKind.Listen, i => { i.Puzzle = "a1"; i.Height = 0.95f; i.Prop = "musicbox"; });
            float[] bx = { 1.4f, 2.35f, 3.3f, 4.25f, 5.2f };
            for (int b = 0; b < 5; b++)
            {
                int idx = b;
                a.AddItem("a1_b" + b, "a1_music", bx[b], 50.6f, ItemKind.Bell, i => { i.Puzzle = "a1"; i.Index = idx; i.Height = 1.75f; i.Prop = "bell"; });
            }
            a.AddItem("a1_letter", "a1_nessa", 14.6f, 46.4f, ItemKind.Note, i => { i.Flag = "a1_letter"; i.Height = 0.48f; i.Prop = "childnote"; i.Speaker = "vanessa"; i.NoteStyle = "child"; });
            a.AddItem("a1_drawing1", "a1_nessa", 11.3f, 38.02f, ItemKind.Drawing, i => { i.Drawing = 1; i.Flag = "a1_drawing1"; i.Once = true; i.Height = 1.35f; i.Prop = "drawing"; i.Yaw = 0f; });
            a.AddItem("a1_exam_surg", "a1_surg", 3.0f, 58.6f, ItemKind.Examine, i => { i.Flag = "a1_table"; i.Height = 0.95f; i.Prop = "surgtable"; i.Sanity = -Balance.ExamineCost; i.Once = true; });
            a.AddItem("a1_hook", "a1_surg", 1.05f, 62.4f, ItemKind.Pickup, i => { i.Flag = "a1_hook"; i.Height = 1.7f; i.Prop = "hook"; i.Once = true; i.Yaw = 90f; });
            a.AddItem("a1_hatch", "a1_fore", 12.5f, 49.1f, ItemKind.Hatch, i => { i.Flag = "a1_hatch"; i.Need = new[] { "a1_hook" }; i.Height = 0.35f; i.Prop = "hatch"; i.Once = true; });
            a.AddItem("a1_oil_cargo", "a1_cargo", 2.4f, 71.5f, ItemKind.Oil, i => { i.Oil = 40f; i.Flag = "a1_oil_cargo"; i.Once = true; i.Height = 0.85f; i.Prop = "oilflask"; });
            a.AddItem("a1_chalk", "a1_cargo", 4.6f, 83.98f, ItemKind.Note, i => { i.Flag = "a1_chalk"; i.Height = 1.5f; i.Prop = "chalk"; i.NoteStyle = "carving"; i.Speaker = "crew"; i.Yaw = 180f; });
            a.AddItem("a1_crate_a", "a1_cargo", 4.0f, 72.0f, ItemKind.Crate, i =>
            {
                i.Puzzle = "a1c"; i.Height = 0.6f; i.Prop = "crate"; i.Goal = 2;
                i.Slots = new[] { new V2(4.0f, 72.0f), new V2(4.0f, 77.0f), new V2(12.5f, 77.0f) };
            });
            a.AddItem("a1_crate_b", "a1_cargo", 12.5f, 71.0f, ItemKind.Crate, i =>
            {
                i.Puzzle = "a1c"; i.Height = 0.6f; i.Prop = "crate"; i.Goal = 2;
                i.Slots = new[] { new V2(12.5f, 71.0f), new V2(8.5f, 71.0f), new V2(12.5f, 74.0f) };
            });
            a.AddItem("a1_reset", "a1_cargo", 9.6f, 68.7f, ItemKind.Lever, i => { i.Puzzle = "a1c"; i.Height = 1.1f; i.Prop = "lever"; });
            a.AddItem("a1_locket", "a1_bowdeck", 4.0f, 96.4f, ItemKind.Examine, i => { i.Flag = "a1_locket"; i.Height = 0.9f; i.Prop = "locket"; i.Once = true; });
            a.AddItem("a1_thorne_final", "a1_capt", 10.6f, 105.4f, ItemKind.Note, i => { i.Flag = "a1_thorne_final"; i.Height = 0.88f; i.Prop = "logbook"; i.Speaker = "thorne"; i.NoteStyle = "log"; });
            a.AddItem("a1_exit", "a1_capt", 6.2f, 105.3f, ItemKind.Exit, i => { i.Flag = "a1_left"; i.Need = new[] { "a1_crates", "a1_locket" }; i.Height = 0.92f; i.Prop = "twoboxes"; i.Achievement = "act1"; });

            // ---- voices
            a.AddVoice("a1_adrian_start", VoiceKind.Adrian, 5.0f, 6.5f, v => { v.TriggerRoom = "a1_stern"; v.Delay = 3.5f; });
            a.AddVoice("a1_adrian_nessa", VoiceKind.Adrian, 12.8f, 43.5f, v => { v.TriggerRoom = "a1_nessa"; v.Delay = 0.8f; });
            a.AddVoice("a1_echo_first", VoiceKind.Echo, 12.8f, 44.0f, v => { v.WhenFlag = "a1_music"; v.Delay = 7f; v.Leads = true; v.SetsFlag = "a1_echo_followed"; v.TriggerRoom = "a1_music"; });
            a.AddVoice("a1_mimic_first", VoiceKind.Mimic, -1.6f, 33.0f, v => { v.WhenFlag = "a1_letter"; v.TriggerRoom = "a1_main"; v.Height = -0.8f; v.Leads = true; v.SetsFlag = "a1_mimic_followed"; });

            a.AddGoal("a1_oil_stern", 3.0f, 6.0f);
            a.AddGoal("a1_log_stern", 12.6f, 5.5f);
            a.AddGoal("a1_music", 3.4f, 43.5f);
            a.AddGoal("a1_letter", 14.6f, 46.4f);
            a.AddGoal("a1_hook", 1.05f, 62.4f);
            a.AddGoal("a1_hatch", 12.5f, 49.1f);
            a.AddGoal("a1_crates", 12.5f, 75.5f);
            a.AddGoal("a1_locket", 4.0f, 96.4f);
            a.AddGoal("a1_left", 6.2f, 105.3f);

            a.CheckpointFlags.AddRange(new[] { "a1_log_stern", "a1_music", "a1_letter", "a1_hook", "a1_hatch", "a1_crates", "a1_locket" });
            return a;
        }
    }
}
