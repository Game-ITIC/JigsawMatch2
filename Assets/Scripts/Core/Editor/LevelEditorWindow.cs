namespace Sirenix.OdinInspector;

public class LevelEditorWindow : UnityEditor.EditorWindow
{
    [UnityEditor.MenuItem("MENUITEM/MENUITEMCOMMAND")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>();
        window.titleContent = new UnityEngine.GUIContent("TITLE");
        window.Show();
    }

    private void CreateGUI()
    {
        
    }
}