using System;
using System.Runtime.CompilerServices;
using Configs;
using Cysharp.Threading.Tasks;
using Data;
using Gley.DailyRewards.API;
using Gley.EasyIAP;
using Itic.Scopes;
using Models;
using Providers;
using Services;
using Services.InApp;
using Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Views;
using ZLinq;
using Object = UnityEngine.Object;

namespace Initializers
{
    public class CountryInitializer : IInitializable
    {
        private readonly MenuView _menuView;
        private readonly SceneLoader _sceneLoader;
        private readonly InAppView _inAppView;
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly StarModel _starModel;
        private readonly InternetState _internetState;
        private readonly InAppConfig _inAppConfig;
        private readonly BoostersProvider _boostersProvider;
        private readonly LevelConfig _levelConfig;
        private readonly RegionModel _regionModel;
        private readonly RegionUpgradeService _regionUpgradeService;
        private readonly HideUnhideScript _hideUnhideScript;
        private readonly HealthSystem _healthSystem;

        public CountryInitializer(MenuView menuView,
            SceneLoader sceneLoader,
            InAppView inAppView,
            CoinModel coinModel,
            GemModel gemModel,
            StarModel starModel,
            InternetState internetState,
            InAppConfig inAppConfig,
            BoostersProvider boostersProvider,
            LevelConfig levelConfig,
            RegionModel regionModel,
            RegionUpgradeService regionUpgradeService,
            HideUnhideScript hideUnhideScript,
            HealthSystem healthSystem
        )
        {
            _menuView = menuView;
            _sceneLoader = sceneLoader;
            _inAppView = inAppView;
            _coinModel = coinModel;
            _gemModel = gemModel;
            _starModel = starModel;
            _internetState = internetState;
            _inAppConfig = inAppConfig;
            _boostersProvider = boostersProvider;
            _levelConfig = levelConfig;
            _regionModel = regionModel;
            _regionUpgradeService = regionUpgradeService;
            _hideUnhideScript = hideUnhideScript;
            _healthSystem = healthSystem;
        }

        public void Initialize()
        {
            _menuView.Warmup();
            _inAppView.Warmup();

            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);
            _menuView.InAppButton.onClick.RemoveAllListeners();
            _menuView.InAppButton.onClick.AddListener(ShowInAppView);
            _menuView.MapButton.onClick.RemoveAllListeners();
            _menuView.MapButton.onClick.AddListener(BackToMap);

            _menuView.ShopOpenButton.onClick.RemoveAllListeners();
            _menuView.ShopOpenButton.onClick.AddListener(() => { Upgrade().Forget(); });

            // _inAppView.NoAdsButton.onClick.RemoveAllListeners();
            // _inAppView.NoAdsButton.onClick.AddListener(() =>
            //     {
            //         HandlePurchaseInApp(ShopProductNames.RemoveAds,
            //             () =>
            //             {
            //                 _internetState.HasRemoveAds = true;
            //                 _inAppView.NoAdsButton.gameObject.SetActive(false);
            //             },
            //             () => { }
            //         ).Forget();
            //     }
            // );
            // _inAppView.NoAdsButton.gameObject.SetActive(!_internetState.HasRemoveAds);

            foreach (var inAppProduct in _inAppConfig.InAppProducts)
            {
                var price = API.GetLocalizedPriceString(inAppProduct.product);

                var parent = _inAppView.ButtonsParent;

                var product = Object.Instantiate(_inAppConfig.ProductViewPrefab, parent);
                product.Init(inAppProduct.productName, inAppProduct.icon, price,
                    inAppProduct.amount.ToString());

                product.BuyButton.onClick.RemoveAllListeners();
                product.BuyButton.onClick.AddListener(() => { HandlePurchaseInApp(inAppProduct.product).Forget(); });
            }

            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            _menuView.StartGameText.SetText("LEVEL " + nextLevel);

            _regionUpgradeService.Initialize(_regionModel);
            Debug.Log(_regionModel.CurrentLevelProgress);
            // var model = _regionModel._buildingsAnimationConfig.data[_regionModel.CurrentLevelProgress - 1];

            _regionUpgradeService.JumpToFrame(0);
            if (_regionModel.CurrentLevelProgress != 0)
            {
                int endFrame = _regionModel._buildingsAnimationConfig.data[_regionModel.CurrentLevelProgress - 1]
                    .endFrame;
                Debug.Log(endFrame);
                _regionUpgradeService.JumpToFrame(endFrame);
            }
        }

        private async UniTask Upgrade()
        {
            if (_regionModel.CanUpgrade())
            {
                _regionModel.Upgrade();

                int endFrame = _regionModel._buildingsAnimationConfig.data[_regionModel.CurrentLevelProgress - 1]
                    .endFrame;

                _hideUnhideScript.OnEyeButtonClick();

                await _regionUpgradeService.PlayToFrame(endFrame);

                _hideUnhideScript.OnEyeButtonClick();
            }
        }

        private void BackToMap()
        {
            _sceneLoader.LoadMapAsync().Forget();
        }

        private void ShowInAppView()
        {
            _inAppView.Show();
        }


        private void StartGame()
        {
            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            if (_levelConfig.Testing)
            {
                nextLevel = _levelConfig.LevelToPlay;
            }

            if (_healthSystem.CanPlay)
            {
                _healthSystem.TryUseLife();
                PlayerPrefs.SetInt("OpenLevel", nextLevel);
                _sceneLoader.LoadGameAsync().Forget();
            }
        }

        private async UniTaskVoid HandlePurchaseInApp(ShopProductNames shopProduct)
        {
            var isBought = await InAppPurchasingService.TryBuyConsumableAsync(shopProduct);

            if (!isBought) return;

            var productConfig = _inAppConfig.InAppProducts.AsValueEnumerable().First(v => v.product == shopProduct);
            //TODO POKAZAT CHTO ON GEY POLUCHIL BABKI SVOI
            switch (shopProduct)
            {
                case ShopProductNames.CoinsSmall:
                    _coinModel.Increase(productConfig.amount);
                    break;
                case ShopProductNames.CoinsMedium:
                    _coinModel.Increase(productConfig.amount);
                    break;
                case ShopProductNames.DiamondSmall:
                    _gemModel.Increase(productConfig.amount);
                    break;
                case ShopProductNames.DiamondMedium:
                    _gemModel.Increase(productConfig.amount);
                    break;
            }
        }
    }
}