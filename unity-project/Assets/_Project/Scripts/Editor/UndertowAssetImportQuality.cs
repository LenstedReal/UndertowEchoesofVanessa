#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Undertow.Editor
{
    /// <summary>Enforces source-asset import settings for the 4K HDRP target.</summary>
    public sealed class UndertowAssetImportQuality : AssetPostprocessor
    {
        private const string TextureRoot = "Assets/_Project/Textures/";
        private const string MeshRoot = "Assets/_Project/Art/Meshes/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TextureRoot)) return;
            var importer = (TextureImporter)assetImporter;
            string file = Path.GetFileNameWithoutExtension(assetPath);
            bool normal = file.EndsWith("_Normal");
            bool mask = file.EndsWith("_Mask");
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.streamingMipmapsPriority = 0;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normal && !mask;
            importer.alphaIsTransparency = file.Contains("TornBlackSail") && !normal && !mask;
        }

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(MeshRoot)) return;
            var importer = (ModelImporter)assetImporter;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.normalSmoothingAngle = 60f;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.weldVertices = true;
            importer.isReadable = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importAnimation = false;
        }

        [MenuItem("Undertow/Configure 4K Texture & Mesh Importers")]
        public static void ConfigureAllProjectAssets()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Textures" });
            int textures = 0;
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                string file = Path.GetFileNameWithoutExtension(path);
                bool normal = file.EndsWith("_Normal");
                bool mask = file.EndsWith("_Mask");
                importer.maxTextureSize = 4096;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.compressionQuality = 100;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 8;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal && !mask;
                importer.SaveAndReimport();
                textures++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Undertow] Configured {textures} source textures for 4K HDRP import (4096 max, HQ compression, mip streaming, correct color spaces).");
        }
    }
}
#endif
