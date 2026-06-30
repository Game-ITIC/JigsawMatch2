using DG.Tweening;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIPopupOpenTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Transform target;
        [SerializeField] private string fallbackTargetPath = "Image";
        [SerializeField] private AnimationManager callbackReceiver;

        [Header("Keyframes")]
        [SerializeField] private Vector3 startScale = new Vector3(1.64f, 0.53f, 1f);
        [SerializeField] private Vector3 firstScale = new Vector3(0.8113925f, 1.2218723f, 1f);
        [SerializeField] private Vector3 secondScale = new Vector3(0.8783424f, 1.3427415f, 1f);
        [SerializeField] private Vector3 thirdScale = new Vector3(1.1781013f, 0.90030205f, 1f);
        [SerializeField] private Vector3 endScale = Vector3.one;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float firstDuration = 0.11666667f;
        [SerializeField, Min(0f)] private float secondDuration = 0.0375f;
        [SerializeField, Min(0f)] private float thirdDuration = 0.1f;
        [SerializeField, Min(0f)] private float finalDuration = 0.07916668f;
        [SerializeField, Min(0f)] private float completeDelay = 0.0125f;
        [SerializeField] private Ease ease = Ease.InOutSine;

        [Header("Legacy Events")]
        [SerializeField] private bool playClickEvents = true;
        [SerializeField, Min(0f)] private float firstClickTime = 0.25f;
        [SerializeField, Min(0f)] private float secondClickTime = 0.32916668f;

        private Sequence sequence;

        private void Awake()
        {
            ResolveReferences();
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
            ResolveReferences();
            if(target == null)
            {
                return;
            }

            KillSequence();
            target.localScale = startScale;

            sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);

            sequence.Append(target.DOScale(firstScale, firstDuration).SetEase(ease));
            sequence.Append(target.DOScale(secondScale, secondDuration).SetEase(ease));
            sequence.Append(target.DOScale(thirdScale, thirdDuration).SetEase(ease));
            sequence.Append(target.DOScale(endScale, finalDuration).SetEase(ease));

            if(playClickEvents && callbackReceiver != null)
            {
                sequence.InsertCallback(firstClickTime, callbackReceiver.PlaySoundButton);
                sequence.InsertCallback(secondClickTime, callbackReceiver.PlaySoundButton);
            }

            if(completeDelay > 0f)
            {
                sequence.AppendInterval(completeDelay);
            }

            sequence.OnComplete(HandleComplete);
        }

        private void ResolveReferences()
        {
            if(target == null && !string.IsNullOrEmpty(fallbackTargetPath))
            {
                target = transform.Find(fallbackTargetPath);
            }

            if(callbackReceiver == null)
            {
                TryGetComponent(out callbackReceiver);
            }
        }

        private void HandleComplete()
        {
            sequence = null;
            callbackReceiver?.OnFinished();
        }

        private void KillSequence()
        {
            if(sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }

            sequence = null;
        }
    }
}
