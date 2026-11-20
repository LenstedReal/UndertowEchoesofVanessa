#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Undertow.Editor
{
    /// <summary>
    /// Import rules for the game's generated assets: albedo maps are sRGB with mips and anisotropy, "_n" maps are
    /// normal maps, UI images stay uncompressed, audio is streamed or decompressed by length.
    /// ConfigureAllProjectAssets (build step 1) re-applies the rules to everything and bakes the HDRP materials.
    /// </summary>
    public sealed class UndertowAssetImportQuality : AssetPostprocessor
    {
        public const string TextureRoot = "Assets/_Project/Resources/Undertow/Textures/";
        public const string UiRoot = "Assets/_Project/Resources/Undertow/UI/";
        public const string AudioRoot = "Assets/_Project/Resources/Undertow/Audio/";

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            if (assetPath.StartsWith(TextureRoot))
            {
                string file = Path.GetFileNameWithoutExtension(assetPath);
                bool normal = file.EndsWith("_n");
                bool drawing = file.StartsWith("drawing") || file == "portrait_a" || file == "familytree_a";
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = normal ? 4 : 8;
                importer.wrapMode = drawing ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.isReadable = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
            }
            else if (assetPath.StartsWith(UiRoot))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
            }
        }

        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioRoot)) return;
            var importer = (AudioImporter)assetImporter;
            string file = Path.GetFileNameWithoutExtension(assetPath);
            bool longClip = file.StartsWith("amb_") || file.StartsWith("mus_") || file.StartsWith("lead_");
            var s = importer.defaultSampleSettings;
            s.loadType = longClip ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = longClip ? 0.6f : 0.75f;
            s.preloadAudioData = !longClip;
            importer.defaultSampleSettings = s;
            importer.loadInBackground = longClip;
            importer.forceToMono = false;
        }

        [MenuItem("Undertow/Configure Texture Importers and Bake Materials")]
        public static void ConfigureAllProjectAssets()
        {
            int n = 0;
            foreach (var root in new[] { TextureRoot, UiRoot, AudioRoot })
            {
                if (!Directory.Exists(root)) continue;
                foreach (var path in Directory.GetFiles(root))
                {
                    if (path.EndsWith(".meta")) continue;
                    AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);
                    n++;
                }
            }
            int mats = UndertowProjectSetup.BakeMaterials();
            Debug.Log("[Undertow] re-imported " + n + " assets, baked " + mats + " HDRP materials.");
        }
    }
}
#endif
