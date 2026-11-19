using System;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Core;

namespace Undertow.Narrative
{
    [Serializable]
    public struct JournalDocumentEntry
    {
        public string EntryId;
        public string TitleKey;
        public string BodyKey;
        public GameAct DiscoveredAct;
    }

    public class ObjectiveJournalSystem : MonoBehaviour
    {
        private string currentObjectiveKey = "OBJ_ACT1_FIND_LANTERN_OIL";
        private readonly List<string> unlockedJournalIds = new List<string>();
        private readonly List<string> discoveredDrawingIds = new List<string>();

        public event Action<string> OnObjectiveUpdated;
        public event Action<string> OnJournalEntryUnlocked;
        public event Action<string, int> OnVanessaDrawingCollected;

        public string CurrentObjectiveKey => currentObjectiveKey;
        public IReadOnlyList<string> UnlockedJournalIds => unlockedJournalIds;
        public IReadOnlyList<string> DiscoveredDrawingIds => discoveredDrawingIds;

        public void SetObjective(string objectiveLocalizationKey)
        {
            if (string.IsNullOrWhiteSpace(objectiveLocalizationKey)) return;
            currentObjectiveKey = objectiveLocalizationKey.Trim();
            OnObjectiveUpdated?.Invoke(currentObjectiveKey);
        }

        public bool UnlockJournalEntry(string entryId)
        {
            if (!CanonicalGameData.TryNormalizeCollectibleId(entryId, out string normalized))
                return false;

            for (int i = 0; i < unlockedJournalIds.Count; i++)
            {
                if (string.Equals(unlockedJournalIds[i], normalized, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            unlockedJournalIds.Add(normalized);
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.RegisterJournalEntry(normalized);
            }
            OnJournalEntryUnlocked?.Invoke(normalized);
            return true;
        }

        public bool CollectVanessaDrawing(string drawingId)
        {
            if (!CanonicalGameData.TryNormalizeCollectibleId(drawingId, out string normalized))
                return false;
            if (discoveredDrawingIds.Count >= CanonicalGameData.TotalVanessaDrawings)
                return false;

            for (int i = 0; i < discoveredDrawingIds.Count; i++)
            {
                if (string.Equals(discoveredDrawingIds[i], normalized, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            discoveredDrawingIds.Add(normalized);
            if (ActProgressionManager.Instance != null)
            {
                ActProgressionManager.Instance.RegisterVanessaDrawing(normalized);
            }
            OnVanessaDrawingCollected?.Invoke(normalized, discoveredDrawingIds.Count);
            return true;
        }
    }
}
