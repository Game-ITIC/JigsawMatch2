using UnityEngine;

public class ClickBreathingIdleLoop : MonoBehaviour
{
    public enum ScalePreset
    {
        Custom,
        CartoonPop,
        SquashStretchBounce,
        ElasticJelly,
        TallStretchReveal,
        BreathingIdleLoop
    }

    [Header("Click Target")]
    public Transform target;

    [Header("Camera")]
    public Camera clickCamera;

    [Header("Raycast")]
    public float rayDistance = 1000f;
    public LayerMask clickableLayers = ~0;

    [Header("Animation Settings")]
    public ScalePreset preset = ScalePreset.BreathingIdleLoop;
    public float duration = 1f;
    public float amount = 1f;
    public bool loop = true;

    [Header("Target Scale")]
    public Vector3 targetScale = Vector3.one;

    [Header("Scale Curves")]
    public AnimationCurve xCurve;
    public AnimationCurve yCurve;
    public AnimationCurve zCurve;

    private float timer;
    private bool isScaling;

    private void Start()
    {
        if (target == null)
            target = transform;

        if (clickCamera == null)
            clickCamera = Camera.main;

        ApplyPreset();

        timer = 0f;
        isScaling = false;
    }

    private void Update()
    {
        CheckClick();
        UpdateScale();
    }

    private void CheckClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (clickCamera == null)
        {
            Debug.LogWarning("ClickBreathingIdleLoop: No camera assigned and no MainCamera found.", this);
            return;
        }

        Vector2 mousePosition = Input.mousePosition;
        Ray ray = clickCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayers))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                Debug.Log("Clicked object: " + gameObject.name);
                StartScaleAnimation();
            }
        }
    }

    public void StartScaleAnimation()
    {
        ApplyPreset();

        timer = 0f;
        isScaling = true;

        // For BreathingIdleLoop do NOT start from zero.
        // Breathing curve starts from 1, so object should already be visible.
        if (preset != ScalePreset.BreathingIdleLoop)
        {
            target.localScale = Vector3.zero;
        }
        else
        {
            target.localScale = targetScale * amount;
        }
    }

    public void StopScaleAnimation()
    {
        isScaling = false;
        timer = 0f;
        target.localScale = targetScale * amount;
    }

    public void RestartScaleAnimation()
    {
        StopScaleAnimation();
        StartScaleAnimation();
    }

    private void UpdateScale()
    {
        if (!isScaling)
            return;

        timer += Time.deltaTime;

        float t = timer / duration;
        t = Mathf.Clamp01(t);

        Vector3 finalTargetScale = targetScale * amount;

        float x = xCurve.Evaluate(t) * finalTargetScale.x;
        float y = yCurve.Evaluate(t) * finalTargetScale.y;
        float z = zCurve.Evaluate(t) * finalTargetScale.z;

        target.localScale = new Vector3(x, y, z);

        if (t >= 1f)
        {
            if (loop)
            {
                timer = 0f;

                // For BreathingIdleLoop do NOT reset to zero.
                // Resetting to zero makes it disappear every loop.
                if (preset != ScalePreset.BreathingIdleLoop)
                    target.localScale = Vector3.zero;
            }
            else
            {
                target.localScale = finalTargetScale;
                isScaling = false;
            }
        }
    }

    private void ApplyPreset()
    {
        switch (preset)
        {
            case ScalePreset.Custom:
                break;

            case ScalePreset.CartoonPop:
                xCurve = Curve(0f, 0f, 0.45f, 1.25f, 0.7f, 0.92f, 1f, 1f);
                yCurve = Curve(0f, 0f, 0.45f, 1.25f, 0.7f, 0.92f, 1f, 1f);
                zCurve = Curve(0f, 0f, 0.45f, 1.25f, 0.7f, 0.92f, 1f, 1f);
                break;

            case ScalePreset.SquashStretchBounce:
                xCurve = Curve(0f, 0f, 0.3f, 1.35f, 0.6f, 0.9f, 1f, 1f);
                yCurve = Curve(0f, 0f, 0.3f, 0.75f, 0.6f, 1.25f, 1f, 1f);
                zCurve = Curve(0f, 0f, 0.3f, 1.35f, 0.6f, 0.9f, 1f, 1f);
                break;

            case ScalePreset.ElasticJelly:
                xCurve = Curve(0f, 0f, 0.25f, 1.4f, 0.45f, 0.75f, 0.65f, 1.15f, 0.85f, 0.95f, 1f, 1f);
                yCurve = Curve(0f, 0f, 0.25f, 0.7f, 0.45f, 1.35f, 0.65f, 0.9f, 0.85f, 1.08f, 1f, 1f);
                zCurve = Curve(0f, 0f, 0.25f, 1.4f, 0.45f, 0.75f, 0.65f, 1.15f, 0.85f, 0.95f, 1f, 1f);
                break;

            case ScalePreset.TallStretchReveal:
                xCurve = Curve(0f, 0.2f, 0.4f, 0.75f, 1f, 1f);
                yCurve = Curve(0f, 0f, 0.35f, 1.35f, 0.65f, 0.9f, 1f, 1f);
                zCurve = Curve(0f, 0.2f, 0.4f, 0.75f, 1f, 1f);
                break;

            case ScalePreset.BreathingIdleLoop:
                xCurve = Curve(0f, 1f, 0.5f, 1.08f, 1f, 1f);
                yCurve = Curve(0f, 1f, 0.5f, 0.96f, 1f, 1f);
                zCurve = Curve(0f, 1f, 0.5f, 1.08f, 1f, 1f);
                break;
        }
    }

    private AnimationCurve Curve(params float[] values)
    {
        Keyframe[] keys = new Keyframe[values.Length / 2];

        for (int i = 0; i < keys.Length; i++)
        {
            float time = values[i * 2];
            float value = values[i * 2 + 1];

            keys[i] = new Keyframe(time, value);
        }

        AnimationCurve curve = new AnimationCurve(keys);

        for (int i = 0; i < curve.length; i++)
        {
            curve.SmoothTangents(i, 0f);
        }

        return curve;
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        amount = Mathf.Max(0f, amount);
        rayDistance = Mathf.Max(0.01f, rayDistance);
    }
}
