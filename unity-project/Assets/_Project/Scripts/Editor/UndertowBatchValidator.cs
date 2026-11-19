#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Undertow.Editor
{
    public static class UndertowBatchValidator
    {
        public static readonly string[] RequiredScenePaths =
        {
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/ActI_MercifulRequiem.unity",
            "Assets/_Project/Scenes/ActII_ShipGraveyard.unity",
            "Assets/_Project/Scenes/ActIII_ForsakenPort.unity",
            "Assets/_Project/Scenes/ActIV_SunkenTemple.unity",
            "Assets/_Project/Scenes/Endings_Resolution.unity"
        };

        public static readonly string[] RequiredMeshAssetPaths =
        {
            "Assets/_Project/Art/Meshes/SM_AntiqueBrassLantern.obj",
            "Assets/_Project/Art/Meshes/SM_ShipBell.obj",
            "Assets/_Project/Art/Meshes/SM_SurgeonsHook.obj",
            "Assets/_Project/Art/Meshes/SM_VanessaMusicBox.obj",
            "Assets/_Project/Art/Meshes/SM_GalleonHullDeck.obj",
            "Assets/_Project/Art/Meshes/SM_GalleonMastAndSails.obj",
            "Assets/_Project/Art/Meshes/SM_StarboardDeckRing.obj",
            "Assets/_Project/Art/Meshes/SM_ClockTowerDial.obj",
            "Assets/_Project/Art/Meshes/SM_SunkenTempleColumnAndMirror.obj",
            "Assets/_Project/Art/Meshes/SM_PaleLeviathanWhale.obj"
        };

        public static readonly string[] RequiredTexturePaths =
        {
            "T_AntiqueBrass_Albedo.png",
            "T_AntiqueBrass_Mask.png",
            "T_AntiqueBrass_Normal.png",
            "T_StormOcean_Albedo.png",
            "T_StormOcean_Mask.png",
            "T_StormOcean_Normal.png",
            "T_SunkenTempleStone_Albedo.png",
            "T_SunkenTempleStone_Mask.png",
            "T_SunkenTempleStone_Normal.png",
            "T_TornBlackSail_Albedo.png",
            "T_TornBlackSail_Mask.png",
            "T_TornBlackSail_Normal.png",
            "T_UnderwaterWater_Albedo.png",
            "T_UnderwaterWater_Mask.png",
            "T_UnderwaterWater_Normal.png",
            "T_WetAgedDeckWood_Albedo.png",
            "T_WetAgedDeckWood_Mask.png",
            "T_WetAgedDeckWood_Normal.png"
        };

        public static void ValidateAll()
        {
            AssetDatabase.Refresh();
            int errors = 0;
            foreach (string path in RequiredScenePaths)
                if (!File.Exists(path)) { Debug.LogError("Missing scene: " + path); errors++; }
            foreach (string path in RequiredMeshAssetPaths)
            {
                if (!File.Exists(path)) { Debug.LogError("Missing OBJ: " + path); errors++; continue; }
                UnityEngine.Object[] imported = AssetDatabase.LoadAllAssetsAtPath(path);
                bool hasMesh = false;
                for (int i = 0; i < imported.Length; i++) if (imported[i] is Mesh) { hasMesh = true; break; }
                if (!hasMesh) { Debug.LogError("OBJ exists but no Mesh was imported: " + path); errors++; }
            }
            foreach (string file in RequiredTexturePaths)
            {
                string path = "Assets/_Project/Textures/" + file;
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null) { Debug.LogError("Missing/unimported texture: " + path); errors++; }
                else if (texture.width < 4096 || texture.height < 4096) { Debug.LogError("Imported texture is not 4K: " + path + " (" + texture.width + "x" + texture.height + ")"); errors++; }
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.maxTextureSize < 4096 || importer.textureCompression != TextureImporterCompression.CompressedHQ || !importer.mipmapEnabled || !importer.streamingMipmaps)
                { Debug.LogError("4K/HQ/mip-streaming importer settings missing: " + path); errors++; }
                if (file.EndsWith("_Normal.png") && importer != null && importer.textureType != TextureImporterType.NormalMap)
                { Debug.LogError("Normal map imported as a color texture: " + path); errors++; }
            }
            string[] materialPaths = AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Project/Materials" });
            if (materialPaths.Length < 5) { Debug.LogError("Expected at least five authored HDRP materials."); errors++; }
            string[] requiredMaterials = { "MAT_WetAgedDeckWood_HDRP", "MAT_AntiqueBrass_HDRP", "MAT_TornBlackSail_HDRP", "MAT_SunkenTempleStone_HDRP", "MAT_LanternFlameEmissive_HDRP" };
            foreach (string materialName in requiredMaterials)
            {
                string[] ids = AssetDatabase.FindAssets(materialName + " t:Material", new[] { "Assets/_Project/Materials" });
                if (ids.Length == 0) { Debug.LogError("Missing authored material: " + materialName); errors++; continue; }
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(ids[0]));
                if (material == null || material.GetTexture("_BaseColorMap") == null && materialName != "MAT_LanternFlameEmissive_HDRP")
                { Debug.LogError("Material is missing its expected base texture: " + materialName); errors++; }
            }
            ValidateSceneHierarchies(ref errors);
            if (errors > 0) throw new Exception("Undertow validation failed: " + errors + " issue(s). See Console.");
            Debug.Log("Undertow asset existence/import validation passed. This is not a runtime or visual QA result.");
        }

        private static void ValidateSceneHierarchies(ref int errors)
        {
            foreach (string path in RequiredScenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                GameObject[] roots = scene.GetRootGameObjects();
                bool hasCamera = false, hasPlayer = false, hasMenu = false, hasEnding = false;
                foreach (GameObject root in roots)
                {
                    if (root.GetComponentInChildren<Camera>(true) != null) hasCamera = true;
                    if (root.GetComponentInChildren<Undertow.Player.FirstPersonController>(true) != null) hasPlayer = true;
                    if (root.GetComponentInChildren<Undertow.UI.UndertowMainMenuHUD>(true) != null) hasMenu = true;
                    if (root.GetComponentInChildren<Undertow.Narrative.UndertowEndingSequence>(true) != null) hasEnding = true;
                }
                bool isMenu = path.EndsWith("MainMenu.unity");
                bool isEnding = path.EndsWith("Endings_Resolution.unity");
                if (!hasCamera || (isMenu && !hasMenu) || (isEnding && !hasEnding) || (!isMenu && !isEnding && !hasPlayer))
                { Debug.LogError("Scene hierarchy validation failed: " + path + " (camera=" + hasCamera + ", player=" + hasPlayer + ", menu=" + hasMenu + ", ending=" + hasEnding + ")"); errors++; }
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [MenuItem("Undertow/Build Windows 64-bit Release Player")]
        public static void BuildWindows64Release()
        {
            ValidateAll();
            Directory.CreateDirectory("Builds/Windows64/Release");
            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = RequiredScenePaths,
                locationPathName = "Builds/Windows64/Release/UndertowEchoesOfVanessa.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.CompressWithLz4HC
            };
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Windows64 Release Build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors)");
            Debug.Log("Windows64 Release Build succeeded: " + report.summary.totalSize + " bytes.");
        }

        [MenuItem("Undertow/Build Windows 64-bit Development Player")]
        public static void BuildWindows64Development()
        {
            ValidateAll();
            Directory.CreateDirectory("Builds/Windows64");
            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = RequiredScenePaths,
                locationPathName = "Builds/Windows64/UndertowEchoesOfVanessa.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Windows64 Build failed: " + report.summary.result + " (" + report.summary.totalErrors + " errors)");
            Debug.Log("Windows64 Development Build succeeded: " + report.summary.totalSize + " bytes.");
        }
    }
}
#endif
