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
        EditorGUILayout.PropertyField(feelPreset, new GUIContent("Пресет Feel"));

        GameFeelPresetType preset = (GameFeelPresetType)feelPreset.enumValueIndex;
        string description = GameFeelPresetLibrary.GetDescription(preset);
        if(!string.IsNullOrEmpty(description))
        {
            EditorGUILayout.HelpBox(description, MessageType.Info);
        }

        if(preset != GameFeelPresetType.Custom && GameFeelPresetLibrary.TryGet(preset, out GameFeelPresetData data))
        {
            EditorGUILayout.LabelField("Отличия", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"Появление поля: {data.revealPattern}  ·  Движение доски: {data.boardMotionStyle}",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(
                $"Свечение: {(data.enableSelectionGlow ? "да" : "нет")}  ·  "
                + $"Вибрация: {(data.enableHaptics ? "да" : "нет")}  ·  "
                + $"Камера: {(data.enableCameraShake ? "да" : "нет")}",
                EditorStyles.wordWrappedMiniLabel);
        }

        if(preset != GameFeelPresetType.Custom
           && GUILayout.Button("Применить пресет сейчас"))
        {
            serializedObject.ApplyModifiedProperties();
            GameFeelManager manager = (GameFeelManager)target;
            manager.SetFeelPreset(preset);
            EditorUtility.SetDirty(manager);

            SerializedProperty fieldTween = serializedObject.FindProperty("fieldTweenAnimation");
            if(fieldTween?.objectReferenceValue is Component tweenComponent)
            {
                EditorUtility.SetDirty(tweenComponent);
            }
        }

        EditorGUILayout.Space(6f);
        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "feelPreset",
            "lastFeelPreset");

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
