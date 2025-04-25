using System;
using System.Runtime.CompilerServices;
using Configs;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Itic.Scopes;
using Models;
using Services.InApp;
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

        public CountryInitializer(MenuView menuView,
            SceneLoader sceneLoader,
            InAppView inAppView,
            CoinModel coinModel,
            GemModel gemModel,
            StarModel starModel,
            InternetState internetState,
            InAppConfig inAppConfig
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
        }

        public void Initialize()
        {
            _menuView.Warmup();
            _menuView.StartGame.onClick.RemoveAllListeners();
            _menuView.StartGame.onClick.AddListener(StartGame);

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
        }

        private void StartGame()
        {
            var nextLevel = PlayerPrefs.GetInt("OpenLevel", 1);
            PlayerPrefs.SetInt("OpenLevel", nextLevel);
            // SceneManager.LoadScene("game");
            _sceneLoader.LoadGameAsync().Forget();
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