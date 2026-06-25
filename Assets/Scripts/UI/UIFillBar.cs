using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class UIFillBar : MonoBehaviour
    {
        [SerializeField] float fillDuration = 0.35f;
        [SerializeField] Ease fillEase = Ease.OutCubic;

        Image _fillImage;
        Tweener _fillTween;

        public float NormalizedFill { get; private set; }

        void Awake()
        {
            _fillImage = GetComponent<Image>();
            if (_fillImage != null && _fillImage.type == Image.Type.Filled)
            {
                _fillImage.fillMethod = Image.FillMethod.Horizontal;
                _fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        void OnDisable()
        {
            KillFillTween();
        }

        public void SetFill(float currentValue, float maxValue, bool animated = true)
        {
            float normalized = maxValue > 0f ? Mathf.Clamp01(currentValue / maxValue) : 0f;
            SetNormalizedFill(normalized, animated);
        }

        public void SetNormalizedFill(float normalized, bool animated = true)
        {
            NormalizedFill = Mathf.Clamp01(normalized);
            ApplyFill(animated);
        }

        void ApplyFill(bool animated)
        {
            if (_fillImage == null)
            {
                return;
            }

            KillFillTween();

            if (!animated || fillDuration <= 0f)
            {
                _fillImage.fillAmount = NormalizedFill;
                return;
            }

            _fillTween = _fillImage
                .DOFillAmount(NormalizedFill, fillDuration)
                .SetEase(fillEase)
                .SetTarget(_fillImage);
        }

        void KillFillTween()
        {
            if (_fillTween != null && _fillTween.IsActive())
            {
                _fillTween.Kill();
            }

            _fillTween = null;
        }
    }
}
