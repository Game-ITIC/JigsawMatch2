using Initializers;
using Monobehaviours;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameEventDispatcher gameEventDispatcher;
        [SerializeField] private Button noAds;
        [SerializeField] private GameCompleteView gameCompleteView;
        [SerializeField] private GamePauseView gamePauseView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private LevelManager levelManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(gameCompleteView);
            builder.RegisterComponent(gamePauseView);
            builder.RegisterComponent(gameOverView);
            builder.RegisterComponent(levelManager);
            
            builder.RegisterComponent<IGameEvents>(gameEventDispatcher).AsSelf();
            
            builder.RegisterEntryPoint<GameInitializer>()
                .WithParameter("noAds", noAds);
        }
    }
}