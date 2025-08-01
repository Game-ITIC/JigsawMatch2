using Configs;
using R3;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class RegionModel
    {
        private readonly StarModel _starModel;
        public readonly BuildingAnimationSettingsProvider _settingsProvider;

        public int CurrentLevelProgress
        {
            get => CurrentLevelProgressReactiveProperty.Value;
            private set => CurrentLevelProgressReactiveProperty.Value = value;
        }

        public readonly ReactiveProperty<int> CurrentLevelProgressReactiveProperty = new();

        public RegionModel(
            StarModel starModel,
            BuildingAnimationSettingsProvider settingsProvider
        )
        {
            _starModel = starModel;
            _settingsProvider = settingsProvider;
            Load();
        }

        public bool CanUpgrade()
        {
            return _starModel.Stars.CurrentValue > 5 && _settingsProvider.ActiveRegion.data.Count > CurrentLevelProgress;
        }

        public void Upgrade()
        {
            CurrentLevelProgress++;
            _starModel.Decrease(5);
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