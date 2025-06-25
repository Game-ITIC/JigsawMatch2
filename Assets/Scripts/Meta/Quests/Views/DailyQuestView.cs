using Meta.Quests.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.Quests.Views
{
    public class DailyQuestView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _questName;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _sliderText;
        [SerializeField] private Image _image;
        [SerializeField] private Button collectButton;

        // Публичные свойства для доступа из презентера
        public TMP_Text QuestName => _questName;
        public Slider Slider => _slider;
        public TMP_Text SliderText => _sliderText;
        public Image Image => _image;
        public Button CollectButton => collectButton;

        public void Init(DailyQuest dailyQuest)
        {
            QuestName.text = dailyQuest.localizationKey;
            Image.sprite = dailyQuest.sprite;
            UpdateData(dailyQuest);
        }

        public void UpdateData(DailyQuest dailyQuest)
        {
            Slider.value = dailyQuest.ProgressNormalized;
            SliderText.text = $"{dailyQuest.currentProgress} / {dailyQuest.targetAmount}";
            
            // Обновляем состояние кнопки
            collectButton.interactable = dailyQuest.isCompleted;
        }

        public void UpdateProgress(int currentProgress, int targetAmount)
        {
            float normalizedProgress = targetAmount > 0 ? Mathf.Clamp01((float)currentProgress / targetAmount) : 0f;
            Slider.value = normalizedProgress;
            SliderText.text = $"{currentProgress} / {targetAmount}";
        }

        public void SetCompleted(bool isCompleted)
        {
            collectButton.interactable = isCompleted;
            
            // Можно добавить визуальные изменения для завершенного квеста
            if (isCompleted)
            {
                // Например, изменить цвет слайдера или добавить эффект
                var sliderColors = Slider.colors;
                sliderColors.normalColor = Color.green;
                Slider.colors = sliderColors;
            }
        }
    }
}