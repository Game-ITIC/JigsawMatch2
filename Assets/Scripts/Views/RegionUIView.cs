using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class RegionUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text regionNameText;
        [SerializeField] private TMP_Text progressCounter;
        [SerializeField] private Slider slider;
        private string _name;

        public RegionUIView SetName(string name)
        {
            _name = name;
            UpdateVisual();
            return this;
        }

        public RegionUIView SetProgress(int currentProgress, int maxProgress)
        {
            slider.wholeNumbers = true;
            slider.maxValue = maxProgress;
            slider.value = currentProgress;
            progressCounter.text = $"{currentProgress} / {maxProgress}";
            return this;
        }

        private void UpdateVisual()
        {
            regionNameText.text = _name;
        }
    }
}