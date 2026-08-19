using System;
using Configs;
using R3;
using Services;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class RegionModel
    {
        public const int DefaultUpgradeCost = 5;

        private readonly Systems.CurrencySystem.Interfaces.ICurrencyService _currencyService;
        private readonly RegionService _regionService;

        public RegionService RegionService => _regionService;

        public int UpgradeCost
        {
            get
            {
                if (_regionService?.ActiveRegionData != null && _regionService.ActiveRegionData.starCost > 0)
                {
                    return _regionService.ActiveRegionData.starCost;
                }
                return DefaultUpgradeCost;
            }
        }

        public int RequiredStars => UpgradeCost;

        public int TotalSteps
        {
            get
            {
                if (_regionService?.ActiveRegion?.data != null)
                {
                    return _regionService.ActiveRegion.data.Count;
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
            RegionService regionService
        )
        {
            _currencyService = currencyService;
            _regionService = regionService;
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
                if (!_regionService.CanLoadNextRegion()) return false;

                _regionService.LoadNextRegion();
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