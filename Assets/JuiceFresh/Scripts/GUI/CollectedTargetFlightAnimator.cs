using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Owns only visual flight animation and guarantees cleanup on finish or cancellation.
public sealed class CollectedTargetFlightAnimator : MonoBehaviour
{
    sealed class ActiveFlight
    {
        public GameObject Visual;
        public CollectedTargetFlyRequest Request;
        public Action<CollectedTargetFlyRequest, bool> Completed;
    }

    readonly HashSet<ActiveFlight> _activeFlights = new();

    float _minDuration = 0.55f;
    float _maxDuration = 0.75f;
    float _arcHeight = 1.5f;
    float _rotationSpeed = 720f;
    string _sortingLayerName = "UI";
    int _sortingOrder = 100;

    public bool IsFlying => _activeFlights.Count > 0;

    public void Configure(
        float minDuration,
        float maxDuration,
        float arcHeight,
        float rotationSpeed,
        string sortingLayerName,
        int sortingOrder)
    {
        _minDuration = Mathf.Max(0.05f, minDuration);
        _maxDuration = Mathf.Max(_minDuration, maxDuration);
        _arcHeight = Mathf.Max(0f, arcHeight);
        _rotationSpeed = rotationSpeed;
        _sortingLayerName = sortingLayerName;
        _sortingOrder = sortingOrder;
    }

    public void Play(
        CollectedTargetFlyRequest request,
        Action<CollectedTargetFlyRequest, bool> completed)
    {
        var flight = new ActiveFlight
        {
            Visual = CreateFlyingObject(request),
            Request = request,
            Completed = completed
        };

        _activeFlights.Add(flight);
        StartCoroutine(Animate(flight, request));
    }

    public void CancelAll()
    {
        if (_activeFlights.Count == 0)
        {
            return;
        }

        var flights = new ActiveFlight[_activeFlights.Count];
        _activeFlights.CopyTo(flights);
        StopAllCoroutines();

        foreach (ActiveFlight flight in flights)
        {
            Finish(flight, arrived: false);
        }
    }

    void OnDisable()
    {
        CancelAll();
    }

    IEnumerator Animate(ActiveFlight flight, CollectedTargetFlyRequest request)
    {
        bool arrived = false;

        try
        {
            float duration = UnityEngine.Random.Range(_minDuration, _maxDuration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (request.Destination == null)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                MoveVisual(flight.Visual.transform, request, progress);
                yield return null;
            }

            if (request.Destination == null)
            {
                yield break;
            }

            SetFinalPosition(flight.Visual.transform, request);
            yield return null;
            arrived = true;
        }
        finally
        {
            Finish(flight, arrived);
        }
    }

    void MoveVisual(Transform visual, CollectedTargetFlyRequest request, float progress)
    {
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        Vector3 endPosition = GetDestinationPosition(request);
        Vector3 controlPoint = Vector3.Lerp(request.StartPosition, endPosition, 0.5f) +
                               Vector3.up * _arcHeight;

        visual.position = EvaluateQuadraticBezier(
            request.StartPosition,
            controlPoint,
            endPosition,
            easedProgress);
        visual.Rotate(Vector3.back, _rotationSpeed * Time.unscaledDeltaTime);
    }

    static void SetFinalPosition(Transform visual, CollectedTargetFlyRequest request)
    {
        visual.position = GetDestinationPosition(request);
    }

    static Vector3 GetDestinationPosition(CollectedTargetFlyRequest request)
    {
        Vector3 position = request.Destination.position;
        position.z = request.StartPosition.z;
        return position;
    }

    GameObject CreateFlyingObject(CollectedTargetFlyRequest request)
    {
        var flyingObject = new GameObject("Collected Target Fly");
        flyingObject.transform.position = request.StartPosition;
        flyingObject.transform.localScale = Vector3.one * request.Scale;

        SpriteRenderer renderer = flyingObject.AddComponent<SpriteRenderer>();
        renderer.sprite = request.Sprite;
        renderer.sortingLayerName = _sortingLayerName;
        renderer.sortingOrder = _sortingOrder;
        return flyingObject;
    }

    void Finish(ActiveFlight flight, bool arrived)
    {
        if (!_activeFlights.Remove(flight))
        {
            return;
        }

        if (flight.Visual != null)
        {
            Destroy(flight.Visual);
        }

        flight.Completed?.Invoke(flight.Request, arrived);
    }

    static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float progress)
    {
        float inverseProgress = 1f - progress;
        return inverseProgress * inverseProgress * start +
               2f * inverseProgress * progress * control +
               progress * progress * end;
    }
}
