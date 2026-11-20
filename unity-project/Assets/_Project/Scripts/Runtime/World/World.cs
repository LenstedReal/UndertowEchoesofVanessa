using System;
using System.Collections.Generic;
using UnityEngine;
using Undertow.Logic;

namespace Undertow
{
    /// <summary>A place where light keeps the dark away (sanity regenerates when Safe).</summary>
    public sealed class LightZone
    {
        public Vector3 Pos;
        public float Radius;
        public bool Safe;
        public Func<bool> Active;
        public bool IsActive => Active == null || Active();
    }

    /// <summary>Everything built for one act: geometry, items, blockers, lights, water, anchors.</summary>
    public sealed class World
    {
        public ActDef Act;
        public CompiledLevel Level;
        public Transform Root;
        public Kit Kit;
        public Atmos Atmos;
        public Light Moon;
        public float MoonLux;
        public readonly Dictionary<string, Interactable> Items = new Dictionary<string, Interactable>();
        public readonly Dictionary<string, Blocker> Blockers = new Dictionary<string, Blocker>();
        public readonly List<LightZone> LightZones = new List<LightZone>();
        public readonly List<WaterSurface> Waters = new List<WaterSurface>();
        public readonly Dictionary<string, Transform> Anchors = new Dictionary<string, Transform>();
        public readonly List<Light> FlickerLights = new List<Light>();
        public readonly List<Distortable> Distortables = new List<Distortable>();
        public readonly Dictionary<string, Action<ActRuntime>> OnFlag = new Dictionary<string, Action<ActRuntime>>();

        public Transform Anchor(string name, Vector3 pos)
        {
            if (!Anchors.TryGetValue(name, out var t))
            {
                t = new GameObject("@" + name).transform;
                t.SetParent(Root, false);
                Anchors[name] = t;
            }
            t.localPosition = pos;
            return t;
        }

        /// <summary>Adds a handler for a story flag (several handlers per flag are fine).</summary>
        public void On(string flag, Action<ActRuntime> handler)
        {
            OnFlag[flag] = OnFlag.TryGetValue(flag, out var existing) ? existing + handler : handler;
        }

        public Transform AnchorOrNull(string name) => Anchors.TryGetValue(name, out var t) ? t : null;

        /// <summary>The room the feet are in: rooms whose floor is near the feet height, smallest first.</summary>
        public RoomDef RoomAt(Vector3 feet)
        {
            RoomDef best = null;
            float bestScore = float.MaxValue;
            foreach (var r in Act.Rooms)
            {
                if (!r.Rect.Contains(feet.x, feet.z)) continue;
                float fy = r.FloorAt(feet.x, feet.z);
                float dy = feet.y - fy;
                if (dy < -0.6f || dy > 1.4f) continue;
                float score = r.Rect.Area + Mathf.Abs(dy) * 400f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = r;
                }
            }
            return best;
        }

        public float FloorY(RoomDef r, Vector3 p) => r != null ? r.FloorAt(p.x, p.z) : 0f;

        public Vector3 ItemPoint(ItemDef i)
        {
            var r = Act.Room(i.Room);
            float y = r != null ? r.FloorAt(i.Pos.X, i.Pos.Z) : 0f;
            return new Vector3(i.Pos.X, y + i.Height, i.Pos.Z);
        }

        public Vector3 Ground(float x, float z, float yHint)
        {
            var r = RoomAt(new Vector3(x, yHint, z));
            return new Vector3(x, r != null ? r.FloorAt(x, z) : yHint, z);
        }
    }

    /// <summary>Builds an act: compiled geometry, dressing, items, blockers, atmosphere.</summary>
    public static class WorldBuilder
    {
        public static World Build(ActDef act, GameState state)
        {
            var w = new World { Act = act };
            var rootGo = new GameObject("World_" + act.SceneName);
            w.Root = rootGo.transform;
            w.Level = LevelCompiler.Compile(act);
            foreach (var e in w.Level.Errors) Game.Warn("level: " + e);
            w.Kit = new Kit(w.Root, act.Id * 7919);

            IActDressing dressing;
            switch (act.Id)
            {
                case 1: dressing = new Act1Dressing(); break;
                case 2: dressing = new Act2Dressing(); break;
                case 3: dressing = new Act3Dressing(); break;
                default: dressing = new Act4Dressing(); break;
            }
            var style = dressing.Style(w);
            GeometryBuilder.Build(w.Kit, w.Level, style);
            dressing.Dress(w, state);
            ItemFactory.SpawnAll(w, state);
            BlockerFactory.SpawnAll(w, state);
            dressing.Light(w, state);
            w.Kit.Finish();
            return w;
        }
    }

    public interface IActDressing
    {
        LevelStyle Style(World w);
        void Dress(World w, GameState state);
        void Light(World w, GameState state);
    }
}
