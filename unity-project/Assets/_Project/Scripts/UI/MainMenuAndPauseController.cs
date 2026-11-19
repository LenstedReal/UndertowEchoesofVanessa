using System;
using UnityEngine;
using Undertow.Core;
using Undertow.Save;

namespace Undertow.UI
{
    public class MainMenuAndPauseController : MonoBehaviour
    {
        private bool isPaused;
        private int selectedSaveSlot = 1;
        private SaveLoadManager saveManager;

        public event Action<bool> OnPauseStateChanged;
        public event Action<int, CampaignState> OnSaveSlotLoaded;

        public bool IsPaused => isPaused;
        public int SelectedSaveSlot => selectedSaveSlot;

        private void Awake()
        {
            saveManager = new SaveLoadManager();
        }

        private void OnDisable()
        {
            if (isPaused)
            {
                isPaused = false;
                Time.timeScale = 1.0f;
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1.0f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetPaused(!isPaused);
            }
        }

        public void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = isPaused ? 0.0f : 1.0f;
            OnPauseStateChanged?.Invoke(isPaused);
        }

        public void SelectSaveSlot(int slotIndex)
        {
            SaveLoadManager.ValidateSlotIndex(slotIndex);
            selectedSaveSlot = slotIndex;
        }

        public CampaignState StartNewGameInSlot(int slotIndex)
        {
            SelectSaveSlot(slotIndex);
            var fresh = new CampaignState { SaveSlotIndex = slotIndex };
            saveManager.SaveSlot(slotIndex, fresh);
            if (ActProgressionManager.Instance != null)
                ActProgressionManager.Instance.LoadState(fresh);
            OnSaveSlotLoaded?.Invoke(slotIndex, fresh);
            return fresh;
        }

        public bool ContinueFromSlot(int slotIndex, out bool recoveredFromCorruption)
        {
            SelectSaveSlot(slotIndex);
            if (saveManager.TryLoadSlot(slotIndex, out CampaignState loaded, out recoveredFromCorruption))
            {
                loaded.SaveSlotIndex = slotIndex;
                if (ActProgressionManager.Instance != null)
                    ActProgressionManager.Instance.LoadState(loaded);
                OnSaveSlotLoaded?.Invoke(slotIndex, loaded);
                return true;
            }
            return false;
        }
    }
}
