using UnityEngine;

public enum BoardMotionStyle
{
    None,
    Flutter,
    VerticalFloat,
    GentleSway,
    ElasticBounce,
    SoftSink,
}

public enum ItemMotionStyle
{
    None,
    SoftPop,
    CrispTap,
    SquashStretch,
    FloatUp,
    Wobble,
}

public enum SelectionAuraStyle
{
    Still,
    Breathe,
    Heartbeat,
    SlowSpin,
}

public enum GlowReleaseStyle
{
    Fade,
    Bloom,
    FloatAway,
    Snap,
}

public enum ChainRhythmStyle
{
    Quiet,
    Milestones,
    EveryThird,
    Rising,
}

/// <summary>
/// Serializable snapshot of all tunables that define one gameplay-feel profile.
/// </summary>
[System.Serializable]
public struct GameFeelPresetData
{
    public bool enableHaptics;
    public bool enableCameraShake;
    public bool enableBoardShake;
    public bool enableFreezeFrames;
    public bool enableItemPunch;
    public bool enableSelectionGlow;
    public bool enableBoardPulseOnMatch;

    public BoardMotionStyle boardMotionStyle;
    public ItemMotionStyle itemMotionStyle;
    public SelectionAuraStyle selectionAuraStyle;
    public GlowReleaseStyle glowReleaseStyle;
    public ChainRhythmStyle chainRhythmStyle;

    public float globalIntensity;
    public float matchShakeStrength;
    public float boardShakeStrength;
    public float comboShakeStrength;
    public float winShakeStrength;
    public float itemPulseStrength;
    public float boardPulseStrength;
    public float matchResponseMultiplier;

    public float selectionGlowAlpha;
    public float selectionGlowScale;
    public float selectionGlowBreathing;
    public float selectionGlowWaveSpeed;
    public float glowReleasePopScale;

    public Color glowColorStart;
    public Color glowColorEnd;

    public float motionDurationScale;
    public float itemPulseDurationScale;
    public float cameraShakeFrequency;
    public int comboTriggerAt;

}
