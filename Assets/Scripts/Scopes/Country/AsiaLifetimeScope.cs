using Initializers;
using Monobehaviours.Buildings;
using Presenters;
using Scopes.Country;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Views;

public class AsiaLifetimeScope : BaseCountryLifetimeScope
{
    protected override void ConfigureCountry(IContainerBuilder builder)
    {
        base.ConfigureCountry(builder);
    }
}