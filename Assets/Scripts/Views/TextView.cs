using TMPro;
using UnityEngine;

namespace Views
{
    public class TextView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void SetText(string newText)
        {
            text.SetText(newText);
        }
    }
}