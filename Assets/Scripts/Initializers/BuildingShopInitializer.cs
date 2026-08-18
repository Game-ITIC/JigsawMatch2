using Configs;
using Monobehaviours.Buildings;
using UnityEngine;
using VContainer.Unity;

namespace Initializers
{
    public class BuildingShopInitializer : IInitializable
    {
        private readonly BuildingShopManager _buildingShopManager;
        private readonly Systems.CurrencySystem.Interfaces.ICurrencyService _currencyService;
        private readonly CountryConfig _countryConfig;

        public BuildingShopInitializer(
            BuildingShopManager buildingShopManager,
            Systems.CurrencySystem.Interfaces.ICurrencyService currencyService,
            CountryConfig countryConfig)
        {
            _buildingShopManager = buildingShopManager;
            _currencyService = currencyService;
            _countryConfig = countryConfig;
        }

        public void Initialize()
        {
            _buildingShopManager.Initialize(_currencyService, _countryConfig);
        }
    }
}