using UnityEngine;

// Immutable data passed from target resolution to the visual animator.
public sealed class CollectedTargetFlyRequest
{
    public CollectedTargetFlyRequest(
        Sprite sprite,
        Vector3 startPosition,
        Transform destination,
        float scale,
        int counterIndex,
        bool incrementBombCounterOnArrival = false)
    {
        Sprite = sprite;
        StartPosition = startPosition;
        Destination = destination;
        Scale = scale;
        CounterIndex = counterIndex;
        IncrementBombCounterOnArrival = incrementBombCounterOnArrival;
    }

    public Sprite Sprite { get; }
    public Vector3 StartPosition { get; }
    public Transform Destination { get; }
    public float Scale { get; }
    public int CounterIndex { get; }
    public bool IncrementBombCounterOnArrival { get; }
}
