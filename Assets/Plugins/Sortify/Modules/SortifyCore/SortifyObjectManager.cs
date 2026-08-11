#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sortify
{
    public static class SortifyObjectManager
    {
        public enum StyleType
        {
            Default,
            Header
        }

        private const string OBJECTS_FILE_NAME = "Sortify_Objects.json";

        private static Color _backgroundColor = new Color(0.22f, 0.22f, 0.22f, 1f);

        private static Dictionary<GameObject, Color> _cachedColors = new Dictionary<GameObject, Color>();
        private static Dictionary<GameObject, StyleType> _cachedStyles = new Dictionary<GameObject, StyleType>();
        private static Dictionary<GameObject, bool> _cachedShowComponents = new Dictionary<GameObject, bool>();

        private static bool _dataLoaded = false;

        static SortifyObjectManager()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

#if UNITY_2019_1_OR_NEWER
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
#endif
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            SaveObjectsToFile();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            LoadObjectsDataFromFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            _dataLoaded = false;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                SaveObjectsToFile();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                LoadObjectsDataFromFile();
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            SaveObjectsToFile();
        }

        private static void OnAfterAssemblyReload()
        {
            _dataLoaded = false;
            LoadDataIfNeeded();
            EditorApplication.RepaintHierarchyWindow();
        }

        #region Colors
        public static Color? LoadColor(GameObject obj)
        {
            LoadDataIfNeeded();
            return _cachedColors.TryGetValue(obj, out Color color) ? (Color?)color : null;
        }

        public static void SaveColor(GameObject obj, Color color)
        {
            LoadDataIfNeeded();
            _cachedColors[obj] = color;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ResetColor(GameObject obj)
        {
            LoadDataIfNeeded();
            _cachedColors.Remove(obj);
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void DrawColor(GameObject obj, Rect selectionRect)
        {
            if (_cachedColors.TryGetValue(obj, out Color color))
            {
                color.a = 0.3f;
                selectionRect.xMin -= 28f;
                selectionRect.xMax += 20f;
                EditorGUI.DrawRect(selectionRect, color);
            }
        }
        #endregion

        #region Styles
        public static StyleType LoadStyleType(GameObject obj)
        {
            LoadDataIfNeeded();
            return _cachedStyles.TryGetValue(obj, out StyleType style) ? style : StyleType.Default;
        }

        public static void SaveStyleType(GameObject obj, StyleType styleType)
        {
            LoadDataIfNeeded();
            _cachedStyles[obj] = styleType;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void ResetStyleType(GameObject obj)
        {
            LoadDataIfNeeded();
            _cachedStyles.Remove(obj);
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }

        public static void DrawStyleType(GameObject obj, Rect selectionRect)
        {
            var styleType = LoadStyleType(obj);
            switch (styleType)
            {
                case StyleType.Default:
                    break;
                case StyleType.Header:
                    DrawHeader(obj, _cachedColors.TryGetValue(obj, out var color) ? (Color?)color : null, selectionRect);
                    break;
            }
        }

        private static readonly GUIContent _foldoutClosedIcon = EditorGUIUtility.IconContent("IN foldout");
        private static readonly GUIContent _foldoutOpenedIcon = EditorGUIUtility.IconContent("IN foldout act");

        private static void DrawHeader(GameObject obj, Color? color, Rect selectionRect)
        {
            if (obj == null)
                return;

            Rect headerRect = new Rect(selectionRect);
            headerRect.xMin -= 28f;
            headerRect.xMax += 20f;

            Color objectColor = color ?? _backgroundColor;
            EditorGUI.DrawRect(headerRect, objectColor);

            bool hasChildren = obj.transform.childCount > 0;
            bool isExpanded = false;

            if (hasChildren)
            {
                isExpanded = SortifyHierarchyHelper.IsExpanded(obj);
                Rect foldoutRect = new Rect(headerRect.xMin + 6f, headerRect.y + (headerRect.height - 12f) / 2f, 12f, 12f);
                GUIContent icon = isExpanded ? _foldoutOpenedIcon : _foldoutClosedIcon;
                if (icon != null && icon.image != null)
                {
                    Color prevColor = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, 0.85f);
                    GUI.DrawTexture(foldoutRect, icon.image, ScaleMode.ScaleToFit);
                    GUI.color = prevColor;
                }
            }

            GUIStyle centerStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 2),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            EditorGUI.LabelField(headerRect, obj.name, centerStyle);

            Event current = Event.current;
            if (current != null && current.type == EventType.MouseDown && current.button == 0)
            {
                bool isPrefab = PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab;
                float rightMargin = 4f + (isPrefab ? 10f : 0f) + 14f;
                if (SortifyUserDataManager.GetUserSetting("ShowComponentIcons", true) && LoadShowComponents(obj, true))
                {
                    Component[] components = obj.GetComponents<Component>();
                    rightMargin += components.Length * (14f + 4f);
                }

                float rightButtonsStartX = selectionRect.xMax - rightMargin;
                Rect clickRect = new Rect(headerRect.xMin, headerRect.yMin, Mathf.Max(0, rightButtonsStartX - headerRect.xMin), headerRect.height);

                if (clickRect.Contains(current.mousePosition))
                {
                    if (hasChildren)
                    {
                        bool recursive = current.alt;
                        SortifyHierarchyHelper.ToggleExpanded(obj, recursive);
                    }

                    if (current.control || current.command)
                    {
                        var currentSelected = new List<GameObject>(Selection.gameObjects);
                        if (currentSelected.Contains(obj))
                            currentSelected.Remove(obj);
                        else
                            currentSelected.Add(obj);
                        Selection.objects = currentSelected.ToArray();
                    }
                    else if (current.shift)
                    {
                        var currentSelected = new List<GameObject>(Selection.gameObjects);
                        if (!currentSelected.Contains(obj))
                            currentSelected.Add(obj);
                        Selection.objects = currentSelected.ToArray();
                    }
                    else
                    {
                        Selection.activeGameObject = obj;
                    }

                    if (current.clickCount == 2 && SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.FrameSelected();
                    }

                    current.Use();
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
        #endregion

        #region ShowComponents
        public static bool LoadShowComponents(GameObject obj, bool defaultValue = true)
        {
            LoadDataIfNeeded();
            return _cachedShowComponents.TryGetValue(obj, out bool show) ? show : defaultValue;
        }

        public static void SaveShowComponents(GameObject obj, bool value)
        {
            LoadDataIfNeeded();
            _cachedShowComponents[obj] = value;
            SaveObjectsToFile();
            EditorApplication.RepaintHierarchyWindow();
        }
        #endregion

        public static void LoadDataIfNeeded(bool forceLoad = false)
        {
            if (forceLoad)
                _dataLoaded = false;

            if (!_dataLoaded)
            {
                LoadObjectsDataFromFile();
                _dataLoaded = true;
            }
        }

        private static void SaveObjectsToFile()
        {
            var colorsData = new List<CustomColorItem>();
            foreach (var item in _cachedColors)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    var color = ColorUtility.ToHtmlStringRGBA(item.Value);
                    colorsData.Add(new CustomColorItem(objID, color));
                }
            }

            var stylesData = new List<CustomStyleItem>();
            foreach (var item in _cachedStyles)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    stylesData.Add(new CustomStyleItem(objID, item.Value));
                }
            }

            var showComponentsData = new List<CustomShowComponentsItem>();
            foreach (var item in _cachedShowComponents)
            {
                if (item.Key != null)
                {
                    var objID = SortifyHelper.GetObjectID(item.Key);
                    showComponentsData.Add(new CustomShowComponentsItem(objID, item.Value));
                }
            }

            var currentSceneName = SceneManager.GetActiveScene().name;
            var currentSceneData = new CustomObjectData(colorsData, stylesData, showComponentsData, currentSceneName);
            var loadedData = SortifyFileManager.LoadFromFile<ObjectsDataSerializer>(OBJECTS_FILE_NAME) ?? new ObjectsDataSerializer();
            loadedData.objectsData ??= new List<CustomObjectData>();
            loadedData.objectsData.RemoveAll(data => data.sceneName == currentSceneName);
            loadedData.objectsData.Add(currentSceneData);
            SortifyFileManager.SaveToFile(OBJECTS_FILE_NAME, loadedData);
        }


        private static void LoadObjectsDataFromFile()
        {
            _cachedColors.Clear();
            _cachedStyles.Clear();
            _cachedShowComponents.Clear();

            var loadedData = SortifyFileManager.LoadFromFile<ObjectsDataSerializer>(OBJECTS_FILE_NAME);
            if (loadedData == null || loadedData.objectsData == null)
                return;

            var currentSceneName = SceneManager.GetActiveScene().name;
            var currentSceneData = loadedData.objectsData.FirstOrDefault(data => data.sceneName == currentSceneName);

            if (currentSceneData != null)
            {
                if (currentSceneData.colors != null)
                {
                    foreach (var item in currentSceneData.colors)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null && ColorUtility.TryParseHtmlString("#" + item.colorValue, out Color color))
                            _cachedColors[obj] = color;
                    }
                }

                if (currentSceneData.styles != null)
                {
                    foreach (var item in currentSceneData.styles)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null)
                            _cachedStyles[obj] = item.styleType;
                    }
                }

                if (currentSceneData.showComponents != null)
                {
                    foreach (var item in currentSceneData.showComponents)
                    {
                        var obj = SortifyHelper.FindObjectByID(item.objID);
                        if (obj != null)
                            _cachedShowComponents[obj] = item.show;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class CustomColorItem
    {
        public string objID;
        public string colorValue;

        public CustomColorItem() { }
        public CustomColorItem(string objID, string colorValue)
        {
            this.objID = objID;
            this.colorValue = colorValue;
        }
    }

    [System.Serializable]
    public class CustomStyleItem
    {
        public string objID;
        public SortifyObjectManager.StyleType styleType;
        public CustomStyleItem() { }
        public CustomStyleItem(string objID, SortifyObjectManager.StyleType styleType)
        {
            this.objID = objID;
            this.styleType = styleType;
        }
    }

    [System.Serializable]
    public class CustomShowComponentsItem
    {
        public string objID;
        public bool show;

        public CustomShowComponentsItem() { }
        public CustomShowComponentsItem(string objID, bool show)
        {
            this.objID = objID;
            this.show = show;
        }
    }

    [System.Serializable]
    public class CustomObjectData
    {
        public List<CustomColorItem> colors;
        public List<CustomStyleItem> styles;
        public List<CustomShowComponentsItem> showComponents;
        public string sceneName;

        public CustomObjectData() { }
        public CustomObjectData(List<CustomColorItem> colors, List<CustomStyleItem> styles, List<CustomShowComponentsItem> showComponents, string sceneName)
        {
            this.colors = colors;
            this.styles = styles;
            this.showComponents = showComponents;
            this.sceneName = sceneName;
        }
    }

    [System.Serializable]
    public class ObjectsDataSerializer
    {
        public List<CustomObjectData> objectsData;

        public ObjectsDataSerializer() { }
        public ObjectsDataSerializer(List<CustomObjectData> objectsData)
        {
            this.objectsData = objectsData;
        }
    }
}
#endif
