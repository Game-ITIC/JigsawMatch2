using Initializers;
using Monobehaviours;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameEventDispatcher gameEventDispatcher;
        [SerializeField] private Button noAds;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent<IGameEvents>(gameEventDispatcher).AsSelf();

            builder.RegisterEntryPoint<GameInitializer>()
                .WithParameter("noAds", noAds);
        }
    }
}