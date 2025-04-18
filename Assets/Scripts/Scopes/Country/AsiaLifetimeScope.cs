using VContainer;

namespace Scopes.Country
{
    public class AsiaLifetimeScope : BaseCountryLifetimeScope
    {
        protected override void ConfigureCountry(IContainerBuilder builder)
        {
            base.ConfigureCountry(builder);
        }
    }
}