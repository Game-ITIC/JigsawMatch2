#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extensions
{
    [InitializeOnLoad]
    public static class BootstrapSceneLoader
    {
        private const string BootstrapScenePath = "Assets/_Scenes/Bootstrap.unity";
        private const string BootstrapSceneName = "Bootstrap";
        private const string MenuSceneName = "game";
        private const string GameSceneName = "JapanRegion";

        static BootstrapSceneLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                {
                    if(SceneManager.GetActiveScene().name == GameSceneName || SceneManager.GetActiveScene().name == MenuSceneName)
                    {
                        var bootstrapSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

                        if(bootstrapSceneAsset != null)
                        {
                            EditorSceneManager.playModeStartScene = bootstrapSceneAsset;
                        }
                        else
                        {
                            Debug.LogError($"Bootstrap scene asset not found at path: {BootstrapScenePath}");
                        }
                    }
                    else
                    {
                        EditorSceneManager.playModeStartScene = null;
                    }

                    break;
                }
                case PlayModeStateChange.EnteredPlayMode:
                    EditorSceneManager.playModeStartScene = null;
                    break;
            }
        }
    }
}
#endif