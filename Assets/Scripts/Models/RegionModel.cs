using Configs;
using R3;
using UnityEngine;
using Utils.Save;

namespace Models
{
    public class RegionModel
    {
        private readonly StarModel _starModel;
        public readonly BuildingsAnimationConfig _buildingsAnimationConfig;

        public int CurrentLevelProgress
        {
            get => CurrentLevelProgressReactiveProperty.Value;
            private set => CurrentLevelProgressReactiveProperty.Value = value;
        }

        public readonly ReactiveProperty<int> CurrentLevelProgressReactiveProperty = new();

        public RegionModel(
            StarModel starModel,
            BuildingsAnimationConfig buildingsAnimationConfig
        )
        {
            _starModel = starModel;
            _buildingsAnimationConfig = buildingsAnimationConfig;
            Load();
        }

        public bool CanUpgrade()
        {
            return _starModel.Stars.CurrentValue > 5 && _buildingsAnimationConfig.data.Count > CurrentLevelProgress;
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