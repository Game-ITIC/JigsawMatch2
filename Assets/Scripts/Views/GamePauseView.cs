using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GamePauseView : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button home;

        public Button ContinueButton => continueButton;
        public Button RestartButton => restartButton;
        public Button Home => home;

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        public void Show() {}
    }
}