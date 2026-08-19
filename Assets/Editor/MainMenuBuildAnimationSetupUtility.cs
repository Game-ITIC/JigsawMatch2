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

        var regionConfig = AssetDatabase.LoadAssetAtPath<RegionConfig>("Assets/Content/Configs/RegionConfig.asset");
        if (regionConfig == null)
        {
            var guids = AssetDatabase.FindAssets("t:RegionConfig");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                regionConfig = AssetDatabase.LoadAssetAtPath<RegionConfig>(path);
            }
        }

        var serializedProvider = new SerializedObject(provider);
        var regionConfigProperty = serializedProvider.FindProperty("regionConfig");
        if (regionConfigProperty != null && regionConfig != null)
        {
            regionConfigProperty.objectReferenceValue = regionConfig;
        }

        serializedProvider.ApplyModifiedProperties();
        EditorUtility.SetDirty(provider);
        EditorSceneManager.MarkSceneDirty(provider.gameObject.scene);

        Debug.Log($"Configured MainMenu BuildingAnimationSettingsProvider with RegionConfig: {(regionConfig != null ? regionConfig.name : "null")}.", provider);
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
