using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Entry point. Everything is created in code before the first scene loads, so every scene
    /// (even an empty one opened directly in the editor) boots into a working game.
    /// </summary>
    public static class Game
    {
        public static GameRoot Root { get; private set; }
        public static GameSettings Settings { get; private set; }
        public static Loc Loc { get; private set; }
        public static InputHub Input { get; private set; }
        public static AudioHub Audio { get; private set; }
        public static SaveService Saves { get; private set; }
        public static AchievementService Achievements { get; private set; }
        public static SceneFlow Flow { get; private set; }
        public static UIRoot UI { get; private set; }
        public static ActRuntime Act { get; internal set; }

        public static bool Booted { get; private set; }
        public static bool Paused { get; internal set; }

        /// <summary>Unscaled time the game has run (for UI animation even while paused).</summary>
        public static float RealTime => Time.unscaledTime;

        public static string VersionLabel => "v" + Application.version + (Debug.isDebugBuild ? " dev" : "");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Root = null;
            Settings = null;
            Loc = null;
            Input = null;
            Audio = null;
            Saves = null;
            Achievements = null;
            Flow = null;
            UI = null;
            Act = null;
            Booted = false;
            Paused = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Booted) return;
            Booted = true;
            Application.runInBackground = false;

            var go = new GameObject("[Undertow]");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Root = go.AddComponent<GameRoot>();

            Saves = new SaveService();
            Settings = GameSettings.Load(Saves.SettingsPath);
            Loc = new Loc();
            Loc.Load(string.IsNullOrEmpty(Settings.Language) ? Loc.SystemLanguageCode() : Settings.Language);
            Input = new InputHub(Saves.BindingsPath);
            Audio = go.AddComponent<AudioHub>();
            Audio.Init();
            Achievements = new AchievementService(Saves);
            UI = UIRoot.Create(go.transform);
            Flow = go.AddComponent<SceneFlow>();
            Settings.Apply();
            SceneManager.sceneLoaded += Flow.OnSceneLoaded;
        }

        public static void Log(string msg) => Debug.Log("[Undertow] " + msg);
        public static void Warn(string msg) => Debug.LogWarning("[Undertow] " + msg);
    }

    /// <summary>The persistent root object: ticks services that need Update and catches global input.</summary>
    public sealed class GameRoot : MonoBehaviour
    {
        void Update()
        {
            Game.Input?.Tick();
            Game.UI?.Tick();
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus && Game.Act != null && !Game.Paused && Game.UI != null && Game.UI.CanPauseNow) Game.UI.OpenPause();
        }

        void OnApplicationQuit()
        {
            Game.Act?.SaveCheckpointNow("quit");
            Game.Settings?.Save(Game.Saves.SettingsPath);
        }
    }
}
