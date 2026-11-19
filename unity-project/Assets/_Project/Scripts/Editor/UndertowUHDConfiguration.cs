#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Undertow.Editor
{
    public static class UndertowUHDConfiguration
    {
        [MenuItem("Undertow/Apply UHD 4K PC Profile")]
        public static void ApplyUHDPlayerProfile()
        {
            PlayerSettings.defaultScreenWidth = 3840;
            PlayerSettings.defaultScreenHeight = 2160;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);

            QualitySettings.vSyncCount = 1;
            // HDRP uses its own temporal AA mode; MSAA is intentionally disabled for the deferred path.
            QualitySettings.antiAliasing = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowProjection = ShadowProjection.StableFit;
            QualitySettings.shadowCascades = 4;
            QualitySettings.shadowDistance = 180f;
            QualitySettings.realtimeReflectionProbes = true;
            QualitySettings.softParticles = true;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsAddAllCameras = true;
            QualitySettings.streamingMipmapsMemoryBudget = 2048f;
            AssetDatabase.SaveAssets();
            Debug.Log("[Undertow] UHD PC profile set: 3840x2160 native fullscreen target, Linear, HDRP TAA (camera profile), 4-cascade very-high shadows, anisotropic filtering and 2GB texture streaming budget. 4K performance must be profiled on target GPU.");
        }
    }
}
#endif
