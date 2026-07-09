using Configs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuBuildAnimationSetupUtility
{
    private const string MenuLifetimeScopeName = "MenuLifetimeScope";
    private const string WorldName = "World";

    private static readonly string[] RegionNames =
    {
        "Japan_Island_Pref",
        "Korea Environment",
        "ChinaPrefabs"
    };

    [MenuItem("Tools/JigsawMatch2/Setup MainMenu Build Animations")]
    public static void Setup()
    {
        var menuLifetimeScope = FindRoot(MenuLifetimeScopeName);

        if(menuLifetimeScope == null)
        {
            Debug.LogError($"{MenuLifetimeScopeName} was not found in the active scene.");
            return;
        }

        var provider = menuLifetimeScope.GetComponent<BuildingAnimationSettingsProvider>();

        if(provider == null)
        {
            Debug.LogError($"{nameof(BuildingAnimationSettingsProvider)} was not found on {MenuLifetimeScopeName}.", menuLifetimeScope);
            return;
        }

        var world = menuLifetimeScope.transform.Find(WorldName);

        if(world == null)
        {
            Debug.LogError($"{WorldName} was not found under {MenuLifetimeScopeName}.", menuLifetimeScope);
            return;
        }

        var configs = new BuildingsAnimationConfig[RegionNames.Length];

        for(var i = 0; i < RegionNames.Length; i++)
        {
            var region = world.Find(RegionNames[i]);

            if(region == null)
            {
                Debug.LogError($"{RegionNames[i]} was not found under {MenuLifetimeScopeName}/{WorldName}.", world);
                return;
            }

            var config = region.GetComponent<BuildingsAnimationConfig>();

            if(config == null)
            {
                Debug.LogError($"{RegionNames[i]} has no {nameof(BuildingsAnimationConfig)} component.", region);
                return;
            }

            if(config.animator == null || config.animationClip == null || config.data == null || config.data.Count == 0)
            {
                Debug.LogWarning($"{RegionNames[i]} has an incomplete build animation config.", region);
            }

            RemoveLegacyAnimationComponents(region.gameObject);
            configs[i] = config;
        }

        var serializedProvider = new SerializedObject(provider);
        var configsProperty = serializedProvider.FindProperty("buildingsAnimationConfigs");

        configsProperty.arraySize = configs.Length;

        for(var i = 0; i < configs.Length; i++)
        {
            configsProperty.GetArrayElementAtIndex(i).objectReferenceValue = configs[i];
        }

        serializedProvider.ApplyModifiedProperties();
        EditorUtility.SetDirty(provider);
        EditorSceneManager.MarkSceneDirty(provider.gameObject.scene);

        Debug.Log($"Configured MainMenu build animations: {string.Join(", ", RegionNames)}.", provider);
    }

    private static void RemoveLegacyAnimationComponents(GameObject root)
    {
        var legacyAnimations = root.GetComponentsInChildren<Animation>(true);

        for(var i = legacyAnimations.Length - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(legacyAnimations[i]);
        }
    }

    private static GameObject FindRoot(string rootName)
    {
        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();

        for(var i = 0; i < roots.Length; i++)
        {
            if(roots[i].name == rootName)
            {
                return roots[i];
            }
        }

        return null;
    }
}
