using Cysharp.Threading.Tasks;
using UnityEngine;
using Utils.Save;

namespace Configs
{
    public class BuildingAnimationSettingsProvider : MonoBehaviour
    {
        [SerializeField] private RegionConfig regionConfig;
        [SerializeField] private Transform regionParent;

        private BuildingsAnimationConfig _activeRegion;
        private RegionData _activeRegionData;

        public RegionConfig RegionConfig => regionConfig;
        public RegionData ActiveRegionData => _activeRegionData;
        public BuildingsAnimationConfig ActiveRegion => _activeRegion;
        public int CurrentRegionIndex => currentRegionIndex;

        private int currentRegionIndex;

        public bool CanLoadNextRegion()
        {
            var nextRegionIndex = currentRegionIndex + 1;
            return regionConfig != null && nextRegionIndex >= 0 && nextRegionIndex < regionConfig.Count;
        }

        public async UniTask Warmup()
        {
            Load();
            LoadRegion(currentRegionIndex);
            await UniTask.Yield();
        }

        private void Load()
        {
            currentRegionIndex = PlayerPrefs.GetInt(PlayerPrefsKeys.RegionIndex, 0);
            var maxCount = GetTotalRegionsCount();

            if (maxCount <= 0)
            {
                currentRegionIndex = -1;
                return;
            }

            if (currentRegionIndex < 0 || currentRegionIndex >= maxCount)
            {
                currentRegionIndex = 0;
                Save();
            }
        }

        private int GetTotalRegionsCount()
        {
            return regionConfig != null ? regionConfig.Count : 0;
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
            if (_activeRegion != null)
            {
                Destroy(_activeRegion.gameObject);
                _activeRegion = null;
            }

            _activeRegionData = null;

            if (regionConfig == null)
            {
                Debug.LogError("[BuildingAnimationSettingsProvider] RegionConfig is not assigned!");
                return;
            }

            if (index < 0 || index >= regionConfig.Count)
            {
                Debug.LogWarning($"[BuildingAnimationSettingsProvider] Invalid region index {index} (Total: {regionConfig.Count})");
                return;
            }

            _activeRegionData = regionConfig.GetRegionByIndex(index);

            if (_activeRegionData == null || _activeRegionData.regionPrefab == null)
            {
                Debug.LogWarning($"[BuildingAnimationSettingsProvider] Region data or regionPrefab is not assigned for index {index}");
                return;
            }

            var instance = Instantiate(_activeRegionData.regionPrefab, regionParent);
            instance.SetActive(true);
            _activeRegion = instance.GetComponent<BuildingsAnimationConfig>() ??
                            instance.GetComponentInChildren<BuildingsAnimationConfig>(true);

            SetupActiveRegionAnimator(instance);
        }

        private void SetupActiveRegionAnimator(GameObject instance)
        {
            if (_activeRegion == null && instance != null)
            {
                _activeRegion = instance.GetComponent<BuildingsAnimationConfig>() ??
                                instance.GetComponentInChildren<BuildingsAnimationConfig>(true);
            }

            if (_activeRegion != null)
            {
                if (_activeRegion.animator == null && instance != null)
                {
                    _activeRegion.animator = instance.GetComponent<Animator>() ??
                                             instance.GetComponentInChildren<Animator>(true);
                }

                if (_activeRegion.animator != null)
                {
                    _activeRegion.animator.speed = 1f;
                }
            }
        }
    }
}
