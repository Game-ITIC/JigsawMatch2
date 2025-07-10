using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GameCompleteView : MonoBehaviour
    {
        [SerializeField] private Button home;
        [SerializeField] private Button next;
        [SerializeField] private Button adsButton;
        [SerializeField] private TMP_Text coinsCollectedText;
        
        public Button Home => home;
        public Button Next => next;
        public Button AdsButton => adsButton;
        public TMP_Text CoinsCollectedText => coinsCollectedText;
    }
}