using System;
using Configs;
using Configs.Tasks;
using Initializers;
using Meta.Quests.Interfaces;
using Meta.Quests.Services;
using Models;
using Monobehaviours.Buildings;
using Presenters;
using Providers;
using Services;
using Services.Tasks;
using Sirenix.OdinInspector;
using Systems.CurrencySystem;
using Systems.CurrencySystem.Interfaces;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes.Country
{
    public class BaseCountryLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuPanel _mainMenuPanel;
        [Title("Core Components")]
        [BoxGroup("CountrySection")]
        [VerticalGroup("CountrySection/Row")]
        [LabelWidth(130)]
        [LabelText("Country Config")]
        [Tooltip("Configuration settings for countries")]
        [SerializeField]
        private CountryConfig countryConfig;

        [VerticalGroup("CountrySection/Row")]
        [ShowIf("@countryConfig == null")]
        [Button("Create")]
        [GUIColor(0.7f, 0.9f, 0.7f)]
        private void CreateCountryConfig()
        {
#if UNITY_EDITOR
            var newConfig = ScriptableObject.CreateInstance<CountryConfig>();
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save Country Config",
                "NewCountryConfig",
                "asset",
                "Please enter a file name to save the country configuration to");

            if (!string.IsNullOrEmpty(path))
            {
                UnityEditor.AssetDatabase.CreateAsset(newConfig, path);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
                countryConfig = newConfig;
                UnityEditor.EditorGUIUtility.PingObject(newConfig);
            }
#endif
        }

        [BoxGroup("CountrySection")]
        [VerticalGroup("CountrySection/Row")]
        [LabelWidth(130)]
        [LabelText("Building Shop Manager")]
        [Tooltip("Manager for building shop operations")]
        [SerializeField]
        private BuildingShopManager buildingShopManager;

        [VerticalGroup("CountrySection/Row")]
        [ShowIf("@buildingShopManager == null")]
        [Button("Find")]
        [GUIColor(0.7f, 0.9f, 0.7f)]
        private void FindBuildingShopManager()
        {
            buildingShopManager = FindObjectOfType<BuildingShopManager>(true);
        }

        [BoxGroup("CountrySection")]
        [VerticalGroup("CountrySection/Row")]
        [LabelWidth(130)]
        [LabelText("InApp View")]
        [Tooltip("View for InApp purchases")]
        [SerializeField]
        private InAppView inAppView;

        [VerticalGroup("CountrySection/Row")]
        [ShowIf("@inAppView == null")]
        [Button("Find")]
        [GUIColor(0.7f, 0.9f, 0.7f)]
        private void FindInAppView()
        {
            inAppView = FindObjectOfType<InAppView>(true);
        }

        [SerializeField] private HideUnhideScript hideUnhideScript;

        [SerializeField] private RegionUIProvider regionUIProvider;
        [SerializeField] private RegionConfig regionConfig;

        [SerializeField] private MenuNavigationProvider menuNavigationProvider;

        [SerializeField] private LifePopup lifePopup;
        [SerializeField] private RewardPopup rewardPopup;
        [SerializeField] private BuildingAnimationSettingsProvider settingsProvider;

        [Title("Tasks")]
        [FormerlySerializedAs("dailyQuestSettings")]
        [SerializeField] private TasksListSO tasksListSO;

        protected override void Configure(IContainerBuilder builder)
        {
            ResolveOptionalSceneReferences();

            RegisterComponentIfPresent(builder, _mainMenuPanel);
            RegisterComponentIfPresent(builder, buildingShopManager);
            RegisterComponentIfPresent(builder, inAppView);
            RegisterComponentIfPresent(builder, hideUnhideScript);
            RegisterComponentIfPresent(builder, regionUIProvider);
            RegisterComponentIfPresent(builder, menuNavigationProvider);
            RegisterComponentIfPresent(builder, lifePopup);
            RegisterComponentIfPresent(builder, rewardPopup);
            RegisterComponentIfPresent(builder, settingsProvider);

            RegisterInstanceIfPresent(builder, countryConfig);
            RegisterInstanceIfPresent(builder, regionConfig);
            RegisterInstanceIfPresent(builder, tasksListSO);

            builder.Register<ITaskService, TaskService>(Lifetime.Singleton);

            var topBarPanel = (_mainMenuPanel != null && _mainMenuPanel.TopBarPanel != null)
                ? _mainMenuPanel.TopBarPanel
                : FindObjectOfType<TopBarPanel>(true);

            var settingsPanel = (_mainMenuPanel != null && _mainMenuPanel.SettingsPanel != null)
                ? _mainMenuPanel.SettingsPanel
                : FindObjectOfType<SettingsPanel>(true);

            if(settingsPanel != null)
            {
                var settingsButton = topBarPanel != null ? topBarPanel.SettingsButton : null;

                builder.Register<SettingsPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(settingsPanel)
                    .WithParameter(settingsButton);
            }

            var taskPanel = (_mainMenuPanel != null && _mainMenuPanel.TaskPanel != null)
                ? _mainMenuPanel.TaskPanel
                : FindObjectOfType<TaskPanel>(true);

            if(taskPanel != null)
            {
                var taskButton = topBarPanel != null ? topBarPanel.TaskButton : null;

                builder.Register<TaskPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(taskPanel)
                    .WithParameter(taskButton);
            }

            var effectiveInAppView = inAppView != null ? inAppView : (_mainMenuPanel != null ? _mainMenuPanel.ShopPanel : null);
            if(effectiveInAppView != null)
            {
                Button navShopButton = null;
                var otherNavButtons = new System.Collections.Generic.List<Button>();

                if(_mainMenuPanel != null && _mainMenuPanel.MenuNavPanel != null)
                {
                    navShopButton = _mainMenuPanel.MenuNavPanel.ShopButton;
                    if(_mainMenuPanel.MenuNavPanel.MenuButton != null) otherNavButtons.Add(_mainMenuPanel.MenuNavPanel.MenuButton);
                    if(_mainMenuPanel.MenuNavPanel.IslandButton != null) otherNavButtons.Add(_mainMenuPanel.MenuNavPanel.IslandButton);
                }

                if(navShopButton == null && menuNavigationProvider != null && menuNavigationProvider.NavigationButtons != null)
                {
                    if(menuNavigationProvider.NavigationButtons.Length > 2)
                    {
                        navShopButton = menuNavigationProvider.NavigationButtons[2];
                    }

                    for(int i = 0; i < menuNavigationProvider.NavigationButtons.Length; i++)
                    {
                        if(i != 2 && menuNavigationProvider.NavigationButtons[i] != null)
                        {
                            otherNavButtons.Add(menuNavigationProvider.NavigationButtons[i]);
                        }
                    }
                }

                builder.Register<ShopPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(effectiveInAppView)
                    .WithParameter(navShopButton)
                    .WithParameter<System.Collections.Generic.IEnumerable<Button>>(otherNavButtons);
            }

            if(topBarPanel != null)
            {
                if(topBarPanel.StarView != null)
                {
                    builder.Register<CurrencyPresenter>(Lifetime.Scoped)
                        .As<IInitializable>()
                        .WithParameter<ICurrencyView>(topBarPanel.StarView)
                        .WithParameter(CurrencyType.Star);
                }

                if(topBarPanel.GemView != null)
                {
                    builder.Register<CurrencyPresenter>(Lifetime.Scoped)
                        .As<IInitializable>()
                        .WithParameter<ICurrencyView>(topBarPanel.GemView)
                        .WithParameter(CurrencyType.Diamond);
                }

                if(topBarPanel.HealthBarView != null)
                {
                    builder.Register<LifePresenter>(Lifetime.Scoped)
                        .As<IInitializable>()
                        .WithParameter(topBarPanel.HealthBarView);
                }
            }

            if(buildingShopManager != null && countryConfig != null)
            {
                builder.Register<BuildingShopInitializer>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .AsSelf();
            }

            builder.Register<DailyCardsPresenter>(Lifetime.Scoped)
                .As<IInitializable>();

            if(settingsProvider != null)
            {
                builder.Register<RegionModel>(Lifetime.Singleton);
                builder.Register<RegionUpgradeService>(Lifetime.Singleton);
            }

            if(menuNavigationProvider != null)
            {
                builder.Register<MenuTabs>(Lifetime.Singleton);
            }

            if(tasksListSO != null)
            {
                builder.Register<RewardService>(Lifetime.Singleton);

                builder.Register<IDailyQuestService, DailyQuestService>(Lifetime.Singleton);
                builder.Register<IQuestProgressTracker, DailyQuestService>(Lifetime.Singleton);
                builder.Register<IQuestDataStorage, PlayerPrefsQuestStorage>(Lifetime.Singleton);
                builder.Register<IQuestGenerator, QuestGenerator>(Lifetime.Singleton);
            }

            ConfigureCountry(builder);
        }

        private void ResolveOptionalSceneReferences()
        {
            if(_mainMenuPanel == null)
            {
                _mainMenuPanel = FindObjectOfType<MainMenuPanel>(true);
            }

            if(inAppView == null && _mainMenuPanel != null && _mainMenuPanel.ShopPanel != null)
            {
                inAppView = _mainMenuPanel.ShopPanel;
            }
            else if(inAppView == null)
            {
                inAppView = GetComponentInChildren<InAppView>(true);
            }
        }

        private static void RegisterComponentIfPresent<T>(IContainerBuilder builder, T component)
            where T : Component
        {
            if(component != null)
            {
                builder.RegisterComponent(component);
            }
        }

        private static void RegisterInstanceIfPresent<T>(IContainerBuilder builder, T instance)
            where T : class
        {
            if(instance != null)
            {
                builder.RegisterInstance(instance);
            }
        }

        protected virtual void ConfigureCountry(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CountryInitializer>();
        }
    }
}
