using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class GameCompleteView : MonoBehaviour
    {
        [SerializeField] private Button home;
        [SerializeField] private Button next;

        public Button Home => home;

        public Button Next => next;
    }
}