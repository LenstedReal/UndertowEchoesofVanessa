using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Save
{
    [Serializable]
    public class SaveEnvelope
    {
        public int SlotIndex;
        public string TimestampIsoUtc;
        public string PayloadSha256;
        public CampaignState Campaign;
    }

    public class SaveLoadManager
    {
        public const int MaxSaveSlots = 3;
        private readonly string saveDirectoryPath;

        public SaveLoadManager(string customSaveDirectory = null)
        {
            saveDirectoryPath = string.IsNullOrWhiteSpace(customSaveDirectory)
                ? Path.Combine(Application.persistentDataPath, "UndertowSaves")
                : customSaveDirectory;

            if (!Directory.Exists(saveDirectoryPath))
                Directory.CreateDirectory(saveDirectoryPath);
        }

        public static void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 1 || slotIndex > MaxSaveSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Save slot must be between 1 and {MaxSaveSlots}.");
        }

        public string GetSlotFilePath(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return Path.Combine(saveDirectoryPath, $"undertow_slot_{slotIndex}.json");
        }

        public string GetBackupFilePath(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return Path.Combine(saveDirectoryPath, $"undertow_slot_{slotIndex}.json.bak");
        }

        public static void SanitizeCampaignState(CampaignState state)
        {
            if (state == null) return;
            if (!Enum.IsDefined(typeof(GameAct), state.CurrentAct))
                state.CurrentAct = GameAct.ActI_CursedGalleon;
            if (string.IsNullOrWhiteSpace(state.ActiveCheckpointId))
                state.ActiveCheckpointId = "ACT1_DECK_START";
            state.LanternFuelNormalized = Mathf.Clamp01(state.LanternFuelNormalized);
            state.SanityValue = Mathf.Clamp(state.SanityValue, 0f, 100f);
            if (state.SaveSlotIndex < 1 || state.SaveSlotIndex > MaxSaveSlots) state.SaveSlotIndex = 1;
            if (state.SavedSceneName == null) state.SavedSceneName = string.Empty;

            if (state.DiscoveredDrawingIds == null)
                state.DiscoveredDrawingIds = new List<string>();
            CanonicalGameData.DeduplicateCollectibleList(state.DiscoveredDrawingIds, CanonicalGameData.TotalVanessaDrawings);

            if (state.UnlockedJournalEntryIds == null)
                state.UnlockedJournalEntryIds = new List<string>();
            CanonicalGameData.DeduplicateCollectibleList(state.UnlockedJournalEntryIds);
        }

        public static string ComputeCampaignHash(CampaignState state)
        {
            string canonicalJson = JsonUtility.ToJson(state);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonicalJson));
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public bool SaveSlot(int slotIndex, CampaignState state)
        {
            ValidateSlotIndex(slotIndex);
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.SaveSlotIndex = slotIndex;
            SanitizeCampaignState(state);

            string primaryPath = GetSlotFilePath(slotIndex);
            string backupPath = GetBackupFilePath(slotIndex);
            string tempPath = primaryPath + ".tmp";

            var envelope = new SaveEnvelope
            {
                SlotIndex = slotIndex,
                TimestampIsoUtc = DateTime.UtcNow.ToString("O"),
                PayloadSha256 = ComputeCampaignHash(state),
                Campaign = state
            };

            string json = JsonUtility.ToJson(envelope, true);
            // Flush the temporary payload before replacing the live save.
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(true);
            }

            if (!File.Exists(primaryPath))
            {
                File.Move(tempPath, primaryPath);
                return true;
            }

            bool currentPrimaryIsValid = TryReadVerifiedEnvelope(primaryPath, slotIndex, out _);
            try
            {
                // File.Replace is atomic on supported desktop filesystems. Only rotate a
                // verified primary into .bak; never overwrite a good backup with corrupt data.
                File.Replace(tempPath, primaryPath, currentPrimaryIsValid ? backupPath : null, true);
            }
            catch (PlatformNotSupportedException)
            {
                if (currentPrimaryIsValid) File.Copy(primaryPath, backupPath, true);
                File.Delete(primaryPath);
                File.Move(tempPath, primaryPath);
            }
            catch (IOException)
            {
                // Some Unity target filesystems do not implement Replace. Keep the valid
                // backup and use the conservative move fallback on those platforms.
                if (currentPrimaryIsValid) File.Copy(primaryPath, backupPath, true);
                if (File.Exists(primaryPath)) File.Delete(primaryPath);
                File.Move(tempPath, primaryPath);
            }
            return true;
        }

        public bool TryLoadSlot(int slotIndex, out CampaignState loadedState, out bool recoveredFromCorruption)
        {
            ValidateSlotIndex(slotIndex);
            loadedState = null;
            recoveredFromCorruption = false;

            string primaryPath = GetSlotFilePath(slotIndex);
            string backupPath = GetBackupFilePath(slotIndex);

            if (TryReadVerifiedEnvelope(primaryPath, slotIndex, out CampaignState primaryCampaign))
            {
                SanitizeCampaignState(primaryCampaign);
                loadedState = primaryCampaign;
                return true;
            }

            // Primary file missing or corrupted -> Attempt automatic recovery from .bak
            if (File.Exists(backupPath) && TryReadVerifiedEnvelope(backupPath, slotIndex, out CampaignState backupCampaign))
            {
                SanitizeCampaignState(backupCampaign);
                loadedState = backupCampaign;
                recoveredFromCorruption = true;
                File.Copy(backupPath, primaryPath, overwrite: true);
                return true;
            }

            // If primary or backup existed on disk but both failed verification, heal the slot to safe Act I checkpoint state
            if (File.Exists(primaryPath) || File.Exists(backupPath))
            {
                loadedState = new CampaignState
                {
                    CurrentAct = GameAct.ActI_CursedGalleon,
                    ActiveCheckpointId = "ACT1_DECK_START_RECOVERED"
                };
                recoveredFromCorruption = true;
                SaveSlot(slotIndex, loadedState);
                return true;
            }

            return false;
        }

        private static bool TryReadVerifiedEnvelope(string path, int expectedSlotIndex, out CampaignState state)
        {
            state = null;
            if (!File.Exists(path)) return false;

            try
            {
                string raw = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(raw)) return false;

                SaveEnvelope env = JsonUtility.FromJson<SaveEnvelope>(raw);
                if (env == null || env.Campaign == null || string.IsNullOrEmpty(env.PayloadSha256))
                    return false;

                if (env.SlotIndex != expectedSlotIndex)
                    return false;

                if (!Enum.IsDefined(typeof(GameAct), env.Campaign.CurrentAct))
                    return false;

                string actualHash = ComputeCampaignHash(env.Campaign);
                if (!string.Equals(actualHash, env.PayloadSha256, StringComparison.OrdinalIgnoreCase))
                    return false;

                state = env.Campaign;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
