using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// Scene changes with a fade and a loading card. Scenes are near-empty shells: the world is
    /// built here after the scene loads, from the act data, so every scene works even if opened alone.
    /// </summary>
    public sealed class SceneFlow : MonoBehaviour
    {
        GameState _pendingState;
        int _pendingSlot = -1;
        bool _busy;
        bool _firstScene = true;
        static readonly string[] TipKeys = { "tip.1", "tip.2", "tip.3", "tip.4", "tip.5", "tip.6", "tip.7", "tip.8", "tip.9", "tip.10" };

        public bool Busy => _busy;

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_firstScene) return;     // later loads are driven by the coroutine below
            _firstScene = false;
            StartCoroutine(FirstScene(scene.name));
        }

        IEnumerator FirstScene(string sceneName)
        {
            yield return null;
            var act = ContentDb.ActForScene(sceneName);
            if (act != null)
            {
                // Started directly in an act scene (editor): play that chapter without touching the save slots.
                _pendingState = ContentDb.ChapterStartState(act.Id);
                _pendingSlot = -1;
                Game.Saves.ActiveSlot = -1;
                BuildForScene(sceneName);
                yield return Game.UI.Fade(0f, 1.2f);
                yield break;
            }
            if (sceneName == ContentDb.EndingsScene)
            {
                var s = ContentDb.ChapterStartState(4);
                s.Answered = 1;
                s.Altar = AltarChoice.Echo;
                s.Ending = EndingId.Surface;
                _pendingState = s;
                BuildForScene(sceneName);
                yield return Game.UI.Fade(0f, 1.2f);
                yield break;
            }
            // Main menu (or an unknown scene): boot sequence, then the menu
            BuildForScene(ContentDb.MenuScene);
            yield return BootSequence.Run();
        }

        void BuildForScene(string sceneName)
        {
            var act = ContentDb.ActForScene(sceneName);
            if (act != null)
            {
                var state = _pendingState ?? ContentDb.ChapterStartState(act.Id);
                _pendingState = null;
                Game.UI.InGame = true;
                ActRuntime.Create(act, state);
                return;
            }
            Game.UI.InGame = false;
            Game.Act = null;
            if (sceneName == ContentDb.EndingsScene)
            {
                var state = _pendingState ?? ContentDb.ChapterStartState(4);
                _pendingState = null;
                EndingsDirector.Create(state);
                return;
            }
            MenuBackdrop.Create();
        }

        // ------------------------------------------------------------------ public flow
        public void NewGame(int slot)
        {
            var s = new GameState();
            s.BeginAct(1);
            Game.Saves.ActiveSlot = slot;
            Game.Saves.Save(slot, s);
            Go(ContentDb.Act(1).SceneName, s, slot);
        }

        public void LoadSlot(int slot)
        {
            if (!Game.Saves.Load(slot, out var s))
            {
                Game.UI.Toasts.Show(Game.Loc["ui.load"], Game.Loc["ui.load_failed"]);
                return;
            }
            Game.Saves.ActiveSlot = slot;
            if (s.Act >= 5 || s.Ending != EndingId.None) Go(ContentDb.EndingsScene, s, slot);
            else Go(ContentDb.Act(Mathf.Clamp(s.Act, 1, 4)).SceneName, s, slot);
        }

        public void Continue()
        {
            int slot = Game.Saves.MostRecentSlot();
            if (slot >= 0) LoadSlot(slot);
        }

        public void StartChapter(int act, int slot)
        {
            var s = Game.Saves.ChapterSnapshot(act) ?? ContentDb.ChapterStartState(act);
            s.BeginAct(act);
            s.Checkpoint = "start";
            Game.Saves.ActiveSlot = slot;
            if (slot >= 0) Game.Saves.Save(slot, s);
            Go(ContentDb.Act(act).SceneName, s, slot);
        }

        /// <summary>Called by an act when its end flag is reached.</summary>
        public void ActFinished(GameState s)
        {
            if (s.Act >= 4)
            {
                s.Act = 5;
                s.Checkpoint = "ending";
                if (Game.Saves.ActiveSlot >= 0) Game.Saves.Save(Game.Saves.ActiveSlot, s);
                Go(ContentDb.EndingsScene, s, Game.Saves.ActiveSlot);
                return;
            }
            int next = s.Act + 1;
            s.BeginAct(next);
            s.Checkpoint = "start";
            s.LanternLit = true;
            if (s.Oil < Balance.StartOil * 0.5f) s.Oil = Balance.StartOil * 0.5f; // never start a chapter in the dark
            Game.Saves.SaveChapterSnapshot(next, s);
            Game.Achievements.MarkActReached(next);
            if (Game.Saves.ActiveSlot >= 0) Game.Saves.Save(Game.Saves.ActiveSlot, s);
            Go(ContentDb.Act(next).SceneName, s, Game.Saves.ActiveSlot);
        }

        public void ToMenu()
        {
            Go(ContentDb.MenuScene, null, -1);
        }

        public void ReloadCheckpoint()
        {
            int slot = Game.Saves.ActiveSlot;
            if (slot >= 0 && Game.Saves.Load(slot, out var s))
            {
                Go(ContentDb.Act(Mathf.Clamp(s.Act, 1, 4)).SceneName, s, slot);
                return;
            }
            if (Game.Act != null) Go(Game.Act.Act.SceneName, Game.Act.CheckpointState ?? ContentDb.ChapterStartState(Game.Act.Act.Id), -1);
        }

        public void Go(string scene, GameState state, int slot)
        {
            if (_busy) return;
            StartCoroutine(GoCo(scene, state, slot));
        }

        IEnumerator GoCo(string scene, GameState state, int slot)
        {
            _busy = true;
            Game.UI.Cinematic = false;
            yield return Game.UI.Fade(1f, 0.9f);
            Game.UI.PopAll();
            var act = ContentDb.ActForScene(scene);
            string title = act != null ? Game.Loc[act.NameKey] : "";
            string line = act != null ? Game.Loc[act.LineKey] : "";
            string tip = act != null ? Game.Loc[TipKeys[Random.Range(0, TipKeys.Length)]] : "";
            if (act != null) Game.UI.ShowLoading(title, line, tip);
            Game.Audio.StopAllWorld();
            Game.UI.Subtitles.Clear();
            Game.Act = null;
            Game.UI.InGame = false;
            Time.timeScale = 1f;
            _pendingState = state;
            _pendingSlot = slot;
            var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            while (op != null && !op.isDone) yield return null;
            yield return null;
            BuildForScene(scene);
            if (act != null)
            {
                // let the loading card breathe and the first frames render
                yield return new WaitForSecondsRealtime(1.6f);
                Game.UI.HideLoading();
            }
            yield return Game.UI.Fade(0f, 1.4f);
            _busy = false;
        }
    }
}
