using DG.Tweening;
using UnityEngine;

/// <summary>
/// Ten clearly distinct gentle feel profiles for jar-matching gameplay.
/// </summary>
public static class GameFeelPresetLibrary
{
    public static bool TryGet(GameFeelPresetType preset, out GameFeelPresetData data)
    {
        if(preset == GameFeelPresetType.Custom)
        {
            data = default;
            return false;
        }

        data = Get(preset);
        return true;
    }

    public static GameFeelPresetData Get(GameFeelPresetType preset)
    {
        switch(preset)
        {
            case GameFeelPresetType.CottonCloud: return CottonCloud();
            case GameFeelPresetType.PetalDrift: return PetalDrift();
            case GameFeelPresetType.HoneyWarmth: return HoneyWarmth();
            case GameFeelPresetType.SilkTouch: return SilkTouch();
            case GameFeelPresetType.BubbleSoft: return BubbleSoft();
            case GameFeelPresetType.LavenderMist: return LavenderMist();
            case GameFeelPresetType.Marshmallow: return Marshmallow();
            case GameFeelPresetType.MorningDew: return MorningDew();
            case GameFeelPresetType.RoseBlush: return RoseBlush();
            case GameFeelPresetType.CozyJar: return CozyJar();
            default: return CozyJar();
        }
    }

    public static string GetDescription(GameFeelPresetType preset)
    {
        switch(preset)
        {
            case GameFeelPresetType.Custom:
                return "Ручная настройка всех параметров.";
            case GameFeelPresetType.CottonCloud:
                return "Статичный кокон: без тряски, без вибрации, поле появляется разом, едва заметное свечение.";
            case GameFeelPresetType.PetalDrift:
                return "Волна с краёв к центру, розовое дыхание свечения, медленное покачивание поля.";
            case GameFeelPresetType.HoneyWarmth:
                return "Золотое сияние, вертикальное «парение» поля, заметный пульс при совпадении.";
            case GameFeelPresetType.SilkTouch:
                return "Почти без картинки — упор на тактильную отдачу и быстрый отклик.";
            case GameFeelPresetType.BubbleSoft:
                return "Упругие отскоки, OutElastic, тряска камеры, игривый «плюх» баночек.";
            case GameFeelPresetType.LavenderMist:
                return "Сонный темп ×2: лавандовое свечение, волна из центра, парящее поле.";
            case GameFeelPresetType.Marshmallow:
                return "Большой зефирный squash & stretch баночек, упругий отскок доски.";
            case GameFeelPresetType.MorningDew:
                return "Быстрый каскад сверху вниз, короткие анимации, свежий зелёный оттенок.";
            case GameFeelPresetType.RoseBlush:
                return "Только розовое свечение — без движения поля, яркая аура при выборе.";
            case GameFeelPresetType.CozyJar:
                return "Диагональная волна, мягкий flutter, сбалансированный уютный профиль.";
            default:
                return string.Empty;
        }
    }

