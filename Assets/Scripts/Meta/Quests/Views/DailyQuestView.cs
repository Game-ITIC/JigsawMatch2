using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Quests.Views
{
    public class DailyQuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text questName;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text sliderText;
        [SerializeField] private Image image;
        [SerializeField] private Button collectButton;
        
        public void SetAmount(int amount)
        {
        }
    }
}