using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Configs;
using Cysharp.Threading.Tasks;
using Data;
using Gley.DailyRewards.API;
using Gley.EasyIAP;
using Itic.Scopes;
using Models;
using Providers;
using R3;
using Services;
using Services.InApp;
using Systems;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Views;
using ZLinq;
using Object = UnityEngine.Object;

namespace Initializers
{
    public class CountryInitializer : IAsyncStartable, IDisposable
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
        private readonly RegionUIProvider _regionUIProvider;
        private readonly RegionConfig _regionConfig;
        private readonly MenuTabs _menuTabs;
        private readonly LifePopup _lifePopup;
        private readonly AdRewardService _adRewardService;
        private readonly IronSourceManager _ironSourceManager;
        private readonly AdEventModel _adEventModel;
        private readonly RewardPopup _rewardPopup;

        private CompositeDisposable _disposable = new();

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
            HealthSystem healthSystem,
            RegionUIProvider regionUIProvider,
            RegionConfig regionConfig,
            MenuTabs menuTabs,
            LifePopup lifePopup,
            AdRewardService adRewardService,
            IronSourceManager ironSourceManager,
            AdEventModel adEventModel,
            RewardPopup rewardPopup
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
            _regionUIProvider = regionUIProvider;
            _regionConfig = regionConfig;
            _menuTabs = menuTabs;
            _lifePopup = lifePopup;
            _adRewardService = adRewardService;
            _ironSourceManager = ironSourceManager;
            _adEventModel = adEventModel;
            _rewardPopup = rewardPopup;
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            await _menuView.Warmup();
            await _inAppView.Warmup();
            await _menuTabs.Warmup();

            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);

            _lifePopup.AdsButton.onClick.RemoveAllListeners();
            _lifePopup.AdsButton.onClick.AddListener(() =>
            {
                _adRewardService.SetAdRewardType(AdRewardType.Life);

                _ironSourceManager.ShowRewardedAd();
            });

            _adEventModel.OnRewardGranted.Subscribe(_ =>
            {
                _lifePopup.Hide();
                _rewardPopup.Show();
            }).AddTo(_disposable);

            _lifePopup.BuyButton.onClick.RemoveAllListeners();
            _lifePopup.BuyButton.onClick.AddListener(() =>
            {
                if (_gemModel.Gems.Value >= 15)
                {
                    _healthSystem.AddLives(1);
                    _gemModel.Decrease(15);
                    _lifePopup.Hide();
                }
            });

            // _menuView.InAppButton.onClick.RemoveAllListeners();
            // _menuView.InAppButton.onClick.AddListener(ShowInAppView);
            // _menuView.MapButton.onClick.RemoveAllListeners();
            // _menuView.MapButton.onClick.AddListener(BackToMap);
            //
            _menuView.BuildButton.onClick.RemoveAllListeners();
            _menuView.BuildButton.onClick.AddListener(() => { Upgrade().Forget(); });

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

            foreach (var regionName in _regionConfig.Regions)
            {
                var region = Object.Instantiate(_regionUIProvider.RegionUIViewPrefab,
                    _regionUIProvider.RegionUIViewParent);
                region.SetName(regionName);
                if (regionName != "Soon")
                {
                    var max = _regionModel._buildingsAnimationConfig.data.Count - 1;
                    var current = _regionModel.CurrentLevelProgress;

                    region.SetProgress(current, max);
                    _regionModel.CurrentLevelProgressReactiveProperty.Subscribe(v =>
                    {
                        var max = _regionModel._buildingsAnimationConfig.data.Count - 1;
                        var current = _regionModel.CurrentLevelProgress;

                        region.SetProgress(current, max);
                    }).AddTo(region);
                }
            }

            _regionUpgradeService.Initialize(_regionModel);
            // var model = _regionModel._buildingsAnimationConfig.data[_regionModel.CurrentLevelProgress - 1];

            _regionUpgradeService.JumpToFrame(0);
            if (_regionModel.CurrentLevelProgress != 0)
            {
                int endFrame = _regionModel._buildingsAnimationConfig.data[_regionModel.CurrentLevelProgress - 1]
                    .endFrame;
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
                PlayerPrefs.SetInt("OpenLevel", nextLevel);
                _sceneLoader.LoadGameAsync().Forget();
            }
            else
            {
                _lifePopup.Show();
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

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}