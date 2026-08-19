#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UIElements;

#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#endif

[InitializeOnLoad]
public static class SceneSwitcherToolbar
{
    public class SceneData
    {
        public string Name;
        public string Path;
        public bool IsInBuild;
    }

    private static List<SceneData> scenesList = new List<SceneData>();
    private static VisualElement toolbarUI;
    private static bool needsSceneListRefresh = false;
    private static float dropdownBoxHeight = 20f;

    private static bool fetchAllScenes
    {
        get => EditorPrefs.GetBool("SceneSwitcher_FetchAllScenes", false);
        set => EditorPrefs.SetBool("SceneSwitcher_FetchAllScenes", value);
    }

#if UNITY_6000_3_OR_NEWER
    private const string k_ElementPath = "Scene Switcher Pro";
#endif

    static SceneSwitcherToolbar()
    {
        RefreshSceneList();

        EditorBuildSettings.sceneListChanged += RefreshSceneList;
        EditorApplication.projectChanged += RefreshSceneList;

        EditorSceneManager.activeSceneChangedInEditMode += (prev, current) => {
            RefreshSceneList();
#if UNITY_6000_3_OR_NEWER
            RefreshMainToolbar();
#endif
        };
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        needsSceneListRefresh = true;
        EditorApplication.delayCall += AddToolbarUI;
    }

#if UNITY_6000_3_OR_NEWER
    [InitializeOnLoadMethod]
    private static void ShowWelcomePopup()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("SceneSwitcherToolbar_HasShownWelcomePopup_63", false))
            {
                EditorPrefs.SetBool("SceneSwitcherToolbar_HasShownWelcomePopup_63", true);
                ToolbarWelcomeWindow.ShowWindow();
            }
        };
    }

    public class ToolbarWelcomeWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            var window = GetWindow<ToolbarWelcomeWindow>(true, "Scene Switcher Pro", true);
            window.minSize = new Vector2(420, 220);
            window.maxSize = new Vector2(420, 220);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(15);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            GUILayout.Label("Scene Switcher Pro", headerStyle);
            GUILayout.Label("(Unity 6 Integration Active)", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter });

            EditorGUILayout.Space(10);

            GUIStyle bodyStyle = new GUIStyle(EditorStyles.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter, wordWrap = true, richText = true };
            GUILayout.Label("The Scene Switcher toolbar button is active on your top Unity toolbar.\n\nIn Unity 6.3+, you can also access or pin it via the <b>Three Dots (⋮)</b> menu near the Play buttons.", bodyStyle);

            EditorGUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Got It!", GUILayout.Width(120), GUILayout.Height(30)))
            {
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    [MainToolbarElement(k_ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
    public static MainToolbarElement CreateSceneSelectorDropdown()
    {
        var activeSceneName = EditorSceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(activeSceneName))
            activeSceneName = "Untitled";

        var iconContent = EditorGUIUtility.IconContent("SceneAsset Icon");
        var icon = iconContent != null ? iconContent.image as Texture2D : null;
        var content = new MainToolbarContent(activeSceneName, icon, "Scene Switcher Pro");
        return new MainToolbarDropdown(content, ShowDropdownMenu);
    }

    private static void ShowDropdownMenu(Rect dropDownRect)
    {
        UnityEditor.PopupWindow.Show(dropDownRect, new SceneSwitcherToolbarPopup());
    }

    internal static void RefreshMainToolbar()
    {
        try
        {
            MainToolbar.Refresh(k_ElementPath);
        }
        catch { }
    }
#endif

    static void AddToolbarUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;

        var toolbar = toolbars[0];
        var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rootField == null) return;

        var root = rootField.GetValue(toolbar) as VisualElement;
        if (root == null) return;

        // Multi-version visual element lookup for Unity 2020 / 2021 / 2022 / 2023 / Unity 6
        var container = root.Q("ToolbarZoneLeftAlign")
                     ?? root.Q("ToolbarZoneRightAlign")
                     ?? root.Q("ToolbarZonePlayMode")
                     ?? root.Q("ToolbarZoneCenter")
                     ?? root.Q("unity-toolbar-left-content")
                     ?? root.Q("unity-toolbar-right-content")
                     ?? root.Q("unity-toolbar-center");

        if (container == null && root.childCount > 0)
        {
            container = root[0];
        }

        if (container == null) return;

        if (toolbarUI != null && container.Contains(toolbarUI))
        {
            container.Remove(toolbarUI);
        }

        toolbarUI = new IMGUIContainer(OnGUI);
        container.Add(toolbarUI);
    }

    static void OnGUI()
    {
        if (needsSceneListRefresh)
        {
            RefreshSceneList();
            needsSceneListRefresh = false;
        }

        bool isPlaying = EditorApplication.isPlaying; 
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(isPlaying);

        GUIStyle popupStyle = new GUIStyle(EditorStyles.popup)
        {
            fixedHeight = dropdownBoxHeight
        };

        string fullName = EditorSceneManager.GetActiveScene().name;
        if (string.IsNullOrEmpty(fullName)) fullName = "Untitled";

        string truncName = (fullName.Length > 18) ? (fullName.Substring(0, 15) + "...") : fullName;
        GUIContent buttonContent = new GUIContent(truncName, $"Active Scene: {fullName}");

        Rect buttonRect = GUILayoutUtility.GetRect(buttonContent, popupStyle, GUILayout.Width(145), GUILayout.Height(dropdownBoxHeight));

        if (GUI.Button(buttonRect, buttonContent, popupStyle))
        {
            UnityEditor.PopupWindow.Show(buttonRect, new SceneSwitcherToolbarPopup());
        }
        
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
    }

    internal static void RepaintToolbar()
    {
        if (toolbarUI != null)
            toolbarUI.MarkDirtyRepaint();
    }

    static void RefreshSceneList()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        scenesList.Clear();

        if (fetchAllScenes)
        {
            var buildPaths = new HashSet<string>(
                EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path.Replace('\\', '/'))
            );

            var sceneFiles = Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories);
            foreach (var file in sceneFiles)
            {
                string normPath = file.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(normPath);
                scenesList.Add(new SceneData
                {
                    Name = name,
                    Path = normPath,
                    IsInBuild = buildPaths.Contains(normPath)
                });
            }
        }
        else
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && File.Exists(scene.path))
                {
                    string normPath = scene.path.Replace('\\', '/');
                    string name = Path.GetFileNameWithoutExtension(normPath);
                    scenesList.Add(new SceneData
                    {
                        Name = name,
                        Path = normPath,
                        IsInBuild = true
                    });
                }
            }
        }

        // Check if active scene is not in the list (e.g. unsaved or omitted build scene)
        string activePath = EditorSceneManager.GetActiveScene().path.Replace('\\', '/');
        if (!string.IsNullOrEmpty(activePath) && !scenesList.Any(s => s.Path == activePath))
        {
            string activeName = Path.GetFileNameWithoutExtension(activePath);
            scenesList.Insert(0, new SceneData
            {
                Name = activeName + " (not in build index)",
                Path = activePath,
                IsInBuild = false
            });
        }

        needsSceneListRefresh = false;
        RepaintToolbar();
    }

    static void OpenScene(SceneData scene)
    {
        if (scene == null || string.IsNullOrEmpty(scene.Path))
            return;

        if (!File.Exists(scene.Path))
        {
            Debug.LogWarning($"[Scene Switcher Pro] Scene file not found at: {scene.Path}");
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scene.Path);
        }
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.delayCall += () => AddToolbarUI();
        }
    }

    private class SceneSwitcherToolbarPopup : UnityEditor.PopupWindowContent
    {
        private Vector2 _scroll;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(250, 320);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.BeginVertical();

            DrawModeButtons();
            EditorGUILayout.Space(4);
            DrawSelectedScene();
            EditorGUILayout.Space(4);
            DrawSceneList();

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedScene()
        {
            string activeSceneName = EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(activeSceneName))
                activeSceneName = "Untitled";

            GUIStyle boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(4, 4, 4, 4)
            };

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageLeft
            };

            EditorGUILayout.BeginVertical(boxStyle);

            var iconContent = EditorGUIUtility.IconContent("SceneAsset Icon");
            Texture icon = iconContent != null ? iconContent.image : null;
            GUIContent content = new GUIContent(activeSceneName, icon, "Currently active scene");
            GUILayout.Label(content, labelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(22));

            EditorGUILayout.EndVertical();
        }

        private void DrawModeButtons()
        {
            bool isAll = fetchAllScenes;

            EditorGUILayout.BeginHorizontal();
            bool newAll = GUILayout.Toggle(isAll, "All Scenes", "Button", GUILayout.Height(28));
            EditorGUILayout.EndHorizontal();

            if (newAll != isAll)
            {
                fetchAllScenes = newAll;
                RefreshSceneList();
#if UNITY_6000_3_OR_NEWER
                RefreshMainToolbar();
#endif
            }
        }

        private void DrawSceneList()
        {
            EditorGUILayout.Space(4);

            string listName = fetchAllScenes ? "All Scenes" : "Build-in Scenes";
            EditorGUILayout.LabelField(listName, EditorStyles.boldLabel);

            if (scenesList == null || scenesList.Count == 0)
            {
                EditorGUILayout.LabelField("No scenes available.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            string activePath = EditorSceneManager.GetActiveScene().path.Replace('\\', '/');

            foreach (var scene in scenesList)
            {
                if (scene == null || string.IsNullOrEmpty(scene.Name)) continue;

                EditorGUILayout.BeginHorizontal();

                bool isActive = (scene.Path == activePath);
                GUIStyle sceneBtnStyle = new GUIStyle(isActive ? EditorStyles.miniButton : GUI.skin.button)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft
                };

                string displayName = scene.Name;
                if (!scene.IsInBuild && fetchAllScenes == false && !displayName.Contains("(not in build index)"))
                {
                    displayName += " (not in build index)";
                }

                if (GUILayout.Button(new GUIContent(displayName, scene.Path), sceneBtnStyle, GUILayout.ExpandWidth(true), GUILayout.Height(24)))
                {
                    OpenScene(scene);
#if UNITY_6000_3_OR_NEWER
                    RefreshMainToolbar();
#endif
                    editorWindow.Close();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif

