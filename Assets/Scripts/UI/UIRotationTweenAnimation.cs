using DG.Tweening;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIRotationTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Vector3 rotationDelta = new Vector3(0f, 0f, -360f);
        [SerializeField, Min(0f)] private float duration = 6f;
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private int loops = 1;

        private Tweener tween;
        private Vector3 initialEulerAngles;
        private bool initialRotationCaptured;

        private void Awake()
        {
            CaptureInitialRotation();
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
            KillTween();
        }

        public void Play()
        {
            CaptureInitialRotation();
            KillTween();
            transform.localEulerAngles = initialEulerAngles;

            tween = transform
                .DOLocalRotate(initialEulerAngles + rotationDelta, duration, RotateMode.FastBeyond360)
                .SetEase(ease)
                .SetLoops(loops)
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);
        }

        private void CaptureInitialRotation()
        {
            if(initialRotationCaptured)
            {
                return;
            }

            initialEulerAngles = transform.localEulerAngles;
            initialRotationCaptured = true;
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
