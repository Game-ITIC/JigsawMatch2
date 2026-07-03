using DG.Tweening;
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

public enum BoardRevealPattern
{
    CenterOut,
    EdgeIn,
    TopToBottom,
    LeftToRight,
    Diagonal,
    RandomPop,
    AllAtOnce,
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
    public BoardRevealPattern revealPattern;

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

    public float cellStartScale;
    public float cellPeakScale;
    public float cellRiseDuration;
    public float cellSettleDuration;

    public float itemStartScale;
    public float itemPeakScale;
    public float itemDelay;
    public float itemRiseDuration;
    public float itemSettleDuration;

    public float waveStagger;
    public float waveJitter;
    public float maxRevealSpread;

    public Ease riseEase;
    public Ease settleEase;
}
