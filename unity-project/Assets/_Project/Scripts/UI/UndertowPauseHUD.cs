using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Core;
using Undertow.Save;

namespace Undertow.UI
{
    public sealed class UndertowPauseHUD : MonoBehaviour
    {
        [SerializeField] private MainMenuAndPauseController pauseController;
        private SaveLoadManager saves;
        private GUIStyle heading, body, action;
        private string status = "";
        private float statusUntil;

        private void Awake() { saves = new SaveLoadManager(); }
        private void Update()
        {
            if (pauseController == null) pauseController = FindObjectOfType<MainMenuAndPauseController>();
            if (pauseController != null && pauseController.IsPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                var look = FindObjectOfType<Undertow.Player.PlayerCameraLook>();
                if (look != null) look.SetLookLocked(true);
            }
        }

        private void EnsureStyles()
        {
            if (heading != null) return;
            heading = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.height / 28, 24, 42), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.9f,.86f,.76f) } };
            body = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.height / 72, 14, 20), alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(.66f,.77f,.81f) } };
            action = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(Screen.height / 68, 15, 21), fixedHeight = Mathf.Clamp(Screen.height / 19, 44, 64) };
        }

        private void OnGUI()
        {
            if (pauseController == null || !pauseController.IsPaused) return;
            EnsureStyles();
            GUI.color = new Color(.006f,.018f,.025f,.88f); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); GUI.color = Color.white;
            float w = Mathf.Min(520, Screen.width - 48); float x = (Screen.width-w)*.5f; float y = Screen.height*.24f;
            GUI.Label(new Rect(x,y,w,64),"UNDERTOW",heading);
            GUI.Label(new Rect(x,y+62,w,34),"THE TIDE IS WAITING",body);
            y += 130;
            if (GUI.Button(new Rect(x+50,y,w-100,50),"RESUME",action)) Resume();
            y += 62;
            if (GUI.Button(new Rect(x+50,y,w-100,50),"SAVE CHECKPOINT",action)) SaveCheckpoint();
            y += 62;
            if (GUI.Button(new Rect(x+50,y,w-100,50),"RETURN TO MAIN MENU",action)) ReturnToMenu();
            y += 68;
            if (!string.IsNullOrEmpty(status) && Time.unscaledTime < statusUntil) GUI.Label(new Rect(x,y,w,34),status,body);
            GUI.Label(new Rect(x,Screen.height-46,w,26),"WASD MOVE   •   MOUSE LOOK   •   E INTERACT   •   F LANTERN   •   Q LISTEN",body);
        }

        private void Resume()
        {
            pauseController.SetPaused(false);
            var look = FindObjectOfType<Undertow.Player.PlayerCameraLook>();
            if (look != null) look.SetLookLocked(false);
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        private void SaveCheckpoint()
        {
            var campaign = ActProgressionManager.Instance;
            if (campaign == null) { status = "SAVE UNAVAILABLE"; statusUntil = Time.unscaledTime+2.5f; return; }
            var autosave = FindObjectOfType<Undertow.World.UndertowCampaignAutosave>();
            if (autosave != null) autosave.Flush(); else campaign.PersistNow();
            status = "CHECKPOINT SAVED  /  SLOT " + campaign.State.SaveSlotIndex;
            statusUntil = Time.unscaledTime+2.5f;
        }
        private void ReturnToMenu()
        {
            SaveCheckpoint();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}
