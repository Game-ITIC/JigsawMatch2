using Itic.Scopes;
using Itic.Services;
using Models;
using Providers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes
{
    public class ServicesLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenView loadingScreenView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingScreenView);

            builder.Register<BoostersProvider>(Lifetime.Singleton);
            builder.Register<CoinModel>(Lifetime.Singleton);
            builder.Register<GemModel>(Lifetime.Singleton);
            builder.Register<LifeModel>(Lifetime.Singleton);
            builder.Register<Models.StarModel>(Lifetime.Singleton);

            builder.Register<ScreenService>(Lifetime.Singleton).AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<InitializationQueue>(Lifetime.Singleton);

            builder.RegisterEntryPoint<ServicesStartup>();
        }
    }
}