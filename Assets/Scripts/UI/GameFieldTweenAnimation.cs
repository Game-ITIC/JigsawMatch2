using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class GameFieldTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private float startOffsetX = 15f;
        [SerializeField, Min(0f)] private float duration = 0.8f;
        [SerializeField] private Ease ease = Ease.OutBack;

        private Tween tween;

        private void OnDisable()
        {
            KillTween();
        }

        public void Play(Vector3 targetLocalPosition, Action onComplete)
        {
            KillTween();

            transform.localPosition = targetLocalPosition + Vector3.right * startOffsetX;
            tween = transform.DOLocalMove(targetLocalPosition, duration)
                .SetEase(ease)
                .SetUpdate(useUnscaledTime)
                .SetTarget(this)
                .OnComplete(() =>
                {
                    tween = null;
                    onComplete?.Invoke();
                });
        }

        public void CompleteImmediately(Vector3 targetLocalPosition, Action onComplete)
        {
            KillTween();
            transform.localPosition = targetLocalPosition;
            onComplete?.Invoke();
        }

        private void KillTween()
        {
            if(tween != null && tween.IsActive())
            {
                tween.Kill();
            }

            tween = null;
        }
    }
}
