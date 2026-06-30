using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIPreCompleteBannerTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool hideOnComplete = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Transform firstBurst;
        [SerializeField] private Transform secondBurst;
        [SerializeField] private Transform topRibbon;
        [SerializeField] private Transform bottomRibbon;
        [SerializeField] private AnimationManager callbackReceiver;

        [Header("Root")]
        [SerializeField] private float rootStartScaleFactor = 0.12f;
        [SerializeField, Min(0f)] private float rootDuration = 0.55f;
        [SerializeField] private Ease rootEase = Ease.OutBack;

        [Header("Stars Entry")]
        [SerializeField, Min(0f)] private float starsStartTime = 0.42f;
        [SerializeField, Min(0f)] private float starStagger = 0.2f;
        [SerializeField, Min(0f)] private float starDuration = 0.6f;
        [SerializeField] private float starSpinDegrees = -540f;
        [SerializeField] private float starStartScaleFactor = 0.08f;
        [SerializeField] private Ease starScaleEase = Ease.OutBack;
        [SerializeField] private Ease starRotationEase = Ease.OutCubic;

        [Header("Stars Exit")]
        [SerializeField, Min(0f)] private float holdAfterEntry = 1.15f;
        [SerializeField, Min(0f)] private float starExitDuration = 0.38f;
        [SerializeField, Min(0f)] private float starExitStagger = 0.1f;
        [SerializeField] private float starExitSpinDegrees = 240f;
        [SerializeField] private float starExitScaleFactor = 0.05f;
        [SerializeField] private Ease starExitScaleEase = Ease.InBack;
        [SerializeField] private Ease starExitRotationEase = Ease.InCubic;

        [Header("Root Exit")]
        [SerializeField, Min(0f)] private float rootExitDuration = 0.45f;
        [SerializeField] private Ease rootExitEase = Ease.InBack;
        [SerializeField, Min(0f)] private float ribbonFadeDuration = 0.35f;

        private Sequence sequence;
        private Vector3 rootEndScale;
        private StarSnapshot leftStarSnapshot;
        private StarSnapshot rightStarSnapshot;
        private Image topRibbonImage;
        private Image bottomRibbonImage;
        private bool snapshotsCaptured;

        public bool IsPlaying { get; private set; }

        private struct StarSnapshot
        {
            public Vector3 Scale;
            public Vector3 Rotation;
            public Image Image;
        }

        private void Awake()
        {
            ResolveReferences();
            CaptureSnapshots();
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
            IsPlaying = false;
        }

        public void Play()
        {
            ResolveReferences();
            CaptureSnapshots();
            if(firstBurst == null || secondBurst == null)
            {
                return;
            }

            KillSequence();
            IsPlaying = true;
            transform.localScale = rootEndScale * rootStartScaleFactor;
            PrepareStar(secondBurst, leftStarSnapshot);
            PrepareStar(firstBurst, rightStarSnapshot);
            SetRibbonAlpha(1f);

            sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);

            sequence.Insert(0f, transform.DOScale(rootEndScale, rootDuration).SetEase(rootEase));
            AnimateStarEntry(secondBurst, leftStarSnapshot, starsStartTime);
            AnimateStarEntry(firstBurst, rightStarSnapshot, starsStartTime + starStagger);

            float exitStart = starsStartTime + starStagger + starDuration + holdAfterEntry;
            AnimateStarExit(firstBurst, rightStarSnapshot, exitStart);
            AnimateStarExit(secondBurst, leftStarSnapshot, exitStart + starExitStagger);
            AnimateRootExit(exitStart);
            sequence.OnComplete(HandleComplete);
        }

        public IEnumerator WaitForCompletion()
        {
            while(IsPlaying)
            {
                yield return null;
            }
        }

        private void AnimateStarEntry(Transform star, StarSnapshot snapshot, float startTime)
        {
            sequence.InsertCallback(startTime, () => star.gameObject.SetActive(true));
            sequence.Insert(startTime,
                star.DOScale(snapshot.Scale, starDuration).SetEase(starScaleEase));
            sequence.Insert(startTime,
                star.DOLocalRotate(snapshot.Rotation, starDuration, RotateMode.FastBeyond360)
                    .SetEase(starRotationEase));

            if(snapshot.Image != null)
            {
                sequence.Insert(startTime,
                    snapshot.Image.DOFade(1f, starDuration * 0.65f).SetEase(Ease.OutQuad));
            }
        }

        private void AnimateStarExit(Transform star, StarSnapshot snapshot, float startTime)
        {
            Vector3 exitScale = snapshot.Scale * starExitScaleFactor;
            Vector3 exitRotation = snapshot.Rotation + Vector3.forward * starExitSpinDegrees;

            sequence.Insert(startTime,
                star.DOScale(exitScale, starExitDuration).SetEase(starExitScaleEase));
            sequence.Insert(startTime,
                star.DOLocalRotate(exitRotation, starExitDuration, RotateMode.FastBeyond360)
                    .SetEase(starExitRotationEase));

            if(snapshot.Image != null)
            {
                sequence.Insert(startTime,
                    snapshot.Image.DOFade(0f, starExitDuration * 0.8f).SetEase(Ease.InQuad));
            }

            sequence.InsertCallback(startTime + starExitDuration, () => star.gameObject.SetActive(false));
        }

        private void AnimateRootExit(float startTime)
        {
            sequence.Insert(startTime,
                transform.DOScale(rootEndScale * rootStartScaleFactor, rootExitDuration).SetEase(rootExitEase));
            InsertRibbonFade(1f, 0f, startTime, ribbonFadeDuration);
        }

        private void PrepareStar(Transform star, StarSnapshot snapshot)
        {
            star.gameObject.SetActive(false);
            star.localScale = snapshot.Scale * starStartScaleFactor;
            star.localEulerAngles = snapshot.Rotation + Vector3.forward * starSpinDegrees;

            if(snapshot.Image != null)
            {
                Color color = snapshot.Image.color;
                color.a = 0f;
                snapshot.Image.color = color;
            }
        }

        private void InsertRibbonFade(float fromAlpha, float toAlpha, float startTime, float duration)
        {
            InsertImageFade(topRibbonImage, fromAlpha, toAlpha, startTime, duration);
            InsertImageFade(bottomRibbonImage, fromAlpha, toAlpha, startTime, duration);
        }

        private void InsertImageFade(Image image, float fromAlpha, float toAlpha, float startTime, float duration)
        {
            if(image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = fromAlpha;
            image.color = color;
            sequence.Insert(startTime, image.DOFade(toAlpha, duration).SetEase(Ease.InSine));
        }

        private void SetRibbonAlpha(float alpha)
        {
            SetImageAlpha(topRibbonImage, alpha);
            SetImageAlpha(bottomRibbonImage, alpha);
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            if(image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        private void ResolveReferences()
        {
            firstBurst ??= transform.Find("Image (6)");
            secondBurst ??= transform.Find("Image (5)");
            topRibbon ??= transform.Find("Image");
            bottomRibbon ??= transform.Find("Image (1)");
            topRibbonImage ??= topRibbon != null ? topRibbon.GetComponent<Image>() : null;
            bottomRibbonImage ??= bottomRibbon != null ? bottomRibbon.GetComponent<Image>() : null;

            if(callbackReceiver == null)
            {
                TryGetComponent(out callbackReceiver);
            }
        }

        private void CaptureSnapshots()
        {
            if(snapshotsCaptured)
            {
                return;
            }

            rootEndScale = transform.localScale;
            leftStarSnapshot = CaptureStarSnapshot(secondBurst);
            rightStarSnapshot = CaptureStarSnapshot(firstBurst);
            snapshotsCaptured = true;
        }

        private static StarSnapshot CaptureStarSnapshot(Transform star)
        {
            if(star == null)
            {
                return default;
            }

            return new StarSnapshot
            {
                Scale = star.localScale,
                Rotation = star.localEulerAngles,
                Image = star.GetComponent<Image>()
            };
        }

        private void HandleComplete()
        {
            sequence = null;
            IsPlaying = false;
            RestoreVisualState();
            callbackReceiver?.OnFinished();

            if(hideOnComplete)
            {
                gameObject.SetActive(false);
            }
        }

        private void RestoreVisualState()
        {
            transform.localScale = rootEndScale;
            SetRibbonAlpha(1f);
            RestoreStar(firstBurst, rightStarSnapshot);
            RestoreStar(secondBurst, leftStarSnapshot);
        }

        private static void RestoreStar(Transform star, StarSnapshot snapshot)
        {
            if(star == null)
            {
                return;
            }

            star.localScale = snapshot.Scale;
            star.localEulerAngles = snapshot.Rotation;

            if(snapshot.Image != null)
            {
                Color color = snapshot.Image.color;
                color.a = 1f;
                snapshot.Image.color = color;
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
    }
}
