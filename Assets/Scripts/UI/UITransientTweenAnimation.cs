using DG.Tweening;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UITransientTweenAnimation : MonoBehaviour
    {
        [Header("Playback")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool deactivateOnComplete = true;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float delay;
        [SerializeField, Min(0f)] private float popInDuration = 0.42f;
        [SerializeField, Min(0f)] private float settleDuration = 0.12f;
        [SerializeField, Min(0f)] private float holdDuration = 0.24f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 0.3f;

        [Header("Motion")]
        [SerializeField] private Vector2 startOffset;
        [SerializeField] private Vector2 endOffset = new Vector2(0f, 38.68f);
        [SerializeField] private Vector3 startScaleMultiplier = Vector3.one * 0.2f;
        [SerializeField] private Vector3 peakScaleMultiplier = Vector3.one * 1.18f;
        [SerializeField] private Vector3 endScaleMultiplier = Vector3.one;

        [Header("Ease")]
        [SerializeField] private Ease popEase = Ease.OutBack;
        [SerializeField] private Ease settleEase = Ease.OutCubic;
        [SerializeField] private Ease moveEase = Ease.OutCubic;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Sequence sequence;
        private Vector2 initialAnchoredPosition;
        private Vector3 initialLocalPosition;
        private Vector3 initialLocalScale;
        private bool initialStateCaptured;

        private void Awake()
        {
            CaptureInitialState();
        }

        private void OnEnable()
        {
            if(playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            KillSequence();
        }

        public void Play()
        {
            if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                return;
            }

            CaptureInitialState();
            KillSequence();

            CanvasGroup group = GetCanvasGroup();
            ApplyStartState(group);

            sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);

            if(delay > 0f)
            {
                sequence.AppendInterval(delay);
            }

            sequence.Append(group.DOFade(1f, popInDuration).SetEase(fadeEase));
            sequence.Join(TweenPosition(GetInitialPosition(), popInDuration).SetEase(moveEase));
            sequence.Join(transform.DOScale(Scaled(peakScaleMultiplier), popInDuration).SetEase(popEase));

            if(settleDuration > 0f)
            {
                sequence.Append(transform.DOScale(initialLocalScale, settleDuration).SetEase(settleEase));
            }

            if(holdDuration > 0f)
            {
                sequence.AppendInterval(holdDuration);
            }

            sequence.Append(TweenPosition(GetInitialPosition() + endOffset, fadeOutDuration).SetEase(moveEase));
            sequence.Join(group.DOFade(0f, fadeOutDuration).SetEase(fadeEase));
            sequence.Join(transform.DOScale(Scaled(endScaleMultiplier), fadeOutDuration).SetEase(settleEase));
            sequence.OnComplete(OnSequenceComplete);
        }

        public void RecaptureInitialState()
        {
            initialStateCaptured = false;
            CaptureInitialState();
        }

        private void CaptureInitialState()
        {
            if(initialStateCaptured)
            {
                return;
            }

            rectTransform = transform as RectTransform;
            initialLocalScale = transform.localScale;
            initialLocalPosition = transform.localPosition;
            initialAnchoredPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            initialStateCaptured = true;
        }

        private CanvasGroup GetCanvasGroup()
        {
            if(canvasGroup == null && !TryGetComponent(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            return canvasGroup;
        }

        private void ApplyStartState(CanvasGroup group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            transform.localScale = Scaled(startScaleMultiplier);
            SetPosition(GetInitialPosition() + startOffset);
        }

        private void OnSequenceComplete()
        {
            sequence = null;

            if(deactivateOnComplete)
            {
                gameObject.SetActive(false);
            }
        }

        private void KillSequence()
        {
            if(sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }

            sequence = null;
        }

        private Vector3 Scaled(Vector3 multiplier)
        {
            return Vector3.Scale(initialLocalScale, multiplier);
        }

        private Vector2 GetInitialPosition()
        {
            if(rectTransform != null)
            {
                return initialAnchoredPosition;
            }

            return initialLocalPosition;
        }

        private void SetPosition(Vector2 position)
        {
            if(rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
                return;
            }

            transform.localPosition = new Vector3(position.x, position.y, initialLocalPosition.z);
        }

        private Tween TweenPosition(Vector2 position, float duration)
        {
            if(rectTransform != null)
            {
                return rectTransform.DOAnchorPos(position, duration);
            }

            return transform.DOLocalMove(new Vector3(position.x, position.y, initialLocalPosition.z), duration);
        }
    }
}
