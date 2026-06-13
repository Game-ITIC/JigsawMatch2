using UnityEditor;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    public class SmartGuidesSettingsWindow : EditorWindow
    {
        private const string PREFS_PREFIX = "SmartGuides_";
        private const string MENU_ROOT = "Tools/Figma UI/";
        private const string ENABLE_MENU_PATH = MENU_ROOT + "Enable Smart Guides";
        
        public static bool IsEnabled
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "IsEnabled", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "IsEnabled", value);
        }

        public static bool ShowSizes
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "ShowSizes", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "ShowSizes", value);
        }

        public static bool ShowDistancesToCanvas
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "ShowDistancesToCanvas", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "ShowDistancesToCanvas", value);
        }

        public static bool ShowDistancesToObjects
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "ShowDistancesToObjects", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "ShowDistancesToObjects", value);
        }

        public static bool ShowRulers
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "ShowRulers", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "ShowRulers", value);
        }

        public static bool EnableSnapping
        {
            get => EditorPrefs.GetBool(PREFS_PREFIX + "EnableSnapping", true);
            set => EditorPrefs.SetBool(PREFS_PREFIX + "EnableSnapping", value);
        }

        public static Color GuideColor
        {
            get
            {
                string hex = EditorPrefs.GetString(PREFS_PREFIX + "GuideColor", "#FF5C00"); // Figma Orange
                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                    return color;
                return new Color(1f, 0.36f, 0f);
            }
            set => EditorPrefs.SetString(PREFS_PREFIX + "GuideColor", "#" + ColorUtility.ToHtmlStringRGBA(value));
        }

        public static Color CustomGuideColor
        {
            get
            {
                string hex = EditorPrefs.GetString(PREFS_PREFIX + "CustomGuideColor", "#00FFFF"); // Cyan
                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                    return color;
                return Color.cyan;
            }
            set => EditorPrefs.SetString(PREFS_PREFIX + "CustomGuideColor", "#" + ColorUtility.ToHtmlStringRGBA(value));
        }

        public static float LineThickness
        {
            get => EditorPrefs.GetFloat(PREFS_PREFIX + "LineThickness", 1.5f);
            set => EditorPrefs.SetFloat(PREFS_PREFIX + "LineThickness", value);
        }

        public static int FontSize
        {
            get => EditorPrefs.GetInt(PREFS_PREFIX + "FontSize", 12);
            set => EditorPrefs.SetInt(PREFS_PREFIX + "FontSize", value);
        }

        [MenuItem(MENU_ROOT + "Settings", false, 2000)]
        public static void ShowWindow()
        {
            var window = GetWindow<SmartGuidesSettingsWindow>("Figma UI Guides");
            window.minSize = new Vector2(300, 350);
            window.Show();
        }

        [MenuItem(ENABLE_MENU_PATH, false, 2001)]
        private static void ToggleSmartGuides()
        {
            IsEnabled = !IsEnabled;
            Menu.SetChecked(ENABLE_MENU_PATH, IsEnabled);
            SceneView.RepaintAll();
        }

        [MenuItem(ENABLE_MENU_PATH, true)]
        private static bool ToggleSmartGuidesValidate()
        {
            Menu.SetChecked(ENABLE_MENU_PATH, IsEnabled);
            return true;
        }

        [MenuItem(MENU_ROOT + "Clear Custom Guides", false, 2002)]
        private static void ClearCustomGuides()
        {
            SmartRulersCore.ClearAllGuides();
        }

        private void OnGUI()
        {
            GUILayout.Label("Figma UI Guides", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            bool isEnabled = EditorGUILayout.ToggleLeft("Enable Smart Guides", IsEnabled, EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            GUILayout.Label("Features", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(!isEnabled);
            
            bool showSizes = EditorGUILayout.Toggle("Show Sizes", ShowSizes);
            bool showDistancesToCanvas = EditorGUILayout.Toggle("Show Distances to Canvas", ShowDistancesToCanvas);
            bool showDistancesToObjects = EditorGUILayout.Toggle("Show Distances to Objects", ShowDistancesToObjects);
            
            EditorGUILayout.Space();
            GUILayout.Label("Rulers & Custom Guides", EditorStyles.boldLabel);
            bool showRulers = EditorGUILayout.Toggle("Show Rulers", ShowRulers);
            bool enableSnapping = EditorGUILayout.Toggle("Enable Snapping", EnableSnapping);

            if (GUILayout.Button("Clear All Custom Guides"))
            {
                SmartRulersCore.ClearAllGuides();
            }

            EditorGUILayout.Space();
            GUILayout.Label("Visuals", EditorStyles.boldLabel);
            
            Color guideColor = EditorGUILayout.ColorField("Smart Guide Color", GuideColor);
            Color customGuideColor = EditorGUILayout.ColorField("Custom Guide Color", CustomGuideColor);
            float lineThickness = EditorGUILayout.Slider("Line Thickness", LineThickness, 0.5f, 5f);
            int fontSize = EditorGUILayout.IntSlider("Font Size", FontSize, 8, 24);

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                IsEnabled = isEnabled;
                ShowSizes = showSizes;
                ShowDistancesToCanvas = showDistancesToCanvas;
                ShowDistancesToObjects = showDistancesToObjects;
                ShowRulers = showRulers;
                EnableSnapping = enableSnapping;
                GuideColor = guideColor;
                CustomGuideColor = customGuideColor;
                LineThickness = lineThickness;
                FontSize = fontSize;
                
                SceneView.RepaintAll();
            }
        }
    }
}
