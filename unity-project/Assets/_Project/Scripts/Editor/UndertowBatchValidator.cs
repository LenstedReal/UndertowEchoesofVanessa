#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Undertow.Logic;

namespace Undertow.Editor
{
    /// <summary>
    /// Build gate: scenes, HDRP asset, every localisation key the game uses, fonts, textures, audio,
    /// baked materials, and a full logic walkthrough of all four acts on every story branch.
    /// It checks data, not looks: it is not a substitute for playing the build.
    /// </summary>
    public static class UndertowBatchValidator
    {
        public static readonly string[] RequiredScenePaths = UndertowProjectSetup.Scenes;
        const string Res = "Assets/_Project/Resources/Undertow/";
        static readonly Regex KeyLiteral = new Regex("\"((?:ui|msg|caption|hint|boot|ending|credits|drawing|speaker|tip|journal|voice|hud|toast)\\.[a-z0-9_.]+)\"", RegexOptions.Compiled);
        static readonly Regex AudioCall = new Regex("(?:Play2D|PlayAt|Loop|SetAmbience|PlayMusic)\\(\\s*\"([a-z0-9_]+)\"", RegexOptions.Compiled);

        [MenuItem("Undertow/Validate Project")]
        public static void ValidateAll()
        {
            AssetDatabase.Refresh();
            var errors = new List<string>();
            var warnings = new List<string>();

            foreach (var p in RequiredScenePaths) if (!File.Exists(p)) errors.Add("missing scene " + p);
            if (!(GraphicsSettings.defaultRenderPipeline is HDRenderPipelineAsset)) errors.Add("Graphics settings: no HDRP asset assigned (run Undertow > Run Project Setup)");

            // ---------------------------------------------------------- localisation
            var keys = new HashSet<string>(ContentDb.RequiredKeys());
            foreach (var file in Directory.GetFiles("Assets/_Project/Scripts/Runtime", "*.cs", SearchOption.AllDirectories))
                foreach (Match m in KeyLiteral.Matches(File.ReadAllText(file))) keys.Add(m.Groups[1].Value);
            for (int i = 0; i < 4; i++) keys.Add("ui.size." + i);
            for (int i = 0; i < 3; i++) keys.Add("ui.display." + i);
            for (int i = 0; i < 4; i++) keys.Add("ui.aa." + i);
            foreach (var k in GameSettings.QualityKeys) keys.Add(k);
            for (int i = 1; i <= 6; i++) keys.Add("drawing." + i);
            for (int i = 1; i <= 10; i++) keys.Add("tip." + i);
            keys.Add("msg.bells_solved_1"); keys.Add("msg.bells_solved_2");
            foreach (var e in new[] { "answer", "surface", "undertow" })
            {
                keys.Add("ending." + e + ".title"); keys.Add("ending." + e + ".line");
                for (int i = 1; i <= 5; i++) keys.Add("ending." + e + "." + i);
            }
            foreach (var k in Enum.GetNames(typeof(VoiceKind))) keys.Add("speaker." + k.ToLowerInvariant());
            foreach (var a in ContentDb.Acts) foreach (var it in a.Items) if (it.Speaker != null) keys.Add("speaker." + it.Speaker);
            keys.RemoveWhere(k => k.EndsWith(".") || k.EndsWith("_"));   // prefixes such as "msg.bells_solved_" + act
            var en = LoadTable("en", errors);
            if (en != null)
                foreach (var k in keys.OrderBy(x => x))
                    if (!en.TryGet(k, out _)) errors.Add("loc en: missing key " + k);
            foreach (var lang in LocTable.Languages)
            {
                if (lang.Code == "en") continue;
                var t = LoadTable(lang.Code, errors);
                if (t == null) continue;
                int missing = keys.Count(k => !t.TryGet(k, out _));
                if (missing > 0) (lang.Reviewed ? errors : warnings).Add("loc " + lang.Code + ": " + missing + " keys fall back to English");
            }

            // ---------------------------------------------------------- fonts, textures, materials
            foreach (var f in new[] { "FiraSans-Regular", "CormorantGaramond-SemiBold", "EBGaramond", "Caveat", "IMFellEnglish" })
                if (Resources.Load<Font>("Undertow/Fonts/" + f) == null) errors.Add("missing font " + f);
            var texNames = new HashSet<string>(MatLib.Defs.Where(d => !string.IsNullOrEmpty(d.Tex)).Select(d => d.Tex + "_a"));
            for (int i = 1; i <= 6; i++) texNames.Add("drawing" + i + "_a");
            texNames.Add("parchment_a"); texNames.Add("paper_a");
            foreach (var t in texNames) if (Resources.Load<Texture2D>("Undertow/Textures/" + t) == null) errors.Add("missing texture " + t);
            foreach (var d in MatLib.Defs)
                if (!File.Exists(UndertowProjectSetup.MaterialRoot + "/" + d.Name + ".mat")) errors.Add("material not baked: " + d.Name + " (run Undertow > Configure Texture Importers and Bake Materials)");

            // ---------------------------------------------------------- audio
            var clips = new HashSet<string>();
            foreach (var file in Directory.GetFiles("Assets/_Project/Scripts/Runtime", "*.cs", SearchOption.AllDirectories))
                foreach (Match m in AudioCall.Matches(File.ReadAllText(file))) clips.Add(m.Groups[1].Value);
            foreach (var s in new[] { "wood", "wetwood", "stone", "sand", "water", "metal", "rug" }) for (int i = 1; i <= 4; i++) clips.Add("step_" + s + "_" + i);
            for (int i = 0; i < 5; i++) { clips.Add("bell_a1_" + i); clips.Add("mb_note_a1_" + i); }
            for (int i = 0; i < 3; i++) { clips.Add("bell_a2_" + i); clips.Add("mb_note_a2_" + i); }
            for (int i = 1; i <= 3; i++) { clips.Add("sfx_whisper_" + i); clips.Add("sfx_hull_creak_" + i); clips.Add("sfx_drip_" + i); }
            for (int i = 1; i <= 2; i++) { clips.Add("sfx_thunder_" + i); clips.Add("sfx_wood_groan_" + i); clips.Add("sfx_temple_settle_" + i); }
            foreach (var k in Enum.GetNames(typeof(VoiceKind))) clips.Add("vo_hum_" + k.ToLowerInvariant());
            clips.Add("lead_echo"); clips.Add("lead_mimic");
            foreach (var a in ContentDb.Acts) clips.Add(a.Ambience);
            foreach (var c in clips.OrderBy(x => x))
                if (Resources.Load<AudioClip>("Undertow/Audio/" + c) == null) errors.Add("missing audio clip " + c);

            // ---------------------------------------------------------- levels: every act, every branch
            foreach (var act in ContentDb.Acts)
            {
                var lv = LevelCompiler.Compile(act);
                foreach (var e in lv.Errors) errors.Add("act " + act.Id + " level: " + e);
                var st = LevelValidator.ValidateStatic(act, lv);
                errors.AddRange(st.Errors.Select(e => "act " + act.Id + ": " + e));
                foreach (bool echo in new[] { true, false })
                    foreach (bool answer in new[] { true, false })
                        foreach (bool altar in new[] { true, false })
                        {
                            var rep = Walkthrough.Run(act, ContentDb.ChapterStartState(act.Id), new Walkthrough.Options { Act2FollowEcho = echo, Act3Answer = answer, AltarEcho = altar });
                            foreach (var e in rep.Errors) errors.Add("act " + act.Id + " walkthrough (echo=" + echo + ", answer=" + answer + ", altar=" + altar + "): " + e);
                        }
            }

            foreach (var w in warnings) Debug.LogWarning("[Undertow] " + w);
            foreach (var e in errors.Distinct()) Debug.LogError("[Undertow] " + e);
            if (errors.Count > 0) throw new Exception("Undertow validation failed: " + errors.Distinct().Count() + " issue(s). See the Console.");
            Debug.Log("[Undertow] validation passed: " + keys.Count + " localisation keys, " + clips.Count + " audio clips, " + texNames.Count + " textures, 4 acts x 8 branches walked. This checks data only; play the build to check how it looks and feels.");
        }

