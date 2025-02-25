using Sirenix.OdinInspector;

[System.Serializable]
public class GoalData
{
    [LabelWidth(80)] public GoalType goalType;
    [LabelWidth(80)] public string goalParam;
    [LabelWidth(80)] public int targetValue;
}