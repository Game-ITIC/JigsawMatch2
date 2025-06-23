using Initializers;
using Meta.Quests.Configs;
using Meta.Quests.Interfaces;
using Meta.Quests.Services;
using Meta.Quests.Views;
using Presenters;
using Providers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes
{
    public class IslandLifetimeScope : LifetimeScope
    {
        [SerializeField] private IslandProvider islandProvider;
        [Space(10)]
        [Title("UI Components")]
        [LabelWidth(130)]
        [LabelText("Coins Display")]
        [Tooltip("Text view for displaying player's coins")]
        [SerializeField]
        private TextView coinTextView;

        [LabelText("Stars Display")] [Tooltip("Text view for displaying player's stars")] [SerializeField]
        private TextView starTextView;

        [LabelText("Gems Display")] [Tooltip("Text view for displaying player's gems")] [SerializeField]
        private TextView gemTextView;

        [SerializeField] private Button dailyButton;
        [SerializeField] private CameraProvider cameraProvider;

        [SerializeField] private DailyQuestSettings dailyQuestSettings;
        [SerializeField] private QuestTemplate questTemplate;
        [FormerlySerializedAs("dailyQuestsView")] [SerializeField] private DailyQuestView dailyQuestView;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(islandProvider);
            builder.RegisterComponent(cameraProvider);
            builder.RegisterComponent(dailyQuestView);
            builder.RegisterInstance(dailyQuestSettings);
            builder.RegisterInstance(questTemplate);
            
            builder.Register<CoinPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(coinTextView);

            builder.Register<StarPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(starTextView);

            builder.Register<GemPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(gemTextView);

            builder.Register<DailyRewardsPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(dailyButton);

            builder.Register<IDailyQuestService, DailyQuestService>(Lifetime.Singleton);
            builder.Register<IQuestProgressTracker, DailyQuestService>(Lifetime.Singleton);
            builder.Register<IQuestDataStorage, PlayerPrefsQuestStorage>(Lifetime.Singleton);
            builder.Register<IQuestGenerator, QuestGenerator>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<IslandInitializer>();
        }
    }
}