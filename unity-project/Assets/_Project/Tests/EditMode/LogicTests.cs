using System;
using System.Linq;
using NUnit.Framework;
using Undertow.Logic;

namespace Undertow.Tests
{
    /// <summary>Rules from the design tables. Pure logic: these run in Unity's Test Runner (EditMode).</summary>
    [TestFixture]
    public class LogicRuleTests
    {
        [Test]
        public void BalanceMatchesDesignTable()
        {
            Assert.AreEqual(0.8f, Balance.DarkDrain, 1e-6f);
            Assert.AreEqual(1.2f, Balance.SeaDrain, 1e-6f);
            Assert.AreEqual(6f, Balance.SeaDelay, 1e-6f);
            Assert.AreEqual(1f, Balance.SafeRegen, 1e-6f);
            Assert.AreEqual(4f, Balance.ExamineCost, 1e-6f);
            Assert.AreEqual(15f, Balance.MusicReward, 1e-6f);
            Assert.AreEqual(3f, Balance.Hysteresis, 1e-6f);
            Assert.AreEqual(0.075f, Balance.OilBurn, 1e-6f);
            Assert.AreEqual(64f, Balance.StartOil, 1e-6f);
        }

        [Test]
        public void DarknessDrainsPointEightPerSecond()
        {
            var s = new SanityModel(80f);
            for (int i = 0; i < 10; i++) s.Tick(new SanityInput { Dark = true, Dt = 1f });
            Assert.AreEqual(72f, s.Value, 1e-3f);
        }

        [Test]
        public void SeaStareOnlyDrainsAfterSixSeconds()
        {
            var s = new SanityModel(80f);
            for (int i = 0; i < 5; i++) s.Tick(new SanityInput { StaringAtSea = true, Dt = 1f });
            Assert.AreEqual(80f, s.Value, 1e-3f, "no drain before the delay");
            for (int i = 0; i < 4; i++) s.Tick(new SanityInput { StaringAtSea = true, Dt = 1f });
            Assert.Less(s.Value, 80f);
            Assert.AreEqual(80f - 1.2f * 3f, s.Value, 1e-3f);
        }

        [Test]
        public void SafeLightRegenerates()
        {
            var s = new SanityModel(50f);
            s.Tick(new SanityInput { InSafeLight = true, Dt = 5f });
            Assert.AreEqual(55f, s.Value, 1e-3f);
        }

        [Test]
        public void TierHysteresisPreventsFlicker()
        {
            var s = new SanityModel(71f);
            Assert.AreEqual(SanityTier.Stable, s.Tier);
            s.Apply(-2f);                      // 69: below 70
            Assert.AreEqual(SanityTier.Uneasy, s.Tier);
            s.Apply(+2f);                      // 71: not yet 70 + 3
            Assert.AreEqual(SanityTier.Uneasy, s.Tier);
            s.Apply(+2f);                      // 73: back to stable
            Assert.AreEqual(SanityTier.Stable, s.Tier);
            s.Set(10f);
            Assert.AreEqual(SanityTier.Breaking, s.Tier);
        }

        [Test]
        public void LanternBurnsAndRunsOut()
        {
            var l = new LanternModel(1f, true);
            bool died = false;
            for (int i = 0; i < 20 && !died; i++) died = l.Tick(1f, false);
            Assert.IsTrue(died);
            Assert.IsFalse(l.Lit);
            Assert.AreEqual(1, l.RunOuts);
            Assert.IsFalse(l.TryLight());
            Assert.AreEqual(42f, l.AddOil(42f), 1e-4f);
            Assert.IsTrue(l.TryLight());
        }

        [Test]
        public void SprintBurnsExtraOil()
        {
            var a = new LanternModel(50f, true);
            var b = new LanternModel(50f, true);
            a.Tick(10f, false);
            b.Tick(10f, true);
            Assert.AreEqual(50f - 0.75f, a.Oil, 1e-4f);
            Assert.AreEqual(50f - 1.25f, b.Oil, 1e-4f);
        }

        [Test]
        public void OilIsCappedAtMaximum()
        {
            var l = new LanternModel(90f, true);
            Assert.AreEqual(10f, l.AddOil(42f), 1e-4f);
            Assert.AreEqual(Balance.OilMax, l.Oil, 1e-4f);
        }

        [TestCase(true, 4, 1, 1)]
        [TestCase(true, 6, 1, 1)]
        [TestCase(true, 3, 1, 2)]
        [TestCase(true, 6, 2, 3)]
        [TestCase(false, 6, 1, 3)]
        [TestCase(false, 0, 2, 3)]
        public void EndingRules(bool answered, int drawings, int altar, int expected)
        {
            Assert.AreEqual((EndingId)expected, Endings.Resolve(answered, drawings, (AltarChoice)altar));
        }

