using System.Collections.Generic;
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
            var nextRegionIndex = currentRegionIndex + 1;

            if(nextRegionIndex < 0 || nextRegionIndex >= buildingsAnimationConfigs.Count)
            {
                Debug.LogWarning($"No region is configured after index {currentRegionIndex}");
                return false;
            }

            return true;
        }

        public async UniTask Warmup()
        {
            Load();
            // _activeRegion = buildingsAnimationConfigs[0];
            LoadRegion(currentRegionIndex);
        }

        private void Load()
        {
            currentRegionIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.RegionIndex, 0);

            if(buildingsAnimationConfigs == null || buildingsAnimationConfigs.Count == 0)
            {
                currentRegionIndex = -1;
                return;
            }

            if(currentRegionIndex < 0 || currentRegionIndex >= buildingsAnimationConfigs.Count)
            {
                currentRegionIndex = 0;
                Save();
            }
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

            if(prefab == null)
            {
                Debug.LogWarning($"Region config at index {index} is not assigned");
                return;
            }

            _activeRegion = Instantiate(prefab, regionParent);
            _activeRegion.gameObject.SetActive(true);
        }
    }
}