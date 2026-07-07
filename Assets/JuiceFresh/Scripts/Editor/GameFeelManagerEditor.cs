#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameFeelManager))]
public sealed class GameFeelManagerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty feelPreset = serializedObject.FindProperty("feelPreset");
        EditorGUILayout.PropertyField(feelPreset, new GUIContent("Feel-пресет"));

        GameFeelPresetType preset = (GameFeelPresetType)feelPreset.enumValueIndex;
        string description = GameFeelPresetLibrary.GetDescription(preset);
        if(!string.IsNullOrEmpty(description))
            EditorGUILayout.HelpBox(description, MessageType.Info);

        if(preset != GameFeelPresetType.Custom && GameFeelPresetLibrary.TryGet(preset, out GameFeelPresetData data))
        {
            EditorGUILayout.LabelField("Поведенческая подпись", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"Item: {data.itemMotionStyle}  ·  Board: {data.boardMotionStyle}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(
                $"Aura: {data.selectionAuraStyle} → {data.glowReleaseStyle}  ·  Rhythm: {data.chainRhythmStyle}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.HelpBox(
                "Появление грида настраивается отдельно на GameFieldTweenAnimation.",
                MessageType.None);
        }

        if(preset != GameFeelPresetType.Custom && GUILayout.Button("Применить feel-пресет сейчас"))
        {
            serializedObject.ApplyModifiedProperties();
            GameFeelManager manager = (GameFeelManager)target;
            manager.SetFeelPreset(preset);
            EditorUtility.SetDirty(manager);
        }

        EditorGUILayout.Space(6f);
        if(preset == GameFeelPresetType.Custom)
        {
            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "feelPreset",
                "lastFeelPreset");
        }
        else
        {
            EditorGUILayout.LabelField("Scene bindings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boardRoot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("persistAcrossScenes"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
