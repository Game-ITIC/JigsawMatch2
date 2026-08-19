using System;
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

        public int UpgradeCost
        {
            get
            {
                if (_settingsProvider?.ActiveRegionData != null && _settingsProvider.ActiveRegionData.starCost > 0)
                {
                    return _settingsProvider.ActiveRegionData.starCost;
                }
                return DefaultUpgradeCost;
            }
        }

        public int RequiredStars => UpgradeCost;

        public int TotalSteps
        {
            get
            {
                if (_settingsProvider?.ActiveRegion?.data != null)
                {
                    return _settingsProvider.ActiveRegion.data.Count;
                }
                return 0;
            }
        }

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
            return stars >= UpgradeCost && TotalSteps > CurrentLevelProgress;
        }

        public bool CanLoadNewRegion()
        {
            var stars = _currencyService?.GetCurrency(Systems.CurrencySystem.CurrencyType.Star)?.Value ?? 0f;
            if (stars >= UpgradeCost && CurrentLevelProgress >= TotalSteps)
            {
                if (!_settingsProvider.CanLoadNextRegion()) return false;

                _settingsProvider.LoadNextRegion();
                CurrentLevelProgress = 0;
                Save();
            }

            return true;
        }

        public void Upgrade()
        {
            var cost = UpgradeCost;
            CurrentLevelProgress++;
            _currencyService?.SpendCurrency(Systems.CurrencySystem.CurrencyType.Star, cost);
            Save();
        }

        private void Load()
        {
            CurrentLevelProgress = PlayerPrefs.GetInt(PlayerPrefsKeys.AsiaBuildingsAnimation, 0);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.AsiaBuildingsAnimation, CurrentLevelProgress);
            PlayerPrefs.Save();
        }
    }
}