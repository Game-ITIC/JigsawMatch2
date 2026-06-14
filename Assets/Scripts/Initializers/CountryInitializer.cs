using System;
using System.Threading;
using Configs;
using Cysharp.Threading.Tasks;
using Data;
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
using VContainer;
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
        private BuildingAnimationSettingsProvider _settingsProvider;

        public CountryInitializer(MenuView menuView,
            SceneLoader sceneLoader,
            CoinModel coinModel,
            GemModel gemModel,
            StarModel starModel,
            InternetState internetState,
            InAppConfig inAppConfig,
            BoostersProvider boostersProvider,
            LevelConfig levelConfig,
            HealthSystem healthSystem,
            AdRewardService adRewardService,
            IronSourceManager ironSourceManager,
            AdEventModel adEventModel,
            IObjectResolver resolver)
        {
            _menuView = menuView;
            _sceneLoader = sceneLoader;
            _coinModel = coinModel;
            _gemModel = gemModel;
            _starModel = starModel;
            _internetState = internetState;
            _inAppConfig = inAppConfig;
            _boostersProvider = boostersProvider;
            _levelConfig = levelConfig;
            _healthSystem = healthSystem;
            _adRewardService = adRewardService;
            _ironSourceManager = ironSourceManager;
            _adEventModel = adEventModel;
            _inAppView = resolver.ResolveOrDefault<InAppView>();
            _regionModel = resolver.ResolveOrDefault<RegionModel>();
            _regionUpgradeService = resolver.ResolveOrDefault<RegionUpgradeService>();
            _hideUnhideScript = resolver.ResolveOrDefault<HideUnhideScript>();
            _regionUIProvider = resolver.ResolveOrDefault<RegionUIProvider>();
            _regionConfig = resolver.ResolveOrDefault<RegionConfig>();
            _menuTabs = resolver.ResolveOrDefault<MenuTabs>();
            _lifePopup = resolver.ResolveOrDefault<LifePopup>();
            _rewardPopup = resolver.ResolveOrDefault<RewardPopup>();
            _settingsProvider = resolver.ResolveOrDefault<BuildingAnimationSettingsProvider>();
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            if(_settingsProvider != null)
            {
                await _settingsProvider.Warmup();
            }

            await _menuView.Warmup();

            if(_inAppView != null)
            {
                await _inAppView.Warmup();
            }

            if(_menuTabs != null)
            {
                await _menuTabs.Warmup();
            }

            if(_menuView.StartGame != null)
            {
                _menuView.StartGame.onClick.RemoveAllListeners();
                _menuView.StartGame.onClick.AddListener(StartGame);
            }

            InitializeLifePopup();

            // _menuView.InAppButton.onClick.RemoveAllListeners();
            // _menuView.InAppButton.onClick.AddListener(ShowInAppView);
            // _menuView.MapButton.onClick.RemoveAllListeners();
            // _menuView.MapButton.onClick.AddListener(BackToMap);
            //
            if(_menuView.BuildButton != null && _regionModel != null && _regionUpgradeService != null)
            {
                _menuView.BuildButton.onClick.RemoveAllListeners();
                _menuView.BuildButton.onClick.AddListener(() => { Upgrade().Forget(); });
            }

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

            if(_inAppView != null && _inAppConfig != null && _inAppView.ButtonsParent != null)
            {
                foreach (var inAppProduct in _inAppConfig.InAppProducts)
                {
                    var price = API.GetLocalizedPriceString(inAppProduct.product);

                    var parent = _inAppView.ButtonsParent;

                    var product = Object.Instantiate(_inAppConfig.ProductViewPrefab, parent);
                    product.Init(inAppProduct.productName,
                                 inAppProduct.icon,
                                 price,
                                 inAppProduct.amount.ToString());

                    product.BuyButton.onClick.RemoveAllListeners();
                    product.BuyButton.onClick.AddListener(() => { HandlePurchaseInApp(inAppProduct.product).Forget(); });
                }
            }

            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            if(_menuView.StartGameText != null)
            {
                _menuView.StartGameText.SetText("LEVEL " + nextLevel);
            }

            if(_regionConfig != null && _regionUIProvider != null && _regionModel != null)
            {
                foreach (var regionName in _regionConfig.Regions)
                {
                    var region = Object.Instantiate(_regionUIProvider.RegionUIViewPrefab,
                                                    _regionUIProvider.RegionUIViewParent);
                    region.SetName(regionName);

                    if(regionName != "Soon" && _regionModel._settingsProvider.ActiveRegion != null)
                    {
                        var max = _regionModel._settingsProvider.ActiveRegion.data.Count - 1;
                        var current = _regionModel.CurrentLevelProgress;

                        region.SetProgress(current, max);
                        _regionModel.CurrentLevelProgressReactiveProperty.Subscribe(v =>
                                                                                    {
                                                                                        var max = _regionModel._settingsProvider.ActiveRegion.data.Count - 1;
                                                                                        var current = _regionModel.CurrentLevelProgress;

                                                                                        region.SetProgress(current, max);
                                                                                    })
                            .AddTo(region);
                    }
                }
            }

            if(_regionUpgradeService != null
               && _regionModel != null
               && _regionModel._settingsProvider.ActiveRegion != null)
            {
                _regionUpgradeService.Initialize(_regionModel);
                _regionUpgradeService.JumpToFrame(0);

                if(_regionModel.CurrentLevelProgress != 0)
                {
                    int endFrame = _regionModel._settingsProvider.ActiveRegion.data[_regionModel.CurrentLevelProgress - 1]
                        .endFrame;
                    _regionUpgradeService.JumpToFrame(endFrame);
                }
            }
        }

        private void InitializeLifePopup()
        {
            if(_lifePopup == null)
            {
                return;
            }

            if(_lifePopup.AdsButton != null && _adRewardService != null && _ironSourceManager != null)
            {
                _lifePopup.AdsButton.onClick.RemoveAllListeners();
                _lifePopup.AdsButton.onClick.AddListener(() =>
                                                         {
                                                             _adRewardService.SetAdRewardType(AdRewardType.Life);
                                                             _ironSourceManager.ShowRewardedAd();
                                                         });
            }

            if(_rewardPopup != null && _adEventModel != null)
            {
                _adEventModel.OnRewardGranted.Subscribe(_ =>
                                                        {
                                                            _lifePopup.Hide();
                                                            _rewardPopup.Show();
                                                        })
                    .AddTo(_disposable);
            }

            if(_lifePopup.BuyButton != null)
            {
                _lifePopup.BuyButton.onClick.RemoveAllListeners();
                _lifePopup.BuyButton.onClick.AddListener(() =>
                                                         {
                                                             if(_gemModel.Gems.Value >= 15)
                                                             {
                                                                 _healthSystem.AddLives(1);
                                                                 _gemModel.Decrease(15);
                                                                 _lifePopup.Hide();
                                                             }
                                                         });
            }
        }

        private async UniTask Upgrade()
        {
            if(_regionModel.CanLoadNewRegion())
            {
                // _regionModel.LoadNewRegion();
            }
            else
            {
                Debug.LogWarning("No regions available");
                return;
            }
            
            if(_regionModel.CanUpgrade())
            {
                _regionModel.Upgrade();

                int endFrame = _regionModel._settingsProvider.ActiveRegion.data[_regionModel.CurrentLevelProgress - 1]
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
            _inAppView?.Show();
        }

        private void StartGame()
        {
            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);

            if(_levelConfig.Testing)
            {
                nextLevel = _levelConfig.LevelToPlay;
            }

            if(_healthSystem.CanPlay)
            {
                PlayerPrefs.SetInt("OpenLevel", nextLevel);
                _sceneLoader.LoadGameAsync().Forget();
            }
            else if(_lifePopup != null)
            {
                _lifePopup.Show();
            }
            else
            {
                Debug.LogWarning("Cannot start game: no lives available and life popup is not assigned.");
            }
        }

        private async UniTaskVoid HandlePurchaseInApp(ShopProductNames shopProduct)
        {
            var isBought = await InAppPurchasingService.TryBuyConsumableAsync(shopProduct);

            if(!isBought) return;

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
