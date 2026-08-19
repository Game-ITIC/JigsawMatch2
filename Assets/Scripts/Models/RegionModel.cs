using Configs;
using R3;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class RegionModel
    {
        public const int DefaultUpgradeCost = 5;

        private readonly Systems.CurrencySystem.Interfaces.ICurrencyService _currencyService;
        public readonly BuildingAnimationSettingsProvider _settingsProvider;

        public int UpgradeCost => DefaultUpgradeCost;
        public int RequiredStars => DefaultUpgradeCost;

        public int CurrentLevelProgress
        {
            get => CurrentLevelProgressReactiveProperty.Value;
            private set => CurrentLevelProgressReactiveProperty.Value = value;
        }

        public readonly ReactiveProperty<int> CurrentLevelProgressReactiveProperty = new();

        public RegionModel(
            Systems.CurrencySystem.Interfaces.ICurrencyService currencyService,
            BuildingAnimationSettingsProvider settingsProvider
        )
        {
            _currencyService = currencyService;
            _settingsProvider = settingsProvider;
            Load();
        }

        public bool CanUpgrade()
        {
            var stars = _currencyService?.GetCurrency(Systems.CurrencySystem.CurrencyType.Star)?.Value ?? 0f;
            if(stars >= UpgradeCost && _settingsProvider.ActiveRegion.data.Count > CurrentLevelProgress) return true;
            return false;
        }

        public bool CanLoadNewRegion()
        {
            var stars = _currencyService?.GetCurrency(Systems.CurrencySystem.CurrencyType.Star)?.Value ?? 0f;
            if(stars >= UpgradeCost && CurrentLevelProgress >= _settingsProvider.ActiveRegion.data.Count)
            {
                if(!_settingsProvider.CanLoadNextRegion()) return false;
                _settingsProvider.LoadNextRegion();
                CurrentLevelProgress = 0;
                Save();
            }

            return true;
        }

        public void Upgrade()
        {
            CurrentLevelProgress++;
            _currencyService?.SpendCurrency(Systems.CurrencySystem.CurrencyType.Star, UpgradeCost);
            Save();
        }

        void Load()
        {
            CurrentLevelProgress = PlayerPrefs.GetInt(PlayerPrefsKeys.AsiaBuildingsAnimation, 0);
        }

        void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.AsiaBuildingsAnimation, CurrentLevelProgress);
            PlayerPrefs.Save();
        }
    }
}