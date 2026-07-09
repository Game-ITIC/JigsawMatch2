# MainMenu Build animations

## Что это

В сцене `Assets/_Scenes/MainMenu.unity` кнопка `Build Button` проигрывает постройку острова по кускам. Один клик Build:

1. Проверяет, что у игрока есть минимум 5 звезд.
2. Списывает 5 звезд.
3. Берет следующий `endFrame` из активного острова.
4. Проигрывает `AnimationClip` активного острова до этого кадра.
5. Когда все кадры острова пройдены, следующий клик Build переключает следующий остров из списка.

## Где настраивается в сцене

Открой `Assets/_Scenes/MainMenu.unity`.

Главный объект: `MenuLifetimeScope`.

На нем важны эти компоненты:

- `MenuView`: поле `Build Button` должно ссылаться на UI-кнопку `Build Button`.
- `BaseCountryLifetimeScope`: поле `Settings Provider` должно ссылаться на `BuildingAnimationSettingsProvider` на этом же объекте.
- `BuildingAnimationSettingsProvider`: главный список островов для Build-анимаций.

В `BuildingAnimationSettingsProvider`:

- `Region Parent` = `MenuLifetimeScope/World`.
- `Buildings Animation Configs` = список островов в порядке прохождения.

Текущий порядок:

1. `Japan_Island_Pref`
2. `Korea Environment`
3. `ChinaPrefabs`

Порядок в этом списке и есть порядок, в котором Build переключает острова.

## Где настраивается каждый остров

Каждый остров лежит под `MenuLifetimeScope/World` и имеет компонент `BuildingsAnimationConfig`.

В `BuildingsAnimationConfig` заполняются:

- `Animator`: Animator объекта, который двигается.
- `Animation Clip`: клип всей постройки острова.
- `Data`: шаги строительства.

На объектах острова не должно быть старого компонента `Animation` с этими же клипами. Build использует `Animator + Playables`. Если рядом висит `Animation`, Unity может писать ошибку вида `AnimationClip must be marked as Legacy`, и анимация Build не будет нормально стартовать.

Каждый элемент `Data`:

- `startFrame`: информационный старт шага.
- `endFrame`: кадр, до которого надо проиграть клип на этом клике Build.

Сейчас код использует именно `endFrame`. Например если `Data[0].endFrame = 50`, первый успешный Build проиграет клип до 50 кадра.

## Где менять цену

Цена сейчас захардкожена как `5` звезд в:

- `Assets/Scripts/Models/RegionModel.cs`

Методы:

- `CanUpgrade()`
- `CanLoadNewRegion()`
- `Upgrade()`

Если нужна другая цена, поменяй все проверки и списание в этих методах на одно и то же значение.

## Runtime flow

Кодовый путь такой:

1. `Assets/Scripts/Initializers/CountryInitializer.cs`
2. `CountryInitializer.StartAsync()` подписывает `MenuView.BuildButton` на `Upgrade()`.
3. `Upgrade()` вызывает `RegionModel.CanLoadNewRegion()`.
4. Если предыдущий остров закончен, `BuildingAnimationSettingsProvider.LoadNextRegion()` создает следующий остров.
5. `RegionUpgradeService.Initialize()` переключает playable-граф на новый активный остров.
6. `RegionModel.CanUpgrade()` проверяет звезды и наличие следующего шага.
7. `RegionModel.Upgrade()` списывает звезды и сохраняет прогресс.
8. `RegionUpgradeService.PlayToFrame(endFrame)` проигрывает анимацию.

## Как быстро восстановить настройку

В Unity есть menu item:

`Tools/JigsawMatch2/Setup MainMenu Build Animations`

Он заново записывает 3 острова из `MenuLifetimeScope/World` в `BuildingAnimationSettingsProvider`:

- `Japan_Island_Pref`
- `Korea Environment`
- `ChinaPrefabs`

Также команда удаляет старые `Animation` components внутри этих островов. Используй ее, если список в инспекторе слетел, стал `None`, или Unity снова ругается на `must be marked as Legacy`.

## PlayerPrefs прогресс

Прогресс сохраняется в PlayerPrefs:

- текущий остров: `PlayerPrefsKeys.RegionIndex`
- шаг внутри острова: `PlayerPrefsKeys.AsiaBuildingsAnimation`
- звезды: `PlayerPrefsKeys.Star`

Если тестируешь с нуля, очисти эти ключи через PlayerPrefs editor или временно сбрось их в коде/инспекторе.

## Частые проблемы

- Build не реагирует: проверь `MenuView -> Build Button`.
- Ничего не проигрывается: проверь `BuildingsAnimationConfig -> Animator` и `Animation Clip`.
- В консоли `AnimationClip must be marked as Legacy`: убери старый `Animation` component с объекта острова или запусти `Tools/JigsawMatch2/Setup MainMenu Build Animations`.
- Остров не переключается: проверь, что в `BuildingAnimationSettingsProvider -> Buildings Animation Configs` больше одного острова.
- Кнопка нажимается, но звезд не хватает: нужно минимум 5 звезд.
- Анимация начинается не с того острова: проверь `PlayerPrefsKeys.RegionIndex` и порядок списка `Buildings Animation Configs`.
