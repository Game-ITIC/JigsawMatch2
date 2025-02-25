using Sirenix.OdinInspector;

[System.Serializable]
public class LevelEvent
{
    [LabelWidth(90)] public LevelEventTrigger triggerType;
    public int triggerValue;
    public LevelEventEffect effectType;
    public int effectValue;
}