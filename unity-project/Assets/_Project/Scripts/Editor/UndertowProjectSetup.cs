#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Undertow.Editor
{
    /// <summary>
    /// One-click (and on-open) project setup: assigns the HDRP asset to Graphics and every Quality level,
    /// writes the build scene list, sets player options, and bakes the material library into
    /// Resources/Undertow/Materials so HDRP keeps every shader variant the game needs in builds.
    /// </summary>
    [InitializeOnLoad]
    public static class UndertowProjectSetup
    {
        public const int Version = 3;
        public const string HdrpPath = "Assets/_Project/Settings/HDRP/UndertowHDRPAsset_PC_Ultra.asset";
        public const string MaterialRoot = "Assets/_Project/Resources/Undertow/Materials";
        public const string TextureRoot = "Assets/_Project/Resources/Undertow/Textures";
        static readonly string PrefKey = "Undertow.SetupVersion." + Application.dataPath.GetHashCode();
        static int _tries;

        public static readonly string[] Scenes =
        {
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/ActI_MercifulRequiem.unity",
            "Assets/_Project/Scenes/ActII_ShipGraveyard.unity",
            "Assets/_Project/Scenes/ActIII_ForsakenPort.unity",
            "Assets/_Project/Scenes/ActIV_SunkenTemple.unity",
            "Assets/_Project/Scenes/Endings_Resolution.unity",
        };

        static UndertowProjectSetup()
        {
            if (Application.isBatchMode) return;   // build automation calls the steps explicitly
            EditorApplication.delayCall += AutoRun;
        }

        static void AutoRun()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (++_tries < 40) EditorApplication.delayCall += AutoRun;
                return;
            }
            bool pipelineOk = GraphicsSettings.defaultRenderPipeline is HDRenderPipelineAsset;
            bool materialsOk = Directory.Exists(MaterialRoot) && Directory.GetFiles(MaterialRoot, "*.mat").Length >= 10;
            if (EditorPrefs.GetInt(PrefKey, 0) >= Version && pipelineOk && materialsOk) return;
            // HDRP shaders may still be compiling on the very first open
            if (Shader.Find("HDRP/Lit") == null)
            {
                if (++_tries < 40) EditorApplication.delayCall += AutoRun;
                return;
            }
            Run(false);
        }

        [MenuItem("Undertow/Run Project Setup")]
        public static void RunFromMenu() => Run(true);

        public static void Run(bool verbose)
        {
            EnsurePipeline();
            EnsureBuildScenes();
            EnsurePlayerSettings();
            int mats = BakeMaterials();
            EditorPrefs.SetInt(PrefKey, Version);
            AssetDatabase.SaveAssets();
            Debug.Log("[Undertow] project setup done (HDRP asset assigned, " + Scenes.Length + " scenes in the build, " + mats + " materials baked).");
            if (verbose) EditorUtility.DisplayDialog("Undertow", "Project setup done.\n\nHDRP asset assigned, build scenes set, " + mats + " materials baked.\nOpen Assets/_Project/Scenes/MainMenu.unity and press Play.", "OK");
        }

        public static HDRenderPipelineAsset EnsurePipeline()
        {
            var asset = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(HdrpPath);
            if (asset == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(HdrpPath));
                asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
                AssetDatabase.CreateAsset(asset, HdrpPath);
            }
            var s = asset.currentPlatformRenderPipelineSettings;
            s.supportVolumetrics = true;
            s.supportSSAO = true;
            s.supportShadowMask = true;
            s.supportMotionVectors = true;
            s.supportDecals = true;
            s.supportTransparentBackface = true;
            s.supportTransparentDepthPrepass = true;
            s.supportTransparentDepthPostpass = true;
            s.supportRayTracing = false;
            asset.currentPlatformRenderPipelineSettings = s;
            EditorUtility.SetDirty(asset);
            if (GraphicsSettings.defaultRenderPipeline != asset) GraphicsSettings.defaultRenderPipeline = asset;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                if (QualitySettings.renderPipeline != asset) QualitySettings.renderPipeline = asset;
            }
            QualitySettings.SetQualityLevel(current, false);
            return asset;
        }

        public static void EnsureBuildScenes()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var p in Scenes) if (File.Exists(p)) list.Add(new EditorBuildSettingsScene(p, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        public static void EnsurePlayerSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.runInBackground = false;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.visibleInBackground = true;
            PlayerSettings.allowFullscreenSwitch = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11, GraphicsDeviceType.Direct3D12 });
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
        }

        /// <summary>Creates or updates one .mat per MatLib definition.</summary>
        public static int BakeMaterials()
        {
            var lit = Shader.Find("HDRP/Lit");
            var unlit = Shader.Find("HDRP/Unlit");
            if (lit == null || unlit == null)
            {
                Debug.LogWarning("[Undertow] HDRP shaders not ready yet; materials will be baked on the next setup run.");
                return 0;
            }
            Directory.CreateDirectory(MaterialRoot);
            int n = 0;
            foreach (var d in MatLib.Defs)
            {
                string path = MaterialRoot + "/" + d.Name + ".mat";
                var shader = d.Unlit ? unlit : lit;
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                bool fresh = m == null;
                if (fresh) m = new Material(shader) { name = d.Name };
                else if (m.shader != shader) m.shader = shader;
                MatLib.Configure(m, d, LoadTexture);
                if (fresh) AssetDatabase.CreateAsset(m, path);
                else EditorUtility.SetDirty(m);
                n++;
            }
            AssetDatabase.SaveAssets();
            MatLib.ClearCache();
            return n;
        }

        static Texture2D LoadTexture(string name)
        {
            foreach (var ext in new[] { ".jpg", ".png", ".tga" })
            {
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureRoot + "/" + name + ext);
                if (t != null) return t;
            }
            return null;
        }
    }
}
#endif
