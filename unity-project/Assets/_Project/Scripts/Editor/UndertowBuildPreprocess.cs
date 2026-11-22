#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Undertow.Editor
{
    /// <summary>
    /// Runs before every player build, including Unity Build Automation. Build Automation works in batch mode,
    /// where the on-open project setup is skipped, so without this step a cloud build ships without the baked
    /// material library (HDRP Lit/Unlit get stripped and the world draws magenta) and without UI/Default
    /// (all code-built UI draws magenta).
    /// </summary>
    class UndertowBuildPreprocess : IPreprocessBuildWithReport
    {
        // before HDRP's own build preprocessor (int.MinValue + 100)
        public int callbackOrder => int.MinValue + 10;

        public void OnPreprocessBuild(BuildReport report)
        {
            int added = 0;
            try
            {
                added = UndertowProjectSetup.EnsureAlwaysIncludedShaders();
            }
            catch (System.Exception ex)
            {
                // GraphicsSettings.asset already lists them; this is the safety net, not a reason to stop the build
                Debug.LogWarning("[Undertow] could not check Always Included Shaders: " + ex.Message);
            }
            int mats = UndertowProjectSetup.BakeMaterials();
            if (mats < MatLib.Defs.Length)
                throw new BuildFailedException("[Undertow] material bake failed (" + mats + "/" + MatLib.Defs.Length +
                    "): HDRP/Lit or HDRP/Unlit was not found. A player built now would render magenta.");
            AssetDatabase.SaveAssets();
            Debug.Log("[Undertow] build preprocess: " + mats + " materials baked into Resources, " + added + " shader(s) added to Always Included Shaders.");
        }
    }
}
#endif
