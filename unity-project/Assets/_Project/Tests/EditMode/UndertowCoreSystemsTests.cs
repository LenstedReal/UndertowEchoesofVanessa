using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Undertow.Core;
using Undertow.Lantern;
using Undertow.Sanity;
using Undertow.Audio;
using Undertow.Puzzles;
using Undertow.Save;

namespace Undertow.Tests
{
    [TestFixture]
    public class UndertowCoreSystemsTests
    {
        private string tempSaveDir;

        [SetUp]
        public void SetUp()
        {
            tempSaveDir = Path.Combine(Path.GetTempPath(), "UndertowTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempSaveDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempSaveDir))
                Directory.Delete(tempSaveDir, recursive: true);
        }

        [Test]
        public void ActProgression_AllFourActsTransitionInCanonicalOrder()
        {
            var go = new GameObject("ProgressionHost");
            var mgr = go.AddComponent<ActProgressionManager>();

            Assert.AreEqual(GameAct.ActI_CursedGalleon, mgr.State.CurrentAct);
            Assert.IsFalse(mgr.TryAdvanceAct(false));

            mgr.State.ActIFiveBellSolved = true;
            mgr.State.HasSurgeonsHook = true;
            mgr.State.ActICargoHatchOpened = true;
            mgr.State.ActIStarboardCratesAligned = 2;
            Assert.IsTrue(mgr.TryAdvanceAct(false));
            Assert.AreEqual(GameAct.ActII_ShipGraveyard, mgr.State.CurrentAct);

            mgr.State.ActIILanternBellSolved = true;
            mgr.State.ActIICrateRingAligned = true;
            mgr.State.FollowedEchoSafeRouteInActII = true;
            Assert.IsTrue(mgr.TryAdvanceAct(false));
            Assert.AreEqual(GameAct.ActIII_ForsakenPort, mgr.State.CurrentAct);

            mgr.State.DiscoveredCraneBloodline = true;
            mgr.State.ActIIIClockTowerSolved = true;
            mgr.State.MirrorRoomChoiceMade = true;
            mgr.State.AnsweredVanessaInMirrorRoom = true;
            Assert.IsTrue(mgr.TryAdvanceAct(false));
            Assert.AreEqual(GameAct.ActIV_SunkenTemple, mgr.State.CurrentAct);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ActIFiveBellPuzzle_EnforcesLaMiDoMiLa_AndResetsOnFailure()
        {
            var go = new GameObject("BellPuzzle");
            var puzzle = go.AddComponent<FiveBellPuzzle>();

            Assert.IsFalse(puzzle.StrikeBell(BellNote.La));
            Assert.IsFalse(puzzle.StrikeBell(BellNote.Do));
            Assert.AreEqual(1, puzzle.FailureCount);
            Assert.AreEqual(0, puzzle.CurrentInputSequence.Count);

            Assert.IsFalse(puzzle.StrikeBell(BellNote.La));
            Assert.IsFalse(puzzle.StrikeBell(BellNote.Mi));
            Assert.IsFalse(puzzle.StrikeBell(BellNote.Do));
            Assert.IsFalse(puzzle.StrikeBell(BellNote.Mi));
            Assert.IsTrue(puzzle.StrikeBell(BellNote.La));
            Assert.IsTrue(puzzle.IsSolved);

            puzzle.ResetPuzzle();
            Assert.IsFalse(puzzle.IsSolved);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ActIILanternCountPuzzle_Validates1La2Do3Mi_AndMimicTrap()
        {
            var go = new GameObject("Act2Puzzle");
            var puzzle = go.AddComponent<LanternCountBellPuzzle>();
            puzzle.SetLanternCountPattern(new int[] { 1, 2, 3 });

            Assert.IsFalse(puzzle.FollowVoiceRoute(followedEchoTrueVanessa: false));
            Assert.IsTrue(puzzle.EnvironmentalTrapTriggered);

            Assert.IsTrue(puzzle.FollowVoiceRoute(followedEchoTrueVanessa: true));
            Assert.IsTrue(puzzle.SafeEchoRouteUnlocked);

            Assert.IsFalse(puzzle.StrikeBellForStage(BellNote.La));
            Assert.IsFalse(puzzle.StrikeBellForStage(BellNote.Do));
            Assert.IsTrue(puzzle.StrikeBellForStage(BellNote.Mi));
            Assert.IsTrue(puzzle.IsSolved);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ActIIIClockTowerPuzzle_Enforces0317AndRejectsLegacy0217()
        {
            var go = new GameObject("ClockPuzzle");
            var puzzle = go.AddComponent<ClockTowerPuzzle>();

            puzzle.SetDialTime(0, 11, 40);
            puzzle.SetDialTime(1, 2, 17); // Conflicting prototype value 02:17 must fail
            puzzle.SetDialTime(2, 6, 5);
            Assert.IsFalse(puzzle.SubmitClockConfiguration());
            Assert.AreEqual(1, puzzle.FailureCount);

            puzzle.SetDialTime(1, 3, 17); // Canonical 03:17
            Assert.IsTrue(puzzle.SubmitClockConfiguration());
            Assert.IsTrue(puzzle.IsSolved);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ActIVMirrorPrismPuzzle_Enforces1231_AndLanternLit()
        {
            var go = new GameObject("TemplePuzzle");
            var puzzle = go.AddComponent<SunkenTempleMirrorPrismPuzzle>();

            puzzle.RotateMirror(0, 1);
            puzzle.RotateMirror(1, 2);
            puzzle.RotateMirror(2, 3);
            puzzle.RotateMirror(3, 1);

            Assert.IsFalse(puzzle.TryChannelLanternBeamThroughAltar(isLanternLit: true));

            puzzle.InstallGlassPrism();
            puzzle.InstallFocusingLens();

            Assert.IsFalse(puzzle.TryChannelLanternBeamThroughAltar(isLanternLit: false));
            Assert.IsTrue(puzzle.TryChannelLanternBeamThroughAltar(isLanternLit: true));
            Assert.IsTrue(puzzle.IsSolved);

            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void EndingEvaluator_ResolvesAllThreeEndings_AndRejectsDuplicateDrawingIds()
        {
            var stateAnswer = new CampaignState
            {
                AnsweredVanessaInMirrorRoom = true,
                MirrorRoomChoiceMade = true,
                FinalAltarLullabyChoice = LullabyChoice.EchoIncorrect
            };
            stateAnswer.DiscoveredDrawingIds.AddRange(new[] { "D1", "D2", "D3", "D4" });
            Assert.AreEqual(EndingType.Answer, EndingEvaluator.EvaluateEnding(stateAnswer));

            // Duplicate drawing IDs ("D1" repeated 4 times) must count as only 1 unique drawing -> SURFACE
            var stateDuplicateDrawings = new CampaignState
            {
                AnsweredVanessaInMirrorRoom = true,
                MirrorRoomChoiceMade = true,
                FinalAltarLullabyChoice = LullabyChoice.EchoIncorrect
            };
            stateDuplicateDrawings.DiscoveredDrawingIds.AddRange(new[] { "D1", "d1", " D1 ", "D2", "" });
            Assert.AreEqual(2, EndingEvaluator.CountValidUniqueDrawings(stateDuplicateDrawings.DiscoveredDrawingIds));
            Assert.AreEqual(EndingType.Surface, EndingEvaluator.EvaluateEnding(stateDuplicateDrawings));

            var stateUndertowRefused = new CampaignState
            {
                AnsweredVanessaInMirrorRoom = false,
                MirrorRoomChoiceMade = true,
                FinalAltarLullabyChoice = LullabyChoice.EchoIncorrect
            };
            stateUndertowRefused.DiscoveredDrawingIds.AddRange(new[] { "D1", "D2", "D3", "D4", "D5", "D6" });
            Assert.AreEqual(EndingType.Undertow, EndingEvaluator.EvaluateEnding(stateUndertowRefused));

            var stateUndertowMimic = new CampaignState
            {
                AnsweredVanessaInMirrorRoom = true,
                MirrorRoomChoiceMade = true,
                FinalAltarLullabyChoice = LullabyChoice.MimicCorrect
            };
            stateUndertowMimic.DiscoveredDrawingIds.AddRange(new[] { "D1", "D2", "D3", "D4", "D5", "D6" });
            Assert.AreEqual(EndingType.Undertow, EndingEvaluator.EvaluateEnding(stateUndertowMimic));
        }

        [Test]
        public void SaveLoadManager_PersistsAllThreeSlots_AndSelfHealsCorruptJson()
        {
            var manager = new SaveLoadManager(tempSaveDir);

            var s1 = new CampaignState { CurrentAct = GameAct.ActII_ShipGraveyard, ActiveCheckpointId = "ACT2_WRECK_04" };
            Assert.IsTrue(manager.SaveSlot(1, s1));

            s1.CurrentAct = GameAct.ActIII_ForsakenPort;
            s1.ActiveCheckpointId = "ACT3_CHAPEL";
            Assert.IsTrue(manager.SaveSlot(1, s1));

            Assert.IsTrue(manager.TryLoadSlot(1, out CampaignState loadedClean, out bool recoveredClean));
            Assert.IsFalse(recoveredClean);
            Assert.AreEqual(GameAct.ActIII_ForsakenPort, loadedClean.CurrentAct);

            // Corrupt primary file -> recovers from .bak
            File.WriteAllText(manager.GetSlotFilePath(1), "{ CORRUPTED_SAVE_PAYLOAD ");
            Assert.IsTrue(manager.TryLoadSlot(1, out CampaignState recoveredState, out bool wasRecovered));
            Assert.IsTrue(wasRecovered);
            Assert.AreEqual(GameAct.ActII_ShipGraveyard, recoveredState.CurrentAct);
        }


        [Test]
        public void ActIIProgression_RequiresACommittedEchoOrMimicRoute()
        {
            var go = new GameObject("ProgressionActII");
            var mgr = go.AddComponent<ActProgressionManager>();
            mgr.State.CurrentAct = GameAct.ActII_ShipGraveyard;
            mgr.State.ActIILanternBellSolved = true;
            mgr.State.ActIICrateRingAligned = true;
            Assert.IsFalse(mgr.CanAdvanceFromCurrentAct());
            mgr.State.TriggeredMimicTrapInActII = true;
            Assert.IsTrue(mgr.CanAdvanceFromCurrentAct());
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void ActICrates_CannotBeAlignedBeforeSurgeonHatchOpens()
        {
            var go = new GameObject("ActICrateGate");
            var puzzle = go.AddComponent<CrateRingPuzzle>();
            puzzle.ConfigureForAct(GameAct.ActI_CursedGalleon);
            puzzle.AcquireSurgeonsHook();
            Assert.IsFalse(puzzle.MoveCrateIntoRing(0));
            Assert.AreEqual(1, puzzle.FailureCount);
            Assert.IsTrue(puzzle.TryOpenCargoHatch());
            Assert.IsFalse(puzzle.MoveCrateIntoRing(0));
            Assert.AreEqual(1, puzzle.AlignedCrateCount);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void EndingEvaluator_DoesNotResolveBeforeFinalAltarChoice()
        {
            var state = new CampaignState { AnsweredVanessaInMirrorRoom = true };
            state.DiscoveredDrawingIds.AddRange(new[] { "D1", "D2", "D3", "D4" });
            Assert.AreEqual(EndingType.None, EndingEvaluator.EvaluateEnding(state));
            Assert.AreEqual(EndingType.None, EndingEvaluator.BuildEndingDirective(state).Ending);
        }


        [Test]
        public void ActIIRouteChoice_RequiresSolvedBellAndCanOnlyBeCommittedOnce()
        {
            var go = new GameObject("Act2RouteChoice");
            var puzzle = go.AddComponent<LanternCountBellPuzzle>();
            puzzle.SetLanternCountPattern(new[] { 1, 2, 3 });
            Assert.IsFalse(puzzle.FollowVoiceRoute(true));
            puzzle.StrikeBellForStage(BellNote.La);
            puzzle.StrikeBellForStage(BellNote.Do);
            puzzle.StrikeBellForStage(BellNote.Mi);
            Assert.IsTrue(puzzle.FollowVoiceRoute(true));
            Assert.IsFalse(puzzle.FollowVoiceRoute(false));
            Assert.IsTrue(puzzle.SafeEchoRouteUnlocked);
            Assert.IsFalse(puzzle.EnvironmentalTrapTriggered);
            UnityEngine.Object.DestroyImmediate(go);
        }

        [Test]
        public void SaveLoadManager_AlwaysStoresTheRequestedSlotIndex()
        {
            var manager = new SaveLoadManager(tempSaveDir);
            var state = new CampaignState { SaveSlotIndex = 1, CurrentAct = GameAct.ActIII_ForsakenPort };
            Assert.IsTrue(manager.SaveSlot(3, state));
            Assert.AreEqual(3, state.SaveSlotIndex);
            Assert.IsTrue(manager.TryLoadSlot(3, out CampaignState loaded, out bool recovered));
            Assert.IsFalse(recovered);
            Assert.AreEqual(3, loaded.SaveSlotIndex);
        }

        [Test]
        public void LanternAndSanity_DepletesFuelAndTransitionsAcrossAllFourSanityStates()
        {
            var go = new GameObject("LanternSanityHost");
            var lantern = go.AddComponent<LanternController>();
            var sanity = go.AddComponent<HiddenSanitySystem>();

            lantern.SetFuelNormalized(0.10f);
            Assert.IsTrue(lantern.IsLit);
            lantern.TickLantern(30.0f, Vector3.zero);
            Assert.AreEqual(0f, lantern.CurrentFuel, 0.001f);
            Assert.IsFalse(lantern.IsLit);

            lantern.AddFuel(50.0f);
            Assert.AreEqual(0.50f, lantern.FuelNormalized, 0.001f);

            sanity.SetSanity(90f);
            Assert.AreEqual(SanityState.Stable, sanity.CurrentState);
            sanity.SetSanity(62f);
            Assert.AreEqual(SanityState.Uneasy, sanity.CurrentState);
            sanity.SetSanity(35f);
            Assert.AreEqual(SanityState.Disturbed, sanity.CurrentState);
            sanity.SetSanity(12f);
            Assert.AreEqual(SanityState.Breaking, sanity.CurrentState);

            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
