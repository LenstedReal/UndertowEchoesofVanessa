#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Undertow.Editor
{
    /// <summary>
    /// The scenes are intentionally empty shells: every act is assembled at runtime from its data
    /// (ContentDb), so a scene works when opened on its own and nothing in it can go stale.
    /// BuildAllScenes (build step 2) rewrites the shells and runs the project setup.
    /// </summary>
    public static class UndertowHDRPSceneBuilder
    {
        [MenuItem("Undertow/Rebuild Scene Shells")]
        public static void BuildAllScenes()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (var path in UndertowProjectSetup.Scenes)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
            }
            UndertowProjectSetup.Run(false);
            EditorSceneManager.OpenScene(UndertowProjectSetup.Scenes[0], OpenSceneMode.Single);
            Debug.Log("[Undertow] scene shells written: " + UndertowProjectSetup.Scenes.Length);
        }

        [MenuItem("Undertow/Open Scene/Main Menu")] static void OpenMenu() => Open(0);
        [MenuItem("Undertow/Open Scene/Act I")] static void OpenA1() => Open(1);
        [MenuItem("Undertow/Open Scene/Act II")] static void OpenA2() => Open(2);
        [MenuItem("Undertow/Open Scene/Act III")] static void OpenA3() => Open(3);
        [MenuItem("Undertow/Open Scene/Act IV")] static void OpenA4() => Open(4);
        [MenuItem("Undertow/Open Scene/Endings")] static void OpenEnd() => Open(5);

        static void Open(int i)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(UndertowProjectSetup.Scenes[i], OpenSceneMode.Single);
        }
    }
}
#endif
