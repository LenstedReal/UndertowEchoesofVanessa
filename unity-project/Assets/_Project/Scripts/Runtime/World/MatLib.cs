using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Undertow
{
    /// <summary>One material of the library. The editor bakes these into .mat assets (so HDRP keeps the shader variants in builds).</summary>
    public sealed class MatDef
    {
        public string Name;
        public string Tex;                  // texture base name in Resources/Undertow/Textures (name_a, name_n)
        public Color Color = Color.white;
        public float Smooth = 0.3f;
        public float Metal;
        public float NormalScale = 1f;
        public Color Emission = Color.black;
        public float EmissionNits;          // radiance in nits
        public bool Transparent;
        public bool DoubleSided;
        public bool Unlit;
        public bool Additive;
    }

    /// <summary>
    /// Material library. Loads Resources/Undertow/Materials/&lt;name&gt; (created by the editor setup);
    /// if a material is missing (setup not run yet) it is built at runtime from the same definition.
    /// </summary>
    public static class MatLib
    {
        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        public static readonly MatDef[] Defs =
        {
            D("deck", "wood_planks", 0.62f, 0.55f, 0.47f, 0.42f),
            D("deck_dry", "wood_planks", 0.68f, 0.61f, 0.52f, 0.2f),
            D("panel", "wood_dark", 0.5f, 0.42f, 0.35f, 0.3f),
            D("hull", "wood_hull", 0.56f, 0.56f, 0.52f, 0.34f),
            D("hull_dark", "wood_hull", 0.3f, 0.3f, 0.29f, 0.4f),
            D("wood", "wood_raw", 0.7f, 0.62f, 0.52f, 0.2f),
            D("wood_dark", "wood_raw", 0.34f, 0.27f, 0.21f, 0.28f),
            D("rope", "rope", 0.72f, 0.64f, 0.5f, 0.12f),
            new MatDef { Name = "sail", Tex = "sail", Color = new Color(0.3f, 0.29f, 0.27f), Smooth = 0.08f, DoubleSided = true },
            new MatDef { Name = "sail_light", Tex = "sail", Color = new Color(0.62f, 0.6f, 0.55f), Smooth = 0.08f, DoubleSided = true },
            new MatDef { Name = "cloth", Tex = "cloth", Color = new Color(0.55f, 0.52f, 0.47f), Smooth = 0.12f, DoubleSided = true },
            new MatDef { Name = "cloth_red", Tex = "cloth", Color = new Color(0.42f, 0.09f, 0.08f), Smooth = 0.15f, DoubleSided = true },
            new MatDef { Name = "cloth_blue", Tex = "cloth", Color = new Color(0.14f, 0.18f, 0.3f), Smooth = 0.15f, DoubleSided = true },
            new MatDef { Name = "iron", Tex = "iron", Color = new Color(0.45f, 0.42f, 0.4f), Smooth = 0.35f, Metal = 0.7f },
            new MatDef { Name = "brass", Tex = "brass", Color = new Color(0.86f, 0.68f, 0.42f), Smooth = 0.62f, Metal = 1f },
            new MatDef { Name = "darkmetal", Tex = "iron", Color = new Color(0.2f, 0.2f, 0.22f), Smooth = 0.42f, Metal = 0.8f },
            D("stone", "stone_blocks", 0.62f, 0.62f, 0.6f, 0.26f),
            D("cobble", "cobble", 0.55f, 0.55f, 0.53f, 0.5f),
            D("plaster", "plaster", 0.72f, 0.7f, 0.65f, 0.15f),
            D("roof", "roof_tiles", 0.5f, 0.38f, 0.33f, 0.3f),
            D("temple", "temple_stone", 0.55f, 0.66f, 0.64f, 0.34f),
            D("temple_floor", "temple_floor", 0.5f, 0.6f, 0.58f, 0.48f),
            D("sand", "sand", 0.62f, 0.58f, 0.5f, 0.1f),
            new MatDef { Name = "water", Tex = "water", Color = new Color(0.018f, 0.03f, 0.036f), Smooth = 0.94f, NormalScale = 0.7f },
            new MatDef { Name = "water_shallow", Tex = "water", Color = new Color(0.05f, 0.075f, 0.075f, 0.62f), Smooth = 0.95f, NormalScale = 0.5f, Transparent = true },
            new MatDef { Name = "glass", Color = new Color(0.55f, 0.65f, 0.7f, 0.22f), Smooth = 0.96f, Transparent = true },
            new MatDef { Name = "mirror", Color = new Color(0.82f, 0.86f, 0.88f), Smooth = 0.985f, Metal = 1f },
            new MatDef { Name = "mirror_old", Tex = "iron", Color = new Color(0.62f, 0.66f, 0.66f), Smooth = 0.9f, Metal = 1f, NormalScale = 0.15f },
            new MatDef { Name = "paper", Tex = "paper", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "parchment", Tex = "parchment", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "portrait", Tex = "portrait", Color = Color.white, Smooth = 0.25f },
            new MatDef { Name = "familytree", Tex = "familytree", Color = Color.white, Smooth = 0.1f },
            new MatDef { Name = "drawing1", Tex = "drawing1", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "drawing2", Tex = "drawing2", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "drawing3", Tex = "drawing3", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "drawing4", Tex = "drawing4", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "drawing5", Tex = "drawing5", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "drawing6", Tex = "drawing6", Color = Color.white, Smooth = 0.05f, DoubleSided = true },
            new MatDef { Name = "carving", Tex = "carving", Color = Color.white, Smooth = 0.25f },
            new MatDef { Name = "carving2", Tex = "carving2", Color = Color.white, Smooth = 0.3f },
            new MatDef { Name = "chalk", Tex = "chalk", Color = Color.white, Smooth = 0.15f },
            new MatDef { Name = "plaque", Tex = "plaque", Color = Color.white, Smooth = 0.3f },
            new MatDef { Name = "nameboard", Tex = "nameboard", Color = Color.white, Smooth = 0.25f },
            new MatDef { Name = "nameboard_req", Tex = "nameboard_req", Color = Color.white, Smooth = 0.35f },
            new MatDef { Name = "relief", Tex = "relief", Color = new Color(0.7f, 0.8f, 0.78f), Smooth = 0.3f },
            new MatDef { Name = "seachart", Tex = "seachart", Color = Color.white, Smooth = 0.1f, DoubleSided = true },
            new MatDef { Name = "black", Color = new Color(0.02f, 0.02f, 0.022f), Smooth = 0.2f },
            new MatDef { Name = "porcelain", Color = new Color(0.9f, 0.86f, 0.8f), Smooth = 0.65f },
            new MatDef { Name = "velvet", Tex = "cloth", Color = new Color(0.26f, 0.05f, 0.06f), Smooth = 0.18f },
            new MatDef { Name = "candle", Color = new Color(0.9f, 0.86f, 0.74f), Smooth = 0.42f },
            new MatDef { Name = "bone", Color = new Color(0.78f, 0.74f, 0.66f), Smooth = 0.3f },
            new MatDef { Name = "algae", Color = new Color(0.08f, 0.2f, 0.16f), Smooth = 0.5f, Emission = new Color(0.2f, 1f, 0.85f), EmissionNits = 0.6f },
            new MatDef { Name = "flame", Color = new Color(1f, 0.6f, 0.2f), Emission = new Color(1f, 0.55f, 0.16f), EmissionNits = 40f, Smooth = 0f },
            new MatDef { Name = "ember", Color = new Color(0.2f, 0.06f, 0.02f), Emission = new Color(1f, 0.32f, 0.08f), EmissionNits = 6f, Smooth = 0.1f },
            new MatDef { Name = "glow_warm", Color = new Color(1f, 0.85f, 0.6f), Emission = new Color(1f, 0.72f, 0.42f), EmissionNits = 12f, Smooth = 0.2f },
            new MatDef { Name = "glow_cold", Color = new Color(0.7f, 0.9f, 1f), Emission = new Color(0.55f, 0.85f, 1f), EmissionNits = 10f, Smooth = 0.2f },
            new MatDef { Name = "footprint", Color = new Color(0.6f, 0.75f, 0.8f), Emission = new Color(0.55f, 0.85f, 1f), EmissionNits = 2f, Smooth = 0.7f },
            new MatDef { Name = "beam", Color = new Color(0.5f, 0.95f, 1f, 0.5f), Emission = new Color(0.45f, 0.95f, 1f), EmissionNits = 60f, Unlit = true, Transparent = true, Additive = true, DoubleSided = true },
            new MatDef { Name = "shaft", Color = new Color(0.4f, 0.9f, 0.95f, 0.08f), Emission = new Color(0.35f, 0.85f, 0.9f), EmissionNits = 3f, Unlit = true, Transparent = true, Additive = true, DoubleSided = true },
            new MatDef { Name = "silhouette", Color = new Color(0.004f, 0.006f, 0.008f), Smooth = 0.6f },
            new MatDef { Name = "water_ceiling", Tex = "water", Color = new Color(0.12f, 0.5f, 0.52f, 0.5f), Smooth = 0.96f, NormalScale = 0.9f, Transparent = true, DoubleSided = true, Emission = new Color(0.2f, 0.8f, 0.85f), EmissionNits = 1.5f },
        };

        static MatDef D(string name, string tex, float r, float g, float b, float smooth)
            => new MatDef { Name = name, Tex = tex, Color = new Color(r, g, b), Smooth = smooth };

        public static MatDef Def(string name)
        {
            foreach (var d in Defs) if (d.Name == name) return d;
            return null;
        }

        public static Material Get(string name)
        {
            if (Cache.TryGetValue(name, out var m) && m != null) return m;
            m = Resources.Load<Material>("Undertow/Materials/" + name);
            if (m == null)
            {
                var d = Def(name) ?? new MatDef { Name = name, Color = Color.magenta };
                m = CreateRuntime(d);
            }
            Cache[name] = m;
            return m;
        }

        /// <summary>A private copy (for per-object animation like scrolling water or fading glows).</summary>
        public static Material Instance(string name)
        {
            var m = new Material(Get(name));
            m.name = name + " (instance)";
            return m;
        }

        public static void ClearCache() => Cache.Clear();

        static Material CreateRuntime(MatDef d)
        {
            var shader = Shader.Find(d.Unlit ? "HDRP/Unlit" : "HDRP/Lit");
            if (shader == null)
            {
                Game.Warn("HDRP shader not found; is the HDRP asset assigned? Material " + d.Name);
                shader = Shader.Find("Hidden/InternalErrorShader");
            }
            var m = new Material(shader) { name = d.Name + " (runtime)" };
            Configure(m, d, n => Resources.Load<Texture2D>("Undertow/Textures/" + n));
            return m;
        }

        /// <summary>Applies a definition to an HDRP Lit/Unlit material. Shared by the runtime fallback and the editor baker.</summary>
        public static void Configure(Material m, MatDef d, Func<string, Texture2D> loadTex)
        {
            var col = d.Color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
            if (m.HasProperty("_UnlitColor")) m.SetColor("_UnlitColor", col);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", d.Smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", d.Metal);
            if (!string.IsNullOrEmpty(d.Tex) && loadTex != null)
            {
                var a = loadTex(d.Tex + "_a");
                if (a != null)
                {
                    if (m.HasProperty("_BaseColorMap")) m.SetTexture("_BaseColorMap", a);
                    if (m.HasProperty("_UnlitColorMap")) m.SetTexture("_UnlitColorMap", a);
                }
                var n = loadTex(d.Tex + "_n");
                if (n != null && m.HasProperty("_NormalMap"))
                {
                    m.SetTexture("_NormalMap", n);
                    m.SetFloat("_NormalScale", d.NormalScale);
                }
            }
            if (d.EmissionNits > 0f)
            {
                var e = d.Emission.linear * d.EmissionNits;
                if (m.HasProperty("_UseEmissiveIntensity")) m.SetFloat("_UseEmissiveIntensity", 0f);
                if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", e);
                if (m.HasProperty("_EmissiveColorLDR")) m.SetColor("_EmissiveColorLDR", d.Emission);
            }
            if (d.DoubleSided && m.HasProperty("_DoubleSidedEnable")) m.SetFloat("_DoubleSidedEnable", 1f);
            if (d.Transparent)
            {
                HDMaterial.SetSurfaceType(m, true);
                if (m.HasProperty("_BlendMode")) m.SetFloat("_BlendMode", d.Additive ? 1f : 0f);
                if (m.HasProperty("_EnableBlendModePreserveSpecularLighting")) m.SetFloat("_EnableBlendModePreserveSpecularLighting", 0f);
                if (m.HasProperty("_TransparentWritingMotionVec")) m.SetFloat("_TransparentWritingMotionVec", 0f);
            }
            try
            {
                HDMaterial.ValidateMaterial(m);
            }
            catch (Exception ex)
            {
                Game.Warn("ValidateMaterial failed for " + d.Name + ": " + ex.Message);
            }
        }

        /// <summary>Sets an emissive radiance at runtime (flames, glows, footprints).</summary>
        public static void SetEmission(Material m, Color c, float nits)
        {
            if (m == null) return;
            if (m.HasProperty("_EmissiveColor")) m.SetColor("_EmissiveColor", c.linear * nits);
        }
    }
}
