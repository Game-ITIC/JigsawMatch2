using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public enum MilestoneRevealMode
    {
        ActivateTarget,
        ActivateFirstChild
    }

    [Serializable]
    public struct UIFillMilestone
    {
        [Range(0f, 1f)] public float threshold;
        public GameObject target;
        public GameObject placeholder;
        public MilestoneRevealMode revealMode;
    }

    public class UIFillBarMilestones : MonoBehaviour
    {
        [SerializeField] UIFillBar fillBar;
        [SerializeField] RectTransform track;
        [SerializeField] bool autoThresholdFromPosition = true;
        [SerializeField] UIFillMilestone[] milestones = Array.Empty<UIFillMilestone>();
        [SerializeField] bool animateReveal = true;
        [SerializeField] float revealDuration = 0.25f;
        [SerializeField] Ease revealEase = Ease.OutBack;

        bool[] _revealed;

        public event Action<int, UIFillMilestone> MilestoneRevealed;

        void Awake()
        {
            if (fillBar == null)
            {
                fillBar = GetComponent<UIFillBar>();
            }

            if (fillBar == null)
            {
                fillBar = GetComponentInParent<UIFillBar>();
            }
        }

        void OnEnable()
        {
            EnsureRevealedStateSize();

            if (fillBar != null)
            {
                fillBar.FillValueChanged += HandleFillChanged;
                HandleFillChanged(fillBar.DisplayFill);
            }
        }

        void OnDisable()
        {
            if (fillBar != null)
            {
                fillBar.FillValueChanged -= HandleFillChanged;
            }
        }

        public void Setup(UIFillBar bar, RectTransform barTrack, UIFillMilestone[] milestoneEntries)
        {
            fillBar = bar;
            track = barTrack;
            milestones = milestoneEntries ?? Array.Empty<UIFillMilestone>();
            autoThresholdFromPosition = false;
            EnsureRevealedStateSize();
            ResetMilestones();
        }

        public void ResetMilestones()
        {
            EnsureRevealedStateSize();
            RecalculateThresholdsFromPositions();

            for (int i = 0; i < milestones.Length; i++)
            {
                _revealed[i] = false;
                ShowPlaceholder(milestones[i]);
                HideMilestone(milestones[i]);
            }

            if (fillBar != null)
            {
                HandleFillChanged(fillBar.DisplayFill);
            }
        }

        void HandleFillChanged(float fillValue)
        {
            for (int i = 0; i < milestones.Length; i++)
            {
                if (_revealed[i] || milestones[i].target == null)
                {
                    continue;
                }

                if (fillValue + 0.0001f < milestones[i].threshold)
                {
                    continue;
                }

                _revealed[i] = true;
                RevealMilestone(milestones[i]);
                MilestoneRevealed?.Invoke(i, milestones[i]);
            }
        }

        void RecalculateThresholdsFromPositions()
        {
            if (!autoThresholdFromPosition || track == null)
            {
                return;
            }

            float width = track.rect.width;
            if (width <= 0f)
            {
                return;
            }

            for (int i = 0; i < milestones.Length; i++)
            {
                var positionSource = milestones[i].target != null
                    ? milestones[i].target.transform
                    : milestones[i].placeholder != null
                        ? milestones[i].placeholder.transform
                        : null;

                if (positionSource is not RectTransform milestoneTransform)
                {
                    continue;
                }

                var localPoint = track.InverseTransformPoint(milestoneTransform.position);
                milestones[i].threshold = Mathf.Clamp01((localPoint.x + width * 0.5f) / width);
            }
        }

        void RevealMilestone(UIFillMilestone milestone)
        {
            GameObject revealObject = GetRevealObject(milestone);
            if (revealObject == null)
            {
                return;
            }

            revealObject.SetActive(true);

            if (!animateReveal)
            {
                revealObject.transform.localScale = Vector3.one;
                return;
            }

            revealObject.transform.DOKill();
            revealObject.transform.localScale = Vector3.zero;
            revealObject.transform
                .DOScale(Vector3.one, revealDuration)
                .SetEase(revealEase)
                .SetTarget(revealObject.transform);
        }

        static void HideMilestone(UIFillMilestone milestone)
        {
            GameObject revealObject = GetRevealObject(milestone);
            if (revealObject == null)
            {
                return;
            }

            revealObject.transform.DOKill();
            revealObject.SetActive(false);
            revealObject.transform.localScale = Vector3.one;
        }

        static void ShowPlaceholder(UIFillMilestone milestone)
        {
            if (milestone.placeholder == null)
            {
                return;
            }

            milestone.placeholder.SetActive(true);
            milestone.placeholder.transform.localScale = Vector3.one;
        }

        static GameObject GetRevealObject(UIFillMilestone milestone)
        {
            if (milestone.target == null)
            {
                return null;
            }

            return milestone.revealMode == MilestoneRevealMode.ActivateFirstChild &&
                   milestone.target.transform.childCount > 0
                ? milestone.target.transform.GetChild(0).gameObject
                : milestone.target;
        }

        void EnsureRevealedStateSize()
        {
            if (milestones == null)
            {
                milestones = Array.Empty<UIFillMilestone>();
            }

            if (_revealed == null || _revealed.Length != milestones.Length)
            {
                _revealed = new bool[milestones.Length];
            }
        }
    }
}
