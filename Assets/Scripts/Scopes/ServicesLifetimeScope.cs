using Configs;
using Data;
using Itic.Scopes;
using Itic.Services;
using Models;
using Providers;
using Services;
using UnityEngine;
using Utils.Debug;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes
{
    public class ServicesLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenView loadingScreenView;
        [SerializeField] private InAppConfig inAppConfig;
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private SystemDebug systemDebug;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingScreenView);
            builder.RegisterComponent(systemDebug);
            builder.RegisterInstance(inAppConfig);
            builder.RegisterInstance(levelConfig);
            
            builder.Register<InternetChecker>(Lifetime.Singleton);
            builder.Register<InternetState>(Lifetime.Singleton);
            builder.Register<BoostersProvider>(Lifetime.Singleton);
            builder.Register<CoinModel>(Lifetime.Singleton);
            builder.Register<GemModel>(Lifetime.Singleton);
            builder.Register<LifeModel>(Lifetime.Singleton);
            builder.Register<Models.StarModel>(Lifetime.Singleton);

            builder.Register<SceneModel>(Lifetime.Singleton);
            builder.Register<ScreenService>(Lifetime.Singleton).AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<InitializationQueue>(Lifetime.Singleton);

            builder.RegisterEntryPoint<ServicesStartup>();
        }
    }
}