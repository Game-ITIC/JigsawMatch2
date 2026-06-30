using JuiceFresh;
using UnityEngine;

// Converts a collected board object into a target-specific flight request.
public sealed class CollectedTargetFlyRequestResolver
{
    const float DefaultScale = 0.5f;
    const float BombScale = 0.25f;

    readonly LevelManager _levelManager;
    readonly CollectedTargetProgressTracker _progressTracker;

    public CollectedTargetFlyRequestResolver(
        LevelManager levelManager,
        CollectedTargetProgressTracker progressTracker)
    {
        _levelManager = levelManager;
        _progressTracker = progressTracker;
    }

    public bool TryResolve(GameObject collectedObject, out CollectedTargetFlyRequest request)
    {
        request = null;

        if (_levelManager == null || collectedObject == null)
        {
            return false;
        }

        return _levelManager.target switch
        {
            Target.COLLECT or Target.ITEMS => TryResolveCollectable(collectedObject, out request),
            Target.BLOCKS => TryResolveBlock(collectedObject, out request),
            Target.BOMBS => TryResolveBomb(collectedObject, out request),
            _ => false
        };
    }

    bool TryResolveCollectable(GameObject collectedObject, out CollectedTargetFlyRequest request)
    {
        request = null;

        if (!collectedObject.TryGetComponent(out Item collectedItem))
        {
            return false;
        }

        int targetCount = Mathf.Min(_levelManager.NumIngredients, _levelManager.ingrTarget.Count);
        for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
        {
            if (!_progressTracker.HasAvailableCount(targetIndex) ||
                !TryGetCollectedSprite(collectedItem, targetIndex, out Sprite sprite))
            {
                continue;
            }

            Transform destination = ResolveIngredientDestination(targetIndex);
            if (destination == null)
            {
                continue;
            }

            request = new CollectedTargetFlyRequest(
                sprite,
                collectedObject.transform.position,
                destination,
                DefaultScale,
                targetIndex);
            return true;
        }

        return false;
    }

    bool TryResolveBlock(GameObject collectedObject, out CollectedTargetFlyRequest request)
    {
        request = null;

        if (_levelManager.targetBlocks <= 0 ||
            !collectedObject.TryGetComponent(out Square _) ||
            !collectedObject.TryGetComponent(out SpriteRenderer sourceRenderer) ||
            sourceRenderer.sprite == null)
        {
            return false;
        }

        Transform destination = ResolveVisualDestination(_levelManager.blocksObject);
        if (destination == null)
        {
            return false;
        }

        request = new CollectedTargetFlyRequest(
            sourceRenderer.sprite,
            collectedObject.transform.position,
            destination,
            DefaultScale,
            counterIndex: 0);
        return true;
    }

    bool TryResolveBomb(GameObject collectedObject, out CollectedTargetFlyRequest request)
    {
        request = null;

        if (!collectedObject.TryGetComponent(out Item collectedItem) ||
            collectedItem.currentType != ItemsTypes.BOMB)
        {
            return false;
        }

        Transform destination = ResolveVisualDestination(_levelManager.bombTargetObject);
        Sprite sprite = collectedItem.sprRenderer != null ? collectedItem.sprRenderer.sprite : null;
        if (destination == null || sprite == null)
        {
            return false;
        }

        request = new CollectedTargetFlyRequest(
            sprite,
            collectedObject.transform.position,
            destination,
            BombScale,
            counterIndex: 0,
            incrementBombCounterOnArrival: true);
        return true;
    }

    bool TryGetCollectedSprite(Item item, int targetIndex, out Sprite sprite)
    {
        sprite = null;

        if (item.currentType == ItemsTypes.NONE)
        {
            return TryGetColoredItemSprite(item, targetIndex, out sprite);
        }

        if (item.currentType != ItemsTypes.INGREDIENT || item.color != targetIndex + 1000 ||
            item.transform.childCount == 0)
        {
            return false;
        }

        SpriteRenderer childRenderer = item.transform.GetChild(0).GetComponent<SpriteRenderer>();
        sprite = childRenderer != null ? childRenderer.sprite : null;
        return sprite != null;
    }

    bool TryGetColoredItemSprite(Item item, int targetIndex, out Sprite sprite)
    {
        sprite = null;

        if (_levelManager.collectItems == null || targetIndex >= _levelManager.collectItems.Length ||
            item.color != (int)_levelManager.collectItems[targetIndex] - 1 ||
            item.items == null || item.color < 0 || item.color >= item.items.Length)
        {
            return false;
        }

        sprite = item.items[item.color];
        return sprite != null;
    }

    Transform ResolveIngredientDestination(int targetIndex)
    {
        Transform targetRoot = _levelManager.ingrObject != null
            ? _levelManager.ingrObject.transform.Find($"Ingr{targetIndex}")
            : null;
        return ResolveVisualDestination(targetRoot != null ? targetRoot.gameObject : null);
    }

    static Transform ResolveVisualDestination(GameObject targetRoot)
    {
        if (targetRoot == null)
        {
            return null;
        }

        TargetGUI targetGui = targetRoot.GetComponent<TargetGUI>();
        return targetGui != null && targetGui.image != null
            ? targetGui.image.transform
            : targetRoot.transform;
    }
}
