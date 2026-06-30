using DG.Tweening;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIScaleTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Vector3 from = Vector3.one;
        [SerializeField] private Vector3 to = Vector3.one * 0.35f;
        [SerializeField, Min(0f)] private float duration = 0.5f;
        [SerializeField] private Ease ease = Ease.Linear;

        private Tweener tween;

        private void OnEnable()
        {
            if(playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            KillTween();
        }

        public void Play()
        {
            KillTween();
            transform.localScale = from;
            tween = transform.DOScale(to, duration)
                .SetEase(ease)
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);
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
