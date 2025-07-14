using Cysharp.Threading.Tasks;
using Interfaces;
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

        public Transform ButtonsParent => buttonsParent;
        public Button NoAdsButton => noAdsButton;
        public Button GemsButton => gemsButton;
        public Button CoinsButton => coinsButton;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public async UniTask Warmup()
        {
            // closeButton.onClick.RemoveAllListeners();
            // closeButton.onClick.AddListener(Hide);
        }
    }
}