    private static GameFeelPresetData CottonCloud()
    {
        return new GameFeelPresetData
        {
            enableHaptics = false,
            enableCameraShake = false,
            enableBoardShake = false,
            enableFreezeFrames = false,
            enableItemPunch = false,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = false,
            boardMotionStyle = BoardMotionStyle.None,
            revealPattern = BoardRevealPattern.AllAtOnce,
            globalIntensity = 0.3f,
            matchShakeStrength = 0f,
            boardShakeStrength = 0f,
            comboShakeStrength = 0f,
            winShakeStrength = 0.03f,
            itemPulseStrength = 0f,
            boardPulseStrength = 0f,
            matchResponseMultiplier = 0f,
            selectionGlowAlpha = 0.14f,
            selectionGlowScale = 1.04f,
            selectionGlowBreathing = 0.01f,
            selectionGlowWaveSpeed = 2.8f,
            glowReleasePopScale = 1.12f,
            glowColorStart = new Color(0.97f, 0.98f, 1f, 0.14f),
            glowColorEnd = new Color(0.95f, 0.97f, 1f, 0.16f),
            motionDurationScale = 1.6f,
            itemPulseDurationScale = 1.4f,
            cameraShakeFrequency = 6f,
            comboTriggerAt = 99,
            cellStartScale = 0.85f,
            cellPeakScale = 1f,
            cellRiseDuration = 0.95f,
            cellSettleDuration = 0.35f,
            itemStartScale = 0.7f,
            itemPeakScale = 1f,
            itemDelay = 0.08f,
            itemRiseDuration = 0.85f,
            itemSettleDuration = 0.3f,
            waveStagger = 0f,
            waveJitter = 0f,
            maxRevealSpread = 0f,
            riseEase = Ease.InOutSine,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData PetalDrift()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.GentleSway,
            revealPattern = BoardRevealPattern.EdgeIn,
            globalIntensity = 0.5f,
            matchShakeStrength = 0.01f,
            boardShakeStrength = 0.04f,
            comboShakeStrength = 0.02f,
            winShakeStrength = 0.05f,
            itemPulseStrength = 0.04f,
            boardPulseStrength = 0.012f,
            matchResponseMultiplier = 0.35f,
            selectionGlowAlpha = 0.32f,
            selectionGlowScale = 1.14f,
            selectionGlowBreathing = 0.05f,
            selectionGlowWaveSpeed = 3.2f,
            glowReleasePopScale = 1.55f,
            glowColorStart = new Color(1f, 0.82f, 0.9f, 0.32f),
            glowColorEnd = new Color(1f, 0.65f, 0.78f, 0.38f),
            motionDurationScale = 1.7f,
            itemPulseDurationScale = 1.5f,
            cameraShakeFrequency = 5f,
            comboTriggerAt = 8,
            cellStartScale = 0.08f,
            cellPeakScale = 1.02f,
            cellRiseDuration = 0.88f,
            cellSettleDuration = 0.32f,
            itemStartScale = 0.05f,
            itemPeakScale = 1.03f,
            itemDelay = 0.28f,
            itemRiseDuration = 0.78f,
            itemSettleDuration = 0.3f,
            waveStagger = 0.18f,
            waveJitter = 0.002f,
            maxRevealSpread = 1.4f,
            riseEase = Ease.OutSine,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData HoneyWarmth()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.VerticalFloat,
            revealPattern = BoardRevealPattern.CenterOut,
            globalIntensity = 0.75f,
            matchShakeStrength = 0.015f,
            boardShakeStrength = 0.05f,
            comboShakeStrength = 0.035f,
            winShakeStrength = 0.1f,
            itemPulseStrength = 0.09f,
            boardPulseStrength = 0.045f,
            matchResponseMultiplier = 1.2f,
            selectionGlowAlpha = 0.42f,
            selectionGlowScale = 1.16f,
            selectionGlowBreathing = 0.045f,
            selectionGlowWaveSpeed = 4.5f,
            glowReleasePopScale = 1.42f,
            glowColorStart = new Color(1f, 0.88f, 0.55f, 0.4f),
            glowColorEnd = new Color(1f, 0.68f, 0.2f, 0.45f),
            motionDurationScale = 1.25f,
            itemPulseDurationScale = 1.1f,
            cameraShakeFrequency = 7f,
            comboTriggerAt = 6,
            cellStartScale = 0.35f,
            cellPeakScale = 1.05f,
            cellRiseDuration = 0.58f,
            cellSettleDuration = 0.2f,
            itemStartScale = 0.2f,
            itemPeakScale = 1.08f,
            itemDelay = 0.16f,
            itemRiseDuration = 0.5f,
            itemSettleDuration = 0.2f,
            waveStagger = 0.1f,
            waveJitter = 0.005f,
            maxRevealSpread = 1f,
            riseEase = Ease.OutCubic,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData SilkTouch()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = false,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = false,
            enableBoardPulseOnMatch = false,
            boardMotionStyle = BoardMotionStyle.None,
            revealPattern = BoardRevealPattern.LeftToRight,
            globalIntensity = 0.35f,
            matchShakeStrength = 0f,
            boardShakeStrength = 0f,
            comboShakeStrength = 0f,
            winShakeStrength = 0.02f,
            itemPulseStrength = 0.025f,
            boardPulseStrength = 0f,
            matchResponseMultiplier = 0f,
            selectionGlowAlpha = 0f,
            selectionGlowScale = 1f,
            selectionGlowBreathing = 0f,
            selectionGlowWaveSpeed = 5f,
            glowReleasePopScale = 1f,
            glowColorStart = Color.clear,
            glowColorEnd = Color.clear,
            motionDurationScale = 0.75f,
            itemPulseDurationScale = 0.55f,
            cameraShakeFrequency = 14f,
            comboTriggerAt = 99,
            cellStartScale = 0.55f,
            cellPeakScale = 1.01f,
            cellRiseDuration = 0.28f,
            cellSettleDuration = 0.08f,
            itemStartScale = 0.5f,
            itemPeakScale = 1.02f,
            itemDelay = 0.04f,
            itemRiseDuration = 0.24f,
            itemSettleDuration = 0.08f,
            waveStagger = 0.035f,
            waveJitter = 0f,
            maxRevealSpread = 0.6f,
            riseEase = Ease.Linear,
            settleEase = Ease.OutSine,
        };
    }

