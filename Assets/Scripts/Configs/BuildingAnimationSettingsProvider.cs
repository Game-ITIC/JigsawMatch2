using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Save;

namespace Configs
{
    public class BuildingAnimationSettingsProvider : MonoBehaviour
    {
        [SerializeField] private List<BuildingsAnimationConfig> buildingsAnimationConfigs;
        [SerializeField] private Transform regionParent;
        private BuildingsAnimationConfig _activeRegion;

        public BuildingsAnimationConfig ActiveRegion => _activeRegion;
        private int currentRegionIndex;

        public bool CanLoadNextRegion()
        {
            if(currentRegionIndex < 0 || currentRegionIndex >= buildingsAnimationConfigs.Count)
            {
                Debug.LogWarning($"Invalid region index {currentRegionIndex}");
                return false;
            }

            return true;
        }

        public async UniTask Warmup()
        {
            Load();
            _activeRegion = buildingsAnimationConfigs[0];
            // LoadRegion(currentRegionIndex);
        }

        private void Load()
        {
            currentRegionIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.RegionIndex, 0);
        }

        private void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.RegionIndex, currentRegionIndex);
            PlayerPrefs.Save();
        }

        public void LoadNextRegion()
        {
            currentRegionIndex++;
            LoadRegion(currentRegionIndex);
            Save();
        }

        public void LoadRegion(int index)
        {
            if(_activeRegion != null)
            {
                Destroy(_activeRegion.gameObject);
                _activeRegion = null;
            }

            if(index < 0 || index >= buildingsAnimationConfigs.Count)
            {
                Debug.LogWarning($"Invalid region index {index}");
                return;
            }

            var prefab = buildingsAnimationConfigs[index];
            _activeRegion = Instantiate(prefab, regionParent);
        }
    }
}