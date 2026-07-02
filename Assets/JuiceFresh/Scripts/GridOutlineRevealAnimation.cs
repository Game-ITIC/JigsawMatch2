using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JuiceFresh
{
    [DisallowMultipleComponent]
    public sealed class GridOutlineRevealAnimation : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private Vector3[] loopPoints;
        private Vector3[] pathPoints;
        private float[] cumulativeLengths;
        private float totalLength;
        private float targetWidth;
        private Color targetStartColor;
        private Color targetEndColor;

        public void Play(
            LineRenderer target,
            IReadOnlyList<Vector3> points,
            float duration,
            float delay)
        {
            if(target == null || points == null || points.Count < 4)
                return;

            lineRenderer = target;
            loopPoints = new Vector3[points.Count];
            pathPoints = new Vector3[points.Count + 1];
            cumulativeLengths = new float[pathPoints.Length];

            for(int i = 0; i < points.Count; i++)
            {
                loopPoints[i] = points[i];
                pathPoints[i] = points[i];
            }

            pathPoints[pathPoints.Length - 1] = points[0];
            totalLength = 0f;
            cumulativeLengths[0] = 0f;

            for(int i = 1; i < pathPoints.Length; i++)
            {
                totalLength += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
                cumulativeLengths[i] = totalLength;
            }

            targetWidth = lineRenderer.widthMultiplier;
            targetStartColor = lineRenderer.startColor;
            targetEndColor = lineRenderer.endColor;
            StartCoroutine(Reveal(Mathf.Max(0.05f, duration), Mathf.Max(0f, delay)));
        }

        private IEnumerator Reveal(float duration, float delay)
        {
            lineRenderer.enabled = false;
            lineRenderer.loop = false;

            if(delay > 0f)
                yield return new WaitForSecondsRealtime(delay);

            if(lineRenderer == null)
                yield break;

            lineRenderer.enabled = true;
            float elapsed = 0f;

            while(elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                SetVisibleDistance(totalLength * eased);
                float widthProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 1.35f));
                lineRenderer.widthMultiplier = targetWidth * Mathf.Lerp(0.32f, 1f, widthProgress);
                lineRenderer.startColor = WithAlpha(targetStartColor, targetStartColor.a * t);
                lineRenderer.endColor = WithAlpha(targetEndColor, targetEndColor.a * t);
                yield return null;
            }

            CompleteReveal();
        }

        private void SetVisibleDistance(float distance)
        {
            int segment = 0;
            while(segment < pathPoints.Length - 2 && cumulativeLengths[segment + 1] <= distance)
                segment++;

            int visiblePointCount = segment + 2;
            lineRenderer.positionCount = visiblePointCount;

            for(int i = 0; i <= segment; i++)
                lineRenderer.SetPosition(i, pathPoints[i]);

            float segmentStartDistance = cumulativeLengths[segment];
            float segmentLength = cumulativeLengths[segment + 1] - segmentStartDistance;
            float segmentProgress = segmentLength > 0.0001f
                ? Mathf.Clamp01((distance - segmentStartDistance) / segmentLength)
                : 1f;
            lineRenderer.SetPosition(
                visiblePointCount - 1,
                Vector3.LerpUnclamped(pathPoints[segment], pathPoints[segment + 1], segmentProgress));
        }

        private void CompleteReveal()
        {
            if(lineRenderer == null)
                return;

            lineRenderer.positionCount = loopPoints.Length;
            lineRenderer.SetPositions(loopPoints);
            lineRenderer.loop = true;
            lineRenderer.widthMultiplier = targetWidth;
            lineRenderer.startColor = targetStartColor;
            lineRenderer.endColor = targetEndColor;
            lineRenderer.enabled = true;
            Destroy(this);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