    private static GameFeelPresetData BubbleSoft()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = true,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.ElasticBounce,
            revealPattern = BoardRevealPattern.RandomPop,
            globalIntensity = 0.95f,
            matchShakeStrength = 0.09f,
            boardShakeStrength = 0.07f,
            comboShakeStrength = 0.12f,
            winShakeStrength = 0.18f,
            itemPulseStrength = 0.15f,
            boardPulseStrength = 0.055f,
            matchResponseMultiplier = 1.5f,
            selectionGlowAlpha = 0.28f,
            selectionGlowScale = 1.18f,
            selectionGlowBreathing = 0.06f,
            selectionGlowWaveSpeed = 6.5f,
            glowReleasePopScale = 1.75f,
            glowColorStart = new Color(0.7f, 0.92f, 1f, 0.28f),
            glowColorEnd = new Color(0.55f, 0.85f, 1f, 0.34f),
            motionDurationScale = 0.82f,
            itemPulseDurationScale = 0.7f,
            cameraShakeFrequency = 16f,
            comboTriggerAt = 5,
            cellStartScale = 0.05f,
            cellPeakScale = 1.12f,
            cellRiseDuration = 0.38f,
            cellSettleDuration = 0.28f,
            itemStartScale = 0.02f,
            itemPeakScale = 1.2f,
            itemDelay = 0.06f,
            itemRiseDuration = 0.34f,
            itemSettleDuration = 0.26f,
            waveStagger = 0.06f,
            waveJitter = 0.025f,
            maxRevealSpread = 0.55f,
            riseEase = Ease.OutElastic,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData LavenderMist()
    {
        return new GameFeelPresetData
        {
            enableHaptics = false,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.VerticalFloat,
            revealPattern = BoardRevealPattern.CenterOut,
            globalIntensity = 0.55f,
            matchShakeStrength = 0.008f,
            boardShakeStrength = 0.025f,
            comboShakeStrength = 0.02f,
            winShakeStrength = 0.06f,
            itemPulseStrength = 0.05f,
            boardPulseStrength = 0.018f,
            matchResponseMultiplier = 0.6f,
            selectionGlowAlpha = 0.36f,
            selectionGlowScale = 1.1f,
            selectionGlowBreathing = 0.03f,
            selectionGlowWaveSpeed = 2.2f,
            glowReleasePopScale = 1.35f,
            glowColorStart = new Color(0.82f, 0.72f, 1f, 0.36f),
            glowColorEnd = new Color(0.68f, 0.55f, 0.95f, 0.4f),
            motionDurationScale = 2.1f,
            itemPulseDurationScale = 1.8f,
            cameraShakeFrequency = 4f,
            comboTriggerAt = 10,
            cellStartScale = 0.12f,
            cellPeakScale = 1.02f,
            cellRiseDuration = 1.15f,
            cellSettleDuration = 0.42f,
            itemStartScale = 0.08f,
            itemPeakScale = 1.04f,
            itemDelay = 0.35f,
            itemRiseDuration = 1.05f,
            itemSettleDuration = 0.4f,
            waveStagger = 0.22f,
            waveJitter = 0.003f,
            maxRevealSpread = 1.8f,
            riseEase = Ease.InOutSine,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData Marshmallow()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.ElasticBounce,
            revealPattern = BoardRevealPattern.Diagonal,
            globalIntensity = 0.88f,
            matchShakeStrength = 0.02f,
            boardShakeStrength = 0.055f,
            comboShakeStrength = 0.06f,
            winShakeStrength = 0.12f,
            itemPulseStrength = 0.18f,
            boardPulseStrength = 0.065f,
            matchResponseMultiplier = 1.35f,
            selectionGlowAlpha = 0.26f,
            selectionGlowScale = 1.2f,
            selectionGlowBreathing = 0.055f,
            selectionGlowWaveSpeed = 5.8f,
            glowReleasePopScale = 1.65f,
            glowColorStart = new Color(1f, 0.94f, 0.98f, 0.26f),
            glowColorEnd = new Color(1f, 0.78f, 0.88f, 0.32f),
            motionDurationScale = 0.95f,
            itemPulseDurationScale = 0.85f,
            cameraShakeFrequency = 9f,
            comboTriggerAt = 6,
            cellStartScale = 0.04f,
            cellPeakScale = 1.1f,
            cellRiseDuration = 0.46f,
            cellSettleDuration = 0.32f,
            itemStartScale = 0.02f,
            itemPeakScale = 1.16f,
            itemDelay = 0.1f,
            itemRiseDuration = 0.42f,
            itemSettleDuration = 0.3f,
            waveStagger = 0.09f,
            waveJitter = 0.008f,
            maxRevealSpread = 1.1f,
            riseEase = Ease.OutBack,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData MorningDew()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.SoftSink,
            revealPattern = BoardRevealPattern.TopToBottom,
            globalIntensity = 0.65f,
            matchShakeStrength = 0.012f,
            boardShakeStrength = 0.03f,
            comboShakeStrength = 0.025f,
            winShakeStrength = 0.07f,
            itemPulseStrength = 0.035f,
            boardPulseStrength = 0.015f,
            matchResponseMultiplier = 0.5f,
            selectionGlowAlpha = 0.2f,
            selectionGlowScale = 1.07f,
            selectionGlowBreathing = 0.025f,
            selectionGlowWaveSpeed = 7f,
            glowReleasePopScale = 1.25f,
            glowColorStart = new Color(0.75f, 0.98f, 0.88f, 0.2f),
            glowColorEnd = new Color(0.55f, 0.9f, 0.75f, 0.24f),
            motionDurationScale = 0.55f,
            itemPulseDurationScale = 0.5f,
            cameraShakeFrequency = 12f,
            comboTriggerAt = 7,
            cellStartScale = 0.5f,
            cellPeakScale = 1.03f,
            cellRiseDuration = 0.22f,
            cellSettleDuration = 0.08f,
            itemStartScale = 0.4f,
            itemPeakScale = 1.04f,
            itemDelay = 0.03f,
            itemRiseDuration = 0.2f,
            itemSettleDuration = 0.07f,
            waveStagger = 0.045f,
            waveJitter = 0.002f,
            maxRevealSpread = 0.7f,
            riseEase = Ease.OutQuad,
            settleEase = Ease.OutSine,
        };
    }

