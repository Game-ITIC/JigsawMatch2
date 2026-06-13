# JigsawMatch2

Unity project for the JigsawMatch2 mobile game.

## Folder Map

- `Assets/_Scenes` - project scenes used by the game and build settings.
- `Assets/Scripts` - first-party C# code, grouped by responsibility:
  - `Configs`, `Data`, `Models`, `Services`, `Systems` for game architecture.
  - `Monobehaviours` for scene/prefab components.
  - `UI` for interface behaviours.
  - `UnityPurchasing` for IAP-related code.
  - `Testing` for temporary/runtime experiment scripts.
- `Assets/Content` - first-party art, prefabs, animation, shaders, UI assets, configs, and models.
  - Region art lives under `Assets/Content/Models/Regions`.
  - Region prefabs and scene art live under `Assets/Content/Prefabs/Regions`.
- `Assets/_TerrainData` - generated terrain and water profile assets.
- `Assets/Settings` - Unity/rendering settings assets that should not live in the root.
- `Assets/Resources` - Unity Resources assets that must be loaded by Resources API.
- `Assets/ThirdParty` - newly imported third-party art/shader packs.
- Vendor/package folders such as `Plugins`, `Gley`, `GoogleMobileAds`, `LevelPlay`, `TextMesh Pro`, `GUI Kit - Yellow Kid`, `CloudPack Smooth`, and similar should stay isolated.

## Structure Rules

- Do not place scripts, models, materials, shaders, or textures directly in `Assets`.
- Move Unity assets together with their `.meta` files, or move them from the Unity Editor, so GUID references remain intact.
- Put gameplay scripts under the closest `Assets/Scripts/...` category instead of creating loose files.
- Put scenes in `Assets/_Scenes`; avoid re-creating a parallel `Assets/Scenes` folder.
- Put project art under `Assets/Content`; put imported store/vendor packs under `Assets/ThirdParty` unless they already have an established vendor folder.
- Keep generated terrain/water data in `Assets/_TerrainData`.

## Naming Rules

- Use descriptive English names for folders, scenes, prefabs, models, animations, and materials.
- Use `PascalCase` for Unity assets and folders, for example `AsiaRegion`, `ChinaVillage`, `MediterraneanSea`, and `ParisLandmarks`.
- Avoid personal names, temporary jokes, transliteration, repeated-letter placeholders, and generated filenames such as `ChatGPT Image ...`.
- Keep test/prototype scenes named by purpose, for example `RegionSandbox`, `AnimationSandbox`, and `VisualPolishSandbox`.
- Keep legacy/source-only assets in clearly named folders such as `Legacy`, `SourceOBJ`, or `SourceMaterials`.

## Mobile URP Quality

Rendering settings live in `Assets/Settings/Rendering`.

- `URP_MobileLow` - low-end phones: 0.75 render scale, no HDR, no MSAA, no URP shadows/additional lights.
- `URP_MobileMedium` - default Android/iOS quality: 0.9 render scale, 2x MSAA, main-light shadows, vertex additional lights.
- `URP_MobileHigh` - stronger devices: 1.0 render scale, 4x MSAA, main-light shadows, soft shadows, opaque texture for higher-end water/refraction-style effects.

Quality levels in `ProjectSettings/QualitySettings.asset` keep six indices for compatibility:

- `Mobile Low` and `Mobile Low+` use `URP_MobileLow`.
- `Mobile Medium` and `Mobile Medium+` use `URP_MobileMedium`.
- `Mobile High` and `Mobile Ultra` use `URP_MobileHigh`.

Android and iOS default to `Mobile Medium`.