        [Test]
        public void AtLeastTwentyAchievements()
        {
            Assert.GreaterOrEqual(AchievementProfile.All.Length, 20);
            Assert.AreEqual(AchievementProfile.All.Length, AchievementProfile.All.Select(a => a.Id).Distinct().Count());
        }
    }

    [TestFixture]
    public class LogicPuzzleTests
    {
        static ActSession Session(int act) => new ActSession(ContentDb.Act(act), ContentDb.ChapterStartState(act));

        [Test]
        public void FiveBellsLaMiDoMiLa()
        {
            var s = Session(1);
            float before = s.Sanity.Value;
            foreach (var b in new[] { "a1_b0", "a1_b2", "a1_b1", "a1_b2" }) Assert.AreEqual(Outcome.BellRung, s.Interact(b).Outcome);
            var r = s.Interact("a1_b4");
            Assert.AreEqual(Outcome.BellSolved, r.Outcome);
            Assert.IsTrue(s.State.Has("a1_music"));
            Assert.AreEqual(Math.Min(100f, before + 15f), s.Sanity.Value, 1e-3f);
            Assert.IsTrue(r.Achievements.Contains("lullaby_bells"));
        }

        [Test]
        public void WrongBellRestartsSequence()
        {
            var s = Session(1);
            s.Interact("a1_b0");
            Assert.AreEqual(Outcome.BellWrong, s.Interact("a1_b3").Outcome);
            Assert.AreEqual(0, s.SequenceProgress("a1"));
            Assert.AreEqual(1, s.State.GetInt("fail:a1"));
            s.Interact("a1_b0");
            Assert.AreEqual(Outcome.BellWrong, s.Interact("a1_b0").Outcome);
            Assert.AreEqual(1, s.SequenceProgress("a1"), "a wrong La counts as a new first note");
        }

        [Test]
        public void CratesNeedBothOnTheRingsAndLeverResets()
        {
            var s = Session(1);
            s.Interact("a1_crate_a");
            s.Interact("a1_crate_a");
            Assert.IsFalse(s.State.Has("a1_crates"));
            Assert.AreEqual(Outcome.Reset, s.Interact("a1_reset").Outcome);
            Assert.AreEqual(0, s.CrateSlot(ContentDb.Act(1).Item("a1_crate_a")));
            s.Interact("a1_crate_a"); s.Interact("a1_crate_a");
            s.Interact("a1_crate_b");
            var r = s.Interact("a1_crate_b");
            Assert.AreEqual(Outcome.CrateSolved, r.Outcome);
            Assert.IsFalse(r.Achievements.Contains("starboard_ring"), "used the reset lever");
            Assert.AreEqual(Outcome.AlreadyDone, s.Interact("a1_crate_a").Outcome);
        }

        [Test]
        public void HatchNeedsTheHook()
        {
            var s = Session(1);
            var r = s.Interact("a1_hatch");
            Assert.AreEqual(Outcome.NeedMissing, r.Outcome);
            Assert.AreEqual("item.a1_hatch.need", r.MessageKey);
            s.State.Set("a1_hook");
            Assert.AreEqual(Outcome.HatchOpened, s.Interact("a1_hatch").Outcome);
        }

        [Test]
        public void ClockDialsAreLockedUntilFiveBuildingsAreRead()
        {
            var s = Session(3);
            Assert.AreEqual(Outcome.NeedMissing, s.Interact("a3_c1").Outcome);
            foreach (var f in Act3Content.ClueFlags) s.State.Set(f);
            s.Interact("a3_c1");
            s.Interact("a3_c2"); s.Interact("a3_c2");
            s.Interact("a3_c3"); s.Interact("a3_c3");
            var r = s.Interact("a3_c3");
            Assert.AreEqual(Outcome.DialSolved, r.Outcome);
            Assert.IsTrue(r.Achievements.Contains("three_clocks"));
        }

        [Test]
        public void DialOptionsAreCanon()
        {
            CollectionContains(Act3Content.DialOptions, "11:40");
            CollectionContains(Act3Content.DialOptions, "03:17");
            CollectionContains(Act3Content.DialOptions, "06:05");
            Assert.IsFalse(Act3Content.DialOptions.Contains("02:17"));
            var act = ContentDb.Act(3);
            var answers = act.Items.Where(i => i.Kind == ItemKind.Clock).OrderBy(i => i.Index).Select(i => i.Options[i.Answer]).ToArray();
            Assert.AreEqual("11:40|03:17|06:05", string.Join("|", answers));
        }

