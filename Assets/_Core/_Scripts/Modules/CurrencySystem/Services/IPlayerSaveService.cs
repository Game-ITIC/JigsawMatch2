namespace Systems.CurrencySystem
{
    public interface IPlayerSaveService
    {
        float LoadCurrency(CurrencyType type, float defaultValue);
        void SaveCurrency(CurrencyType type, float amount);
        bool TryLoadEnergyRecoveryTimestamp(out long utcTicks);
        void SaveEnergyRecoveryTimestamp(long utcTicks);
        void ClearEnergyRecoveryTimestamp();
    }
}
