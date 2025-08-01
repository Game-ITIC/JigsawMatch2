using System;
using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    public class BuildingAnimationSettingsProvider : MonoBehaviour
    {
        [SerializeField] private List<BuildingsAnimationConfig> buildingsAnimationConfigs;
        [SerializeField] private Transform regionParent;
        private BuildingsAnimationConfig _activeRegion;

        public BuildingsAnimationConfig ActiveRegion => _activeRegion;

        private void Awake()
        {
            // TODO Change
            _activeRegion = buildingsAnimationConfigs[0];
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