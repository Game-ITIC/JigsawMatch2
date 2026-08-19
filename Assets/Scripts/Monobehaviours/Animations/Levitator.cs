using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class Levitator : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Offset relative to starting position (e.g. (0, 0.5, 0) for vertical Y-axis levitation)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);

    [Tooltip("Duration of one direction movement in seconds")]
    [SerializeField, Min(0.01f)] private float duration = 1.5f;

    [Header("Easing")]
    [Tooltip("DOTween easing curve for smooth motion (e.g. InOutSine, InOutQuad, InOutCubic)")]
    [SerializeField] private Ease ease = Ease.InOutSine;

    [Header("Settings")]
    [Tooltip("If true, moves in local space; otherwise moves in world space")]
    [SerializeField] private bool useLocalPosition = true;

    [Tooltip("Automatically start levitating on Enable")]
    [SerializeField] private bool playOnEnable = true;

    [Tooltip("Use unscaled time (works even if Time.timeScale is 0)")]
    [SerializeField] private bool useUnscaledTime = false;

    [Tooltip("Optional initial delay before starting")]
    [SerializeField, Min(0f)] private float startDelay = 0f;

    private Vector3 _startPosition;
    private Tweener _tween;
    private bool _isInitialized;

    public Vector3 Offset
    {
        get => offset;
        set
        {
            offset = value;
            if (Application.isPlaying && _tween != null && _tween.IsActive())
            {
                Play();
            }
        }
    }

    public Ease EaseType
    {
        get => ease;
        set
        {
            ease = value;
            if (Application.isPlaying && _tween != null && _tween.IsActive())
            {
                Play();
            }
        }
    }

    public float Duration
    {
        get => duration;
        set
        {
            duration = Mathf.Max(0.01f, value);
            if (Application.isPlaying && _tween != null && _tween.IsActive())
            {
                Play();
            }
        }
    }

    private void Awake()
    {
        CaptureStartPosition();
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    private void OnDisable()
    {
        KillTween();
    }

    private void OnDestroy()
    {
        KillTween();
    }

    public void CaptureStartPosition()
    {
        _startPosition = useLocalPosition ? transform.localPosition : transform.position;
        _isInitialized = true;
    }

    public void Play()
    {
        KillTween();

        if (!_isInitialized)
        {
            CaptureStartPosition();
        }

        if (useLocalPosition)
        {
            transform.localPosition = _startPosition;
            Vector3 targetPosition = _startPosition + offset;

            _tween = transform.DOLocalMove(targetPosition, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(useUnscaledTime)
                .SetDelay(startDelay)
                .SetTarget(this);
        }
        else
        {
            transform.position = _startPosition;
            Vector3 targetPosition = _startPosition + offset;

            _tween = transform.DOMove(targetPosition, duration)
                .SetEase(ease)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(useUnscaledTime)
                .SetDelay(startDelay)
                .SetTarget(this);
        }
    }

    public void Stop(bool resetPosition = true)
    {
        KillTween();

        if (resetPosition && _isInitialized)
        {
            if (useLocalPosition)
            {
                transform.localPosition = _startPosition;
            }
            else
            {
                transform.position = _startPosition;
            }
        }
    }

    public void Pause()
    {
        _tween?.Pause();
    }

    public void Resume()
    {
        _tween?.Play();
    }

    private void KillTween()
    {
        if (_tween != null && _tween.IsActive())
        {
            _tween.Kill();
        }

        _tween = null;
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);

        if (Application.isPlaying && _tween != null && _tween.IsActive())
        {
            Play();
        }
    }
}
