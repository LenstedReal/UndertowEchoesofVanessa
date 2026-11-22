using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Undertow
{
    /// <summary>A full-screen menu or panel on the screen stack.</summary>
    public abstract class UIScreen
    {
        public RectTransform Root;
        public virtual bool PausesGame => true;
        public virtual bool ShowCursor => true;
        public virtual bool DimBackground => true;
        protected Selectable FirstSelected;

        public abstract void Build(RectTransform root);
        public virtual void OnShow() => Select(FirstSelected);
        public virtual void OnHide() { }
        public virtual void Tick() { }

        /// <summary>Back / Escape / B. Default: close this screen.</summary>
        public virtual void OnBack()
        {
            UIKit.PlayBack();
            Game.UI.Pop(this);
        }

        /// <summary>Set when the language changed while this screen was covered: it rebuilds when shown again.</summary>
        public bool LanguageDirty;
        public virtual void OnLanguageChanged() => LanguageDirty = true;

        /// <summary>Rebuilds the screen in place (new language, new layout).</summary>
        public void Rebuild()
        {
            LanguageDirty = false;
            foreach (Transform c in Root) UnityEngine.Object.Destroy(c.gameObject);
            Build(Root);
            OnShow();
        }

        protected static void Select(Selectable s)
        {
            if (s == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(s.gameObject);
        }
    }

    /// <summary>
    /// The single persistent canvas: HUD, screen stack, subtitles, toasts, fades and loading.
    /// Decides cursor lock and whether gameplay input is active.
    /// </summary>
    public sealed class UIRoot : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public RectTransform HudLayer { get; private set; }
        public RectTransform ScreenLayer { get; private set; }
        public RectTransform OverlayLayer { get; private set; }
        public Hud Hud { get; private set; }
        public Subtitles Subtitles { get; private set; }
        public Toasts Toasts { get; private set; }

        readonly List<UIScreen> _stack = new List<UIScreen>();
        Image _fade;
        Image _dim;
        RectTransform _loading;
        Text _loadTitle, _loadLine, _loadTip;
        Image _spinner;
        Text _version;
        Coroutine _fadeCo;
        EventSystem _events;
        bool _hudHidden;
        float _fadeAlpha;

        public bool InGame { get; set; }
        public bool Cinematic { get; set; }
        public int ScreenCount => _stack.Count;
        public UIScreen Top => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
        public bool CanPauseNow => InGame && !Cinematic && _stack.Count == 0 && !Loading;
        public bool Loading { get; private set; }

        public static UIRoot Create(Transform parent)
        {
            var go = new GameObject("UI");
            go.layer = 5;
            go.transform.SetParent(parent, false);
            var ui = go.AddComponent<UIRoot>();
            ui.Build();
            return ui;
        }

        void Build()
        {
            Canvas = gameObject.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 100;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform.parent, false);
            _events = esGo.AddComponent<EventSystem>();
            var module = esGo.AddComponent<InputSystemUIInputModule>();
            module.deselectOnBackgroundClick = false;

            var root = (RectTransform)transform;
            HudLayer = UIKit.Stretch(UIKit.Rect(root, "Hud"));
            TouchControls.Build(root);   // TEST BUILD: on-screen buttons above the HUD, below every menu
            _dim = UIKit.Image(root, "Dim", new Color(0, 0, 0, 0.55f));
            UIKit.Stretch((RectTransform)_dim.transform);
            _dim.enabled = false;
            ScreenLayer = UIKit.Stretch(UIKit.Rect(root, "Screens"));
            OverlayLayer = UIKit.Stretch(UIKit.Rect(root, "Overlay"));

            Hud = new Hud(HudLayer);
            Subtitles = new Subtitles(OverlayLayer);
            Toasts = new Toasts(OverlayLayer);

            _version = UIKit.Label(OverlayLayer, "Version", Game.VersionLabel, 16, FontRole.Ui, TextAnchor.LowerRight, new Color(1, 1, 1, 0.28f));
            UIKit.Anchor((RectTransform)_version.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-14, 8), new Vector2(400, 24));

            BuildLoading();
            _fade = UIKit.Image(OverlayLayer, "Fade", Color.black);
            UIKit.Stretch((RectTransform)_fade.transform);
            _fade.raycastTarget = false;
            SetFade(1f);

            Game.Loc.Changed += OnLanguage;
            Game.Achievements.Unlocked += a => Toasts.Achievement(a);
            Game.Settings.Changed += ApplySettings;
            ApplySettings();
        }

        void BuildLoading()
        {
            _loading = UIKit.Stretch(UIKit.Rect(OverlayLayer, "Loading"));
            var bg = _loading.gameObject.AddComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = true;
            _loadTitle = UIKit.Label(_loading, "Title", "", 76, FontRole.Title, TextAnchor.MiddleCenter);
            UIKit.Anchor((RectTransform)_loadTitle.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1600, 110));
            _loadLine = UIKit.Label(_loading, "Line", "", 30, FontRole.Document, TextAnchor.MiddleCenter, UIKit.Dim);
            UIKit.Anchor((RectTransform)_loadLine.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1400, 60));
            _loadLine.fontStyle = FontStyle.Italic;
            _loadTip = UIKit.Label(_loading, "Tip", "", 24, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Faint);
            UIKit.Anchor((RectTransform)_loadTip.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1400, 60));
            _spinner = UIKit.Image(_loading, "Spinner", UIKit.Accent, UIKit.Ring);
            UIKit.Anchor((RectTransform)_spinner.transform, new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(-70, 70), new Vector2(46, 46));
            _loading.gameObject.SetActive(false);
        }

        void ApplySettings()
        {
            _version.enabled = Game.Settings.ShowVersion;
            Subtitles.ApplySettings();
            Hud.ApplySettings();
        }

        void OnLanguage()
        {
            var screens = _stack.ToArray();
            foreach (var s in screens) s.OnLanguageChanged();
            var top = Top;
            if (top != null && top.LanguageDirty) top.Rebuild();
            Hud.OnLanguageChanged();
        }

        // ------------------------------------------------------------------ screen stack
        public T Push<T>(T screen) where T : UIScreen
        {
            screen.Root = UIKit.Stretch(UIKit.Rect(ScreenLayer, screen.GetType().Name));
            screen.Build(screen.Root);
            foreach (var s in _stack) s.Root.gameObject.SetActive(false);
            _stack.Add(screen);
            screen.OnShow();
            RefreshMode();
            return screen;
        }

        public void Pop(UIScreen screen)
        {
            int i = _stack.IndexOf(screen);
            if (i < 0) return;
            _stack.RemoveAt(i);
            screen.OnHide();
            Destroy(screen.Root.gameObject);
            var top = Top;
            if (top != null)
            {
                top.Root.gameObject.SetActive(true);
                if (top.LanguageDirty) top.Rebuild();
                else top.OnShow();
            }
            RefreshMode();
        }

        public void PopAll()
        {
            while (_stack.Count > 0) Pop(_stack[_stack.Count - 1]);
        }

        public void Replace(UIScreen screen)
        {
            PopAll();
            Push(screen);
        }

        public void RefreshMode()
        {
            var top = Top;
            bool menu = top != null;
            bool pauses = menu && top.PausesGame && InGame;
            Game.Paused = pauses;
            Time.timeScale = pauses ? 0f : 1f;
            AudioListener.pause = pauses;
            _dim.enabled = menu && top.DimBackground && InGame;
            bool cursor = menu ? top.ShowCursor : !InGame;
            Cursor.visible = cursor && !Game.Input.UsingGamepad;
            Cursor.lockState = cursor ? CursorLockMode.None : CursorLockMode.Locked;
            Game.Input.SetGameplay(InGame && !menu && !Cinematic && !Loading);
            HudLayer.gameObject.SetActive(InGame && !menu && !_hudHidden && Game.Settings.HudVisible);
            if (pauses) Game.Input.StopRumble();
        }

        public void OpenPause()
        {
            if (!CanPauseNow) return;
            Push(new PauseMenu());
        }

        public void OpenJournal(int tab = 0)
        {
            if (!CanPauseNow) return;
            Push(new JournalScreen(tab));
        }

        public void Tick()
        {
            TouchControls.Tick();
            // back / cancel for the top screen (gameplay map may be disabled, read devices directly)
            var top = Top;
            if (top != null && !Game.Input.Rebinding)
            {
                bool back = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                            (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame && top is PauseMenu));
                if (back) top.OnBack();
                else top.Tick();
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && Game.Input.UsingGamepad)
                {
                    var sel = top.Root.GetComponentInChildren<Selectable>();
                    if (sel != null) EventSystem.current.SetSelectedGameObject(sel.gameObject);
                }
            }
            else if (CanPauseNow)
            {
                if (Game.Input.Pause.WasPressedThisFrame() || TouchControls.TakePause()) OpenPause();
                else if (Game.Input.Journal.WasPressedThisFrame() || TouchControls.TakeJournal()) OpenJournal();
                else if (Game.Input.Photo.WasPressedThisFrame() && Game.Act != null) Push(new PhotoMode());
                else if (Game.Input.HideHud.WasPressedThisFrame())
                {
                    _hudHidden = !_hudHidden;
                    RefreshMode();
                }
            }
            bool cursor = top != null ? top.ShowCursor : (!InGame || TouchControls.Active);
            Cursor.visible = cursor && !Game.Input.UsingGamepad;
            // the touch test controls need a free cursor during play; RefreshMode locks it otherwise
            if (TouchControls.Enabled && top == null && InGame) Cursor.lockState = cursor ? CursorLockMode.None : CursorLockMode.Locked;
            if (_loading.gameObject.activeSelf) _spinner.rectTransform.Rotate(0, 0, -240f * Time.unscaledDeltaTime);
            Hud.Tick();
            Subtitles.Tick();
            Toasts.Tick();
        }

        // ------------------------------------------------------------------ fades & loading
        public void SetFade(float a)
        {
            _fadeAlpha = a;
            _fade.color = new Color(0, 0, 0, a);
            _fade.enabled = a > 0.001f;
            _fade.raycastTarget = a > 0.5f;
        }

        public float FadeAlpha => _fadeAlpha;

        public Coroutine Fade(float to, float seconds)
        {
            if (_fadeCo != null) StopCoroutine(_fadeCo);
            _fadeCo = StartCoroutine(FadeCo(to, seconds));
            return _fadeCo;
        }

        IEnumerator FadeCo(float to, float seconds)
        {
            float from = _fadeAlpha;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                SetFade(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds)));
                yield return null;
            }
            SetFade(to);
            _fadeCo = null;
        }

        public void ShowLoading(string title, string line, string tip)
        {
            Loading = true;
            _loadTitle.text = title ?? "";
            _loadLine.text = line ?? "";
            _loadTip.text = tip ?? "";
            _loadTitle.font = UIKit.F(FontRole.Title);
            _loadLine.font = UIKit.F(FontRole.Document);
            _loadTip.font = UIKit.F(FontRole.Ui);
            _loading.gameObject.SetActive(true);
            RefreshMode();
        }

        public void HideLoading()
        {
            Loading = false;
            _loading.gameObject.SetActive(false);
            RefreshMode();
        }
    }
}
