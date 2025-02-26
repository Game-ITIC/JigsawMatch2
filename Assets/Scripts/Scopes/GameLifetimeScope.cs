using System;
using System.Collections.Generic;
using Core.Grid.Factories;
using Core.Grid.Interfaces;
using Initializers;
using Loaders;
using Monobehaviours;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        // [SerializeField] private GameEventDispatcher gameEventDispatcher;
        [SerializeField] private Button noAds;
        [SerializeField] private TileViewFactory tileViewFactory;

        protected override void Configure(IContainerBuilder builder)
        {
            // builder.RegisterComponent<IGameEvents>(gameEventDispatcher).AsSelf();

            builder.Register<ILevelAssetLoader, ResourcesLevelAssetLoader>(Lifetime.Scoped)
                .WithParameter("assetPath", "Levels");

            builder.Register<IGridFactory, GridFactory>(Lifetime.Scoped);
            builder.Register<ITileFactory, TileFactory>(Lifetime.Scoped).WithParameter("possibleTileIds", new List<string> {"Red"});
            builder.RegisterComponent<ITileViewFactory>(tileViewFactory);

            builder.Register<ILevelService, LevelService>(Lifetime.Scoped);

            builder.RegisterEntryPoint<GameInitializer>()
                .WithParameter("noAds", noAds);
        }
    }
}