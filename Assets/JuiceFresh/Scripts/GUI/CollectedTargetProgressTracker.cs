using System.Collections.Generic;

// Owns pending reservations and applies counters only after a successful arrival.
public sealed class CollectedTargetProgressTracker
{
    readonly LevelManager _levelManager;
    readonly Dictionary<int, int> _pendingByTarget = new();

    public CollectedTargetProgressTracker(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public bool HasAvailableCount(int counterIndex)
    {
        if (_levelManager == null || counterIndex < 0 || counterIndex >= _levelManager.ingrTarget.Count)
        {
            return false;
        }

        _pendingByTarget.TryGetValue(counterIndex, out int pending);
        return _levelManager.ingrTarget[counterIndex].count - pending > 0;
    }

    public void Reserve(int counterIndex)
    {
        _pendingByTarget.TryGetValue(counterIndex, out int pending);
        _pendingByTarget[counterIndex] = pending + 1;
    }

    public void Complete(CollectedTargetFlyRequest request, bool arrived)
    {
        Release(request.CounterIndex);

        if (!arrived || _levelManager == null)
        {
            return;
        }

        DecrementTargetCounter(request.CounterIndex);

        if (request.IncrementBombCounterOnArrival)
        {
            _levelManager.TargetBombs++;
        }
    }

    void DecrementTargetCounter(int counterIndex)
    {
        if (counterIndex >= 0 && counterIndex < _levelManager.ingrTarget.Count &&
            _levelManager.ingrTarget[counterIndex].count > 0)
        {
            _levelManager.ingrTarget[counterIndex].count--;
        }
    }

    void Release(int counterIndex)
    {
        if (!_pendingByTarget.TryGetValue(counterIndex, out int pending))
        {
            return;
        }

        if (pending <= 1)
        {
            _pendingByTarget.Remove(counterIndex);
        }
        else
        {
            _pendingByTarget[counterIndex] = pending - 1;
        }
    }
}
