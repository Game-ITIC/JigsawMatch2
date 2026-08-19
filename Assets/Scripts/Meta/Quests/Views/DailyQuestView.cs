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

        public TMP_Text QuestName => _questName;
        public Slider Slider => _slider;
        public TMP_Text SliderText => _sliderText;
        public Image Image => _image;
        public Button CollectButton => collectButton;

        public void Init(DailyQuest dailyQuest)
        {
            if (_questName) _questName.text = dailyQuest.localizationKey;
            
            if (_image)
            {
                _image.sprite = dailyQuest.sprite;
                _image.gameObject.SetActive(dailyQuest.sprite != null);
            }

            UpdateData(dailyQuest);
        }

        public void UpdateData(DailyQuest dailyQuest)
        {
            if (_slider) _slider.value = dailyQuest.ProgressNormalized;
            if (_sliderText) _sliderText.text = $"{dailyQuest.currentProgress} / {dailyQuest.targetAmount}";
            if (collectButton) collectButton.interactable = dailyQuest.isCompleted;
        }

        public void UpdateProgress(int currentProgress, int targetAmount)
        {
            var normalizedProgress = targetAmount > 0 ? Mathf.Clamp01((float)currentProgress / targetAmount) : 0f;
            if (_slider) _slider.value = normalizedProgress;
            if (_sliderText) _sliderText.text = $"{currentProgress} / {targetAmount}";
        }

        public void SetCompleted(bool isCompleted)
        {
            if (collectButton) collectButton.interactable = isCompleted;

            if (isCompleted && _slider)
            {
                var sliderColors = _slider.colors;
                sliderColors.normalColor = Color.green;
                _slider.colors = sliderColors;
            }
        }
    }
}