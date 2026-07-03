using System;
using System.Collections.Generic;
using DG.Tweening;
using JuiceFresh;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class GameFieldTweenAnimation : MonoBehaviour
    {
        private const float DefaultItemScale = 0.9f;

        [SerializeField] private bool useUnscaledTime;

        [Header("Cell Bloom")]
        [SerializeField, Range(0.05f, 0.8f)] private float cellStartScale = 0.32f;
        [SerializeField, Range(1f, 1.15f)] private float cellPeakScale = 1.045f;
        [SerializeField, Min(0.05f)] private float cellRiseDuration = 0.48f;
        [SerializeField, Min(0f)] private float cellSettleDuration = 0.14f;

        [Header("Butterfly Reveal")]
        [SerializeField, Range(0.05f, 0.8f)] private float itemStartScale = 0.2f;
        [SerializeField, Range(1f, 1.2f)] private float itemPeakScale = 1.08f;
        [SerializeField, Min(0f)] private float itemDelay = 0.13f;
        [SerializeField, Min(0.05f)] private float itemRiseDuration = 0.42f;
        [SerializeField, Min(0f)] private float itemSettleDuration = 0.16f;

        [Header("Wave")]
        [SerializeField, Min(0f)] private float waveStagger = 0.08f;
        [SerializeField, Range(0f, 0.05f)] private float waveJitter = 0.01f;
        [SerializeField, Min(0f)] private float maxRevealSpread = 1f;
        [SerializeField] private BoardRevealPattern revealPattern = BoardRevealPattern.CenterOut;

        [Header("Ease")]
        [SerializeField] private Ease riseEase = Ease.OutCubic;
        [SerializeField] private Ease settleEase = Ease.InOutSine;

        private Sequence sequence;
        private readonly List<CellRevealGroup> revealGroups = new List<CellRevealGroup>();

        private void OnDisable()
        {
            KillTween();
        }

        public void ApplyFeelPreset(GameFeelPresetData preset)
        {
            cellStartScale = preset.cellStartScale;
            cellPeakScale = preset.cellPeakScale;
            cellRiseDuration = preset.cellRiseDuration;
            cellSettleDuration = preset.cellSettleDuration;
            itemStartScale = preset.itemStartScale;
            itemPeakScale = preset.itemPeakScale;
            itemDelay = preset.itemDelay;
            itemRiseDuration = preset.itemRiseDuration;
            itemSettleDuration = preset.itemSettleDuration;
            waveStagger = preset.waveStagger;
            waveJitter = preset.waveJitter;
            maxRevealSpread = preset.maxRevealSpread;
            revealPattern = preset.revealPattern;
            riseEase = preset.riseEase;
            settleEase = preset.settleEase;
        }

        public void Play(Vector3 targetLocalPosition, Action onComplete)
        {
            KillTween();

            transform.localPosition = targetLocalPosition;
            BuildRevealGroups();

            if(revealGroups.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(this);

            for(int i = 0; i < revealGroups.Count; i++)
            {
                CellRevealGroup group = revealGroups[i];
                float delay = Mathf.Max(0f, group.WaveDelay + UnityEngine.Random.Range(-waveJitter, waveJitter));
                float itemStartTime = delay + itemDelay;

                group.SquareTransform.localScale = group.SquareTargetScale * cellStartScale;
                SetAlpha(group.SquareRenderer, 0f);
                sequence.Insert(
                    delay,
                    group.SquareTransform
                        .DOScale(group.SquareTargetScale * cellPeakScale, cellRiseDuration)
                        .SetEase(riseEase)
                        .SetUpdate(useUnscaledTime)
                        .SetTarget(this));
                if(group.SquareRenderer != null)
                {
                    sequence.Insert(
                        delay,
                        group.SquareRenderer
                            .DOFade(group.SquareTargetAlpha, cellRiseDuration * 0.78f)
                            .SetEase(Ease.OutSine)
                            .SetUpdate(useUnscaledTime)
                            .SetTarget(this));
                }
                sequence.Insert(
                    delay + cellRiseDuration,
                    group.SquareTransform
                        .DOScale(group.SquareTargetScale, cellSettleDuration)
                        .SetEase(settleEase)
                        .SetUpdate(useUnscaledTime)
                        .SetTarget(this));

                if(group.ItemTransform == null)
                    continue;

                group.ItemTransform.localScale = group.ItemTargetScale * itemStartScale;
                group.ItemTransform.gameObject.SetActive(true);
                SetAlpha(group.ItemRenderer, 0f);
                sequence.Insert(
                    itemStartTime,
                    group.ItemTransform
                        .DOScale(group.ItemTargetScale * itemPeakScale, itemRiseDuration)
                        .SetEase(riseEase)
                        .SetUpdate(useUnscaledTime)
                        .SetTarget(this));
                if(group.ItemRenderer != null)
                {
                    sequence.Insert(
                        itemStartTime,
                        group.ItemRenderer
                            .DOFade(group.ItemTargetAlpha, itemRiseDuration * 0.82f)
                            .SetEase(Ease.OutSine)
                            .SetUpdate(useUnscaledTime)
                            .SetTarget(this));
                }
                sequence.Insert(
                    itemStartTime + itemRiseDuration,
                    group.ItemTransform
                        .DOScale(group.ItemTargetScale, itemSettleDuration)
                        .SetEase(settleEase)
                        .SetUpdate(useUnscaledTime)
                        .SetTarget(this));
            }

            sequence.OnComplete(() =>
            {
                sequence = null;
                RestoreRevealGroups();
                onComplete?.Invoke();
            });
        }

        public void CompleteImmediately(Vector3 targetLocalPosition, Action onComplete)
        {
            KillTween();
            transform.localPosition = targetLocalPosition;
            RestoreRevealGroups();
            onComplete?.Invoke();
        }

        private void BuildRevealGroups()
        {
            revealGroups.Clear();

            LevelManager levelManager = LevelManager.THIS;
            Square[] squares = levelManager?.squaresArray;
            if(squares == null || levelManager == null)
                return;

            float centerCol = (levelManager.maxCols - 1) * 0.5f;
            float centerRow = (levelManager.maxRows - 1) * 0.5f;
            Vector2 gridCenter = new Vector2(centerCol, centerRow);
            float maxDistance = 0f;

            for(int i = 0; i < squares.Length; i++)
            {
                Square square = squares[i];
                if(square == null || square.IsNone())
                    continue;

                float distance = Vector2.Distance(new Vector2(square.col, square.row), gridCenter);
                if(distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }

            if(maxDistance < 0.001f)
            {
                maxDistance = 1f;
            }

            float spread = maxRevealSpread > 0f ? maxRevealSpread : 1f;

            for(int i = 0; i < squares.Length; i++)
            {
                Square square = squares[i];
                if(square == null || square.IsNone())
                    continue;

                Vector2 cell = new Vector2(square.col, square.row);
                float distance = Vector2.Distance(cell, gridCenter);
                float waveDelay = CalculateRevealDelay(
                    revealPattern,
                    cell,
                    gridCenter,
                    distance,
                    maxDistance,
                    square.row,
                    square.col,
                    levelManager.maxRows,
                    levelManager.maxCols,
                    waveStagger,
                    spread);
                SpriteRenderer squareRenderer = square.GetComponent<SpriteRenderer>();
                float squareTargetAlpha = squareRenderer != null ? squareRenderer.color.a : 1f;

                Transform itemTransform = null;
                Vector3 itemTargetScale = Vector3.one * DefaultItemScale;
                SpriteRenderer itemRenderer = null;
                float itemTargetAlpha = 1f;

                if(square.item != null)
                {
                    itemTransform = square.item.transform;
                    itemTargetScale = GetItemTargetScale(itemTransform);
                    itemRenderer = square.item.sprRenderer != null
                        ? square.item.sprRenderer
                        : itemTransform.GetComponentInChildren<SpriteRenderer>();
                    itemTargetAlpha = itemRenderer != null ? itemRenderer.color.a : 1f;
                }

                revealGroups.Add(new CellRevealGroup(
                    square.transform,
                    square.transform.localScale,
                    squareRenderer,
                    squareTargetAlpha,
                    itemTransform,
                    itemTargetScale,
                    itemRenderer,
                    itemTargetAlpha,
                    waveDelay));
            }
        }

        private static float CalculateRevealDelay(
            BoardRevealPattern pattern,
            Vector2 cell,
            Vector2 gridCenter,
            float distanceFromCenter,
            float maxDistance,
            int row,
            int col,
            int maxRows,
            int maxCols,
            float stagger,
            float spread)
        {
            switch(pattern)
            {
                case BoardRevealPattern.AllAtOnce:
                    return 0f;
                case BoardRevealPattern.EdgeIn:
                    return (maxDistance - distanceFromCenter) * stagger * spread;
                case BoardRevealPattern.TopToBottom:
                    return row * stagger * spread;
                case BoardRevealPattern.LeftToRight:
                    return col * stagger * spread;
                case BoardRevealPattern.Diagonal:
                    return (col + row) * stagger * spread;
                case BoardRevealPattern.RandomPop:
                    return UnityEngine.Random.Range(0f, Mathf.Max(stagger, maxDistance * stagger) * spread);
                case BoardRevealPattern.CenterOut:
                default:
                    return distanceFromCenter * stagger * spread;
            }
        }

        private static Vector3 GetItemTargetScale(Transform itemTransform)
        {
            Vector3 scale = itemTransform.localScale;
            if(scale.sqrMagnitude < 0.01f)
                return Vector3.one * DefaultItemScale;

            return scale;
        }

        private void RestoreRevealGroups()
        {
            for(int i = 0; i < revealGroups.Count; i++)
            {
                CellRevealGroup group = revealGroups[i];
                if(group.SquareTransform != null)
                    group.SquareTransform.localScale = group.SquareTargetScale;

                SetAlpha(group.SquareRenderer, group.SquareTargetAlpha);

                if(group.ItemTransform == null)
                    continue;

                group.ItemTransform.localScale = group.ItemTargetScale;
                group.ItemTransform.gameObject.SetActive(true);
                SetAlpha(group.ItemRenderer, group.ItemTargetAlpha);
            }
        }

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            if(renderer == null)
                return;

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }

        private void KillTween()
        {
            if(sequence != null && sequence.IsActive())
                sequence.Kill();

            sequence = null;
            RestoreRevealGroups();
            revealGroups.Clear();
        }

        private readonly struct CellRevealGroup
        {
            public CellRevealGroup(
                Transform squareTransform,
                Vector3 squareTargetScale,
                SpriteRenderer squareRenderer,
                float squareTargetAlpha,
                Transform itemTransform,
                Vector3 itemTargetScale,
                SpriteRenderer itemRenderer,
                float itemTargetAlpha,
                float waveDelay)
            {
                SquareTransform = squareTransform;
                SquareTargetScale = squareTargetScale;
                SquareRenderer = squareRenderer;
                SquareTargetAlpha = squareTargetAlpha;
                ItemTransform = itemTransform;
                ItemTargetScale = itemTargetScale;
                ItemRenderer = itemRenderer;
                ItemTargetAlpha = itemTargetAlpha;
                WaveDelay = waveDelay;
            }

            public Transform SquareTransform { get; }
            public Vector3 SquareTargetScale { get; }
            public SpriteRenderer SquareRenderer { get; }
            public float SquareTargetAlpha { get; }
            public Transform ItemTransform { get; }
            public Vector3 ItemTargetScale { get; }
            public SpriteRenderer ItemRenderer { get; }
            public float ItemTargetAlpha { get; }
            public float WaveDelay { get; }
        }
    }
}
