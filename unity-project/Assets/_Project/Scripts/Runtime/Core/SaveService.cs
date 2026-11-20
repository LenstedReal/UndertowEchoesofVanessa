using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Three save slots, each one playthrough that autosaves at checkpoints. Writes are atomic
    /// (temp file, then swap, previous copy kept as .bak) so a crash or power cut never corrupts a save.
    /// Everything lives under persistentDataPath, which Steam Auto-Cloud can mirror.
    /// </summary>
    public sealed class SaveService
    {
        public const int SlotCount = 3;

        public string Root => Path.Combine(Application.persistentDataPath, "Saves");
        public string SettingsPath => Path.Combine(Application.persistentDataPath, "settings.json");
        public string BindingsPath => Path.Combine(Application.persistentDataPath, "bindings.json");
        public string ProfilePath => Path.Combine(Root, "profile.json");
        public string SnapshotPath(int act) => Path.Combine(Root, "chapter" + act + ".json");
        string SlotPath(int slot) => Path.Combine(Root, "slot" + slot + ".json");

        public int ActiveSlot = -1;

        public bool Save(int slot, GameState state)
        {
            if (slot < 0 || slot >= SlotCount || state == null) return false;
            return WriteAtomic(SlotPath(slot), SaveCodec.Encode(state, slot, DateTime.UtcNow));
        }

        public bool Load(int slot, out GameState state)
        {
            state = null;
            if (slot < 0 || slot >= SlotCount) return false;
            foreach (var p in new[] { SlotPath(slot), SlotPath(slot) + ".bak" })
            {
                if (!File.Exists(p)) continue;
                try
                {
                    if (SaveCodec.TryDecode(File.ReadAllText(p), out state, out _)) return true;
                }
                catch (Exception e)
                {
                    Game.Warn($"save {p} unreadable: {e.Message}");
                }
            }
            return false;
        }

        public SaveSummary Summary(int slot)
        {
            var p = SlotPath(slot);
            foreach (var path in new[] { p, p + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    if (SaveCodec.TryDecode(File.ReadAllText(path), out _, out var sum))
                    {
                        sum.Slot = slot;
                        return sum;
                    }
                }
                catch (Exception)
                {
                    // fall through to the backup
                }
            }
            return new SaveSummary { Slot = slot, Valid = false };
        }

        public bool HasAnySave()
        {
            for (int i = 0; i < SlotCount; i++) if (Summary(i).Valid) return true;
            return false;
        }

        /// <summary>Slot with the newest save (for Continue), or -1.</summary>
        public int MostRecentSlot()
        {
            int best = -1;
            DateTime bestTime = DateTime.MinValue;
            for (int i = 0; i < SlotCount; i++)
            {
                var s = Summary(i);
                if (!s.Valid) continue;
                DateTime.TryParse(s.SavedUtc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var t);
                if (best < 0 || t > bestTime)
                {
                    best = i;
                    bestTime = t;
                }
            }
            return best;
        }

        public int FirstFreeSlot()
        {
            for (int i = 0; i < SlotCount; i++) if (!Summary(i).Valid) return i;
            return -1;
        }

        public void Delete(int slot)
        {
            try
            {
                var p = SlotPath(slot);
                if (File.Exists(p)) File.Delete(p);
                if (File.Exists(p + ".bak")) File.Delete(p + ".bak");
            }
            catch (Exception e)
            {
                Game.Warn("delete failed: " + e.Message);
            }
        }

        // ---------------------------------------------------------------- chapter snapshots
        public void SaveChapterSnapshot(int act, GameState state)
        {
            var s = state.Clone();
            s.Checkpoint = "start";
            WriteAtomic(SnapshotPath(act), SaveCodec.Encode(s, -1, DateTime.UtcNow));
        }

        public GameState ChapterSnapshot(int act)
        {
            try
            {
                var p = SnapshotPath(act);
                if (File.Exists(p) && SaveCodec.TryDecode(File.ReadAllText(p), out var s, out _)) return s;
            }
            catch (Exception e)
            {
                Game.Warn("snapshot unreadable: " + e.Message);
            }
            return null;
        }

        public bool ChapterUnlocked(int act) => act == 1 || File.Exists(SnapshotPath(act));

        // ---------------------------------------------------------------- profile
        public AchievementProfile LoadProfile()
        {
            try
            {
                if (File.Exists(ProfilePath)) return AchievementProfile.Decode(File.ReadAllText(ProfilePath));
                if (File.Exists(ProfilePath + ".bak")) return AchievementProfile.Decode(File.ReadAllText(ProfilePath + ".bak"));
            }
            catch (Exception e)
            {
                Game.Warn("profile unreadable: " + e.Message);
            }
            return new AchievementProfile();
        }

        public void SaveProfile(AchievementProfile p) => WriteAtomic(ProfilePath, p.Encode());

        // ---------------------------------------------------------------- io
        public static bool WriteAtomic(string path, string text)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, text);
                if (File.Exists(path))
                {
                    string bak = path + ".bak";
                    if (File.Exists(bak)) File.Delete(bak);
                    File.Move(path, bak);
                }
                File.Move(tmp, path);
                return true;
            }
            catch (Exception e)
            {
                Game.Warn($"write {path} failed: {e.Message}");
                return false;
            }
        }
    }

    /// <summary>Hook for a future Steamworks integration; local profile works without it.</summary>
    public interface IPlatformAchievements
    {
        bool Available { get; }
        void Unlock(string id);
    }

    public sealed class AchievementService
    {
        readonly SaveService _saves;
        public AchievementProfile Profile { get; private set; }
        public IPlatformAchievements Platform;
        public event Action<AchievementDef> Unlocked;

        public AchievementService(SaveService saves)
        {
            _saves = saves;
            Profile = saves.LoadProfile();
        }

        public void Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || !Profile.Unlock(id)) return;
            _saves.SaveProfile(Profile);
            var def = AchievementProfile.Find(id);
            if (Platform != null && Platform.Available) Platform.Unlock(id);
            if (def != null) Unlocked?.Invoke(def);
        }

        public void UnlockAll(IEnumerable<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids) Unlock(id);
        }

        public void MarkActReached(int act)
        {
            if (act <= Profile.HighestActReached) return;
            Profile.HighestActReached = Mathf.Clamp(act, 1, 4);
            _saves.SaveProfile(Profile);
        }

        public void MarkEnding(EndingId e)
        {
            Profile.EndingsSeen.Add((int)e);
            Profile.FinishedOnce = true;
            _saves.SaveProfile(Profile);
        }

        public void Save() => _saves.SaveProfile(Profile);
    }
}
