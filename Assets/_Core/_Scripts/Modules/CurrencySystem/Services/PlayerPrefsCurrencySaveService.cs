using UnityEngine;

namespace Systems.CurrencySystem
{
    public sealed class PlayerPrefsCurrencySaveService : IPlayerSaveService
    {
        private const string KeyPrefix = "Crosslight.Currency.";
        private const string EnergyRecoveryTimestampKey = KeyPrefix + "EnergyRecoveryTimestamp";

        public const string KeyCoin = "butterfly-match-coin";
        public const string KeyGem = "butterfly-match-gem";
        public const string KeyStar = "butterfly-match-star";

        public float LoadCurrency(CurrencyType type, float defaultValue)
        {
            var key = GetKey(type);
            if(!PlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            if(type is CurrencyType.Cash or CurrencyType.Diamond or CurrencyType.Star)
            {
                int intVal = PlayerPrefs.GetInt(key, int.MinValue);
                if(intVal != int.MinValue)
                {
                    return intVal;
                }
            }

            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SaveCurrency(CurrencyType type, float amount)
        {
            var key = GetKey(type);
            if(type is CurrencyType.Cash or CurrencyType.Diamond or CurrencyType.Star)
            {
                PlayerPrefs.SetInt(key, (int)amount);
            }
            else
            {
                PlayerPrefs.SetFloat(key, amount);
            }
            PlayerPrefs.Save();
        }

        public bool TryLoadEnergyRecoveryTimestamp(out long utcTicks)
        {
            return long.TryParse(
                PlayerPrefs.GetString(EnergyRecoveryTimestampKey, string.Empty),
                out utcTicks) &&
                utcTicks > 0;
        }

        public void SaveEnergyRecoveryTimestamp(long utcTicks)
        {
            PlayerPrefs.SetString(EnergyRecoveryTimestampKey, utcTicks.ToString());
            PlayerPrefs.Save();
        }

        public void ClearEnergyRecoveryTimestamp()
        {
            PlayerPrefs.DeleteKey(EnergyRecoveryTimestampKey);
            PlayerPrefs.Save();
        }

        private static string GetKey(CurrencyType type) => type switch
        {
            CurrencyType.Cash => KeyCoin,
            CurrencyType.Diamond => KeyGem,
            CurrencyType.Star => KeyStar,
            _ => $"{KeyPrefix}{type}"
        };
    }
}
