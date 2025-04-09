using Configs;
using Initializers;
using Monobehaviours.Buildings;
using Presenters;
using Sirenix.OdinInspector;
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

            if (!string.IsNullOrEmpty(path))
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

        [Tooltip("Manager for the building shop system")] [SerializeField]
        private BuildingShopManager buildingShopManager;

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

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(menuView);
            builder.RegisterComponent(buildingShopManager);

            builder.RegisterInstance(countryConfig);
            
            builder.Register<CoinPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(coinTextView);

            builder.Register<StarPresenter>(Lifetime.Scoped)
                .As<IInitializable>()
                .WithParameter(starTextView);

            builder.Register<BuildingShopInitializer>(Lifetime.Scoped)
                .As<IInitializable>()
                .AsSelf();

            ConfigureCountry(builder);
        }

        protected virtual void ConfigureCountry(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<CountryInitializer>();
        }
    }
}