using UnityEngine;
using Utils.Reactive;
using Utils.Save;

namespace Models
{
    public class StarModel
    {
        public ReactiveProperty<int> Stars = new(0);

        public StarModel()
        {
            Load();
        }

        public bool HasStars()
        {
            return Stars.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Stars.Value + value;

            Stars.Value = targetValue;
            Save();
        }

        public void Decrease(int value)
        {
            int targetValue = Stars.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Stars.Value = targetValue;
            Save();
        }


        public void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Star,Stars.Value);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Stars.Value = PlayerPrefs.GetInt(PlayerPrefsKeys.Star, Stars.Value);
        }
    }
}