# TODO: Dynamic In-App Reward Text Generation

## Context
`rewardText` and `amount` fields have been removed from `InAppProduct` struct in `Assets/Scripts/Configs/InAppConfig.cs` and from `InAppConfig.asset`.

## Pending Task
In `Assets/Scripts/Initializers/CountryInitializer.cs` (around line 150), implement dynamic generation for `rewardText` based on the item contents of each `InAppProduct`:
- `gems` (e.g., "300 Gems")
- `bombs` (e.g., "+3 Bombs")
- `butterflies` (e.g., "+3 Butterflies")
- `extraMoves` (e.g., "+3 Moves")
- `lives` (e.g., "+5 Lives")
- `unlimitedLivesMinutes` (e.g., "Lives 30m" / "Lives 1h")

Pass the dynamically constructed string into `product.Init(inAppProduct.productName, inAppProduct.icon, price, rewardText)`.
