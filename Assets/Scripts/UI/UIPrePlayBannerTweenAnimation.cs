using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class UIPrePlayBannerTweenAnimation : MonoBehaviour
    {
        [SerializeField] private bool playOnEnable;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private RectTransform banner;
        [SerializeField] private RectTransform cloud;
        [SerializeField] private RectTransform targetDescription;
        [SerializeField] private AnimationManager callbackReceiver;
        [SerializeField] private PrePlay prePlay;

        [Header("Entry offsets")]
        [SerializeField] private float bannerDropOffset = 220f;
        [SerializeField] private float goalDropOffset = 90f;
        [SerializeField] private float cloudRiseOffset = 120f;

        [Header("Entry timing")]
        [SerializeField, Min(0f)] private float bannerDelay;
        [SerializeField, Min(0f)] private float goalDelay = 0.18f;
        [SerializeField, Min(0f)] private float cloudDelay = 0.32f;
        [SerializeField, Min(0f)] private float bannerDuration = 0.52f;
        [SerializeField, Min(0f)] private float goalDuration = 0.42f;
        [SerializeField, Min(0f)] private float cloudDuration = 0.48f;

        [Header("Exit")]
        [SerializeField, Min(0f)] private float holdDuration = 1.35f;
        [SerializeField, Min(0f)] private float exitDuration = 0.48f;
        [SerializeField] private float exitLiftOffset = 180f;
        [SerializeField, Min(0f)] private float completeDelay = 0.04f;

        [Header("Events")]
        [SerializeField, Min(0f)] private float swishTime = 0.72f;

        private Sequence sequence;
        private Vector2 bannerRestPosition;
        private Vector2 cloudRestPosition;
        private Vector2 goalRestPosition;
        private Vector3 bannerRestScale;
        private Vector3 cloudRestScale;
        private Vector3 goalRestScale;
        private Graphic goalGraphic;
        private bool snapshotsCaptured;

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
        }

        public void Play()
        {
            ResolveReferences();
            CaptureSnapshots();
            if(banner == null || cloud == null || targetDescription == null)
            {
                return;
            }

            KillSequence();
            ResetToEntryState();

            sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);

            sequence.Insert(bannerDelay,
                banner.DOAnchorPos(bannerRestPosition, bannerDuration).SetEase(Ease.OutQuart));
            sequence.Insert(bannerDelay,
                banner.DOScale(bannerRestScale, bannerDuration).SetEase(Ease.OutQuart));

            sequence.Insert(goalDelay,
                targetDescription.DOAnchorPos(goalRestPosition, goalDuration).SetEase(Ease.OutCubic));
            sequence.Insert(goalDelay,
                targetDescription.DOScale(goalRestScale, goalDuration).SetEase(Ease.OutCubic));
            if(goalGraphic != null)
            {
                sequence.Insert(goalDelay, goalGraphic.DOFade(1f, goalDuration * 0.85f).SetEase(Ease.OutSine));
            }

            sequence.Insert(cloudDelay,
                cloud.DOAnchorPos(cloudRestPosition, cloudDuration).SetEase(Ease.OutCubic));
            sequence.Insert(cloudDelay,
                cloud.DOScale(cloudRestScale, cloudDuration).SetEase(Ease.OutBack, 1.05f));

            if(callbackReceiver != null)
            {
                sequence.InsertCallback(swishTime, callbackReceiver.SwishSound);
            }

            float exitStart = bannerDelay + bannerDuration + holdDuration;
            Vector2 exitLift = Vector2.up * exitLiftOffset;

            sequence.Insert(exitStart,
                banner.DOAnchorPos(bannerRestPosition + exitLift, exitDuration).SetEase(Ease.InCubic));
            sequence.Insert(exitStart,
                cloud.DOAnchorPos(cloudRestPosition + exitLift, exitDuration).SetEase(Ease.InCubic));
            sequence.Insert(exitStart,
                targetDescription.DOAnchorPos(goalRestPosition + exitLift, exitDuration).SetEase(Ease.InCubic));
            sequence.Insert(exitStart,
                banner.DOScale(bannerRestScale * 0.94f, exitDuration).SetEase(Ease.InCubic));
            sequence.Insert(exitStart,
                cloud.DOScale(cloudRestScale * 0.94f, exitDuration).SetEase(Ease.InCubic));
            sequence.Insert(exitStart,
                targetDescription.DOScale(goalRestScale * 0.94f, exitDuration).SetEase(Ease.InCubic));
            if(goalGraphic != null)
            {
                sequence.Insert(exitStart + exitDuration * 0.15f,
                    goalGraphic.DOFade(0f, exitDuration * 0.85f).SetEase(Ease.InSine));
            }

            sequence.AppendInterval(completeDelay);
            sequence.OnComplete(HandleComplete);
        }

        private void ResetToEntryState()
        {
            banner.anchoredPosition = bannerRestPosition + Vector2.up * bannerDropOffset;
            banner.localScale = bannerRestScale * 0.88f;

            targetDescription.anchoredPosition = goalRestPosition + Vector2.up * goalDropOffset;
            targetDescription.localScale = goalRestScale * 0.82f;
            SetGoalAlpha(0f);

            cloud.anchoredPosition = cloudRestPosition + Vector2.down * cloudRiseOffset;
            cloud.localScale = cloudRestScale * 0.78f;
        }

        private void ResolveReferences()
        {
            banner ??= transform.Find("Banner") as RectTransform;
            cloud ??= transform.Find("Cloud") as RectTransform;
            targetDescription ??= transform.Find("TargetDescription1") as RectTransform;
            goalGraphic ??= targetDescription != null ? targetDescription.GetComponent<Graphic>() : null;

            if(callbackReceiver == null)
            {
                TryGetComponent(out callbackReceiver);
            }

            if(prePlay == null)
            {
                TryGetComponent(out prePlay);
            }
        }

        private void CaptureSnapshots()
        {
            if(snapshotsCaptured || banner == null || cloud == null || targetDescription == null)
            {
                return;
            }

            bannerRestPosition = banner.anchoredPosition;
            cloudRestPosition = cloud.anchoredPosition;
            goalRestPosition = targetDescription.anchoredPosition;
            bannerRestScale = banner.localScale;
            cloudRestScale = cloud.localScale;
            goalRestScale = targetDescription.localScale;
            snapshotsCaptured = true;
        }

        private void SetGoalAlpha(float alpha)
        {
            if(goalGraphic == null)
            {
                return;
            }

            Color color = goalGraphic.color;
            color.a = alpha;
            goalGraphic.color = color;
        }

        private void HandleComplete()
        {
            sequence = null;
            RestoreRestState();

            if(prePlay != null)
            {
                prePlay.CompleteIntro();
                return;
            }

            callbackReceiver?.OnFinished();
        }

        private void RestoreRestState()
        {
            banner.anchoredPosition = bannerRestPosition;
            cloud.anchoredPosition = cloudRestPosition;
            targetDescription.anchoredPosition = goalRestPosition;
            banner.localScale = bannerRestScale;
            cloud.localScale = cloudRestScale;
            targetDescription.localScale = goalRestScale;
            SetGoalAlpha(1f);
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
