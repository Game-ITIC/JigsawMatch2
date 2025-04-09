using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

public class SettingsEditorWindow : OdinMenuEditorWindow
{
    // Opens the window from the Tools menu
    [MenuItem("Tools/Settings Editor")]
    private static void OpenWindow()
    {
        // This will create (or focus) the window
        GetWindow<SettingsEditorWindow>("Settings Editor").Show();
    }

    // Build the Odin menu tree
    protected override OdinMenuTree BuildMenuTree()
    {
        // Create a new tree; passing 'true' enables drag & drop support, among other things.
        var tree = new OdinMenuTree(true)
        {
            // Enable the built-in search toolbar
            Config =
            {
                DrawSearchToolbar = true,
                // Optionally set the icon size or other styling options here.
            }
        };

        // Add all ScriptableObjects under the folder "Assets/ScriptableObjects" recursively.
        // The first parameter ("Scriptable Objects") is the root label in the tree.
        // If your assets are organized in subfolders (e.g., "Player", "Level", etc.),
        // Odin will automatically group them according to the folder structure.
        tree.AddAllAssetsAtPath("Scriptable Objects", "Assets/Content/Configs", typeof(ScriptableObject), true, false);

        return tree;
    }

    // (Optional) If you want to add a custom toolbar or header above the selected editor area,
    // override this method.
    protected override void OnBeginDrawEditors()
    {
        SirenixEditorGUI.Title("Settings Editor", "Edit your ScriptableObject settings", TextAlignment.Left, true);
        GUILayout.Space(5);
        base.OnBeginDrawEditors();
    }
}