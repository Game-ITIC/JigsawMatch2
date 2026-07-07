using UnityEngine;

/// <summary>
/// Curated gameplay-feel recipes. A preset changes behavior and timing language,
/// not only the strength of the same animation.
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
                return "Ручная настройка. Появление грида настраивается отдельно на GameFieldTweenAnimation.";
            case GameFeelPresetType.CottonCloud:
                return "Почти невесомый режим: предметы чуть всплывают, аура спокойно растворяется, доска неподвижна.";
            case GameFeelPresetType.PetalDrift:
                return "Лепестковый дрейф: предмет приподнимается, аура медленно вращается и улетает вверх, цепочка нарастает волной.";
            case GameFeelPresetType.HoneyWarmth:
                return "Тёплый ритм: мягкий pop, сердцебиение ауры и золотой bloom на ключевых шагах цепочки.";
            case GameFeelPresetType.SilkTouch:
                return "Точный тактильный щелчок: быстрый tap, короткое схлопывание ауры и акцент на каждом третьем выборе.";
            case GameFeelPresetType.BubbleSoft:
                return "Пузырьковая упругость: squash & stretch предметов, elastic-отскок доски и заметный bloom.";
            case GameFeelPresetType.LavenderMist:
                return "Медленный туман: плавное всплытие, дышащая аура и тихое исчезновение без частых акцентов.";
            case GameFeelPresetType.Marshmallow:
                return "Тяжёлый мягкий зефир: выраженный squash & stretch, двойной ритм цепочки и пружинящая доска.";
            case GameFeelPresetType.MorningDew:
                return "Сухой быстрый отклик: crisp tap, короткий snap ауры и ритм по тройкам без лишней инерции.";
            case GameFeelPresetType.RoseBlush:
                return "Розовый характер: лёгкий wobble каждого предмета, пульсирующее свечение и крупный цветочный bloom.";
            case GameFeelPresetType.CozyJar:
                return "Сбалансированный фирменный вариант: soft pop, дыхание ауры, milestone-ритм и мягкий flutter доски.";
            default:
                return string.Empty;
        }
    }

    private static GameFeelPresetData BaseRecipe(
        BoardMotionStyle boardMotion,
        ItemMotionStyle itemMotion,
        SelectionAuraStyle auraStyle,
        GlowReleaseStyle releaseStyle,
        ChainRhythmStyle rhythm,
        Color glowStart,
        Color glowEnd)
    {
        return new GameFeelPresetData
        {
            enableHaptics = true,
            enableCameraShake = false,
            enableBoardShake = boardMotion != BoardMotionStyle.None,
            enableFreezeFrames = false,
            enableItemPunch = itemMotion != ItemMotionStyle.None,
            enableSelectionGlow = true,
            enableBoardPulseOnMatch = true,
            boardMotionStyle = boardMotion,
            itemMotionStyle = itemMotion,
            selectionAuraStyle = auraStyle,
            glowReleaseStyle = releaseStyle,
            chainRhythmStyle = rhythm,
            globalIntensity = 0.8f,
            matchShakeStrength = 0.025f,
            boardShakeStrength = 0.045f,
            comboShakeStrength = 0.06f,
            winShakeStrength = 0.1f,
            itemPulseStrength = 0.09f,
            boardPulseStrength = 0.028f,
            matchResponseMultiplier = 1f,
            selectionGlowAlpha = glowStart.a,
            selectionGlowScale = 1.12f,
            selectionGlowBreathing = 0.035f,
            selectionGlowWaveSpeed = 5f,
            glowReleasePopScale = 1.48f,
            glowColorStart = glowStart,
            glowColorEnd = glowEnd,
            motionDurationScale = 1f,
            itemPulseDurationScale = 1f,
            cameraShakeFrequency = 10f,
            comboTriggerAt = 6,
        };
    }

    private static GameFeelPresetData CottonCloud()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.None,
            ItemMotionStyle.FloatUp,
            SelectionAuraStyle.Still,
            GlowReleaseStyle.Fade,
            ChainRhythmStyle.Quiet,
            new Color(0.94f, 0.98f, 1f, 0.12f),
            new Color(1f, 1f, 1f, 0.15f));
        p.enableHaptics = false;
        p.enableBoardPulseOnMatch = false;
        p.globalIntensity = 0.45f;
        p.itemPulseStrength = 0.045f;
        p.selectionGlowScale = 1.04f;
        p.selectionGlowBreathing = 0f;
        p.motionDurationScale = 1.45f;
        p.itemPulseDurationScale = 1.35f;
        p.matchResponseMultiplier = 0f;
        p.comboTriggerAt = 99;
        return p;
    }

    private static GameFeelPresetData PetalDrift()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.GentleSway,
            ItemMotionStyle.FloatUp,
            SelectionAuraStyle.SlowSpin,
            GlowReleaseStyle.FloatAway,
            ChainRhythmStyle.Rising,
            new Color(1f, 0.76f, 0.86f, 0.24f),
            new Color(1f, 0.55f, 0.72f, 0.34f));
        p.enableHaptics = false;
        p.globalIntensity = 0.66f;
        p.boardShakeStrength = 0.034f;
        p.itemPulseStrength = 0.065f;
        p.boardPulseStrength = 0.018f;
        p.selectionGlowBreathing = 0.02f;
        p.selectionGlowWaveSpeed = 2.2f;
        p.motionDurationScale = 1.55f;
        p.itemPulseDurationScale = 1.35f;
        p.comboTriggerAt = 8;
        return p;
    }

    private static GameFeelPresetData HoneyWarmth()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.VerticalFloat,
            ItemMotionStyle.SoftPop,
            SelectionAuraStyle.Heartbeat,
            GlowReleaseStyle.Bloom,
            ChainRhythmStyle.Milestones,
            new Color(1f, 0.82f, 0.34f, 0.28f),
            new Color(1f, 0.55f, 0.12f, 0.38f));
        p.globalIntensity = 0.82f;
        p.boardShakeStrength = 0.042f;
        p.itemPulseStrength = 0.095f;
        p.boardPulseStrength = 0.038f;
        p.selectionGlowScale = 1.14f;
        p.selectionGlowBreathing = 0.045f;
        p.glowReleasePopScale = 1.58f;
        p.motionDurationScale = 1.15f;
        return p;
    }

    private static GameFeelPresetData SilkTouch()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.SoftSink,
            ItemMotionStyle.CrispTap,
            SelectionAuraStyle.Still,
            GlowReleaseStyle.Snap,
            ChainRhythmStyle.EveryThird,
            new Color(0.72f, 0.92f, 1f, 0.16f),
            new Color(0.48f, 0.78f, 1f, 0.22f));
        p.enableSelectionGlow = true;
        p.globalIntensity = 0.72f;
        p.boardShakeStrength = 0.022f;
        p.itemPulseStrength = 0.07f;
        p.boardPulseStrength = 0.012f;
        p.selectionGlowScale = 1.05f;
        p.selectionGlowBreathing = 0f;
        p.motionDurationScale = 0.55f;
        p.itemPulseDurationScale = 0.5f;
        p.comboTriggerAt = 9;
        return p;
    }

    private static GameFeelPresetData BubbleSoft()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.ElasticBounce,
            ItemMotionStyle.SquashStretch,
            SelectionAuraStyle.Breathe,
            GlowReleaseStyle.Bloom,
            ChainRhythmStyle.EveryThird,
            new Color(0.48f, 0.92f, 1f, 0.3f),
            new Color(0.62f, 0.5f, 1f, 0.4f));
        p.enableCameraShake = true;
        p.globalIntensity = 1.05f;
        p.matchShakeStrength = 0.045f;
        p.boardShakeStrength = 0.075f;
        p.comboShakeStrength = 0.09f;
        p.itemPulseStrength = 0.16f;
        p.boardPulseStrength = 0f;
        p.selectionGlowScale = 1.18f;
        p.selectionGlowBreathing = 0.06f;
        p.glowReleasePopScale = 1.72f;
        p.cameraShakeFrequency = 12f;
        p.comboTriggerAt = 6;
        return p;
    }

    private static GameFeelPresetData LavenderMist()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.VerticalFloat,
            ItemMotionStyle.FloatUp,
            SelectionAuraStyle.Breathe,
            GlowReleaseStyle.FloatAway,
            ChainRhythmStyle.Quiet,
            new Color(0.78f, 0.66f, 1f, 0.32f),
            new Color(0.58f, 0.42f, 0.92f, 0.4f));
        p.enableHaptics = false;
        p.globalIntensity = 0.56f;
        p.boardShakeStrength = 0.02f;
        p.itemPulseStrength = 0.045f;
        p.boardPulseStrength = 0.014f;
        p.selectionGlowScale = 1.09f;
        p.selectionGlowWaveSpeed = 1.8f;
        p.motionDurationScale = 2f;
        p.itemPulseDurationScale = 1.85f;
        p.matchResponseMultiplier = 0.55f;
        p.comboTriggerAt = 10;
        return p;
    }

    private static GameFeelPresetData Marshmallow()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.ElasticBounce,
            ItemMotionStyle.SquashStretch,
            SelectionAuraStyle.Heartbeat,
            GlowReleaseStyle.Bloom,
            ChainRhythmStyle.Rising,
            new Color(1f, 0.92f, 0.97f, 0.28f),
            new Color(1f, 0.65f, 0.82f, 0.38f));
        p.globalIntensity = 0.95f;
        p.boardShakeStrength = 0.06f;
        p.itemPulseStrength = 0.19f;
        p.boardPulseStrength = 0f;
        p.selectionGlowScale = 1.2f;
        p.selectionGlowBreathing = 0.065f;
        p.glowReleasePopScale = 1.82f;
        p.matchResponseMultiplier = 1.35f;
        p.itemPulseDurationScale = 1.2f;
        p.comboTriggerAt = 5;
        return p;
    }

    private static GameFeelPresetData MorningDew()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.SoftSink,
            ItemMotionStyle.CrispTap,
            SelectionAuraStyle.Still,
            GlowReleaseStyle.Snap,
            ChainRhythmStyle.EveryThird,
            new Color(0.52f, 1f, 0.78f, 0.18f),
            new Color(0.22f, 0.85f, 0.55f, 0.24f));
        p.globalIntensity = 0.7f;
        p.boardShakeStrength = 0.028f;
        p.itemPulseStrength = 0.055f;
        p.boardPulseStrength = 0.012f;
        p.selectionGlowScale = 1.06f;
        p.selectionGlowBreathing = 0f;
        p.motionDurationScale = 0.48f;
        p.itemPulseDurationScale = 0.42f;
        p.comboTriggerAt = 6;
        return p;
    }

    private static GameFeelPresetData RoseBlush()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.None,
            ItemMotionStyle.Wobble,
            SelectionAuraStyle.Heartbeat,
            GlowReleaseStyle.Bloom,
            ChainRhythmStyle.Milestones,
            new Color(1f, 0.62f, 0.78f, 0.46f),
            new Color(1f, 0.3f, 0.58f, 0.58f));
        p.enableHaptics = false;
        p.enableBoardPulseOnMatch = false;
        p.globalIntensity = 0.8f;
        p.itemPulseStrength = 0.08f;
        p.selectionGlowScale = 1.22f;
        p.selectionGlowBreathing = 0.075f;
        p.glowReleasePopScale = 1.95f;
        p.motionDurationScale = 1.2f;
        p.itemPulseDurationScale = 1.25f;
        p.matchResponseMultiplier = 0f;
        p.comboTriggerAt = 99;
        return p;
    }

    private static GameFeelPresetData CozyJar()
    {
        GameFeelPresetData p = BaseRecipe(
            BoardMotionStyle.Flutter,
            ItemMotionStyle.SoftPop,
            SelectionAuraStyle.Breathe,
            GlowReleaseStyle.Bloom,
            ChainRhythmStyle.Milestones,
            new Color(0.82f, 0.94f, 1f, 0.26f),
            new Color(1f, 0.78f, 0.46f, 0.34f));
        p.globalIntensity = 0.78f;
        p.boardShakeStrength = 0.038f;
        p.itemPulseStrength = 0.085f;
        p.boardPulseStrength = 0.026f;
        p.selectionGlowScale = 1.11f;
        p.selectionGlowBreathing = 0.032f;
        p.glowReleasePopScale = 1.5f;
        p.motionDurationScale = 1.05f;
        p.itemPulseDurationScale = 0.95f;
        return p;
    }
}
