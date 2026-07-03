using UnityEngine;

/// <summary>
/// Ten soft gameplay-feel profiles tuned for a gentle, cozy jar-matching experience.
/// Pick one in the Inspector dropdown on <see cref="GameFeelManager"/>.
/// </summary>
public enum GameFeelPresetType
{
    [InspectorName("Свой (ручная настройка)")]
    Custom = 0,

    [InspectorName("01 · Пуховое облако")]
    CottonCloud,

    [InspectorName("02 · Лепесток")]
    PetalDrift,

    [InspectorName("03 · Мёдовое тепло")]
    HoneyWarmth,

    [InspectorName("04 · Шёлковое касание")]
    SilkTouch,

    [InspectorName("05 · Мыльный пузырь")]
    BubbleSoft,

    [InspectorName("06 · Лавандовый туман")]
    LavenderMist,

    [InspectorName("07 · Зефир")]
    Marshmallow,

    [InspectorName("08 · Утренняя роса")]
    MorningDew,

    [InspectorName("09 · Розовый румянец")]
    RoseBlush,

    [InspectorName("10 · Уютная баночка ★")]
    CozyJar,
}
