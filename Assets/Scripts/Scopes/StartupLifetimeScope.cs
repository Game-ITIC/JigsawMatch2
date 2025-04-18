using Itic.Services;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class StartupLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Startup>();
        }
    }
}