    private static GameFeelPresetData RoseBlush()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = false,
            enableFreezeFrames = false,
            enableItemPunch = false,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = false,
            boardMotionStyle = BoardMotionStyle.None,
            revealPattern = BoardRevealPattern.CenterOut,
            globalIntensity = 0.7f,
            matchShakeStrength = 0f,
            boardShakeStrength = 0f,
            comboShakeStrength = 0f,
            winShakeStrength = 0.04f,
            itemPulseStrength = 0f,
            boardPulseStrength = 0f,
            matchResponseMultiplier = 0f,
            selectionGlowAlpha = 0.52f,
            selectionGlowScale = 1.22f,
            selectionGlowBreathing = 0.065f,
            selectionGlowWaveSpeed = 4f,
            glowReleasePopScale = 1.85f,
            glowColorStart = new Color(1f, 0.75f, 0.85f, 0.52f),
            glowColorEnd = new Color(1f, 0.5f, 0.68f, 0.58f),
            motionDurationScale = 1.3f,
            itemPulseDurationScale = 1.2f,
            cameraShakeFrequency = 6f,
            comboTriggerAt = 99,
            cellStartScale = 0.4f,
            cellPeakScale = 1.02f,
            cellRiseDuration = 0.65f,
            cellSettleDuration = 0.22f,
            itemStartScale = 0.35f,
            itemPeakScale = 1.03f,
            itemDelay = 0.12f,
            itemRiseDuration = 0.55f,
            itemSettleDuration = 0.2f,
            waveStagger = 0.13f,
            waveJitter = 0.004f,
            maxRevealSpread = 1.2f,
            riseEase = Ease.OutSine,
            settleEase = Ease.InOutSine,
        };
    }

    private static GameFeelPresetData CozyJar()
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = true,
            enableFreezeFrames = false,
            enableItemPunch = true,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = BoardMotionStyle.Flutter,
            revealPattern = BoardRevealPattern.Diagonal,
            globalIntensity = 0.62f,
            matchShakeStrength = 0.022f,
            boardShakeStrength = 0.038f,
            comboShakeStrength = 0.048f,
            winShakeStrength = 0.09f,
            itemPulseStrength = 0.07f,
            boardPulseStrength = 0.028f,
            matchResponseMultiplier = 0.85f,
            selectionGlowAlpha = 0.28f,
            selectionGlowScale = 1.11f,
            selectionGlowBreathing = 0.032f,
            selectionGlowWaveSpeed = 5.2f,
            glowReleasePopScale = 1.45f,
            glowColorStart = new Color(0.88f, 0.95f, 1f, 0.28f),
            glowColorEnd = new Color(1f, 0.86f, 0.62f, 0.32f),
            motionDurationScale = 1.1f,
            itemPulseDurationScale = 1f,
            cameraShakeFrequency = 10f,
            comboTriggerAt = 6,
            cellStartScale = 0.28f,
            cellPeakScale = 1.05f,
            cellRiseDuration = 0.52f,
            cellSettleDuration = 0.16f,
            itemStartScale = 0.16f,
            itemPeakScale = 1.07f,
            itemDelay = 0.12f,
            itemRiseDuration = 0.44f,
            itemSettleDuration = 0.15f,
            waveStagger = 0.08f,
            waveJitter = 0.006f,
            maxRevealSpread = 1f,
            riseEase = Ease.OutCubic,
            settleEase = Ease.InOutSine,
        };
    }
}
