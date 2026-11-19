using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Core;

namespace Undertow.UI
{
    /// <summary>
    /// Main menu. Draws the key-art (Resources/MainMenuKeyArt) letter-boxed to 16:9 and places
    /// invisible hit areas over the five baked menu entries (Yeni Oyun / Oyunu Yükle / Ayarlar /
    /// Ekstralar / Çıkış). Falls back to the plain IMGUI menu if the key-art is missing.
    /// </summary>
    public sealed class UndertowMainMenuHUD : MonoBehaviour
    {
        private const float ArtW = 1672f, ArtH = 941f;
        // Y centres of the five baked entries in the 1672x941 key-art.
        private static readonly float[] EntryY = { 478f, 543f, 607f, 671f, 736f };
        private static readonly string[] EntryName = { "Yeni Oyun", "Oyunu Yükle", "Ayarlar", "Ekstralar", "Çıkış" };

        private enum Panel { None, Load, Settings, Extras }

        [SerializeField] private MainMenuAndPauseController menu;
        private Texture2D keyArt;
        private Panel panel = Panel.None;
        private GUIStyle title, subtitle, button, body;
        private string statusMessage = string.Empty;
        private bool fullscreen = true;

        private void Awake()
        {
            if (menu == null) menu = GetComponent<MainMenuAndPauseController>();
            keyArt = Resources.Load<Texture2D>("MainMenuKeyArt");
            fullscreen = Screen.fullScreen;
        }

        private void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.height / 13, 34, 76), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.88f, 0.84f, 0.76f) } };
            subtitle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.48f, 0.68f, 0.75f) } };
            button = new GUIStyle(GUI.skin.button) { fontSize = 16, fixedHeight = 42 };
            body = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true, alignment = TextAnchor.UpperLeft, normal = { textColor = new Color(0.85f, 0.88f, 0.9f) } };
        }

        private void StartNewGame(int slot)
        {
            menu.StartNewGameInSlot(slot);
            SceneManager.LoadScene("ActI_MercifulRequiem");
        }

        private void LoadGame(int slot)
        {
            if (menu.ContinueFromSlot(slot, out _))
            {
                GameAct act = ActProgressionManager.Instance != null ? ActProgressionManager.Instance.State.CurrentAct : GameAct.ActI_CursedGalleon;
                SceneManager.LoadScene(ActProgressionManager.GetSceneNameForAct(act));
            }
            else statusMessage = "Slot " + slot + " boş.";
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (keyArt == null) { DrawLegacy(); return; }

            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float scale = Mathf.Min(Screen.width / ArtW, Screen.height / ArtH);
            float drawW = ArtW * scale, drawH = ArtH * scale;
            float ox = (Screen.width - drawW) * 0.5f, oy = (Screen.height - drawH) * 0.5f;
            GUI.DrawTexture(new Rect(ox, oy, drawW, drawH), keyArt, ScaleMode.StretchToFill, false);

            if (panel == Panel.None)
            {
                for (int i = 0; i < EntryY.Length; i++)
                {
                    Rect r = new Rect(ox + 165f * scale, oy + (EntryY[i] - 30f) * scale, 400f * scale, 60f * scale);
                    bool hover = r.Contains(Event.current.mousePosition);
                    if (hover && i != 0)
                    {
                        GUI.color = new Color(0.55f, 0.04f, 0.06f, 0.35f);
                        GUI.DrawTexture(r, Texture2D.whiteTexture);
                        GUI.color = Color.white;
                    }
                    if (GUI.Button(r, GUIContent.none, GUIStyle.none)) HandleEntry(i);
                }
            }
            else DrawPanel(ox, oy, scale);
        }

        private void HandleEntry(int index)
        {
            statusMessage = string.Empty;
            switch (index)
            {
                case 0: StartNewGame(1); break;
                case 1: panel = Panel.Load; break;
                case 2: panel = Panel.Settings; break;
                case 3: panel = Panel.Extras; break;
                case 4:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        private void DrawPanel(float ox, float oy, float scale)
        {
            Rect box = new Rect(ox + 170f * scale, oy + 430f * scale, 520f * scale, 400f * scale);
            GUI.color = new Color(0.02f, 0.035f, 0.05f, 0.94f);
            GUI.DrawTexture(box, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(box.x + 18, box.y + 14, box.width - 36, box.height - 28));
            GUILayout.Label(panel == Panel.Load ? "OYUNU YÜKLE" : panel == Panel.Settings ? "AYARLAR" : "EKSTRALAR", subtitle);
            if (panel == Panel.Load)
            {
                for (int slot = 1; slot <= 3; slot++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Slot " + slot + " — Yükle", button)) LoadGame(slot);
                    if (GUILayout.Button("Yeni Oyun", button, GUILayout.Width(110))) StartNewGame(slot);
                    GUILayout.EndHorizontal();
                }
            }
            else if (panel == Panel.Settings)
            {
                GUILayout.Label("Ana ses: " + Mathf.RoundToInt(AudioListener.volume * 100f) + "%", body);
                AudioListener.volume = GUILayout.HorizontalSlider(AudioListener.volume, 0f, 1f);
                GUILayout.Space(8);
                bool newFullscreen = GUILayout.Toggle(fullscreen, " Tam ekran");
                if (newFullscreen != fullscreen) { fullscreen = newFullscreen; Screen.fullScreen = fullscreen; }
                if (GUILayout.Button("3840x2160 (UHD) iste", button))
                {
                    Resolution d = Screen.currentResolution;
                    int w = Mathf.Min(3840, d.width), h = Mathf.Min(2160, d.height);
                    Screen.SetResolution(w, h, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
                    statusMessage = "Çözünürlük: " + w + "x" + h + " (ekran desteğiyle sınırlı)";
                }
            }
            else
            {
                GUILayout.Label("UNDERTOW: ECHOES OF VANESSA\nYapımcı: LenstedReal\nUnity 6 · HDRP\n\nKontroller: WASD hareket · Fare bakış · E etkileşim · F fener · Q dinle · Shift temkinli yürüyüş · C eğil · Esc duraklat", body);
            }
            if (!string.IsNullOrEmpty(statusMessage)) GUILayout.Label(statusMessage, subtitle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Geri", button)) { panel = Panel.None; statusMessage = string.Empty; }
            GUILayout.EndArea();
        }

        private void DrawLegacy()
        {
            GUI.color = new Color(0.025f, 0.045f, 0.06f, 0.92f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            float w = Mathf.Min(460, Screen.width - 40); float x = (Screen.width - w) * .5f;
            GUI.Label(new Rect(x, Screen.height * .18f, w, 100), "UNDERTOW", title);
            GUI.Label(new Rect(x, Screen.height * .18f + 82, w, 32), "ECHOES OF VANESSA", subtitle);
            float y = Screen.height * .53f;
            for (int slot = 1; slot <= 3; slot++)
            {
                if (GUI.Button(new Rect(x + 28, y, w - 56, 42), "NEW GAME  /  SAVE SLOT " + slot, button)) StartNewGame(slot);
                y += 52;
            }
            for (int slot = 1; slot <= 3; slot++)
            {
                if (GUI.Button(new Rect(x + 28, y, w - 56, 36), "LOAD SAVE SLOT " + slot, button)) LoadGame(slot);
                y += 43;
            }
        }
    }
}
