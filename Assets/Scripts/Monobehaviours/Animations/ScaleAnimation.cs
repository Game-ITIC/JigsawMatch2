using System.Collections.Generic;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
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

    [Header("Start")]
    public bool startOnZero = false;

    [Header("Click Target")]
    public Transform target;

    [Header("Camera")]
    public Camera clickCamera;

    [Header("Raycast")]
    public float rayDistance = 1000f;
    public LayerMask clickableLayers = ~0;

    [Header("Preset Dropdown")]
    public ScalePreset preset = ScalePreset.CartoonPop;

    [Header("Settings")]
    public float duration = 1f;
    public float amount = 1f;
    public bool loop = false;

    [Header("Single Particle System")]
    public ParticleSystem particleSystem;

    [Header("Particle Systems")]
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    public bool autoFindChildParticles = true;
    public bool restartParticleOnLoop = true;

    [Header("Target Scale")]
    public Vector3 targetScale = Vector3.one;

    [Header("Scale Curves")]
    public AnimationCurve xCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve yCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve zCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float timer;
    private bool isScaling;

    private float particleTimer;
    private bool isParticlePlaying;

    [SerializeField, HideInInspector]
    private ScalePreset lastPreset;

    private bool wasScalingPreviousFrame = false;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        CheckClick();
        UpdateScale();
        UpdateParticleDuration();
    }

    public void Init()
    {
        if (target == null)
            target = transform;

        if (clickCamera == null)
            clickCamera = Camera.main;

        // IMPORTANT:
        // Save original scale before changing anything.
        // Example: if object scale is 2,3,2, this becomes the real final scale.
        targetScale = transform.localScale;

        ApplyPreset();

        timer = 0f;
        particleTimer = 0f;

        isScaling = false;
        isParticlePlaying = false;
        wasScalingPreviousFrame = false;

        if (startOnZero)
        {
            transform.localScale = Vector3.zero;
        }

        if (autoFindChildParticles && particleSystems.Count == 0)
        {
            ParticleSystem[] foundParticles = GetComponentsInChildren<ParticleSystem>(true);
            particleSystems.AddRange(foundParticles);
        }

        if (particleSystem != null && !particleSystems.Contains(particleSystem))
        {
            particleSystems.Add(particleSystem);
        }

        SetupParticles();
    }

    private void CheckClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (clickCamera == null)
        {
            Debug.LogWarning("ScaleAnimation: No camera assigned and no MainCamera found.", this);
            return;
        }

        Vector2 mousePosition = Input.mousePosition;
        Ray ray = clickCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, clickableLayers))
        {
            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                Debug.Log("Clicked object: " + gameObject.name);
                StartActiveAnimation();
            }
            else
            {
                StopScaleAnimationSmooth();
                Debug.Log("Clicked not target object.");
            }
        }
        else
        {
            StopScaleAnimationSmooth();
            Debug.Log("Clicked empty space.");
        }
    }

    public void StartScaleAnimation()
    {
        ApplyPreset();

        timer = 0f;
        isScaling = true;
        wasScalingPreviousFrame = true;

        // Curves start from 0, so this reveals object from zero to targetScale.
        transform.localScale = Vector3.zero;

        PlayParticles();
    }

    public void StartActiveAnimation()
    {
        loop = true;
        duration = 1f;
        preset = ScalePreset.BreathingIdleLoop;

        ApplyPreset();

        timer = 0f;
        isScaling = true;
        wasScalingPreviousFrame = true;

        if (particleSystem != null)
        {
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = duration;
            main.loop = true;

            particleTimer = 0f;
            particleSystem.Play(true);
        }

        PlayParticles();
    }

    // Keeps old function name working if other scripts call it.
    public void startActiveAnimation()
    {
        StartActiveAnimation();
    }

    public void StopScaleAnimation()
    {
        isScaling = false;
        wasScalingPreviousFrame = false;
        isParticlePlaying = false;

        loop = false;

        transform.localScale = targetScale;

        StopParticles(ParticleSystemStopBehavior.StopEmitting);
    }

    public void StopScaleAnimationSmooth()
    {
        // This does NOT instantly reset scale.
        // It lets current animation cycle finish, then returns to targetScale.
        isScaling = false;
        loop = false;

        if (particleSystem != null)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = false;
        }

        StopParticles(ParticleSystemStopBehavior.StopEmitting);
    }

    public void RestartScaleAnimation()
    {
        StopScaleAnimation();
        StartScaleAnimation();
    }

    private void UpdateScale()
    {
        if (!isScaling)
        {
            if (!wasScalingPreviousFrame)
                return;

            // Continue current cycle until it reaches the end.
            loop = false;
        }

        wasScalingPreviousFrame = true;

        timer += Time.deltaTime;

        float t = timer / duration;
        t = Mathf.Clamp01(t);

        float x = xCurve.Evaluate(t);
        float y = yCurve.Evaluate(t);
        float z = zCurve.Evaluate(t);

        // IMPORTANT FIX:
        // Curves are multipliers, not final scale.
        // If targetScale is 2,3,2 and curve is 1.08,0.96,1.08,
        // result becomes 2.16,2.88,2.16.
        transform.localScale = new Vector3(
            targetScale.x * x,
            targetScale.y * y,
            targetScale.z * z
        );

        if (t >= 1f)
        {
            if (loop)
            {
                timer = 0f;

                if (restartParticleOnLoop)
                    PlayParticles();
            }
            else
            {
                isScaling = false;
                wasScalingPreviousFrame = false;

                // Always finish at original scale, not 1,1,1.
                transform.localScale = targetScale;
            }
        }
    }

    private void PlayParticles()
    {
        if (particleSystems == null || particleSystems.Count == 0)
            return;

        particleTimer = 0f;
        isParticlePlaying = true;

        for (int i = 0; i < particleSystems.Count; i++)
        {
            ParticleSystem ps = particleSystems[i];

            if (ps == null)
                continue;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.duration = duration;
            main.loop = loop;

            ps.Play(true);
        }
    }

    private void UpdateParticleDuration()
    {
        if (!isParticlePlaying)
            return;

        if (loop)
            return;

        particleTimer += Time.deltaTime;

        if (particleTimer >= duration)
        {
            StopParticles(ParticleSystemStopBehavior.StopEmitting);
            isParticlePlaying = false;
        }
    }

    private void StopParticles(ParticleSystemStopBehavior stopBehavior)
    {
        if (particleSystems == null)
            return;

        for (int i = 0; i < particleSystems.Count; i++)
        {
            ParticleSystem ps = particleSystems[i];

            if (ps == null)
                continue;

            ParticleSystem.MainModule main = ps.main;
            main.loop = false;

            ps.Stop(true, stopBehavior);
        }
    }

    private void SetupParticles()
    {
        if (particleSystems == null)
            return;

        for (int i = 0; i < particleSystems.Count; i++)
        {
            ParticleSystem ps = particleSystems[i];

            if (ps == null)
                continue;

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.duration = duration;
            main.loop = loop;
        }
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        amount = Mathf.Max(0f, amount);

        if (target == null)
            target = transform;

        if (particleSystems != null)
        {
            for (int i = 0; i < particleSystems.Count; i++)
            {
                ParticleSystem ps = particleSystems[i];

                if (ps == null)
                    continue;

                ParticleSystem.MainModule main = ps.main;
                main.playOnAwake = false;
            }
        }

        if (preset != lastPreset)
        {
            ApplyPreset();
            lastPreset = preset;
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
                loop = true;
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
}