        static LocTable LoadTable(string code, List<string> errors)
        {
            var ta = Resources.Load<TextAsset>("Undertow/Loc/" + code);
            if (ta == null)
            {
                errors.Add("missing localisation table " + code);
                return null;
            }
            var parseErrors = new List<string>();
            var t = LocTable.Parse(code, ta.text, parseErrors);
            foreach (var e in parseErrors) errors.Add("loc " + code + ": " + e);
            return t;
        }

        [MenuItem("Undertow/Build Windows 64-bit Release Player")]
        public static void BuildWindows64Release() => Build(false);

        [MenuItem("Undertow/Build Windows 64-bit Development Player")]
        public static void BuildWindows64Development() => Build(true);

        static void Build(bool development)
        {
            UndertowProjectSetup.Run(false);
            ValidateAll();
            string dir = development ? "Builds/Windows64/Development" : "Builds/Windows64/Release";
            Directory.CreateDirectory(dir);
            var opts = new BuildPlayerOptions
            {
                scenes = RequiredScenePaths,
                locationPathName = dir + "/UndertowEchoesOfVanessa.exe",
                target = BuildTarget.StandaloneWindows64,
                options = development ? (BuildOptions.Development | BuildOptions.AllowDebugging) : BuildOptions.CompressWithLz4HC,
            };
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Windows64 build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors)");
            Debug.Log("[Undertow] Windows64 " + (development ? "development" : "release") + " build succeeded: " + report.summary.totalSize / (1024 * 1024) + " MB in " + dir);
        }
    }
}
#endif
