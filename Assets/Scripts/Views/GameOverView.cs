using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button adsButton;
        [SerializeField] private Button home;

        public Button RestartButton => restartButton;
        public Button AdsButton => adsButton;
        public Button Home => home;
    }
}