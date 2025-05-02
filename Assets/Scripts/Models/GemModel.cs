using R3;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class GemModel
    {
        public ReactiveProperty<int> Gems = new();

        public GemModel()
        {
            Load();
        }

        public bool HasGems()
        {
            return Gems.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Gems.Value + value;

            Gems.Value = targetValue;

            Save();
        }

        public void Decrease(int value)
        {
            int targetValue = Gems.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Gems.Value = targetValue;

            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Gem, Gems.Value);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Gems.Value = PlayerPrefs.GetInt(PlayerPrefsKeys.Gem, Gems.Value);
        }
    }
}