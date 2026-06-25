using TMPro;
using UI;
using UnityEngine;

namespace Views
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [SerializeField] private UIFillBar fillBar;
        [SerializeField] private UIFillBarMilestones milestones;
        [SerializeField] private TMP_Text percentText;

        public void Show()
        {
            screen.SetActive(true);
            ResetProgress();
        }

        public void Hide()
        {
            screen.SetActive(false);
        }

        public void SetProgress(float normalized)
        {
            float clamped = Mathf.Clamp01(normalized);
            fillBar?.SetNormalizedFill(clamped);
            UpdatePercentText(clamped);
        }

        void ResetProgress()
        {
            milestones?.ResetMilestones();
            fillBar?.SetNormalizedFill(0f, animated: false);
            UpdatePercentText(0f);
        }

        void UpdatePercentText(float normalized)
        {
            if (percentText == null)
            {
                return;
            }

            percentText.text = $"Loading {Mathf.RoundToInt(normalized * 100f)}%";
        }
    }
}
