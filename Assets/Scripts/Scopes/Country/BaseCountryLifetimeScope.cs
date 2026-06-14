using System;
using Configs;
using Initializers;
using Models;
using Monobehaviours.Buildings;
using Presenters;
using Providers;
using Services;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Views;

namespace Scopes.Country
{
    public class BaseCountryLifetimeScope : LifetimeScope
    {
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
            // Create a new CountryConfig ScriptableObject
            CountryConfig newConfig = ScriptableObject.CreateInstance<CountryConfig>();

            // Create a save file dialog to let the user choose where to save
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save Country Config",
                "NewCountryConfig",
                "asset",
                "Please enter a file name to save the country configuration to");

            if(!string.IsNullOrEmpty(path))
            {
                // Save the asset and refresh the AssetDatabase
                UnityEditor.AssetDatabase.CreateAsset(newConfig, path);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();

                // Assign the newly created config to our field
                countryConfig = newConfig;

                // Ping the new asset in the Project window
                UnityEditor.EditorGUIUtility.PingObject(newConfig);
            }
#endif
        }

        [Tooltip("Main menu view controller")] [SerializeField]
        private MenuView menuView;

        [Tooltip("Enable old Gley DailyRewards Calendar on the assigned daily button.")]
        [SerializeField] private bool enableLegacyDailyRewards;

        [Tooltip("Manager for the building shop system")] [SerializeField]
        private BuildingShopManager buildingShopManager;

        [Space(10)] [Title("UI Components")] [LabelWidth(130)] [LabelText("Coins Display")] [Tooltip("Text view for displaying player's coins")] [SerializeField]
        private TextView coinTextView;

        [LabelText("Stars Display")] [Tooltip("Text view for displaying player's stars")] [SerializeField]
        private TextView starTextView;

        [LabelText("Gems Display")] [Tooltip("Text view for displaying player's gems")] [SerializeField]
        private TextView gemTextView;

        [LabelText("Lifes Display")] [Tooltip("Text view for displaying player's lifes")] [SerializeField]
        private TextView lifeTextView;

        [SerializeField] private InAppView inAppView;

        [SerializeField] private HideUnhideScript hideUnhideScript;

        [SerializeField] private RegionUIProvider regionUIProvider;
        [SerializeField] private RegionConfig regionConfig;

        [SerializeField] private MenuNavigationProvider menuNavigationProvider;

        [SerializeField] private LifePopup lifePopup;
        [SerializeField] private RewardPopup rewardPopup;
        [SerializeField] private BuildingAnimationSettingsProvider settingsProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterComponentIfPresent(builder, menuView);
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

            if(coinTextView != null)
            {
                builder.Register<CoinPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(coinTextView);
            }

            if(starTextView != null)
            {
                builder.Register<StarPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(starTextView);
            }

            if(gemTextView != null)
            {
                builder.Register<GemPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(gemTextView);
            }

            if(buildingShopManager != null && countryConfig != null)
            {
                builder.Register<BuildingShopInitializer>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .AsSelf();
            }

            if(enableLegacyDailyRewards && menuView != null && menuView.DailyRewardsButton != null)
            {
                builder.Register<DailyRewardsPresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(menuView.DailyRewardsButton);
            }

            if(lifeTextView != null)
            {
                builder.Register<LifePresenter>(Lifetime.Scoped)
                    .As<IInitializable>()
                    .WithParameter(lifeTextView);
            }

            if(settingsProvider != null)
            {
                builder.Register<RegionModel>(Lifetime.Singleton);
                builder.Register<RegionUpgradeService>(Lifetime.Singleton);
            }

            if(menuNavigationProvider != null)
            {
                builder.Register<MenuTabs>(Lifetime.Singleton);
            }

            ConfigureCountry(builder);
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
