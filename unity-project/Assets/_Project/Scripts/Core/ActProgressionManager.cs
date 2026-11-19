using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Undertow.Core
{
    [Serializable]
    public class CampaignState
    {
        public GameAct CurrentAct = GameAct.ActI_CursedGalleon;
        public string ActiveCheckpointId = "ACT1_DECK_START";
        public float LanternFuelNormalized = 0.65f;
        public float SanityValue = 92.0f;
        public int SaveSlotIndex = 1;
        public bool HasPlayerTransform;
        public string SavedSceneName = "";
        public Vector3 SavedPlayerPosition;
        public float SavedPlayerYaw;

        // Act I flags
        public bool HasCaptainThorneJournal;
        public bool ActIFiveBellSolved;
        public bool HasSurgeonsHook;
        public bool ActICargoHatchOpened;
        public int ActIStarboardCratesAligned; // requires 2

        // Act II flags
        public bool ActIILanternBellSolved;
        public bool ActIICrateRingAligned;
        public bool TriggeredMimicTrapInActII;
        public bool FollowedEchoSafeRouteInActII;

        // Act III flags
        public bool DiscoveredCraneBloodline;
        public bool ActIIIClockTowerSolved;
        public bool AnsweredVanessaInMirrorRoom;
        public bool MirrorRoomChoiceMade;

        // Act IV flags
        public bool ActIVMirrorsAligned;
        public bool HasGlassPrism;
        public bool HasFocusingLens;
        public LullabyChoice FinalAltarLullabyChoice = LullabyChoice.None;

        // Collectibles
        public List<string> DiscoveredDrawingIds = new List<string>();
        public List<string> UnlockedJournalEntryIds = new List<string>();
    }

    public class ActProgressionManager : MonoBehaviour
    {
        public static ActProgressionManager Instance { get; private set; }

        [SerializeField] private CampaignState state = new CampaignState();

        public event Action<GameAct, GameAct> OnActTransitioned;
        public event Action<string> OnCheckpointReached;
        public event Action<EndingType> OnGameEndingResolved;

        public CampaignState State => state;
        private Undertow.Save.SaveLoadManager saveManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            saveManager = new Undertow.Save.SaveLoadManager();
            if (Application.isPlaying)
            {
                // Persist only the campaign-state object. Never preserve a scene root that
                // also owns transient UI, audio, or environment systems.
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void LoadState(CampaignState loaded)
        {
            if (loaded == null) throw new ArgumentNullException(nameof(loaded));
            CanonicalGameData.DeduplicateCollectibleList(loaded.DiscoveredDrawingIds, CanonicalGameData.TotalVanessaDrawings);
            CanonicalGameData.DeduplicateCollectibleList(loaded.UnlockedJournalEntryIds);
            state = loaded;
            saveManager = saveManager ?? new Undertow.Save.SaveLoadManager();
            if (state.SaveSlotIndex < 1 || state.SaveSlotIndex > Undertow.Save.SaveLoadManager.MaxSaveSlots) state.SaveSlotIndex = 1;
        }

        public void PersistNow()
        {
            if (state == null) return;
            try
            {
                if (saveManager == null) saveManager = new Undertow.Save.SaveLoadManager();
                saveManager.SaveSlot(state.SaveSlotIndex, state);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Undertow] Autosave failed: " + exception.Message);
            }
        }

        public bool RegisterVanessaDrawing(string drawingId)
        {
            if (!CanonicalGameData.TryNormalizeCollectibleId(drawingId, out string normalized))
                return false;

            if (state.DiscoveredDrawingIds == null)
                state.DiscoveredDrawingIds = new List<string>();

            CanonicalGameData.DeduplicateCollectibleList(state.DiscoveredDrawingIds, CanonicalGameData.TotalVanessaDrawings);
            if (state.DiscoveredDrawingIds.Count >= CanonicalGameData.TotalVanessaDrawings)
                return false;

            for (int i = 0; i < state.DiscoveredDrawingIds.Count; i++)
            {
                if (string.Equals(state.DiscoveredDrawingIds[i], normalized, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            state.DiscoveredDrawingIds.Add(normalized);
            PersistNow();
            return true;
        }

        public bool RegisterJournalEntry(string entryId)
        {
            if (!CanonicalGameData.TryNormalizeCollectibleId(entryId, out string normalized))
                return false;

            if (state.UnlockedJournalEntryIds == null)
                state.UnlockedJournalEntryIds = new List<string>();

            for (int i = 0; i < state.UnlockedJournalEntryIds.Count; i++)
            {
                if (string.Equals(state.UnlockedJournalEntryIds[i], normalized, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            state.UnlockedJournalEntryIds.Add(normalized);
            if (string.Equals(normalized, "JOURNAL_CAPTAIN_THORNE_1724", StringComparison.OrdinalIgnoreCase))
                state.HasCaptainThorneJournal = true;

            PersistNow();
            return true;
        }

        public void RecordCheckpoint(string checkpointId)
        {
            if (string.IsNullOrWhiteSpace(checkpointId)) return;
            state.ActiveCheckpointId = checkpointId.Trim();
            PersistNow();
            OnCheckpointReached?.Invoke(state.ActiveCheckpointId);
        }

        public bool CanAdvanceFromCurrentAct()
        {
            switch (state.CurrentAct)
            {
                case GameAct.ActI_CursedGalleon:
                    return state.ActIFiveBellSolved &&
                           state.HasSurgeonsHook &&
                           state.ActICargoHatchOpened &&
                           state.ActIStarboardCratesAligned >= 2;

                case GameAct.ActII_ShipGraveyard:
                    return state.ActIILanternBellSolved &&
                           state.ActIICrateRingAligned &&
                           (state.FollowedEchoSafeRouteInActII || state.TriggeredMimicTrapInActII);

                case GameAct.ActIII_ForsakenPort:
                    return state.DiscoveredCraneBloodline &&
                           state.ActIIIClockTowerSolved &&
                           state.MirrorRoomChoiceMade;

                case GameAct.ActIV_SunkenTemple:
                    return state.ActIVMirrorsAligned &&
                           state.HasGlassPrism &&
                           state.HasFocusingLens &&
                           state.FinalAltarLullabyChoice != LullabyChoice.None;

                default:
                    return false;
            }
        }

        public bool TryAdvanceAct(bool loadUnityScene = false)
        {
            if (!CanAdvanceFromCurrentAct())
                return false;

            GameAct previous = state.CurrentAct;
            if (previous == GameAct.ActIV_SunkenTemple)
            {
                EndingType ending = EndingEvaluator.EvaluateEnding(state);
                RecordCheckpoint("ACT4_ALTAR_RESOLUTION");
                OnGameEndingResolved?.Invoke(ending);

                if (loadUnityScene && Application.isPlaying && Application.CanStreamedLevelBeLoaded("Endings_Resolution"))
                {
                    SceneManager.LoadScene("Endings_Resolution");
                }
                return true;
            }

            state.CurrentAct = (GameAct)((int)previous + 1);
            PersistNow();
            RecordCheckpoint($"ACT{(int)state.CurrentAct}_ENTRY");
            OnActTransitioned?.Invoke(previous, state.CurrentAct);

            if (loadUnityScene && Application.isPlaying)
            {
                string nextScene = GetSceneNameForAct(state.CurrentAct);
                if (Application.CanStreamedLevelBeLoaded(nextScene))
                {
                    SceneManager.LoadScene(nextScene);
                }
            }

            return true;
        }

        public static string GetSceneNameForAct(GameAct act)
        {
            switch (act)
            {
                case GameAct.ActI_CursedGalleon: return "ActI_MercifulRequiem";
                case GameAct.ActII_ShipGraveyard: return "ActII_ShipGraveyard";
                case GameAct.ActIII_ForsakenPort: return "ActIII_ForsakenPort";
                case GameAct.ActIV_SunkenTemple: return "ActIV_SunkenTemple";
                default: return "MainMenu";
            }
        }
    }
}
