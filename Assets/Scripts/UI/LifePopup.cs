using System;
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
        [SerializeField] private bool animatePanelTransitions = true;
        [SerializeField, Min(0f)] private float panelOpenDuration = 0.3f;
        [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
        [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
        [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
        [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

        public Button AdsButton => adsButton;
        public Button BuyButton => buyButton;

        private void Start()
        {
            if(closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if(popupPanel == null)
            {
                return;
            }

            CurvedUIPanelAnimator.Show(
                popupPanel,
                animatePanelTransitions ? panelOpenDuration : 0f,
                panelOpenCurve,
                panelFadeCurve);
        }

        public void Hide()
        {
            if(popupPanel == null)
            {
                gameObject.SetActive(false);
                return;
            }

            CurvedUIPanelAnimator.Hide(
                popupPanel,
                animatePanelTransitions ? panelCloseDuration : 0f,
                panelCloseCurve,
                panelFadeCurve,
                () => gameObject.SetActive(false));
        }
    }
}
