using Initializers;
using Monobehaviours.Buildings;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class AsiaLifetimeScope : LifetimeScope
{
    [SerializeField] private MenuView menuView;
    [SerializeField] private BuildingShopManager buildingShopManager;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(menuView);
        builder.RegisterComponent(buildingShopManager).WithParameter("countryId", "Asia");

        builder.RegisterEntryPoint<AsiaInitializer>();
    }
}