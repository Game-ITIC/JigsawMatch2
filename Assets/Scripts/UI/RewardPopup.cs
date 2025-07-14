using System;
using DG.Tweening;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RewardPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Button okButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text description;

        private void Start()
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(Hide);
        }

        public void Init(RewardInfo rewardInfo)
        {
            iconImage.sprite = rewardInfo.Icon;
            description.text = rewardInfo.Description;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            popupPanel.SetActive(true);

            popupPanel.transform.localScale = Vector3.zero;
            popupPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        private void Hide()
        {
            popupPanel.transform.DOScale(0f, 0.2f)
                .OnComplete(() =>
                {
                    popupPanel.SetActive(false);
                    gameObject.SetActive(false);
                });
        }
    }
}