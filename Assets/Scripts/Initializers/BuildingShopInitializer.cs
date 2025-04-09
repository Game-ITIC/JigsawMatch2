using Configs;
using Monobehaviours.Buildings;
using UnityEngine;
using VContainer.Unity;

namespace Initializers
{
    public class BuildingShopInitializer : IInitializable
    {
        private readonly BuildingShopManager _buildingShopManager;
        private readonly Models.StarModel _starModel;
        private readonly CountryConfig _countryConfig;

        public BuildingShopInitializer(
            BuildingShopManager buildingShopManager,
            Models.StarModel starModel,
            CountryConfig countryConfig)
        {
            _buildingShopManager = buildingShopManager;
            _starModel = starModel;
            _countryConfig = countryConfig;
        }

        public void Initialize()
        {
            _buildingShopManager.Initialize(_starModel, _countryConfig);
        }
    }
}