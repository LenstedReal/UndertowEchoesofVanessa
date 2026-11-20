using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>
    /// An item the player can use. Holds its prop, moving part and feedback animations
    /// (bells swing, crates slide, dial hands turn, mirrors rotate, hatch lids lift).
    /// </summary>
    public sealed class Interactable : MonoBehaviour
    {
        public ItemDef Item;
        public Transform Moving;          // animated part (bell, lid, hand, mirror head, crate body)
        public Transform Hand2;           // minute hand for dials
        public World World;
        public Light Glow;                // brazier fire etc.
        public Renderer[] Visuals;
        public Vector3 Point;             // world point the prompt refers to
        bool _gone;

        public bool Gone => _gone;

        public string Prompt
        {
            get
            {
                var loc = Game.Loc;
                if (Item.Kind == ItemKind.Clock && Game.Act != null)
                {
                    int v = Game.Act.Session.DialValue(Item);
                    return loc[Item.PromptKey] + "  <color=#ffffffaa>" + Item.Options[v] + "</color>";
                }
                return loc[Item.PromptKey];
            }
        }

        public void Hide()
        {
            _gone = true;
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            StartCoroutine(Shrink());
        }

        IEnumerator Shrink()
        {
            var s = transform.localScale;
            for (float t = 0; t < 0.35f; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(s, s * 0.01f, t / 0.35f);
                yield return null;
            }
            gameObject.SetActive(false);
        }

        /// <summary>Puts moving parts where the saved state says (after a load).</summary>
        public void Sync(ActSession s)
        {
            if (s.IsConsumed(Item) && (Item.Kind == ItemKind.Pickup || Item.Kind == ItemKind.Oil || Item.Kind == ItemKind.Drawing))
            {
                _gone = true;
                gameObject.SetActive(false);
                return;
            }
            switch (Item.Kind)
            {
                case ItemKind.Crate:
                {
                    var slot = Item.Slots[s.CrateSlot(Item)];
                    transform.position = World.Ground(slot.X, slot.Z, transform.position.y + 0.5f);
                    break;
                }
                case ItemKind.Clock:
                    SetDial(Item.Options[s.DialValue(Item)], true);
                    break;
                case ItemKind.Mirror:
                    if (Moving != null) Moving.localRotation = Quaternion.Euler(0, s.MirrorTurns(Item) * 90f, 0);
                    break;
                case ItemKind.Hatch:
                    if (s.State.Has(Item.Flag) && Moving != null) Moving.localRotation = Quaternion.Euler(112f, 0, 0);
                    break;
                case ItemKind.Brazier:
                    SetFire(s.State.Has(Item.Flag), true);
                    break;
            }
        }

        // ------------------------------------------------------------------ feedback
        public void Feedback(InteractResult r)
        {
            switch (r.Outcome)
            {
                case Outcome.BellRung:
                case Outcome.BellSolved:
                case Outcome.BellWrong:
                    StartCoroutine(Swing());
                    break;
                case Outcome.CrateMoved:
                case Outcome.CrateSolved:
                {
                    var slot = Item.Slots[r.Slot];
                    StartCoroutine(Slide(World.Ground(slot.X, slot.Z, transform.position.y + 0.5f)));
                    break;
                }
                case Outcome.DialTurned:
                case Outcome.DialSolved:
                    SetDial(Item.Options[r.Dial], false);
                    break;
                case Outcome.MirrorTurned:
                case Outcome.MirrorAligned:
                    if (Moving != null) StartCoroutine(Turn(Moving, Moving.localRotation * Quaternion.Euler(0, 90f, 0), 0.7f));
                    break;
                case Outcome.HatchOpened:
                    if (Moving != null) StartCoroutine(Turn(Moving, Quaternion.Euler(112f, 0, 0), 1.6f));
                    foreach (var c in GetComponentsInChildren<Collider>()) if (!c.isTrigger) c.enabled = false;
                    break;
                case Outcome.Reset:
                    if (Moving != null) StartCoroutine(Pull());
                    break;
                case Outcome.Pickup:
                case Outcome.Oil:
                case Outcome.Drawing:
                    Hide();
                    break;
                case Outcome.BrazierLit:
                    SetFire(true, false);
                    break;
            }
        }

        IEnumerator Swing()
        {
            if (Moving == null) yield break;
            var baseRot = Moving.localRotation;
            float amp = 16f;
            for (float t = 0; t < 2.4f; t += Time.deltaTime)
            {
                float a = amp * Mathf.Exp(-t * 1.6f) * Mathf.Sin(t * 9f);
                Moving.localRotation = baseRot * Quaternion.Euler(a, 0, 0);
                yield return null;
            }
            Moving.localRotation = baseRot;
        }

        IEnumerator Slide(Vector3 to)
        {
            var from = transform.position;
            Game.Audio.PlayAt("sfx_crate_scrape", from, Bus.Effects, 0.8f, Random.Range(0.92f, 1.06f));
            for (float t = 0; t < 1f; t += Time.deltaTime / 1.1f)
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.015f;
                yield return null;
            }
            transform.position = to;
            Game.Audio.PlayAt("sfx_crate_thud", to, Bus.Effects, 0.7f);
        }

        static IEnumerator Turn(Transform t, Quaternion to, float seconds)
        {
            var from = t.localRotation;
            for (float e = 0; e < seconds; e += Time.deltaTime)
            {
                t.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, e / seconds));
                yield return null;
            }
            t.localRotation = to;
        }

        IEnumerator Pull()
        {
            var baseRot = Moving.localRotation;
            yield return Turn(Moving, baseRot * Quaternion.Euler(55f, 0, 0), 0.35f);
            yield return new WaitForSeconds(0.3f);
            yield return Turn(Moving, baseRot, 0.6f);
        }

        void SetDial(string hhmm, bool instant)
        {
            if (Moving == null || string.IsNullOrEmpty(hhmm) || hhmm.Length < 5) return;
            int h = (hhmm[0] - '0') * 10 + (hhmm[1] - '0');
            int m = (hhmm[3] - '0') * 10 + (hhmm[4] - '0');
            float hourA = ((h % 12) + m / 60f) * 30f;
            float minA = m * 6f;
            var hr = Quaternion.Euler(0, 0, -hourA);
            var mr = Quaternion.Euler(0, 0, -minA);
            if (instant)
            {
                Moving.localRotation = hr;
                if (Hand2 != null) Hand2.localRotation = mr;
                return;
            }
            StartCoroutine(Turn(Moving, hr, 0.8f));
            if (Hand2 != null) StartCoroutine(Turn(Hand2, mr, 0.8f));
        }

        void SetFire(bool on, bool instant)
        {
            if (Moving != null) Moving.gameObject.SetActive(on);
            if (Glow != null) Glow.enabled = on;
            if (on && !instant)
            {
                Game.Audio.PlayAt("sfx_fire_ignite", transform.position + Vector3.up, Bus.Effects, 0.9f);
                Game.UI.Subtitles.Caption(Game.Loc["caption.fire"], 2.5f);
            }
        }
    }

    /// <summary>Spawns the prop for every item of the act and wires its Interactable.</summary>
    public static class ItemFactory
    {
        public static void SpawnAll(World w, GameState state)
        {
            var parent = new GameObject("Items").transform;
            parent.SetParent(w.Root, false);
            foreach (var it in w.Act.Items)
            {
                var go = Spawn(w, it, parent);
                if (go == null) continue;
                var ia = go.GetComponent<Interactable>();
                w.Items[it.Id] = ia;
            }
        }

        static Quaternion Facing(ItemDef it) => Quaternion.Euler(0f, it.Yaw, 0f);

        static GameObject Root(World w, ItemDef it, Transform parent, Vector3 pos, out Interactable ia)
        {
            var go = new GameObject("item_" + it.Id);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Facing(it);
            ia = go.AddComponent<Interactable>();
            ia.Item = it;
            ia.World = w;
            ia.Point = w.ItemPoint(it);
            return go;
        }

        static void Trigger(GameObject go, Vector3 center, Vector3 size)
        {
            var bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.center = center;
            bc.size = new Vector3(Mathf.Max(0.3f, size.x), Mathf.Max(0.3f, size.y), Mathf.Max(0.3f, size.z));
        }

        static void Solid(GameObject go, Vector3 center, Vector3 size)
        {
            var child = new GameObject("solid");
            child.transform.SetParent(go.transform, false);
            var bc = child.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }

        static GameObject Child(PropMesh pm, string name, Transform parent, Vector3 local, Quaternion rot)
        {
            var c = pm.Spawn(name, parent, local, rot);
            return c;
        }

        /// <summary>Furniture under a small item so it sits at the right height (static, baked).</summary>
        static void Support(World w, ItemDef it, Vector3 floorPos, float top)
        {
            if (top < 0.25f) return;
            var rot = Facing(it);
            string ch = it.Room;
            PropMesh pm;
            if (top > 0.82f && top < 1.02f) pm = Props.Barrel(top, 0.32f);
            else if (top <= 0.82f && top > 0.5f) pm = Props.Table(0.9f, 0.6f, top);
            else if (top <= 0.5f) pm = Props.Crate(new Vector3(0.6f, top, 0.6f));
            else pm = Props.Crate(new Vector3(0.7f, top, 0.7f));
            pm.BakeInto(w.Kit, ch, floorPos, rot);
            w.Kit.Collider(floorPos + Vector3.up * top * 0.5f, new Vector3(0.62f, top, 0.62f), rot);
        }

        public static GameObject Spawn(World w, ItemDef it, Transform parent)
        {
            var room = w.Act.Room(it.Room);
            float floor = room != null ? room.FloorAt(it.Pos.X, it.Pos.Z) : 0f;
            var floorPos = new Vector3(it.Pos.X, floor, it.Pos.Z);
            var point = new Vector3(it.Pos.X, floor + it.Height, it.Pos.Z);
            GameObject go;
            Interactable ia;
            switch (it.Prop)
            {
                case "oilskin":
                case "oilflask":
                {
                    float top = Mathf.Max(0f, it.Height - 0.12f);
                    Support(w, it, floorPos, top);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    Child(Props.OilFlask(), "flask", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.15f, 0), new Vector3(0.35f, 0.4f, 0.35f));
                    break;
                }
                case "logstand":
                case "lectern":
                    go = Root(w, it, parent, floorPos, out ia);
                    Child(Props.Lectern(), "lectern", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.55f, 0), new Vector3(0.5f, 1.1f, 0.45f));
                    Trigger(go, new Vector3(0, 1.05f, 0), new Vector3(0.6f, 0.4f, 0.5f));
                    break;
                case "pocketwatch":
                case "wristwatch":
                {
                    float top = it.Height - 0.02f;
                    if (it.Prop == "wristwatch") Support(w, it, floorPos, top);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    Child(Props.PocketWatch(), "watch", go.transform, Vector3.zero, it.Prop == "pocketwatch" ? Quaternion.Euler(70f, 0, 0) * Quaternion.Euler(0, 180f, 0) : Quaternion.identity);
                    Trigger(go, Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f));
                    break;
                }
                case "carving":
                case "chalk":
                case "windowplaque":
                {
                    go = Root(w, it, parent, point, out ia);
                    string mat = it.Prop == "chalk" ? "chalk" : (it.Act == 2 ? "carving2" : (it.Prop == "windowplaque" ? "plaque" : "carving"));
                    var pm = new PropMesh();
                    pm[mat].QuadUV(new Vector3(-0.45f, -0.18f, 0.01f), new Vector3(0.45f, -0.18f, 0.01f), new Vector3(0.45f, 0.18f, 0.01f), new Vector3(-0.45f, 0.18f, 0.01f),
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));
                    pm.Spawn("text", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0, 0.1f), new Vector3(1.0f, 0.5f, 0.3f));
                    break;
                }
                case "bunknote":
                case "childnote":
                case "counternote":
                case "map":
                case "sealedletter":
                {
                    float top = it.Height - 0.01f;
                    if (it.Prop != "childnote" && it.Prop != "bunknote" && it.Prop != "counternote") Support(w, it, floorPos, top);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    var paper = Props.Paper(it.Prop == "map" ? "parchment" : "paper", it.Prop == "map" ? 0.42f : 0.21f, it.Prop == "map" ? 0.32f : 0.28f);
                    paper.Spawn("paper", go.transform, Vector3.zero, Quaternion.Euler(0, 12f, 0));
                    if (it.Prop == "sealedletter")
                    {
                        var seal = new PropMesh();
                        seal["cloth_red"].Cylinder(new Vector3(0.03f, 0.004f, 0.02f), Quaternion.identity, 0.006f, 0.022f, 0.022f, 10);
                        seal.Spawn("seal", go.transform, Vector3.zero, Quaternion.identity);
                    }
                    Trigger(go, new Vector3(0, 0.06f, 0), new Vector3(0.45f, 0.25f, 0.45f));
                    break;
                }
                case "drawing":
                    go = Root(w, it, parent, point + Vector3.down * 0.2f, out ia);
                    Props.Pinned("drawing" + it.Drawing, 0.3f, 0.4f).Spawn("drawing", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.2f, 0.08f), new Vector3(0.45f, 0.5f, 0.25f));
                    break;
                case "portrait":
                    go = Root(w, it, parent, point + Vector3.down * 0.55f, out ia);
                    Props.Framed("portrait", 0.8f, 1.05f, "wood_dark").Spawn("portrait", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.55f, 0.15f), new Vector3(1.0f, 1.2f, 0.3f));
                    break;
                case "familytree":
                    go = Root(w, it, parent, point + Vector3.down * 0.9f, out ia);
                    Props.Framed("familytree", 2.2f, 1.5f, "wood_dark", 0.06f).Spawn("tree", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.75f, 0.2f), new Vector3(2.3f, 1.6f, 0.4f));
                    break;
                case "musicbox":
                case "twoboxes":
                case "echobox":
                case "mimicbox":
                {
                    float top = it.Height - 0.05f;
                    var tableRot = Facing(it);
                    Props.Table(it.Prop == "twoboxes" ? 1.5f : 0.8f, 0.7f, top, it.Prop == "twoboxes" ? "wood_dark" : "wood").BakeInto(w.Kit, it.Room, floorPos, tableRot);
                    w.Kit.Collider(floorPos + Vector3.up * top * 0.5f, new Vector3(it.Prop == "twoboxes" ? 1.5f : 0.8f, top, 0.7f), tableRot);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    if (it.Prop == "twoboxes")
                    {
                        Child(Props.MusicBox(100f, true), "boxEcho", go.transform, new Vector3(-0.3f, 0, 0), Quaternion.Euler(0, 15f, 0));
                        Child(Props.MusicBox(95f, false), "boxMimic", go.transform, new Vector3(0.3f, 0, 0), Quaternion.Euler(0, -15f, 0));
                    }
                    else
                    {
                        var box = Child(Props.MusicBox(it.Prop == "mimicbox" ? 100f : 110f, it.Prop != "mimicbox"), "box", go.transform, Vector3.zero, Quaternion.identity);
                        ia.Moving = box.transform;
                    }
                    Trigger(go, new Vector3(0, 0.12f, 0), new Vector3(it.Prop == "twoboxes" ? 1.2f : 0.5f, 0.4f, 0.5f));
                    break;
                }
                case "bell":
                {
                    float r = it.Act == 1 ? 0.2f - it.Index * 0.016f : 0.26f - it.Index * 0.03f;
                    go = Root(w, it, parent, point + Vector3.up * (r * 1.42f * 0.5f + 0.05f), out ia);
                    var pivot = new GameObject("pivot").transform;
                    pivot.SetParent(go.transform, false);
                    Child(Props.Bell(r), "bell", pivot, Vector3.zero, Quaternion.identity);
                    ia.Moving = pivot;
                    var rope = new PropMesh();
                    rope["rope"].Tube(new[] { new Vector3(0, -r * 1.3f, 0), new Vector3(0.02f, -r * 1.3f - 0.45f, 0.01f) }, 0.008f, 5, 0.2f);
                    rope.Spawn("pull", pivot, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, -r * 0.8f, 0), new Vector3(r * 2.4f, r * 2.2f + 0.4f, r * 2.4f));
                    break;
                }
                case "surgtable":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    var t = Props.Table(1.9f, 0.8f, 0.86f, "wood_dark");
                    t["cloth"].Box(new Vector3(0, 0.875f, 0), new Vector3(1.7f, 0.02f, 0.7f), Quaternion.Euler(0, 0, 1.5f));
                    t["iron"].Box(new Vector3(0.5f, 0.9f, 0.1f), new Vector3(0.18f, 0.01f, 0.02f), Quaternion.Euler(0, 30, 0));
                    t["iron"].Box(new Vector3(0.62f, 0.9f, -0.05f), new Vector3(0.14f, 0.01f, 0.02f), Quaternion.Euler(0, -12, 0));
                    t["parchment"].Box(new Vector3(-0.4f, 0.89f, 0.0f), new Vector3(0.5f, 0.004f, 0.36f), Quaternion.Euler(0, 8, 0));
                    t.Spawn("table", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.43f, 0), new Vector3(1.9f, 0.86f, 0.8f));
                    Trigger(go, new Vector3(0, 0.95f, 0), new Vector3(1.9f, 0.4f, 0.9f));
                    break;
                }
                case "hook":
                case "nail":
                    go = Root(w, it, parent, point, out ia);
                    if (it.Prop == "hook") Child(Props.Hook(), "hook", go.transform, new Vector3(0, -0.25f, 0.05f), Quaternion.identity);
                    else
                    {
                        Child(Props.Figurehead(), "figurehead", go.transform, new Vector3(0, -1.45f, -0.25f), Quaternion.identity);
                        Child(Props.Nail(), "nail", go.transform, new Vector3(0, 0, 0.02f), Quaternion.identity);
                    }
                    Trigger(go, Vector3.zero, new Vector3(0.45f, 0.5f, 0.4f));
                    break;
                case "hatch":
                {
                    // lid over the stair hole; walkable while closed, hinge at the far edge
                    var stair = w.Act.Rooms.Find(r => r.OpenFlag == it.Flag);
                    var rect = stair != null ? stair.Rect : new RectXZ(it.Pos.X - 2, it.Pos.Z + 1, 4, 6);
                    float holeD = 6f;
                    var hinge = new Vector3(rect.X + rect.W * 0.5f, 0.0f, rect.Z + holeD);
                    go = Root(w, it, parent, point, out ia);
                    go.transform.rotation = Quaternion.identity;
                    var pivot = new GameObject("lidPivot").transform;
                    pivot.SetParent(go.transform, false);
                    pivot.position = hinge;
                    var lid = Props.Hatch(rect.W + 0.3f, holeD + 0.1f);
                    lid.Spawn("lid", pivot, new Vector3(0, -0.1f, -holeD * 0.5f), Quaternion.identity);
                    var lc = new GameObject("lidCollider");
                    lc.transform.SetParent(pivot, false);
                    var bc = lc.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0, -0.05f, -holeD * 0.5f);
                    bc.size = new Vector3(rect.W + 0.3f, 0.2f, holeD + 0.1f);
                    ia.Moving = pivot;
                    Trigger(go, Vector3.zero, new Vector3(1.6f, 0.6f, 1.0f));
                    break;
                }
                case "crate":
                case "debriscrate":
                {
                    var slot = it.Slots[0];
                    go = Root(w, it, parent, new Vector3(slot.X, floor, slot.Z), out ia);
                    go.transform.rotation = Quaternion.identity;
                    var size = it.Prop == "crate" ? new Vector3(1.0f, 0.9f, 1.0f) : new Vector3(1.1f, 0.8f, 1.1f);
                    var pm = Props.Crate(size);
                    if (it.Prop == "debriscrate") pm["wood"].Box(new Vector3(0.1f, size.y + 0.05f, 0), new Vector3(1.5f, 0.08f, 0.22f), Quaternion.Euler(0, 30, 6));
                    pm.Spawn("crate", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, size.y * 0.5f, 0), size);
                    Trigger(go, new Vector3(0, size.y * 0.5f, 0), size + Vector3.one * 0.25f);
                    break;
                }
                case "lever":
                    go = Root(w, it, parent, floorPos, out ia);
                    var lev = Props.Lever();
                    lev.Spawn("lever", go.transform, Vector3.zero, Quaternion.identity);
                    var arm = new GameObject("arm").transform;
                    arm.SetParent(go.transform, false);
                    arm.localPosition = new Vector3(0, 0.6f, 0.02f);
                    ia.Moving = arm;
                    Trigger(go, new Vector3(0, 0.9f, 0.1f), new Vector3(0.5f, 0.9f, 0.4f));
                    break;
                case "locket":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.RopeCoil(0.34f).Spawn("coil", go.transform, Vector3.zero, Quaternion.identity);
                    Child(Props.Locket(), "locket", go.transform, new Vector3(0.05f, 0.2f, 0.02f), Quaternion.Euler(0, 40, 0));
                    Trigger(go, new Vector3(0, 0.2f, 0), new Vector3(0.7f, 0.5f, 0.7f));
                    break;
                case "logbook":
                case "ledger":
                case "pact":
                {
                    float top = it.Height - 0.03f;
                    Props.Table(1.1f, 0.7f, top, "wood_dark").BakeInto(w.Kit, it.Room, floorPos, Facing(it));
                    w.Kit.Collider(floorPos + Vector3.up * top * 0.5f, new Vector3(1.1f, top, 0.7f), Facing(it));
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    if (it.Prop == "pact")
                    {
                        Props.Paper("parchment", 0.36f, 0.48f, 2f).Spawn("pact", go.transform, Vector3.zero, Quaternion.Euler(0, -8, 0));
                        var quill = new PropMesh();
                        quill["bone"].Tube(new[] { new Vector3(0.25f, 0.01f, -0.1f), new Vector3(0.35f, 0.03f, 0.15f) }, 0.004f, 4, 0.1f);
                        quill["black"].Cylinder(new Vector3(0.3f, 0, -0.18f), Quaternion.identity, 0.05f, 0.025f, 0.025f, 8);
                        quill.Spawn("quill", go.transform, Vector3.zero, Quaternion.identity);
                    }
                    else Props.Book(true, it.Prop == "ledger" ? "wood_dark" : "cloth_red").Spawn("book", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.08f, 0), new Vector3(0.8f, 0.35f, 0.6f));
                    break;
                }
                case "kegs":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    var pm = new PropMesh();
                    for (int i = 0; i < 3; i++)
                    {
                        var b = Props.Barrel(0.55f, 0.22f);
                        foreach (var kv in b.Parts) pm[kv.Key].Append(kv.Value, Matrix4x4.Translate(new Vector3(-0.25f + i * 0.25f, 0, (i % 2) * 0.2f)));
                    }
                    var top = Props.Barrel(0.55f, 0.22f);
                    foreach (var kv in top.Parts) pm[kv.Key].Append(kv.Value, Matrix4x4.Translate(new Vector3(0, 0.55f, 0.1f)));
                    pm.Spawn("kegs", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.55f, 0.1f), new Vector3(0.9f, 1.1f, 0.6f));
                    Trigger(go, new Vector3(0, 0.6f, 0.1f), new Vector3(1.0f, 1.2f, 0.8f));
                    break;
                }
                case "nameboard":
                case "board":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    if (it.Prop == "board") Props.Board().Spawn("board", go.transform, Vector3.zero, Quaternion.identity);
                    else
                    {
                        var pm = new PropMesh();
                        pm["wood_dark"].Box(new Vector3(0, it.Height * 0.5f, 0), new Vector3(0.1f, it.Height + 0.2f, 0.1f), Quaternion.identity);
                        pm["nameboard"].Box(new Vector3(0, it.Height, 0.06f), new Vector3(1.0f, 0.36f, 0.04f), Quaternion.Euler(0, 0, 3f));
                        pm.Spawn("board", go.transform, Vector3.zero, Quaternion.identity);
                    }
                    Solid(go, new Vector3(0, 0.6f, 0), new Vector3(0.3f, 1.2f, 0.3f));
                    Trigger(go, new Vector3(0, it.Height, 0.1f), new Vector3(1.2f, 0.7f, 0.4f));
                    break;
                }
                case "headphones":
                    go = Root(w, it, parent, floorPos + Vector3.up * Mathf.Max(0f, it.Height - 0.05f), out ia);
                    Props.Headphones().Spawn("headphones", go.transform, Vector3.zero, Quaternion.Euler(0, 30, 0));
                    Trigger(go, Vector3.zero, new Vector3(0.5f, 0.4f, 0.5f));
                    break;
                case "shellhorn":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.ShellHorn().Spawn("horn", go.transform, new Vector3(0, 1.0f, 0), Quaternion.identity);
                    Solid(go, new Vector3(0, 0.55f, -0.1f), new Vector3(0.25f, 1.1f, 0.25f));
                    Trigger(go, new Vector3(0, 1.15f, 0), new Vector3(0.6f, 0.6f, 0.6f));
                    break;
                case "foggate":
                case "rowboat":
                case "seastairs":
                case "greatmirror":
                {
                    go = Root(w, it, parent, point, out ia);
                    if (it.Prop == "rowboat")
                    {
                        Props.Rowboat().Spawn("boat", go.transform, new Vector3(0, -it.Height - 0.25f, 0.6f), Quaternion.Euler(0, 90, 0));
                        Trigger(go, Vector3.zero, new Vector3(2.0f, 1.0f, 3.0f));
                    }
                    else if (it.Prop == "greatmirror")
                    {
                        var fm = Props.Framed("mirror", 2.2f, 3.0f, "brass", 0.08f);
                        fm.Spawn("mirror", go.transform, new Vector3(0, -1.5f, 0), Quaternion.identity);
                        Trigger(go, new Vector3(0, 0, 0.3f), new Vector3(2.6f, 3.0f, 0.8f));
                    }
                    else Trigger(go, Vector3.zero, new Vector3(3.0f, 2.5f, 1.5f));
                    break;
                }
                case "sealedchest":
                case "chest":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.SeaChest().Spawn("chest", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.3f, 0), new Vector3(0.85f, 0.6f, 0.5f));
                    Trigger(go, new Vector3(0, 0.45f, 0), new Vector3(1.0f, 0.8f, 0.7f));
                    break;
                case "statue":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.Statue(true).Spawn("statue", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 1.4f, 0), new Vector3(1.2f, 2.8f, 1.2f));
                    Trigger(go, new Vector3(0, 1.5f, 0), new Vector3(1.6f, 3.0f, 1.6f));
                    break;
                case "dial":
                {
                    go = Root(w, it, parent, point, out ia);
                    Props.Dial(0.42f).Spawn("face", go.transform, Vector3.zero, Quaternion.identity);
                    var hour = new GameObject("hour").transform;
                    hour.SetParent(go.transform, false);
                    Props.ClockHand(0.24f, 0.035f).Spawn("h", hour, Vector3.zero, Quaternion.identity);
                    var min = new GameObject("minute").transform;
                    min.SetParent(go.transform, false);
                    Props.ClockHand(0.36f, 0.022f).Spawn("m", min, Vector3.forward * 0.004f, Quaternion.identity);
                    ia.Moving = hour;
                    ia.Hand2 = min;
                    Trigger(go, new Vector3(0, 0, 0.1f), new Vector3(1.0f, 1.0f, 0.4f));
                    break;
                }
                case "doll":
                {
                    float top = it.Height - 0.1f;
                    Support(w, it, floorPos, top);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    Props.Doll().Spawn("doll", go.transform, Vector3.zero, Quaternion.Euler(0, 160, 0));
                    Trigger(go, new Vector3(0, 0.15f, 0), new Vector3(0.4f, 0.45f, 0.4f));
                    break;
                }
                case "pedestal":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.Pedestal(1.0f).Spawn("pedestal", go.transform, Vector3.zero, Quaternion.identity);
                    var plaque = new PropMesh();
                    plaque["plaque"].QuadUV(new Vector3(-0.28f, 1.0f, -0.2f), new Vector3(0.28f, 1.0f, -0.2f), new Vector3(0.28f, 1.06f, 0.2f), new Vector3(-0.28f, 1.06f, 0.2f), Vector2.zero, Vector2.right, Vector2.one, Vector2.up);
                    plaque.Spawn("plaque", go.transform, Vector3.up * 0.005f, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 1.0f, 0.7f));
                    Trigger(go, new Vector3(0, 1.0f, 0), new Vector3(0.9f, 0.5f, 0.9f));
                    break;
                case "mirror":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.MirrorBase(Mathf.Max(1, it.Answer)).Spawn("base", go.transform, Vector3.zero, Quaternion.identity);
                    var head = new GameObject("head").transform;
                    head.SetParent(go.transform, false);
                    head.localPosition = new Vector3(0, 0.7f, 0);
                    Props.MirrorHead().Spawn("mirror", head, Vector3.zero, Quaternion.identity);
                    ia.Moving = head;
                    Solid(go, new Vector3(0, 0.9f, 0), new Vector3(0.75f, 1.8f, 0.75f));
                    Trigger(go, new Vector3(0, 1.2f, 0), new Vector3(1.1f, 1.4f, 1.1f));
                    break;
                }
                case "brazier":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.Brazier().Spawn("brazier", go.transform, Vector3.zero, Quaternion.identity);
                    var fire = new PropMesh();
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * 72f * Mathf.Deg2Rad;
                        fire["flame"].Lathe(new Vector3(Mathf.Cos(a) * 0.12f, 0.86f, Mathf.Sin(a) * 0.12f), Quaternion.identity,
                            new[] { new Vector2(0f, 0f), new Vector2(0.09f, 0.08f), new Vector2(0.05f, 0.3f), new Vector2(0f, 0.45f) }, 6);
                    }
                    fire["ember"].Disc(new Vector3(0, 0.865f, 0), Quaternion.identity, 0.33f, 14);
                    var fgo = fire.Spawn("fire", go.transform, Vector3.zero, Quaternion.identity, false);
                    ia.Moving = fgo.transform;
                    var l = LightKit.Point("fireLight", go.transform, new Vector3(0, 1.35f, 0), LightKit.Candle, 650f, 9f, true, false, 1f, 0.25f);
                    var fl = l.gameObject.AddComponent<Flicker>();
                    fl.Target = l;
                    fl.Base = 650f;
                    fl.Amount = 0.18f;
                    ia.Glow = l;
                    fgo.SetActive(false);
                    l.enabled = false;
                    Solid(go, new Vector3(0, 0.45f, 0), new Vector3(0.7f, 0.9f, 0.7f));
                    Trigger(go, new Vector3(0, 0.9f, 0), new Vector3(1.0f, 0.6f, 1.0f));
                    var zone = new LightZone { Pos = floorPos, Radius = 5.5f, Safe = true };
                    var item = it;
                    zone.Active = () => Game.Act != null && Game.Act.Session.State.Has(item.Flag);
                    w.LightZones.Add(zone);
                    break;
                }
                case "lens":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.Pedestal(1.0f).Spawn("stand", go.transform, Vector3.zero, Quaternion.identity);
                    var lens = new PropMesh();
                    lens["glass"].Cylinder(new Vector3(0, 1.25f, -0.03f), Quaternion.Euler(90, 0, 0), 0.06f, 0.18f, 0.18f, 18);
                    lens["brass"].Cylinder(new Vector3(0, 1.25f, -0.035f), Quaternion.Euler(90, 0, 0), 0.07f, 0.2f, 0.2f, 18, false);
                    lens["brass"].Box(new Vector3(0, 1.08f, 0), new Vector3(0.05f, 0.18f, 0.05f), Quaternion.identity);
                    var lgo = lens.Spawn("lens", go.transform, Vector3.zero, Quaternion.identity);
                    ia.Moving = lgo.transform;
                    Solid(go, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 1.0f, 0.7f));
                    Trigger(go, new Vector3(0, 1.2f, 0), new Vector3(0.8f, 0.6f, 0.8f));
                    break;
                case "shellwall":
                {
                    go = Root(w, it, parent, floorPos, out ia);
                    var pm = new PropMesh();
                    var rng = new Rng(Rng.Hash(it.Id));
                    for (int i = 0; i < 60; i++)
                    {
                        var p = new Vector3(rng.Range(-1.4f, 1.4f), rng.Range(0.3f, 2.4f), rng.Range(-0.05f, 0.05f));
                        float s = rng.Range(0.05f, 0.12f);
                        pm["bone"].Lathe(p, Quaternion.Euler(rng.Range(60, 120), rng.Range(0, 360), 0), new[] { new Vector2(0.001f, 0f), new Vector2(s, s * 0.6f), new Vector2(s * 0.3f, s * 1.6f), new Vector2(0.001f, s * 1.8f) }, 7);
                    }
                    pm.Spawn("shells", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 1.2f, 0.1f), new Vector3(3.0f, 2.4f, 0.6f));
                    break;
                }
                case "taperecorder":
                {
                    float top = it.Height - 0.05f;
                    Support(w, it, floorPos, top);
                    go = Root(w, it, parent, floorPos + Vector3.up * top, out ia);
                    Props.TapeRecorder().Spawn("tape", go.transform, Vector3.zero, Quaternion.identity);
                    Trigger(go, new Vector3(0, 0.1f, 0), new Vector3(0.5f, 0.35f, 0.45f));
                    break;
                }
                case "altar":
                    go = Root(w, it, parent, floorPos, out ia);
                    Props.Altar().Spawn("altar", go.transform, Vector3.zero, Quaternion.identity);
                    Solid(go, new Vector3(0, 0.6f, 0), new Vector3(2.5f, 1.2f, 1.5f));
                    Trigger(go, new Vector3(0, 1.0f, 0), new Vector3(2.8f, 0.8f, 1.8f));
                    break;
                default:
                    go = Root(w, it, parent, point, out ia);
                    Trigger(go, Vector3.zero, new Vector3(0.6f, 0.6f, 0.6f));
                    break;
            }
            ia.Visuals = go.GetComponentsInChildren<Renderer>();
            return go;
        }
    }
}