        static void CollectionContains(string[] a, string v) => Assert.IsTrue(a.Contains(v), v);

        [Test]
        public void MirrorBeamBreaksDownstreamAndLocksAfterLens()
        {
            var s = Session(4);
            var act = ContentDb.Act(4);
            void Turn(int k, int n) { for (int i = 0; i < n; i++) s.Interact("a4_m" + (k + 1)); }
            Turn(0, 1); Turn(1, 2); Turn(2, 3); Turn(3, 1);
            Assert.AreEqual(4, s.BeamPrefix);
            Assert.IsTrue(s.State.Has("a4_m4"));
            Turn(1, 1);
            Assert.AreEqual(1, s.BeamPrefix);
            Assert.IsFalse(s.State.Has("a4_m4"));
            Assert.IsFalse(s.State.Has("a4_m2"));
            Turn(1, 3);
            Assert.AreEqual(4, s.BeamPrefix);
            Assert.AreEqual(Outcome.Pickup, s.Interact("a4_lens").Outcome);
            Assert.AreEqual(Outcome.MirrorLocked, s.Interact("a4_m1").Outcome);
            Assert.AreEqual(4, s.BeamPrefix);
            Assert.IsNotNull(act);
        }

        [Test]
        public void BraziersCostOilAndCanFail()
        {
            var s = Session(4);
            s.Lantern.Restore(30f, true);
            Assert.AreEqual(Outcome.BrazierLit, s.Interact("a4_brazier1").Outcome);
            Assert.AreEqual(10f, s.Lantern.Oil, 1e-3f);
            Assert.AreEqual(Outcome.BrazierNoOil, s.Interact("a4_brazier2").Outcome);
            Assert.AreEqual(1, s.State.BraziersLit);
        }

        [Test]
        public void OilFlaskStaysWhenLanternIsFull()
        {
            var s = Session(1);
            s.Lantern.Restore(100f, true);
            Assert.AreEqual(Outcome.OilFull, s.Interact("a1_oil_stern").Outcome);
            Assert.IsFalse(s.IsConsumed(ContentDb.Act(1).Item("a1_oil_stern")));
            s.Lantern.Restore(20f, true);
            Assert.AreEqual(Outcome.Oil, s.Interact("a1_oil_stern").Outcome);
            Assert.IsTrue(s.IsConsumed(ContentDb.Act(1).Item("a1_oil_stern")));
        }

        [Test]
        public void HintsEscalateWithFailures()
        {
            var s = Session(1);
            Assert.AreEqual(0, s.HintLevel("a1"));
            for (int i = 0; i < 3; i++) { s.Interact("a1_b0"); s.Interact("a1_b3"); }
            Assert.AreEqual(1, s.HintLevel("a1"));
            s.TickPuzzleTime("a1_music", Balance.HintFirstSeconds + Balance.HintStepSeconds * 2 + 1f);
            Assert.AreEqual(3, s.HintLevel("a1"));
        }

        [Test]
        public void Act3SilenceStillLeadsToActFour()
        {
            var s = Session(3);
            var r = s.RecordAnswer(false);
            Assert.IsTrue(s.State.Has("a3_left"));
            Assert.AreEqual(0, s.State.Answered);
            Assert.IsTrue(r.Achievements.Contains("silent"));
        }
    }

    [TestFixture]
    public class LogicDataTests
    {
        [Test]
        public void SaveRoundTrip()
        {
            var s = ContentDb.ChapterStartState(3);
            s.Set("a3_board");
            s.MarkNote("a3_board");
            s.AddDrawing(4);
            s.AddDrawing(1);
            s.SetInt("dial:a3_c1", 2);
            s.Oil = 33.5f;
            s.Sanity = 61.25f;
            s.Act2Path = EchoPath.Mimic;
            s.PlaySeconds = 4321.5;
            var json = SaveCodec.Encode(s, 2, new DateTime(2026, 11, 19, 21, 0, 0, DateTimeKind.Utc));
            Assert.IsTrue(SaveCodec.TryDecode(json, out var back, out var sum));
            Assert.AreEqual(3, back.Act);
            Assert.IsTrue(back.Has("a3_board"));
            Assert.AreEqual(2, back.DrawingCount);
            Assert.AreEqual(2, back.GetInt("dial:a3_c1"));
            Assert.AreEqual(33.5f, back.Oil, 1e-3f);
            Assert.AreEqual(61.25f, back.Sanity, 1e-3f);
            Assert.AreEqual(EchoPath.Mimic, back.Act2Path);
            Assert.AreEqual(2, sum.Slot);
            Assert.AreEqual(4321.5, sum.PlaySeconds, 1e-6);
        }

