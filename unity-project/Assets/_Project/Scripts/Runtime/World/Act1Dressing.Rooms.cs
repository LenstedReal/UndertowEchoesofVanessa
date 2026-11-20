using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    public sealed partial class Act1Dressing
    {
        const float G = -4f; // gun deck floor

        static Quaternion Yaw(float deg) => Quaternion.Euler(0f, deg, 0f);

        // ------------------------------------------------------------------ gun deck corridor
        void Spine(World w)
        {
            var room = w.Act.Room("a1_spine");
            DressUtil.CeilingBeams(w, room, 1.25f);
            // knees: brackets where the beams meet the corridor walls
            for (float z = 22.6f; z < 66f; z += 2.5f)
            {
                if (Mathf.Abs(z - 32f) < 1f || Mathf.Abs(z - 46f) < 1f || Mathf.Abs(z - 59f) < 1f || Mathf.Abs(z - 30f) < 1f || Mathf.Abs(z - 43.5f) < 1f) continue;
                w.Kit.Box("a1_spine", "wood_dark", new Vector3(6.1f, -1.75f, z), new Vector3(0.2f, 0.5f, 0.14f), Quaternion.Euler(0, 0, -35));
                w.Kit.Box("a1_spine", "wood_dark", new Vector3(9.9f, -1.75f, z), new Vector3(0.2f, 0.5f, 0.14f), Quaternion.Euler(0, 0, 35));
            }
            foreach (float z in new[] { 26f, 38.5f, 52.5f, 62.5f })
                DressUtil.Lantern(w, "spine_lantern_" + z, new Vector3(CX, -1.52f, z), false, 0, 0, false, false, 1.8f);
            var rng = _rng;
            DressUtil.Bake(w, "a1_spine", Props.Barrel(0.9f, 0.3f), new Vector3(9.6f, G, 24.4f), Yaw(30), new Vector3(0.62f, 0.9f, 0.62f));
            DressUtil.Bake(w, "a1_spine", Props.Barrel(0.9f, 0.3f), new Vector3(9.62f, G, 25.1f), Yaw(80), new Vector3(0.62f, 0.9f, 0.62f));
            DressUtil.Bake(w, "a1_spine", Props.Crate(new Vector3(0.7f, 0.6f, 0.7f)), new Vector3(6.45f, G, 38.2f), Yaw(4), new Vector3(0.7f, 0.6f, 0.7f));
            DressUtil.Bake(w, "a1_spine", Props.RopeCoil(0.3f), new Vector3(6.5f, G, 52.8f), Yaw(0));
            DressUtil.Bake(w, "a1_spine", Props.Barrel(0.9f, 0.3f), new Vector3(9.6f, G, 55.4f), Yaw(12), new Vector3(0.62f, 0.9f, 0.62f));
            DressUtil.Bake(w, "a1_spine", Props.Planks(rng, 4, 1.6f), new Vector3(9.3f, G, 64.6f), Yaw(10));
            DressUtil.Bake(w, "a1_spine", Props.Rubble(rng, 7, 0.45f, "wood_dark"), new Vector3(7.2f, G, 65.3f), Yaw(0));
            // one barrel that is not always where you left it
            DressUtil.Shifty(w, "spine_barrel", Props.Barrel(0.9f, 0.3f), new Vector3(6.42f, G, 49.4f), Yaw(0), new Vector3(0.6f, 0.9f, 0.6f), rng);
            DressUtil.Puddles(w, room, 9, rng);
            // the bilge pump at the far end of the corridor
            var pump = new PropMesh();
            pump["wood_dark"].Cylinder(Vector3.zero, Quaternion.identity, 1.1f, 0.16f, 0.16f, 10);
            pump["iron"].Box(new Vector3(0, 1.15f, 0), new Vector3(0.9f, 0.06f, 0.06f), Quaternion.Euler(0, 0, 12));
            pump["darkmetal"].Cylinder(new Vector3(0.16f, 0.8f, 0), Quaternion.Euler(0, 0, -90), 0.25f, 0.04f, 0.035f, 6);
            DressUtil.Bake(w, "a1_spine", pump, new Vector3(6.55f, G, 61.5f), Yaw(0), new Vector3(0.36f, 1.1f, 0.36f));
        }

        // ------------------------------------------------------------------ crew quarters
        void Crew(World w)
        {
            var room = w.Act.Room("a1_crew");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.3f);
            foreach (float z in new[] { 27.3f, 30.2f, 33.6f, 36.6f })
            {
                Props.Bunk(1.9f, 0.8f, true).BakeInto(w.Kit, "a1_crew", new Vector3(1.1f, G, z), Quaternion.identity);
                w.Kit.Collider(new Vector3(1.1f, G + 0.235f, z), new Vector3(0.8f, 0.47f, 1.9f));
                // blanket thrown back on the lower bunk, a cap on the upper
                w.Kit.Box("a1_crew", "cloth_blue", new Vector3(1.12f, G + 0.48f, z + 0.45f), new Vector3(0.7f, 0.035f, 0.7f), Quaternion.Euler(0, rng.Range(-6f, 6f), 2));
            }
            foreach (float z in new[] { 27.3f, 36.6f })
                DressUtil.Bake(w, "a1_crew", Props.SeaChest(), new Vector3(2.0f, G, z), Yaw(90), new Vector3(0.82f, 0.6f, 0.5f));
            Props.Hammock(2.2f).BakeInto(w.Kit, "a1_crew", new Vector3(2.75f, G - 0.05f, 34.4f), Quaternion.identity);
            w.Kit.Collider(new Vector3(2.75f, G + 1.38f, 34.4f), new Vector3(0.6f, 0.7f, 1.9f));
            // mess table with the evening left on it
            Props.Table(1.3f, 0.8f, 0.76f, "wood").BakeInto(w.Kit, "a1_crew", new Vector3(3.9f, G, 31.0f), Yaw(90));
            w.Kit.Collider(new Vector3(3.9f, G + 0.38f, 31.0f), new Vector3(0.8f, 0.76f, 1.3f));
            float top = G + 0.76f;
            var mug = new PropMesh();
            mug["wood_dark"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.045f, 0f), new Vector2(0.05f, 0.1f), new Vector2(0.045f, 0.11f), new Vector2(0.0f, 0.105f) }, 8);
            mug.BakeInto(w.Kit, "a1_crew", new Vector3(3.75f, top, 30.6f), Yaw(0));
            mug.BakeInto(w.Kit, "a1_crew", new Vector3(4.1f, top, 31.45f), Yaw(0));
            Props.Bottle(rng).BakeInto(w.Kit, "a1_crew", new Vector3(4.05f, top, 30.9f), Yaw(0));
            var fallen = Props.Bottle(rng);
            fallen.BakeInto(w.Kit, "a1_crew", new Vector3(3.6f, top + 0.04f, 31.3f), Quaternion.Euler(0, 40, 90));
            for (int i = 0; i < 6; i++)
                w.Kit.Box("a1_crew", "paper", new Vector3(3.8f + rng.Range(-0.2f, 0.2f), top + 0.002f + i * 0.001f, 31.15f + rng.Range(-0.25f, 0.25f)), new Vector3(0.06f, 0.002f, 0.09f), Yaw(rng.Range(0, 180)));
            Props.Candles(rng, 1).BakeInto(w.Kit, "a1_crew", new Vector3(3.95f, top, 31.2f), Yaw(0));
            foreach (var p in new[] { new Vector3(3.15f, G, 30.6f), new Vector3(3.15f, G, 31.5f), new Vector3(4.7f, G, 30.4f) })
                DressUtil.Bake(w, "a1_crew", Props.Stool(), p, Yaw(rng.Range(0, 90)));
            DressUtil.Shifty(w, "crew_stool", Props.Stool(), new Vector3(4.75f, G, 31.7f), Yaw(20), new Vector3(0.35f, 0.5f, 0.35f), rng);
            // coats on pegs by the door
            for (int i = 0; i < 4; i++)
            {
                float x = 3.0f + i * 0.55f;
                w.Kit.Box("a1_crew", "iron", new Vector3(x, G + 1.7f, 26.06f), new Vector3(0.03f, 0.03f, 0.12f));
                if (i == 1 || i == 3) w.Kit.Box("a1_crew", i == 1 ? "cloth" : "cloth_blue", new Vector3(x, G + 1.25f, 26.12f), new Vector3(0.42f, 0.85f, 0.08f), Quaternion.Euler(4, 0, rng.Range(-3f, 3f)));
            }
            DressUtil.Lantern(w, "crew_lantern", new Vector3(3.6f, -1.72f, 32.4f), false, 0, 0, false, false, 2f);
            DressUtil.Clutter(w, room, 0.6f, rng, new[] { "bottle", "coil", "stool" });
        }

        // ------------------------------------------------------------------ the music room (safe)
        void Music(World w)
        {
            var room = w.Act.Room("a1_music");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.4f);
            DressUtil.Rug(w, room, new Vector3(3.3f, 0, 45.6f), 3.4f, 4.6f, "cloth_red", 0f);
            // the bell beam: five ship's bells, hung by size, each marked on the wall with its number of dots
            w.Kit.Box("a1_music", "wood_dark", new Vector3(3.25f, -1.85f, 50.6f), new Vector3(5.5f, 0.16f, 0.18f));
            float[] bx = { 1.4f, 2.35f, 3.3f, 4.25f, 5.2f };
            for (int b = 0; b < 5; b++)
            {
                float r = 0.2f - b * 0.016f;
                float pivot = G + 1.75f + r * 0.71f + 0.05f;
                w.Kit.B("a1_music", "iron").Cylinder(new Vector3(bx[b], pivot, 50.6f), Quaternion.identity, -1.93f - pivot + 0.01f, 0.012f, 0.012f, 6, false);
                DressUtil.Dots(w, "a1_music", new Vector3(bx[b], G + 2.1f, 51.995f), Vector3.back, b + 1);
            }
            // spinet against the port wall, its lid open
            var sp = new PropMesh();
            sp["wood_dark"].Box(new Vector3(0, 0.62f, 0), new Vector3(0.62f, 0.26f, 1.5f), Quaternion.identity);
            foreach (var s in new[] { new Vector2(-0.25f, -0.65f), new Vector2(-0.25f, 0.65f), new Vector2(0.25f, -0.65f), new Vector2(0.25f, 0.65f) })
                sp["wood_dark"].Lathe(new Vector3(s.x, 0, s.y), Quaternion.identity, new[] { new Vector2(0.035f, 0f), new Vector2(0.03f, 0.3f), new Vector2(0.045f, 0.4f), new Vector2(0.03f, 0.5f) }, 8);
            sp["porcelain"].Box(new Vector3(0.33f, 0.73f, 0), new Vector3(0.16f, 0.03f, 1.3f), Quaternion.identity);
            for (int k = 0; k < 18; k++)
                if (k % 7 != 2 && k % 7 != 6) sp["black"].Box(new Vector3(0.3f, 0.755f, -0.6f + k * 0.072f), new Vector3(0.09f, 0.025f, 0.035f), Quaternion.identity);
            sp["wood_dark"].Box(new Vector3(-0.18f, 1.02f, 0), new Vector3(0.04f, 0.55f, 1.46f), Quaternion.Euler(0, 0, -28));
            sp["paper"].Box(new Vector3(0.1f, 0.9f, 0.1f), new Vector3(0.01f, 0.24f, 0.36f), Quaternion.Euler(0, 0, -15));
            DressUtil.Bake(w, "a1_music", sp, new Vector3(1.0f, G, 42.0f), Yaw(0), new Vector3(0.62f, 0.85f, 1.5f));
            DressUtil.Bake(w, "a1_music", Props.Stool(), new Vector3(1.75f, G, 42.0f), Yaw(10));
            // a chair turned towards the music box, as if someone had been listening
            DressUtil.Bake(w, "a1_music", Props.Chair("wood_dark"), new Vector3(3.4f, G, 42.55f), Yaw(0), new Vector3(0.45f, 0.9f, 0.45f));
            // music shelf with sheets and a violin case
            var sh = Props.Shelf(1.1f, 1.6f, 0.3f, rng, "wood_dark");
            DressUtil.Bake(w, "a1_music", sh, new Vector3(5.83f, G, 41.4f), Yaw(-90), new Vector3(0.3f, 1.6f, 1.1f));
            var vc = new PropMesh();
            vc["cloth"].Box(new Vector3(0, 0.06f, 0), new Vector3(0.24f, 0.12f, 0.75f), Quaternion.identity);
            vc["brass"].Box(new Vector3(0.12f, 0.06f, 0.1f), new Vector3(0.01f, 0.03f, 0.05f), Quaternion.identity);
            vc.BakeInto(w.Kit, "a1_music", new Vector3(5.25f, G, 50.9f), Yaw(80));
            for (int i = 0; i < 5; i++)
                w.Kit.Box("a1_music", "paper", new Vector3(rng.Range(1.2f, 5.3f), G + 0.012f, rng.Range(44.5f, 49.8f)), new Vector3(0.21f, 0.002f, 0.28f), Yaw(rng.Range(0, 180)));
            // the safe lamp: warm, steady, it is why the room feels calm
            var lamp = DressUtil.Lantern(w, "music_lamp", new Vector3(3.2f, -1.72f, 46.5f), true, 420f, 7f, true, true, 1.2f);
            MusicLamp = lamp;
            w.LightZones.Add(new LightZone { Pos = new Vector3(3.2f, G + 1.2f, 46.5f), Radius = 4.6f, Safe = true });
        }

        Light MusicLamp;

        // ------------------------------------------------------------------ the surgeon's cabin
        void Surgery(World w)
        {
            var room = w.Act.Room("a1_surg");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.3f);
            // instrument rail hanging from the beams; the boat hook hangs among the saws
            var rail = new PropMesh();
            rail["iron"].Cylinder(new Vector3(0, 0, -0.95f), Quaternion.Euler(90, 0, 0), 1.9f, 0.015f, 0.015f, 6);
            rail["iron"].Cylinder(new Vector3(0, 0, -0.85f), Quaternion.identity, 0.32f, 0.008f, 0.008f, 4, false);
            rail["iron"].Cylinder(new Vector3(0, 0, 0.85f), Quaternion.identity, 0.32f, 0.008f, 0.008f, 4, false);
            rail.BakeInto(w.Kit, "a1_surg", new Vector3(1.1f, G + 2.08f, 62.4f), Quaternion.identity);
            foreach (float z in new[] { 61.75f, 62.0f, 62.85f, 63.15f })
            {
                var tool = new PropMesh();
                tool["iron"].Cylinder(new Vector3(0, -0.1f, 0), Quaternion.identity, 0.1f, 0.006f, 0.006f, 4, false);
                if (z < 62f) tool["darkmetal"].Box(new Vector3(0, -0.26f, 0.05f), new Vector3(0.01f, 0.22f, 0.12f), Quaternion.identity);
                else tool["iron"].Box(new Vector3(0, -0.22f, 0), new Vector3(0.012f, 0.24f, 0.025f), Quaternion.Euler(10, 0, 0));
                tool["wood_dark"].Box(new Vector3(0, -0.1f, 0), new Vector3(0.025f, 0.08f, 0.04f), Quaternion.identity);
                tool.BakeInto(w.Kit, "a1_surg", new Vector3(1.1f, G + 2.08f, z), Quaternion.identity);
            }
            var cab = Props.Shelf(1.2f, 1.8f, 0.32f, rng, "wood_dark");
            DressUtil.Bake(w, "a1_surg", cab, new Vector3(3.6f, G, 63.82f), Yaw(180), new Vector3(1.2f, 1.8f, 0.32f));
            for (int i = 0; i < 7; i++)
                Props.Bottle(rng).BakeInto(w.Kit, "a1_surg", new Vector3(3.1f + i * 0.16f, G + 1.22f, 63.78f), Yaw(0));
            var bucket = new PropMesh();
            bucket["wood_dark"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.16f, 0f), new Vector2(0.19f, 0.32f), new Vector2(0.17f, 0.32f), new Vector2(0.14f, 0.02f), new Vector2(0f, 0.02f) }, 12);
            bucket["iron"].Cylinder(new Vector3(0, 0.08f, 0), Quaternion.identity, 0.03f, 0.165f, 0.165f, 12, false);
            bucket["cloth_red"].Disc(new Vector3(0, 0.22f, 0), Quaternion.identity, 0.16f, 10);
            DressUtil.Bake(w, "a1_surg", bucket, new Vector3(4.55f, G, 60.1f), Yaw(0), new Vector3(0.38f, 0.32f, 0.38f));
            for (int i = 0; i < 3; i++)
                w.Kit.Box("a1_surg", "cloth_red", new Vector3(rng.Range(2.2f, 4.2f), G + 0.006f, rng.Range(57.2f, 60.2f)), new Vector3(0.3f, 0.01f, 0.22f), Yaw(rng.Range(0, 180)));
            DressUtil.Shifty(w, "surg_chair", Props.Chair("wood_dark"), new Vector3(1.4f, G, 56.0f), Yaw(60), new Vector3(0.45f, 0.9f, 0.45f), rng);
            DressUtil.Lantern(w, "surg_lantern", new Vector3(3.0f, -1.72f, 58.6f), false, 0, 0, false, false, 1.5f);
            Props.SeaChest().BakeInto(w.Kit, "a1_surg", new Vector3(1.2f, G, 55.0f), Yaw(90));
            w.Kit.Collider(new Vector3(1.2f, G + 0.35f, 55.0f), new Vector3(0.5f, 0.7f, 0.82f));
        }

        // ------------------------------------------------------------------ the store room
        void Store(World w)
        {
            var room = w.Act.Room("a1_store");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.3f);
            DressUtil.Bake(w, "a1_store", Props.Shelf(1.6f, 1.9f, 0.34f, rng, "wood"), new Vector3(11.2f, G, 24.18f), Yaw(0), new Vector3(1.6f, 1.9f, 0.34f));
            DressUtil.Bake(w, "a1_store", Props.Shelf(1.6f, 1.9f, 0.34f, rng, "wood"), new Vector3(14.3f, G, 24.18f), Yaw(0), new Vector3(1.6f, 1.9f, 0.34f));
            foreach (float z in new[] { 28.2f, 28.9f, 29.6f, 31.6f })
                DressUtil.Bake(w, "a1_store", Props.Barrel(0.92f, 0.31f), new Vector3(15.12f, G, z), Yaw(rng.Range(0, 360)), new Vector3(0.62f, 0.92f, 0.62f));
            Props.Barrel(0.92f, 0.31f).BakeInto(w.Kit, "a1_store", new Vector3(15.1f, G + 0.92f, 28.55f), Yaw(40));
            for (int i = 0; i < 5; i++)
                DressUtil.Sack(w, "a1_store", new Vector3(14.6f - (i % 3) * 0.55f, G + (i >= 3 ? 0.55f : 0f), 35.35f - (i >= 3 ? 0.1f : 0f)), Quaternion.Euler(rng.Range(-8f, 8f), rng.Range(0, 180), rng.Range(-8f, 8f)), rng);
            w.Kit.Collider(new Vector3(14.05f, G + 0.45f, 35.3f), new Vector3(1.7f, 0.9f, 0.7f));
            DressUtil.Bake(w, "a1_store", Props.Crate(new Vector3(0.9f, 0.8f, 0.9f)), new Vector3(10.9f, G, 35.1f), Yaw(5), new Vector3(0.9f, 1.5f, 0.9f));
            Props.Crate(new Vector3(0.7f, 0.65f, 0.7f)).BakeInto(w.Kit, "a1_store", new Vector3(10.95f, G + 0.8f, 35.1f), Yaw(22));
            // a net full of floats hung from the beams
            var rope = w.Kit.B("a1_store", "rope", 0.3f);
            for (int i = 0; i < 6; i++)
                rope.Rope(new Vector3(12.2f + i * 0.3f, -1.62f, 32.6f), new Vector3(12.3f + i * 0.28f, -2.4f, 32.9f), 0.15f, 0.008f, 5);
            for (int i = 0; i < 4; i++)
                w.Kit.B("a1_store", "glass").Lathe(new Vector3(12.4f + i * 0.4f, -2.45f, 32.9f), Quaternion.identity, new[] { new Vector2(0f, 0f), new Vector2(0.08f, 0.04f), new Vector2(0.1f, 0.12f), new Vector2(0.07f, 0.2f), new Vector2(0f, 0.22f) }, 8);
            DressUtil.Puddles(w, room, 3, rng);
        }

        // ------------------------------------------------------------------ Vanessa's cabin
        void Nessa(World w)
        {
            var room = w.Act.Room("a1_nessa");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.3f);
            DressUtil.Rug(w, room, new Vector3(12.8f, 0, 43.8f), 2.6f, 3.4f, "cloth_blue", 6f);
            // her bed against the starboard side; the letter waits on the quilt
            var bed = Props.Bunk(1.7f, 0.8f, false);
            bed["cloth_blue"].Box(new Vector3(0, 0.475f, 0.35f), new Vector3(0.74f, 0.03f, 0.9f), Quaternion.Euler(1.5f, 0, 0));
            bed.BakeInto(w.Kit, "a1_nessa", new Vector3(14.85f, G, 46.4f), Quaternion.identity);
            w.Kit.Collider(new Vector3(14.85f, G + 0.235f, 46.4f), new Vector3(0.8f, 0.47f, 1.7f));
            DressUtil.Shifty(w, "nessa_doll", Props.Doll(), new Vector3(14.95f, G + 0.5f, 45.82f), Yaw(-80), null, rng);
            // nightstand with a single candle somebody lit for her
            var ns = new PropMesh();
            ns["wood"].Box(new Vector3(0, 0.25f, 0), new Vector3(0.42f, 0.5f, 0.4f), Quaternion.identity);
            ns["brass"].Box(new Vector3(-0.21f, 0.33f, 0), new Vector3(0.01f, 0.03f, 0.06f), Quaternion.identity);
            DressUtil.Bake(w, "a1_nessa", ns, new Vector3(15.0f, G, 44.85f), Yaw(0), new Vector3(0.42f, 0.5f, 0.4f));
            var cnd = new PropMesh();
            cnd["candle"].Cylinder(Vector3.zero, Quaternion.identity, 0.09f, 0.02f, 0.019f, 8);
            cnd["brass"].Lathe(new Vector3(0, -0.01f, 0), Quaternion.identity, new[] { new Vector2(0.06f, 0f), new Vector2(0.06f, 0.012f), new Vector2(0.025f, 0.02f), new Vector2(0.022f, 0.03f) }, 10);
            cnd.BakeInto(w.Kit, "a1_nessa", new Vector3(15.0f, G + 0.51f, 44.85f), Quaternion.identity);
            var flame = new PropMesh();
            flame["flame"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0f, 0f), new Vector2(0.01f, 0.012f), new Vector2(0.006f, 0.035f), new Vector2(0f, 0.048f) }, 6);
            var fgo = flame.Spawn("nessa_candle_flame", w.Root, new Vector3(15.0f, G + 0.605f, 44.85f), Quaternion.identity, false);
            var cl = LightKit.Point("nessa_candle", fgo.transform, new Vector3(0, 0.03f, 0), LightKit.Candle, 55f, 3.2f, false, false, 0.6f, 0.02f);
            var fl = fgo.AddComponent<Flicker>();
            fl.Target = cl; fl.Base = 55f; fl.Amount = 0.18f; fl.Speed = 11f;
            fl.Flame = fgo.GetComponentInChildren<Renderer>();
            w.FlickerLights.Add(cl);
            w.LightZones.Add(new LightZone { Pos = new Vector3(15.0f, G + 0.7f, 44.85f), Radius = 1.6f, Safe = false, Active = () => fgo != null && fgo.activeSelf });
            w.On("a1_letter", rt =>
            {
                // reading the letter: the candle gutters out behind you
                if (fgo != null) fgo.SetActive(false);
                Game.Audio.PlayAt("sfx_candle_out", new Vector3(15.0f, G + 0.6f, 44.85f), Bus.Effects, 0.8f);
            });
            // little drawing table, crayons worn to stubs
            Props.Table(0.8f, 0.55f, 0.55f, "wood").BakeInto(w.Kit, "a1_nessa", new Vector3(12.1f, G, 39.35f), Yaw(0));
            w.Kit.Collider(new Vector3(12.1f, G + 0.275f, 39.35f), new Vector3(0.8f, 0.55f, 0.55f));
            string[] cols = { "cloth_red", "cloth_blue", "brass", "black", "algae" };
            for (int i = 0; i < 5; i++)
                w.Kit.B("a1_nessa", cols[i]).Cylinder(new Vector3(11.9f + i * 0.06f, G + 0.556f, 39.3f + rng.Range(-0.05f, 0.05f)), Quaternion.Euler(0, rng.Range(0, 180), 90), 0.07f, 0.006f, 0.006f, 6);
            w.Kit.Box("a1_nessa", "paper", new Vector3(12.25f, G + 0.553f, 39.4f), new Vector3(0.28f, 0.002f, 0.21f), Yaw(-8));
            DressUtil.Bake(w, "a1_nessa", Props.Stool(), new Vector3(12.1f, G, 39.95f), Yaw(0));
            // toy chest, lid open
            DressUtil.Bake(w, "a1_nessa", Props.Chest(true), new Vector3(14.95f, G, 39.6f), Yaw(-90), new Vector3(0.5f, 0.5f, 0.8f));
            // rocking horse
            var horse = new PropMesh();
            horse["wood"].Box(new Vector3(0, 0.42f, 0), new Vector3(0.16f, 0.18f, 0.55f), Quaternion.identity);
            horse["wood"].Box(new Vector3(0, 0.58f, 0.3f), new Vector3(0.12f, 0.26f, 0.12f), Quaternion.Euler(-25, 0, 0));
            horse["wood"].Box(new Vector3(0, 0.68f, 0.38f), new Vector3(0.11f, 0.1f, 0.2f), Quaternion.identity);
            horse["cloth_red"].Box(new Vector3(0, 0.52f, -0.02f), new Vector3(0.18f, 0.03f, 0.2f), Quaternion.identity);
            foreach (float x in new[] { -0.08f, 0.08f })
            {
                horse["wood"].Box(new Vector3(x, 0.22f, 0.2f), new Vector3(0.04f, 0.3f, 0.04f), Quaternion.Euler(-10, 0, 0));
                horse["wood"].Box(new Vector3(x, 0.22f, -0.2f), new Vector3(0.04f, 0.3f, 0.04f), Quaternion.Euler(10, 0, 0));
                var rk = new List<Vector3>();
                for (int k = 0; k <= 8; k++) { float t = k / 8f * 2f - 1f; rk.Add(new Vector3(x, 0.04f + t * t * 0.08f, t * 0.42f)); }
                horse["wood_dark"].Tube(rk, 0.018f, 5, 0.2f);
            }
            DressUtil.Bake(w, "a1_nessa", horse, new Vector3(12.3f, G, 47.7f), Yaw(30), new Vector3(0.3f, 0.7f, 0.85f));
            // a mobile of shells turning slowly under the beams
            var mob = new GameObject("nessa_mobile").transform;
            mob.SetParent(w.Root, false);
            mob.position = new Vector3(13.2f, -1.62f, 44.3f);
            var mm = new PropMesh();
            mm["wood"].Box(new Vector3(0, -0.12f, 0), new Vector3(0.5f, 0.012f, 0.012f), Quaternion.identity);
            mm["wood"].Box(new Vector3(0, -0.12f, 0), new Vector3(0.012f, 0.012f, 0.5f), Quaternion.identity);
            foreach (var o in new[] { new Vector3(0.24f, 0, 0), new Vector3(-0.24f, 0, 0), new Vector3(0, 0, 0.24f), new Vector3(0, 0, -0.24f) })
            {
                mm["rope"].Tube(new[] { o + new Vector3(0, -0.12f, 0), o + new Vector3(0, -0.34f, 0) }, 0.003f, 3, 0.1f);
                mm["porcelain"].Lathe(o + new Vector3(0, -0.4f, 0), Quaternion.identity, new[] { new Vector2(0f, 0f), new Vector2(0.03f, 0.02f), new Vector2(0.035f, 0.04f), new Vector2(0f, 0.06f) }, 6);
            }
            mm["rope"].Tube(new[] { Vector3.zero, new Vector3(0, -0.12f, 0) }, 0.003f, 3, 0.1f);
            mm.Spawn("shells", mob, Vector3.zero, Quaternion.identity);
            var sw = mob.gameObject.AddComponent<Sway>();
            sw.Amp = 6f; sw.Speed = 0.3f;
            // more of her drawings pinned up, blank where the sea took the colour
            foreach (var (pos, yaw) in new[] { (new Vector3(10.03f, G + 1.45f, 40.4f), 90f), (new Vector3(10.03f, G + 1.3f, 47.0f), 90f), (new Vector3(13.4f, G + 1.5f, 48.97f), 180f) })
                Props.Pinned("paper", 0.28f, 0.36f).BakeInto(w.Kit, "a1_nessa", pos, Yaw(yaw));
        }

        // ------------------------------------------------------------------ cargo hold
        void Cargo(World w)
        {
            var room = w.Act.Room("a1_cargo");
            var rng = _rng;
            // ribs along both sides and the forward bulkhead
            for (float z = 68.9f; z < 84f; z += 1.5f)
            {
                w.Kit.Box("a1_cargo", "wood_dark", new Vector3(1.61f, G + 1.3f, z), new Vector3(0.22f, 2.6f, 0.26f));
                w.Kit.Box("a1_cargo", "wood_dark", new Vector3(14.39f, G + 1.3f, z), new Vector3(0.22f, 2.6f, 0.26f));
            }
            w.Kit.Box("a1_cargo", "wood_dark", new Vector3(1.62f, G + 2.55f, 76f), new Vector3(0.26f, 0.12f, 16f));
            w.Kit.Box("a1_cargo", "wood_dark", new Vector3(14.38f, G + 2.55f, 76f), new Vector3(0.26f, 0.12f, 16f));
            // goal marks: iron ring-bolts and chalk circles where the crates belong
            foreach (var p in new[] { new Vector3(12.5f, G, 77f), new Vector3(12.5f, G, 74f) })
            {
                var ring = new List<Vector3>();
                for (int i = 0; i <= 16; i++) { float a = i / 16f * Mathf.PI * 2f; ring.Add(p + new Vector3(Mathf.Cos(a) * 0.62f, 0.006f, Mathf.Sin(a) * 0.62f)); }
                w.Kit.B("a1_cargo", "chalk").Tube(ring, 0.012f, 3, 0.2f);
                w.Kit.B("a1_cargo", "iron").Cylinder(p + new Vector3(0.75f, 0.02f, 0), Quaternion.Euler(90, 0, 0), 0.02f, 0.06f, 0.06f, 10);
                w.Kit.B("a1_cargo", "iron").Cylinder(p + new Vector3(-0.75f, 0.02f, 0), Quaternion.Euler(90, 0, 0), 0.02f, 0.06f, 0.06f, 10);
            }
            // cargo along the walls, away from the crates' paths
            foreach (var (p, s) in new[] { (new Vector3(2.4f, G, 80.6f), 1.0f), (new Vector3(3.5f, G, 82.9f), 0.95f), (new Vector3(2.35f, G, 82.9f), 0.9f), (new Vector3(13.5f, G, 82.8f), 1.0f), (new Vector3(13.6f, G, 81.6f), 0.85f) })
            {
                DressUtil.Bake(w, "a1_cargo", Props.Crate(new Vector3(s, s * 0.9f, s)), p, Yaw(rng.Range(-8f, 8f)), new Vector3(s, s * 0.9f, s));
                if (rng.Chance(0.6f)) Props.Crate(new Vector3(s * 0.8f, s * 0.75f, s * 0.8f)).BakeInto(w.Kit, "a1_cargo", p + Vector3.up * s * 0.9f, Yaw(rng.Range(-25f, 25f)));
            }
            foreach (float z in new[] { 73.6f, 74.4f, 78.6f, 79.4f })
                DressUtil.Bake(w, "a1_cargo", Props.Barrel(0.92f, 0.31f), new Vector3(2.08f, G, z), Yaw(rng.Range(0, 360)), new Vector3(0.62f, 0.92f, 0.62f));
            Props.Barrel(0.92f, 0.31f).BakeInto(w.Kit, "a1_cargo", new Vector3(2.2f, G + 0.31f, 76.4f), Quaternion.Euler(0, 12, 90));
            // a gun that fell through when the deck went
            Props.Cannon().BakeInto(w.Kit, "a1_cargo", new Vector3(6.0f, G + 0.15f, 81.8f), Quaternion.Euler(-8, 140, 14));
            w.Kit.Collider(new Vector3(6.0f, G + 0.4f, 81.8f), new Vector3(1.0f, 0.8f, 1.8f), Yaw(140));
            // deck wreckage leaning against the starboard side
            for (int i = 0; i < 5; i++)
            {
                float z = 78.6f + i * 0.5f;
                w.Kit.Box("a1_cargo", "deck", new Vector3(14.0f, G + 1.0f, z), new Vector3(0.06f, 2.2f, 0.22f), Quaternion.Euler(0, 0, 22 + rng.Range(-6f, 6f)));
            }
            w.Kit.Collider(new Vector3(13.95f, G + 0.9f, 79.6f), new Vector3(0.8f, 1.8f, 2.6f));
            DressUtil.Bake(w, "a1_cargo", Props.Planks(rng, 6, 2.2f), new Vector3(4.2f, G, 69.0f), Yaw(80));
            DressUtil.Bake(w, "a1_cargo", Props.Rubble(rng, 10, 0.6f, "wood_dark"), new Vector3(7.4f, G, 79.6f), Yaw(0));
            DressUtil.Bake(w, "a1_cargo", Props.Rubble(rng, 8, 0.5f, "wood_dark"), new Vector3(10.6f, G, 82.6f), Yaw(0));
            for (int i = 0; i < 4; i++)
                DressUtil.Sack(w, "a1_cargo", new Vector3(3.1f + i * 0.5f, G, 69.0f + (i % 2) * 0.2f), Yaw(rng.Range(0, 180)), rng);
            DressUtil.Puddles(w, room, 10, rng);
            // the lever's mechanism: chain running up the bulkhead to the bow grate
            var ch = w.Kit.B("a1_cargo", "iron", 0.3f);
            ch.Rope(new Vector3(9.6f, G + 1.6f, 68.12f), new Vector3(9.6f, G + 2.5f, 68.12f), 0.0f, 0.015f, 2);
            ch.Rope(new Vector3(9.6f, G + 2.5f, 68.2f), new Vector3(9.0f, G + 2.5f, 84.0f), 0.35f, 0.015f, 14);
        }

        // ------------------------------------------------------------------ bow
        void BowDeck(World w)
        {
            var rng = _rng;
            // anchor lashed along the port bulwark
            var an = new PropMesh();
            an["darkmetal"].Cylinder(Vector3.zero, Quaternion.Euler(90, 0, 0), 3.0f, 0.07f, 0.06f, 8);
            var arm = new List<Vector3>();
            for (int i = 0; i <= 10; i++) { float t = i / 10f * 2f - 1f; arm.Add(new Vector3(t * 0.85f, 0, 0.1f + (1f - t * t) * -0.35f + 0.35f)); }
            an["darkmetal"].Tube(arm, 0.06f, 6, 0.3f);
            an["darkmetal"].Box(new Vector3(-0.9f, 0, 0.42f), new Vector3(0.32f, 0.05f, 0.4f), Yaw(-35));
            an["darkmetal"].Box(new Vector3(0.9f, 0, 0.42f), new Vector3(0.32f, 0.05f, 0.4f), Yaw(35));
            an["wood_dark"].Box(new Vector3(0, 0, 2.7f), new Vector3(1.6f, 0.14f, 0.14f), Quaternion.identity);
            an["iron"].Cylinder(new Vector3(0, 0, 3.05f), Quaternion.Euler(0, 0, 90), 0.04f, 0.13f, 0.13f, 10, false);
            an.BakeInto(w.Kit, "a1_bowdeck", new Vector3(2.5f, 0.08f, 100.4f), Yaw(0) * Quaternion.Euler(0, 0, 90));
            w.Kit.Collider(new Vector3(2.5f, 0.45f, 101.9f), new Vector3(0.6f, 0.9f, 3.3f));
            DressUtil.Bake(w, "a1_bowdeck", Props.Barrel(0.92f, 0.31f), new Vector3(13.3f, 0f, 93.0f), Yaw(10), new Vector3(0.62f, 0.92f, 0.62f));
            DressUtil.Bake(w, "a1_bowdeck", Props.Barrel(0.92f, 0.31f), new Vector3(13.25f, 0f, 93.75f), Yaw(70), new Vector3(0.62f, 0.92f, 0.62f));
            DressUtil.Bake(w, "a1_bowdeck", Props.Crate(new Vector3(0.75f, 0.7f, 0.75f)), new Vector3(2.7f, 0f, 93.1f), Yaw(12), new Vector3(0.75f, 0.7f, 0.75f));
            // bitts with the hawser
            foreach (float x in new[] { 6.4f, 9.6f })
            {
                w.Kit.Box("a1_bowdeck", "wood_dark", new Vector3(x, 0.45f, 107.55f), new Vector3(0.3f, 0.9f, 0.3f), true);
                w.Kit.Box("a1_bowdeck", "wood_dark", new Vector3(x, 0.7f, 107.55f), new Vector3(0.7f, 0.1f, 0.1f));
            }
            w.Kit.B("a1_bowdeck", "rope", 0.3f).Rope(new Vector3(6.4f, 0.65f, 107.6f), new Vector3(9.6f, 0.65f, 107.6f), 0.25f, 0.05f, 10);
            w.Kit.B("a1_bowdeck", "rope", 0.3f).Rope(new Vector3(9.6f, 0.6f, 107.7f), new Vector3(8.3f, 0.5f, 113.5f), 0.5f, 0.05f, 12);
            DressUtil.Lantern(w, "bow_lantern", new Vector3(4.15f, 2.35f, 99.0f - 0.3f), false, 0, 0, false, false, 3f);
            w.Kit.Box("a1_bowdeck", "iron", new Vector3(4.15f, 2.42f, 98.86f), new Vector3(0.03f, 0.03f, 0.3f));
        }

        void Captain(World w)
        {
            var room = w.Act.Room("a1_capt");
            var rng = _rng;
            DressUtil.CeilingBeams(w, room, 1.4f);
            DressUtil.Rug(w, room, new Vector3(CX, 0, 102.9f), 3.2f, 2.4f, "cloth_red", 0f);
            // box bed with curtains
            var bed = Props.Bunk(2.0f, 1.0f, false);
            bed["wood_dark"].Box(new Vector3(0, 1.75f, 0), new Vector3(1.04f, 0.08f, 2.04f), Quaternion.identity);
            bed["cloth_red"].Box(new Vector3(0.5f, 1.1f, -0.55f), new Vector3(0.02f, 1.25f, 0.8f), Quaternion.identity);
            bed["cloth_red"].Box(new Vector3(0.5f, 1.1f, 0.62f), new Vector3(0.02f, 1.25f, 0.7f), Quaternion.identity);
            foreach (var s in new[] { new Vector2(-0.5f, -1f), new Vector2(0.5f, -1f), new Vector2(-0.5f, 1f), new Vector2(0.5f, 1f) })
                bed["wood_dark"].Box(new Vector3(s.x, 0.9f, s.y), new Vector3(0.06f, 1.8f, 0.06f), Quaternion.identity);
            DressUtil.Bake(w, "a1_capt", bed, new Vector3(4.62f, 0f, 101.25f), Yaw(0), new Vector3(1.0f, 1.8f, 2.0f));
            // wardrobe
            var wr = new PropMesh();
            wr["wood_dark"].Box(new Vector3(0, 1.0f, 0), new Vector3(0.6f, 2.0f, 1.1f), Quaternion.identity);
            wr["wood"].Box(new Vector3(0.305f, 1.05f, -0.27f), new Vector3(0.01f, 1.7f, 0.5f), Quaternion.identity);
            wr["wood"].Box(new Vector3(0.305f, 1.05f, 0.27f), new Vector3(0.01f, 1.7f, 0.5f), Quaternion.identity);
            wr["brass"].Box(new Vector3(0.32f, 1.05f, -0.04f), new Vector3(0.02f, 0.1f, 0.02f), Quaternion.identity);
            wr["brass"].Box(new Vector3(0.32f, 1.05f, 0.04f), new Vector3(0.02f, 0.1f, 0.02f), Quaternion.identity);
            DressUtil.Bake(w, "a1_capt", wr, new Vector3(4.32f, 0f, 103.4f), Yaw(0), new Vector3(0.6f, 2.0f, 1.1f));
            DressUtil.Bake(w, "a1_capt", Props.Shelf(1.4f, 2.0f, 0.32f, rng, "wood_dark"), new Vector3(11.83f, 0f, 101.0f), Yaw(-90), new Vector3(0.32f, 2.0f, 1.4f));
            // globe
            var gl = new PropMesh();
            gl["wood_dark"].Lathe(Vector3.zero, Quaternion.identity, new[] { new Vector2(0.18f, 0f), new Vector2(0.18f, 0.04f), new Vector2(0.04f, 0.08f), new Vector2(0.035f, 0.7f), new Vector2(0f, 0.72f) }, 10);
            var sph = new List<Vector2>();
            for (int i = 0; i <= 10; i++) { float a = i / 10f * Mathf.PI; sph.Add(new Vector2(Mathf.Sin(a) * 0.25f, -Mathf.Cos(a) * 0.25f + 0.25f)); }
            gl["seachart"].Lathe(new Vector3(0, 0.72f, 0), Quaternion.Euler(0, 0, 23), sph, 16, 0.5f);
            gl["brass"].Tube(new[] { new Vector3(0, 0.68f, -0.04f), new Vector3(0, 0.97f, -0.29f), new Vector3(0, 1.26f, -0.04f) }, 0.012f, 5, 0.2f);
            DressUtil.Bake(w, "a1_capt", gl, new Vector3(11.2f, 0f, 102.7f), Yaw(20), new Vector3(0.5f, 1.2f, 0.5f));
            DressUtil.Shifty(w, "capt_chair", Props.Chair("wood_dark"), new Vector3(10.6f, 0f, 104.65f), Yaw(0), new Vector3(0.45f, 0.9f, 0.45f), rng);
            DressUtil.Bake(w, "a1_capt", Props.Chair("wood_dark"), new Vector3(6.2f, 0f, 104.55f), Yaw(-10), new Vector3(0.45f, 0.9f, 0.45f));
            Props.Candles(rng, 3).BakeInto(w.Kit, "a1_capt", new Vector3(11.0f, 0.85f, 105.55f), Yaw(0));
            // the sea chart, and the clock that stopped at 03:17
            Props.Framed("seachart", 1.2f, 0.8f, "wood_dark").BakeInto(w.Kit, "a1_capt", new Vector3(10.4f, 1.2f, 99.02f), Yaw(0));
            var clock = new PropMesh();
            clock["wood_dark"].Cylinder(new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0), 0.05f, 0.2f, 0.2f, 18);
            clock["paper"].Disc(new Vector3(0, 0, 0.052f), Quaternion.Euler(90, 0, 0), 0.17f, 18);
            for (int i = 0; i < 12; i++)
            {
                var d = Quaternion.Euler(0, 0, -i * 30f) * Vector3.up;
                clock["black"].Box(d * 0.145f + new Vector3(0, 0, 0.054f), new Vector3(0.01f, 0.03f, 0.002f), Quaternion.Euler(0, 0, -i * 30f));
            }
            float hourA = (3f + 17f / 60f) * 30f, minA = 17f * 6f;
            clock["black"].Box(Quaternion.Euler(0, 0, -hourA) * new Vector3(0, 0.05f, 0) + new Vector3(0, 0, 0.058f), new Vector3(0.014f, 0.1f, 0.003f), Quaternion.Euler(0, 0, -hourA));
            clock["black"].Box(Quaternion.Euler(0, 0, -minA) * new Vector3(0, 0.07f, 0) + new Vector3(0, 0, 0.061f), new Vector3(0.009f, 0.14f, 0.003f), Quaternion.Euler(0, 0, -minA));
            clock.BakeInto(w.Kit, "a1_capt", new Vector3(5.5f, 1.95f, 99.03f), Yaw(0));
            // window over the two tables, the moon comes in through it
            var win = new PropMesh();
            win["glass"].Box(new Vector3(0, 0, 0.01f), new Vector3(1.6f, 0.9f, 0.01f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(0, 0.5f, 0.03f), new Vector3(1.8f, 0.1f, 0.06f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(0, -0.5f, 0.05f), new Vector3(1.9f, 0.1f, 0.12f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(-0.85f, 0, 0.03f), new Vector3(0.1f, 0.9f, 0.06f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(0.85f, 0, 0.03f), new Vector3(0.1f, 0.9f, 0.06f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(0, 0, 0.025f), new Vector3(0.04f, 0.9f, 0.04f), Quaternion.identity);
            win["wood_dark"].Box(new Vector3(0, 0, 0.025f), new Vector3(1.6f, 0.04f, 0.04f), Quaternion.identity);
            win.BakeInto(w.Kit, "a1_capt", new Vector3(8.4f, 1.55f, 106.97f), Yaw(180));
            w.Anchor("boxes", new Vector3(6.2f, 0.95f, 105.3f));
            w.Anchor("capt_window", new Vector3(8.4f, 1.55f, 106.8f));
        }

        // ------------------------------------------------------------------ water
        void SeaAndBilge(World w)
        {
            var sea = WaterSurface.CreateSea(w.Root, new Vector3(CX, 0f, 56f), 280f, Sea, 80);
            w.Waters.Add(sea);
            w.Waters.Add(WaterSurface.CreateFlat(w.Root, "bilge_aft", new RectXZ(0.35f, 0.2f, 15.3f, 72f), G - 0.62f, false));
            w.Waters.Add(WaterSurface.CreateFlat(w.Root, "bilge_fwd", new RectXZ(2.3f, 72.2f, 11.4f, 34f), G - 0.62f, false));
        }

        // ------------------------------------------------------------------ light
        public void Light(World w, GameState state)
        {
            var act = w.Act;
            var preset = new AtmosPreset
            {
                FogDistance = act.FogDistance,
                FogTint = new Color(act.FogColor[0], act.FogColor[1], act.FogColor[2]),
                FogAlbedo = new Color(0.6f, 0.68f, 0.72f),
                FogHeight = 9f,
                SkyTop = new Color(0.02f, 0.03f, 0.05f),
                SkyMid = new Color(0.06f, 0.08f, 0.1f),
                SkyBottom = new Color(0.1f, 0.12f, 0.13f),
                SkyExposure = 1.4f,
                Exposure = 2.0f,
                Saturation = -22f,
                Contrast = 14f,
                Filter = new Color(0.9f, 0.97f, 1f),
                Bloom = 0.24f,
                Vignette = 0.3f,
            };
            w.Atmos = Atmos.Create(w.Root, preset);
            w.MoonLux = 4f * act.Moon;
            w.Moon = LightKit.Directional("Moon", w.Root, Quaternion.Euler(31f, 202f, 0f), LightKit.Moon, w.MoonLux, true);

            foreach (var r in act.Rooms)
            {
                if (!(r.Ceil || r.Id == "a1_cargo" || r.Id == "a1_well" || r.Id == "a1_stair")) continue;
                float h = r.Ceil ? r.CeilH : 3.6f;
                var c = new Vector3(r.Rect.X + r.Rect.W * 0.5f, r.MinFloor + h * 0.5f, r.Rect.Z + r.Rect.D * 0.5f);
                var size = new Vector3(r.Rect.W, h + (r.IsRamp ? Mathf.Abs(r.FloorTo - r.Floor) : 0f), r.Rect.D);
                w.Atmos.Interior(r.Id, c, size, r.Ambient, r.Id == "a1_cargo" ? 22f : 34f);
            }

            // the one light on deck: a storm lantern by the helm, guttering in the wind
            var helm = LightKit.Point("helm_lantern", w.Root, HelmLantern + new Vector3(0, -0.24f, 0), LightKit.Lantern, 230f, 8f, true, false, 1f, 0.04f);
            var hf = helm.gameObject.AddComponent<Flicker>();
            hf.Target = helm; hf.Base = 230f; hf.Amount = 0.14f; hf.Wind = 0.8f;
            w.FlickerLights.Add(helm);
            w.LightZones.Add(new LightZone { Pos = HelmLantern + Vector3.down * 0.8f, Radius = 3.2f, Safe = false });

            // moonlight pouring into the hold through the broken deck
            var shaft = LightKit.Spot("cargo_moonshaft", w.Root, new Vector3(7.4f, 15f, 75.2f), Quaternion.Euler(84f, 8f, 0f), LightKit.Moon, 14000f, 34f, 46f, true, true, 1.6f);
            w.LightZones.Add(new LightZone { Pos = new Vector3(7.6f, G + 1f, 76.5f), Radius = 3.4f, Safe = false });
            // moon through the captain's window
            LightKit.Spot("capt_window_moon", w.Root, new Vector3(8.4f, 1.9f, 106.75f), Quaternion.Euler(28f, 180f, 0f), LightKit.Moon, 1500f, 8f, 62f, true, false, 1.2f);
            // the captain's lamp, still burning
            var capt = DressUtil.Lantern(w, "capt_lamp", new Vector3(CX, 2.38f, 103.2f), true, 380f, 7.5f, true, true, 1.6f);
            w.Anchors["capt_lamp"] = capt.transform.parent;
            w.LightZones.Add(new LightZone { Pos = new Vector3(CX, 1.2f, 103.2f), Radius = 3.6f, Safe = false });

            w.On("a1_music", rt =>
            {
                // the room "answers": the lamp swells for a moment and the watch bell on the fore deck tolls once
                if (MusicLamp != null) MusicLamp.GetComponentInParent<Flicker>().Strength = 1.35f;
                var bell = w.AnchorOrNull("ship_bell");
                if (bell != null) Game.Audio.PlayAt("sfx_ship_bell", bell.position, Bus.Effects, 1f, 1f, 3f, 60f);
            });
            w.On("a1_locket", rt =>
            {
                var lamp = w.AnchorOrNull("capt_lamp");
                var f = lamp != null ? lamp.GetComponent<Flicker>() : null;
                if (f != null) f.Amount = 0.35f;
            });
            w.On("a1_crates", rt =>
            {
                Game.Audio.PlayAt("sfx_grate_open", new Vector3(9f, G + 0.5f, 84f), Bus.Effects, 1f);
            });
        }
    }
}
