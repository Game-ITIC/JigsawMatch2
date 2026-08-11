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
        private const string MainMenuSceneName = "MainMenu";
        private const string LegacyGameSceneName = "game";
        private const string RegionSceneName = "AsiaRegion";
        private const string MenuItemPath = "Tools/Bootstrap/Enable Auto Bootstrap";

        static BootstrapSceneLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuItemPath + " %t")]
        private static void ToggleAutoBootstrap()
        {
            var enabled = !BootstrapPlayModeSettings.IsPlayFromBootstrapEnabled();
            BootstrapPlayModeSettings.SetEnabled(enabled);
            Menu.SetChecked(MenuItemPath, enabled);
            ShowToggleNotification(enabled);
        }

        [MenuItem(MenuItemPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuItemPath, BootstrapPlayModeSettings.IsPlayFromBootstrapEnabled());
            return true;
        }

        private static void ShowToggleNotification(bool enabled)
        {
            var status = enabled ? "enabled" : "disabled";
            var message = $"Auto Bootstrap {status}";

            EditorWindow.focusedWindow?.ShowNotification(new GUIContent(message));
            Debug.Log(message);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                {
                    var activeScene = SceneManager.GetActiveScene().name;

                    if (BootstrapPlayModeSettings.IsPlayFromBootstrapEnabled() && ShouldStartFromBootstrap(activeScene))
                    {
                        var bootstrapSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapScenePath);

                        if (bootstrapSceneAsset != null)
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

        private static bool ShouldStartFromBootstrap(string activeSceneName)
        {
            return activeSceneName == MainMenuSceneName
                   || activeSceneName == LegacyGameSceneName
                   || activeSceneName == RegionSceneName;
        }
    }
}
#endif
