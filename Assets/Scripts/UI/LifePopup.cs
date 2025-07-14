using System;
using DG.Tweening;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LifePopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Button adsButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        public Button AdsButton => adsButton;
        public Button BuyButton => buyButton;

        private void Start()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            popupPanel.SetActive(true);

            popupPanel.transform.localScale = Vector3.zero;
            popupPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        public void Hide()
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