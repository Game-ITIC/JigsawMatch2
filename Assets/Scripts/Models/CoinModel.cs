using R3;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class CoinModel
    {
        public ReactiveProperty<int> Coins = new();

        public CoinModel()
        {
            Load();
        }

        public bool HasCoins()
        {
            return Coins.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Coins.Value + value;

            Coins.Value = targetValue;

            Save();
        }

        public void Decrease(int value)
        {
            int targetValue = Coins.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Coins.Value = targetValue;

            Save();
        }

        public void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Coin, Coins.Value);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            Coins.Value = PlayerPrefs.GetInt(PlayerPrefsKeys.Coin, Coins.Value);
        }
    }
}