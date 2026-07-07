using UnityEngine;

/// <summary>
/// Gameplay-feel recipes. Grid intro has its own independent preset.
/// </summary>
public enum GameFeelPresetType
{
    [InspectorName("Свой (ручная настройка)")]
    Custom = 0,

    [InspectorName("01 · Пуховое облако")]
    CottonCloud,

    [InspectorName("02 · Лепестковый дрейф")]
    PetalDrift,

    [InspectorName("03 · Медовое тепло")]
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
