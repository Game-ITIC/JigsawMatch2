using Itic.Services;
using Models;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class StartupLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {

            // builder.Register<AdEventModel>(Lifetime.Singleton);

            builder.RegisterEntryPoint<Startup>();
        }
    }
}