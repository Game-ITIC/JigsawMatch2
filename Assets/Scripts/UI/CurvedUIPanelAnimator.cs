using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public static class CurvedUIPanelAnimator
    {
        private const float HiddenScaleMultiplier = 0.94f;

        public static AnimationCurve CreateDefaultOpenCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.7f, 1.06f),
                new Keyframe(1f, 1f));
        }

        public static AnimationCurve CreateDefaultCloseCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.35f, 0.72f),
                new Keyframe(1f, 1f));
        }

        public static AnimationCurve CreateDefaultFadeCurve()
        {
            return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        public static void Show(GameObject panel, float duration, AnimationCurve scaleCurve, AnimationCurve fadeCurve)
        {
            if(panel == null)
            {
                return;
            }

            ActivateParents(panel);
            BringToFront(panel);

            var state = GetState(panel);
            var panelTransform = panel.transform;

            Kill(panelTransform, state.CanvasGroup);
            panel.SetActive(true);
            state.CanvasGroup.interactable = true;
            state.CanvasGroup.blocksRaycasts = true;
            state.CanvasGroup.alpha = 0f;
            panelTransform.localScale = state.ShownScale * HiddenScaleMultiplier;

            if(duration <= 0f)
            {
                state.CanvasGroup.alpha = 1f;
                panelTransform.localScale = state.ShownScale;
                return;
            }

            var sequence = DOTween.Sequence().SetUpdate(true).SetTarget(panelTransform);
            sequence.Join(panelTransform.DOScale(state.ShownScale, duration).SetEase(scaleCurve ?? CreateDefaultOpenCurve()));
            sequence.Join(state.CanvasGroup.DOFade(1f, duration * 0.85f).SetEase(fadeCurve ?? CreateDefaultFadeCurve()));
        }

        public static void Hide(GameObject panel, float duration, AnimationCurve scaleCurve, AnimationCurve fadeCurve, Action onComplete = null)
        {
            if(panel == null)
            {
                onComplete?.Invoke();
                return;
            }

            var state = GetState(panel);
            var panelTransform = panel.transform;

            Kill(panelTransform, state.CanvasGroup);
            state.CanvasGroup.interactable = false;
            state.CanvasGroup.blocksRaycasts = false;

            if(duration <= 0f || !panel.activeSelf)
            {
                HideImmediate(panel);
                onComplete?.Invoke();
                return;
            }

            var sequence = DOTween.Sequence().SetUpdate(true).SetTarget(panelTransform);
            sequence.Join(panelTransform.DOScale(state.ShownScale * HiddenScaleMultiplier, duration).SetEase(scaleCurve ?? CreateDefaultCloseCurve()));
            sequence.Join(state.CanvasGroup.DOFade(0f, duration).SetEase(fadeCurve ?? CreateDefaultFadeCurve()));
            sequence.OnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public static void HideImmediate(GameObject panel)
        {
            if(panel == null)
            {
                return;
            }

            var state = GetState(panel);
            var panelTransform = panel.transform;

            Kill(panelTransform, state.CanvasGroup);
            state.CanvasGroup.alpha = 0f;
            state.CanvasGroup.interactable = false;
            state.CanvasGroup.blocksRaycasts = false;
            panelTransform.localScale = state.ShownScale * HiddenScaleMultiplier;
            panel.SetActive(false);
        }

        private static void ActivateParents(GameObject panel)
        {
            if(panel == null) return;
            var parent = panel.transform.parent;
            while(parent != null)
            {
                if(!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                }
                parent = parent.parent;
            }
        }

        private static void BringToFront(GameObject panel)
        {
            if(panel == null) return;
            var parent = panel.transform.parent;
            if(parent != null)
            {
                parent.SetAsLastSibling();
            }
            panel.transform.SetAsLastSibling();
        }

        private static CurvedUIPanelAnimationState GetState(GameObject panel)
        {
            var state = panel.GetComponent<CurvedUIPanelAnimationState>();
            if(state == null)
            {
                state = panel.AddComponent<CurvedUIPanelAnimationState>();
            }

            state.Capture();
            return state;
        }

        private static void Kill(Transform panelTransform, CanvasGroup canvasGroup)
        {
            panelTransform.DOKill();
            canvasGroup.DOKill();
        }
    }
}
