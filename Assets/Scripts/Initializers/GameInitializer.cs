using System;
using System.Threading;
using Core.Grid.Interfaces;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using JetBrains.Annotations;
using Monobehaviours;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Initializers
{
    [UsedImplicitly]
    public class GameInitializer : IAsyncStartable, IDisposable
    {
        private readonly IronSourceManager _ironSourceManager;

        private readonly ILevelService _levelService;

        // private readonly IGameEvents _gameEvents;
        private readonly Button _noAds;
        private readonly InternetState _internetState;
        private readonly ITileViewFactory _tileViewFactory;

        public GameInitializer(
            IronSourceManager ironSourceManager,
            // IGameEvents gameEvents,
            ILevelService levelService,
            Button noAds,
            InternetState internetState,
            ITileViewFactory tileViewFactory
        )
        {
            _ironSourceManager = ironSourceManager;
            _levelService = levelService;
            // _gameEvents = gameEvents;
            _noAds = noAds;
            _internetState = internetState;
            _tileViewFactory = tileViewFactory;
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            await _tileViewFactory.WarmUp();
            // _gameEvents.OnGameLost += ShowInterstitial;
            // _gameEvents.OnGameWon += ShowInterstitial;
            // _gameEvents.OnEnterGame += GameStart;
            await _levelService.InitializeLevel(1);
            // _noAds.onClick.RemoveAllListeners();
            // _noAds.onClick.AddListener(() =>
            // {
            //     Gley.EasyIAP.API.BuyProduct(ShopProductNames.RemoveAds, (status, message, product) =>
            //     {
            //         if (status == IAPOperationStatus.Success)
            //         {
            //             _internetState.HasRemoveAds = true;
            //             _noAds.gameObject.SetActive(false);
            //         }
            //     });
            // });
            // _noAds.gameObject.SetActive(!_internetState.HasRemoveAds);
            await UniTask.Yield();
        }

        private void GameStart()
        {
            if (_internetState.HasInternet && !_internetState.HasRemoveAds)
                _ironSourceManager.LoadInterstitial();
            // _ironSourceManager.LoadBannerAd();
        }

        private void ShowInterstitial()
        {
            if (_internetState.HasInternet && !_internetState.HasRemoveAds)
                _ironSourceManager.ShowInterstitial();
        }

        public void Dispose()
        {
            // _gameEvents.OnGameLost -= ShowInterstitial;
            // _gameEvents.OnGameWon -= ShowInterstitial;
            // _gameEvents.OnEnterGame -= GameStart;
        }
    }
}