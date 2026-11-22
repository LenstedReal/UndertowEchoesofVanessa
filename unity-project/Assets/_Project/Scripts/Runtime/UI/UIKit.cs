using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Undertow
{
    /// <summary>Colours, sizes and builders for the uGUI interface (everything is created in code).</summary>
    public static class UIKit
    {
        public static readonly Color Ink = new Color(0.86f, 0.83f, 0.78f, 1f);
        public static readonly Color Dim = new Color(0.56f, 0.56f, 0.55f, 1f);
        public static readonly Color Faint = new Color(0.36f, 0.37f, 0.38f, 1f);
        public static readonly Color Accent = new Color(1f, 0.573f, 0.153f, 1f);   // #FF9227, the lantern
        public static readonly Color AccentSoft = new Color(1f, 0.573f, 0.153f, 0.18f);
        public static readonly Color Panel = new Color(0.025f, 0.03f, 0.036f, 0.9f);
        public static readonly Color PanelLight = new Color(0.07f, 0.08f, 0.09f, 0.92f);
        public static readonly Color Line = new Color(1f, 1f, 1f, 0.08f);
        public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

        static Sprite _white, _dot, _ring, _soft, _vignette;

        public static Sprite White => _white != null ? _white : (_white = MakeSprite(Texture2D.whiteTexture));

        /// <summary>Soft round dot (crosshair, markers).</summary>
        public static Sprite Dot => _dot != null ? _dot : (_dot = MakeSprite(Radial(64, (r) => Mathf.Clamp01((0.5f - r) * 12f))));

        public static Sprite Ring => _ring != null ? _ring : (_ring = MakeSprite(Radial(128, (r) => Mathf.Clamp01(1f - Mathf.Abs(r - 0.42f) * 40f))));

        /// <summary>Very soft radial glow.</summary>
        public static Sprite Soft => _soft != null ? _soft : (_soft = MakeSprite(Radial(128, (r) => Mathf.Pow(Mathf.Clamp01(1f - r * 2f), 2.2f))));

        public static Sprite Vignette => _vignette != null ? _vignette : (_vignette = MakeSprite(Radial(256, (r) => Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((r - 0.32f) * 2.6f)))));

        static Texture2D Radial(int n, Func<float, float> alpha)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "ui_radial" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n - 0.5f, dy = (y + 0.5f) / n - 0.5f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(alpha(r))));
                }
            t.SetPixels32(px);
            t.Apply(false, true);
            return t;
        }

        static Sprite MakeSprite(Texture2D t) => Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f);

        public static Font F(FontRole role) => Game.Loc.FontFor(role);

        // ------------------------------------------------------------------ rect helpers
        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>A band anchored to the left edge that always spans the full screen height (any aspect ratio).</summary>
        public static RectTransform FullHeight(RectTransform rt, float pivotX, float x, float width, float extraHeight = 0f)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(pivotX, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.sizeDelta = new Vector2(width, extraHeight);
            return rt;
        }

        public static RectTransform Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>
        /// A clickable text for the mouse and touch, kept out of keyboard / pad navigation.
        /// Plays no sound itself: the action decides (move, confirm or back).
        /// </summary>
        public static RectTransform TextButton(Transform parent, string name, string text, int size, Action onClick, Color? background = null)
        {
            var rt = Rect(parent, name);
            var bg = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
            bg.color = background ?? new Color(1f, 1f, 1f, 0.06f);
            bg.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>();
            b.transition = Selectable.Transition.None;
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            b.onClick.AddListener(() => onClick?.Invoke());
            var t = Label(rt, "Text", text, size, FontRole.Ui, TextAnchor.MiddleCenter, Ink);
            Stretch((RectTransform)t.transform);
            return rt;
        }

        public static Image Image(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite ?? White;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, FontRole role = FontRole.Ui, TextAnchor anchor = TextAnchor.MiddleLeft, Color? color = null)
        {
            var rt = Rect(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = F(role);
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color ?? Ink;
            t.text = text;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            t.lineSpacing = 1.05f;
            return t;
        }

        public static VerticalLayoutGroup VList(RectTransform rt, float spacing, TextAnchor align = TextAnchor.UpperLeft, RectOffset pad = null)
        {
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.childAlignment = align;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            if (pad != null) v.padding = pad;
            return v;
        }

        public static LayoutElement Size(Component c, float height = -1, float width = -1, float flexW = -1)
        {
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            if (height >= 0) { le.minHeight = height; le.preferredHeight = height; }
            if (width >= 0) { le.minWidth = width; le.preferredWidth = width; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            return le;
        }

        /// <summary>Scrolling vertical list. Returns the content rect to add rows to.</summary>
        public static RectTransform ScrollList(RectTransform parent, out ScrollRect scroll, float spacing = 6f)
        {
            var view = Rect(parent, "Viewport");
            Stretch(view);
            view.gameObject.AddComponent<RectMask2D>();
            var hit = view.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            var content = Rect(view, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            VList(content, spacing);
            var fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll = parent.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = view;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            return content;
        }

        /// <summary>Wires up/down navigation for a vertical menu (wrapping).</summary>
        public static void ChainVertical(IList<Selectable> items, bool wrap = true)
        {
            int n = items.Count;
            for (int i = 0; i < n; i++)
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit };
                nav.selectOnUp = i > 0 ? items[i - 1] : (wrap ? items[n - 1] : null);
                nav.selectOnDown = i < n - 1 ? items[i + 1] : (wrap ? items[0] : null);
                items[i].navigation = nav;
            }
        }

        public static void ChainHorizontal(IList<Selectable> items, bool wrap = true)
        {
            int n = items.Count;
            for (int i = 0; i < n; i++)
            {
                var nav = items[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnLeft = i > 0 ? items[i - 1] : (wrap ? items[n - 1] : null);
                nav.selectOnRight = i < n - 1 ? items[i + 1] : (wrap ? items[0] : null);
                items[i].navigation = nav;
            }
        }

        public static void PlayMove() => Game.Audio?.Play2D("ui_move", Bus.Ui, 0.45f);
        public static void PlayConfirm() => Game.Audio?.Play2D("ui_confirm", Bus.Ui, 0.6f);
        public static void PlayBack() => Game.Audio?.Play2D("ui_back", Bus.Ui, 0.5f);
    }

    /// <summary>A text menu entry: lantern-orange with a flame marker when selected.</summary>
    public class MenuButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        public Text Label;
        public Image Marker;
        public Image Highlight;
        public Action OnClick;
        public bool Disabled;
        protected bool Selected;

        public static T Create<T>(Transform parent, string text, int size, Action onClick, float height = 58f) where T : MenuButton
        {
            var rt = UIKit.Rect(parent, "Item");
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0);
            bg.raycastTarget = true;
            var b = rt.gameObject.AddComponent<T>();
            b.targetGraphic = bg;
            b.transition = Transition.None;
            b.Highlight = UIKit.Image(rt, "Highlight", UIKit.AccentSoft, UIKit.White);
            UIKit.Stretch((RectTransform)b.Highlight.transform);
            b.Highlight.enabled = false;
            b.Marker = UIKit.Image(rt, "Marker", UIKit.Accent, UIKit.Dot);
            UIKit.Anchor((RectTransform)b.Marker.transform, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14, 0), new Vector2(14, 14));
            b.Marker.enabled = false;
            b.Label = UIKit.Label(rt, "Label", text, size, FontRole.Title, TextAnchor.MiddleLeft);
            UIKit.Stretch((RectTransform)b.Label.transform, 34, 0, 12, 0);
            b.OnClick = onClick;
            UIKit.Size(b, height);
            b.Refresh();
            return b;
        }

        public virtual void Refresh()
        {
            if (Label == null) return;
            Label.color = Disabled ? UIKit.Faint : (Selected ? UIKit.Accent : UIKit.Ink);
            if (Marker != null) Marker.enabled = Selected && !Disabled;
            if (Highlight != null) Highlight.enabled = Selected;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (!Selected) UIKit.PlayMove();
            Selected = true;
            Refresh();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Selected = false;
            Refresh();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsInteractable() && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData) => Press();

        protected virtual void Press()
        {
            if (Disabled || !IsInteractable()) return;
            UIKit.PlayConfirm();
            OnClick?.Invoke();
        }
    }

    /// <summary>"Label ........ &lt; value &gt;" — left/right (keys, stick, or the arrows) change the value.</summary>
    public class MenuOption : MenuButton, IMoveHandler
    {
        public Text Value;
        public Func<string> Read;
        public Action<int> Step;      // -1 / +1
        Text _left, _right;

        public static MenuOption Make(Transform parent, string label, Func<string> read, Action<int> step)
        {
            var o = Create<MenuOption>(parent, label, 30, null, 52f);
            o.Label.font = UIKit.F(FontRole.Ui);
            o.Label.fontSize = 27;
            o.Read = read;
            o.Step = step;
            var vr = UIKit.Rect(o.transform, "Value");
            UIKit.Anchor(vr, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-54, 0), new Vector2(360, 48));
            o.Value = vr.gameObject.AddComponent<Text>();
            o.Value.font = UIKit.F(FontRole.Ui);
            o.Value.fontSize = 26;
            o.Value.alignment = TextAnchor.MiddleCenter;
            o.Value.color = UIKit.Ink;
            o.Value.raycastTarget = false;
            o._left = o.Arrow("<", -1, new Vector2(-430, 0));
            o._right = o.Arrow(">", 1, new Vector2(-14, 0));
            o.OnClick = () => o.Change(1);
            o.Refresh();
            return o;
        }

        Text Arrow(string glyph, int dir, Vector2 pos)
        {
            var rt = UIKit.Rect(transform, glyph == "<" ? "Left" : "Right");
            UIKit.Anchor(rt, new Vector2(1, 0.5f), new Vector2(1, 0.5f), pos, new Vector2(40, 44));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.transition = Transition.None;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(() => Change(dir));
            var t = UIKit.Label(rt, "G", glyph, 28, FontRole.Ui, TextAnchor.MiddleCenter, UIKit.Dim);
            UIKit.Stretch((RectTransform)t.transform);
            return t;
        }

        public void Change(int dir)
        {
            if (Disabled) return;
            Step?.Invoke(dir);
            UIKit.PlayMove();
            Refresh();
        }

        protected override void Press() => Change(1);

        public override void Refresh()
        {
            base.Refresh();
            if (Value != null && Read != null) Value.text = Read();
            if (_left != null) _left.color = Selected ? UIKit.Accent : UIKit.Faint;
            if (_right != null) _right.color = Selected ? UIKit.Accent : UIKit.Faint;
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left) { Change(-1); eventData.Use(); return; }
            if (eventData.moveDir == MoveDirection.Right) { Change(1); eventData.Use(); return; }
            base.OnMove(eventData);
        }
    }

    /// <summary>Slider row: value bar plus number; left/right step, mouse click sets.</summary>
    public sealed class MenuSlider : MenuOption, IPointerDownHandler, IDragHandler
    {
        public Image Fill;
        public Func<float> Get01;
        public Action<float> Set01;
        RectTransform _bar;

        public static MenuSlider MakeSlider(Transform parent, string label, Func<float> get01, Action<float> set01, Func<string> text, float step = 0.05f)
        {
            var s = Create<MenuSlider>(parent, label, 30, null, 52f);
            s.Label.font = UIKit.F(FontRole.Ui);
            s.Label.fontSize = 27;
            s.Get01 = get01;
            s.Set01 = set01;
            s.Read = text;
            s.Step = d => s.Set01(Mathf.Clamp01(s.Get01() + d * step));
            s._bar = UIKit.Rect(s.transform, "Bar");
            UIKit.Anchor(s._bar, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-120, 0), new Vector2(300, 6));
            var bg = s._bar.gameObject.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.12f);
            s.Fill = UIKit.Image(s._bar, "Fill", UIKit.Ink);
            var fr = (RectTransform)s.Fill.transform;
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = new Vector2(0, 1);
            fr.pivot = new Vector2(0, 0.5f);
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = Vector2.zero;
            var vr = UIKit.Rect(s.transform, "Value");
            UIKit.Anchor(vr, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(96, 44));
            s.Value = vr.gameObject.AddComponent<Text>();
            s.Value.font = UIKit.F(FontRole.Ui);
            s.Value.fontSize = 24;
            s.Value.alignment = TextAnchor.MiddleRight;
            s.Value.color = UIKit.Ink;
            s.Value.raycastTarget = false;
            s.OnClick = null;
            s.Refresh();
            return s;
        }

        public override void Refresh()
        {
            if (Label == null) return;
            Label.color = Disabled ? UIKit.Faint : (Selected ? UIKit.Accent : UIKit.Ink);
            if (Marker != null) Marker.enabled = Selected;
            if (Highlight != null) Highlight.enabled = Selected;
            if (Value != null && Read != null) Value.text = Read();
            if (Fill != null && Get01 != null)
            {
                var fr = (RectTransform)Fill.transform;
                fr.anchorMax = new Vector2(Mathf.Clamp01(Get01()), 1);
                Fill.color = Selected ? UIKit.Accent : UIKit.Ink;
            }
        }

        protected override void Press() { }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            SetFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData) => SetFromPointer(eventData);

        void SetFromPointer(PointerEventData e)
        {
            if (_bar == null || Set01 == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_bar, e.position, e.pressEventCamera, out var local)) return;
            float t = Mathf.Clamp01((local.x - _bar.rect.xMin) / _bar.rect.width);
            if (local.y < -40f || local.y > 40f) return;
            Set01(t);
            Refresh();
        }
    }

    /// <summary>
    /// Keeps the selected row of a scroll list in view after keyboard or gamepad navigation, and leaves the
    /// list alone while the mouse drives it (wheel, drag, hover) so the two never fight over the position.
    /// </summary>
    public sealed class ScrollFollow
    {
        GameObject _last;
        float _until;

        public void Tick(ScrollRect scroll, float speed = 10f)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null || scroll == null) return;
            var sel = es.currentSelectedGameObject;
            var m = UnityEngine.InputSystem.Mouse.current;
            bool mouseBusy = m != null && (m.scroll.ReadValue().sqrMagnitude > 0.001f || m.delta.ReadValue().sqrMagnitude > 0.001f || m.leftButton.isPressed);
            if (sel != _last)
            {
                _last = sel;
                _until = mouseBusy ? 0f : Time.unscaledTime + 0.8f;
            }
            else if (mouseBusy) _until = 0f;
            if (sel == null || Time.unscaledTime > _until) return;
            var rt = sel.transform as RectTransform;
            if (rt == null || scroll.content == null || scroll.viewport == null || !rt.IsChildOf(scroll.content)) return;
            float contentH = scroll.content.rect.height, viewH = scroll.viewport.rect.height;
            if (contentH <= viewH) return;
            float y = -rt.anchoredPosition.y;
            float target = Mathf.Clamp01((y - viewH * 0.5f) / (contentH - viewH));
            scroll.verticalNormalizedPosition = Mathf.Lerp(scroll.verticalNormalizedPosition, 1f - target, Time.unscaledDeltaTime * speed);
        }
    }
}
