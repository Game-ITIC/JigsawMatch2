using Configs;
using Data;
using Initializers;
using Models;
using Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private IronSourceInitializer ironSourceInitializer;
        [SerializeField] private IronSourceManager ironSourceManager;
        [SerializeField] private IronSourceConfigSO ironSourceConfig;
        [SerializeField] private SceneLoadingConfig sceneLoadingConfig;
        [SerializeField] private LoadingScreenView loadingScreenView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(ironSourceInitializer);
            builder.RegisterComponent(ironSourceManager);
            builder.RegisterComponent(ironSourceConfig);
            builder.RegisterComponent(sceneLoadingConfig);
            builder.RegisterComponent(loadingScreenView);

            builder.Register<InternetState>(Lifetime.Singleton);
            builder.Register<InternetChecker>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton).AsSelf();

            builder.Register<CoinModel>(Lifetime.Singleton);
            builder.Register<LifeModel>(Lifetime.Singleton);
            builder.Register<Models.StarModel>(Lifetime.Singleton);

            builder.RegisterEntryPoint<ProjectInitializer>();
        }
    }
}