        [Test]
        public void CorruptSaveIsRejected()
        {
            Assert.IsFalse(SaveCodec.TryDecode("{\"magic\":\"undertow-save\",\"act\":", out _, out _));
            Assert.IsFalse(SaveCodec.TryDecode("{\"magic\":\"other\"}", out _, out _));
            Assert.IsFalse(SaveCodec.TryDecode("", out _, out _));
        }

        [Test]
        public void JsonIsCultureInvariantAndUnicodeSafe()
        {
            var prev = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
                var d = new System.Collections.Generic.Dictionary<string, object> { ["f"] = 1.5, ["s"] = "İsim \"ğüşıöç\"\n" };
                var j = MiniJson.Serialize(d);
                Assert.IsTrue(j.Contains("1.5"));
                var back = MiniJson.Obj(MiniJson.Parse(j));
                Assert.AreEqual(1.5, MiniJson.Num(back, "f"), 1e-9);
                Assert.AreEqual("İsim \"ğüşıöç\"\n", MiniJson.Str(back, "s"));
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = prev;
            }
        }

        [Test]
        public void LocTableParsesAndFallsBack()
        {
            var en = LocTable.Parse("en", "# comment\nui.a = Hello\nui.b = Two\\nlines\n");
            var tr = LocTable.Parse("tr", "ui.a = Merhaba\n");
            tr.Fallback = en;
            Assert.AreEqual("Merhaba", tr.Get("ui.a"));
            Assert.AreEqual("Two\nlines", tr.Get("ui.b"));
            Assert.AreEqual("#ui.c", tr.Get("ui.c"));
            Assert.AreEqual(10, LocTable.Languages.Length);
        }

        [Test]
        public void AchievementProfileRoundTrip()
        {
            var p = new AchievementProfile();
            Assert.IsTrue(p.Unlock("act1"));
            Assert.IsFalse(p.Unlock("act1"));
            Assert.IsFalse(p.Unlock("nope"));
            p.EndingsSeen.Add(2);
            p.FinishedOnce = true;
            var back = AchievementProfile.Decode(p.Encode());
            Assert.IsTrue(back.IsUnlocked("act1"));
            Assert.IsTrue(back.FinishedOnce);
            Assert.IsTrue(back.EndingsSeen.Contains(2));
        }
    }

    [TestFixture]
    public class LogicLevelTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void LevelCompilesWithoutErrors(int act)
        {
            var lv = LevelCompiler.Compile(ContentDb.Act(act));
            Assert.AreEqual(0, lv.Errors.Count, string.Join("; ", lv.Errors));
            Assert.Greater(lv.Floors.Count, 0);
            Assert.Greater(lv.Walls.Count, 0);
        }

        [TestCase(1, true, true, true)]
        [TestCase(2, true, true, true)]
        [TestCase(2, false, true, true)]
        [TestCase(3, true, true, true)]
        [TestCase(3, true, false, true)]
        [TestCase(4, true, true, true)]
        [TestCase(4, true, true, false)]
        public void ActCanBeFinishedWithoutSoftLock(int act, bool echo, bool answer, bool altarEcho)
        {
            var rep = Walkthrough.Run(ContentDb.Act(act), ContentDb.ChapterStartState(act),
                new Walkthrough.Options { Act2FollowEcho = echo, Act3Answer = answer, AltarEcho = altarEcho });
            Assert.IsTrue(rep.Ok, rep.ToString());
        }

        [Test]
        public void SixDrawingsAcrossTheGame()
        {
            var d = ContentDb.Acts.SelectMany(a => a.Items).Where(i => i.Kind == ItemKind.Drawing).Select(i => i.Drawing).OrderBy(x => x).ToArray();
            Assert.AreEqual("1,2,3,4,5,6", string.Join(",", d));
        }

        [Test]
        public void EveryActHasClipMomentExitAndCheckpoints()
        {
            foreach (var a in ContentDb.Acts)
            {
                Assert.IsNotNull(a.EndFlag);
                Assert.GreaterOrEqual(a.CheckpointFlags.Count, 4, "act " + a.Id);
                Assert.IsTrue(a.Goals.Last().Flag == a.EndFlag || a.Id == 3, "last goal of act " + a.Id);
            }
        }
    }
}
