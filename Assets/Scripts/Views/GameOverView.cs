using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button home;

        public Button RestartButton => restartButton;
        public Button Home => home;
    }
}