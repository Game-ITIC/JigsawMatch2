using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class ScorePopupTweenSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private Transform popupParent;
        [SerializeField] private bool showPopups = true;
        [SerializeField] private Color[] outlineColors;

        [Header("Animation")]
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private Vector3 visibleScale = Vector3.one * 0.6666667f;
        [SerializeField] private Vector3 startScale = Vector3.one * 0.2f;
        [SerializeField] private Vector3 peakScale = Vector3.one * 0.78f;
        [SerializeField] private float riseDistance = 0.8f;
        [SerializeField, Min(0f)] private float popDuration = 0.24f;
        [SerializeField, Min(0f)] private float settleDuration = 0.1f;
        [SerializeField, Min(0f)] private float holdDuration = 0.55f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.32f;

        public bool ShowPopups
        {
            get => showPopups;
            set => showPopups = value;
        }

        public Color[] OutlineColors => outlineColors;

        public void Show(int value, Vector3 worldPosition, int colorIndex, Color[] textColors)
        {
            if(!showPopups || popupPrefab == null)
            {
                return;
            }

            Transform parent = ResolveParent();
            if(parent == null)
            {
                return;
            }

            GameObject popup = Instantiate(popupPrefab, worldPosition, Quaternion.identity, parent);
            popup.transform.position = worldPosition;

            Text text = popup.GetComponentInChildren<Text>();
            if(text != null)
            {
                text.text = value.ToString();
                if(IsValidIndex(textColors, colorIndex))
                {
                    text.color = textColors[colorIndex];
                }
            }

            Outline outline = popup.GetComponentInChildren<Outline>();
            if(outline != null && IsValidIndex(outlineColors, colorIndex))
            {
                outline.effectColor = outlineColors[colorIndex];
            }

            PlayTween(popup);
        }

        public void SetOutlineColor(int index, Color color)
        {
            EnsureOutlinePaletteSize(index + 1);
            outlineColors[index] = color;
        }

        private void PlayTween(GameObject popup)
        {
            Transform popupTransform = popup.transform;
            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if(canvasGroup == null)
            {
                canvasGroup = popup.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            popupTransform.localScale = startScale;

            Vector3 endPosition = popupTransform.position + Vector3.up * riseDistance;
            Sequence sequence = DOTween.Sequence()
                .SetUpdate(useUnscaledTime)
                .SetTarget(popup);

            sequence.Append(popupTransform.DOScale(peakScale, popDuration).SetEase(Ease.OutBack));
            sequence.Append(popupTransform.DOScale(visibleScale, settleDuration).SetEase(Ease.OutCubic));
            sequence.AppendInterval(holdDuration);
            sequence.Append(popupTransform.DOMove(endPosition, fadeDuration).SetEase(Ease.OutCubic));
            sequence.Join(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InSine));
            sequence.OnComplete(() => Destroy(popup));
        }

        private Transform ResolveParent()
        {
            if(popupParent != null)
            {
                return popupParent;
            }

            GameObject canvasScore = GameObject.Find("CanvasScore");
            popupParent = canvasScore != null ? canvasScore.transform : null;
            return popupParent;
        }

        private void EnsureOutlinePaletteSize(int size)
        {
            if(outlineColors != null && outlineColors.Length >= size)
            {
                return;
            }

            Color[] resized = new Color[size];
            for(int i = 0; i < resized.Length; i++)
            {
                resized[i] = Color.white;
            }

            if(outlineColors != null)
            {
                outlineColors.CopyTo(resized, 0);
            }

            outlineColors = resized;
        }

        private static bool IsValidIndex(Color[] colors, int index)
        {
            return colors != null && index >= 0 && index < colors.Length;
        }
    }
}
