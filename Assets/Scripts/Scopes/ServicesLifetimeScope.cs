using Configs;
using Data;
using Initializers;
using Itic.Scopes;
using Itic.Services;
using Models;
using Providers;
using Services;
using Systems;
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

        [SerializeField] private IronSourceInitializer ironSourceInitializer;
        [SerializeField] private IronSourceManager ironSourceManager;
        [SerializeField] private IronSourceConfigSO ironSourceConfig;
        [SerializeField] private SceneLoadingConfig sceneLoadingConfig;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(loadingScreenView);
            builder.RegisterComponent(systemDebug);
            builder.RegisterInstance(inAppConfig);
            builder.RegisterInstance(levelConfig);

            builder.RegisterComponent(ironSourceInitializer);
            builder.RegisterComponent(ironSourceManager);
            builder.RegisterComponent(ironSourceConfig);
            builder.RegisterComponent(sceneLoadingConfig);


            builder.Register<InternetChecker>(Lifetime.Singleton);
            builder.Register<InternetState>(Lifetime.Singleton);
            builder.Register<BoostersProvider>(Lifetime.Singleton);
            builder.Register<CoinModel>(Lifetime.Singleton);
            builder.Register<GemModel>(Lifetime.Singleton);
            builder.Register<LifeModel>(Lifetime.Singleton);
            builder.Register<Models.StarModel>(Lifetime.Singleton);
            
            builder.Register<HealthSystem>(Lifetime.Singleton);

            builder.Register<SceneModel>(Lifetime.Singleton);
            builder.Register<ScreenService>(Lifetime.Singleton).AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<InitializationQueue>(Lifetime.Singleton);

            builder.Register<AdEventModel>(Lifetime.Singleton);
            builder.Register<AdsInitializer>(Lifetime.Singleton);
            builder.Register<AdRewardService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterEntryPoint<ServicesStartup>();
        }
    }
}