using UnityEngine;

namespace Systems.CurrencySystem
{
    public sealed class PlayerPrefsCurrencySaveService : IPlayerSaveService
    {
        private const string KeyPrefix = "Crosslight.Currency.";
        private const string EnergyRecoveryTimestampKey = KeyPrefix + "EnergyRecoveryTimestamp";

        public float LoadCurrency(CurrencyType type, float defaultValue)
        {
            return PlayerPrefs.GetFloat(GetKey(type), defaultValue);
        }

        public void SaveCurrency(CurrencyType type, float amount)
        {
            PlayerPrefs.SetFloat(GetKey(type), amount);
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

        private static string GetKey(CurrencyType type) => $"{KeyPrefix}{type}";
    }
}
