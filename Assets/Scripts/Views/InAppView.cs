using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InAppView : MonoBehaviour
    {
        [SerializeField] private Button noAdsButton;
        [SerializeField] private Button gemsButton;
        [SerializeField] private Button coinsButton;
        [SerializeField] private Transform buttonsParent;

        public Transform ButtonsParent => buttonsParent;
        public Button NoAdsButton => noAdsButton;
        public Button GemsButton => gemsButton;
        public Button CoinsButton => coinsButton;
    }
}