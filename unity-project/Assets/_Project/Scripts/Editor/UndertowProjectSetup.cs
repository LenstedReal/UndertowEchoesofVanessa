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
        public const int Version = 4;
        public const string HdrpPath = "Assets/_Project/Settings/HDRP/UndertowHDRPAsset_PC_Ultra.asset";
        public const string MaterialRoot = "Assets/_Project/Resources/Undertow/Materials";
        public const string TextureRoot = "Assets/_Project/Resources/Undertow/Textures";
        const string HdrpLitPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/Lit.shader";
        const string HdrpUnlitPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.shader";

        /// <summary>
        /// Built-in shaders the player needs although no asset in the build references them:
        /// all UI is created from code, so nothing pulls UI/Default in and runtime-built UI would draw magenta.
        /// </summary>
        public static readonly string[] AlwaysIncludedShaders = { "UI/Default", "Sprites/Default" };
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
            EnsureAlwaysIncludedShaders();
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

        /// <summary>
        /// Adds <see cref="AlwaysIncludedShaders"/> to Graphics > Always Included Shaders when missing.
        /// Returns how many were added.
        /// </summary>
        public static int EnsureAlwaysIncludedShaders()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (settings == null)
            {
                Debug.LogWarning("[Undertow] ProjectSettings/GraphicsSettings.asset could not be loaded; Always Included Shaders not checked.");
                return 0;
            }
            var so = new SerializedObject(settings);
            var list = so.FindProperty("m_AlwaysIncludedShaders");
            if (list == null || !list.isArray)
            {
                Debug.LogWarning("[Undertow] m_AlwaysIncludedShaders not found in GraphicsSettings; Always Included Shaders not checked.");
                return 0;
            }
            int added = 0;
            foreach (var name in AlwaysIncludedShaders)
            {
                var shader = Shader.Find(name);
                if (shader == null)
                {
                    Debug.LogWarning("[Undertow] built-in shader '" + name + "' not found.");
                    continue;
                }
                bool present = false;
                for (int i = 0; i < list.arraySize && !present; i++)
                    present = list.GetArrayElementAtIndex(i).objectReferenceValue == shader;
                if (present) continue;
                int at = list.arraySize;
                list.InsertArrayElementAtIndex(at);
                list.GetArrayElementAtIndex(at).objectReferenceValue = shader;
                added++;
            }
            if (added > 0)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                Debug.Log("[Undertow] added " + added + " shader(s) to Always Included Shaders.");
            }
            return added;
        }

        static Shader FindShader(string name, string packagePath)
        {
            var s = Shader.Find(name);
            if (s == null) s = AssetDatabase.LoadAssetAtPath<Shader>(packagePath);
            return s;
        }

        /// <summary>Creates or updates one .mat per MatLib definition.</summary>
        public static int BakeMaterials()
        {
            var lit = FindShader("HDRP/Lit", HdrpLitPath);
            var unlit = FindShader("HDRP/Unlit", HdrpUnlitPath);
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
