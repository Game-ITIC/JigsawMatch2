using Cysharp.Threading.Tasks;
using Interfaces;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InAppView : MonoBehaviour, IPreload
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button noAdsButton;
        [SerializeField] private Button gemsButton;
        [SerializeField] private Button coinsButton;
        [SerializeField] private Transform buttonsParent;
        [SerializeField] private bool animatePanelTransitions = true;
        [SerializeField, Min(0f)] private float panelOpenDuration = 0.28f;
        [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
        [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
        [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
        [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

        public Transform ButtonsParent => buttonsParent;
        public Button NoAdsButton => noAdsButton;
        public Button GemsButton => gemsButton;
        public Button CoinsButton => coinsButton;

        public void Show()
        {
            CurvedUIPanelAnimator.Show(
                gameObject,
                animatePanelTransitions ? panelOpenDuration : 0f,
                panelOpenCurve,
                panelFadeCurve);
        }

        public void Hide()
        {
            CurvedUIPanelAnimator.Hide(
                gameObject,
                animatePanelTransitions ? panelCloseDuration : 0f,
                panelCloseCurve,
                panelFadeCurve);
        }

        public async UniTask Warmup()
        {
            if(closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            await UniTask.Yield();
        }
    }
}
