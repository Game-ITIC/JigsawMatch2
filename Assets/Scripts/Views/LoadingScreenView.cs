using UnityEngine;

namespace Views
{
    public class LoadingScreenView : MonoBehaviour
    {

        [SerializeField] private GameObject screen;

        public void Show()
        {
            screen.SetActive(true);
        }

        public void Hide()
        {
            screen.SetActive(false);
        }
    }
}