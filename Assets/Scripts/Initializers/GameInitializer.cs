using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using JetBrains.Annotations;
using Monobehaviours;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

namespace Initializers
{
    [UsedImplicitly]
    public class GameInitializer : IAsyncStartable, IDisposable
    {
        private readonly IronSourceManager _ironSourceManager;
        private readonly IGameEvents _gameEvents;
        private readonly Button _noAds;
        private readonly InternetState _internetState;

        public GameInitializer(
            IronSourceManager ironSourceManager,
            IGameEvents gameEvents,
            Button noAds,
            InternetState internetState
        )
        {
            _ironSourceManager = ironSourceManager;
            _gameEvents = gameEvents;
            _noAds = noAds;
            _internetState = internetState;
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            _gameEvents.OnGameLost += ShowInterstitial;
            _gameEvents.OnGameWon += ShowInterstitial;
            _gameEvents.OnEnterGame += GameStart;

            _noAds.onClick.RemoveAllListeners();
            _noAds.onClick.AddListener(() =>
            {
                Gley.EasyIAP.API.BuyProduct(ShopProductNames.RemoveAds, (status, message, product) =>
                {
                    if (status == IAPOperationStatus.Success)
                    {
                        _internetState.HasRemoveAds = true;
                        _noAds.gameObject.SetActive(false);
                    }
                });
            });
            _noAds.gameObject.SetActive(!_internetState.HasRemoveAds);
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
            _gameEvents.OnGameLost -= ShowInterstitial;
            _gameEvents.OnGameWon -= ShowInterstitial;
            _gameEvents.OnEnterGame -= GameStart;
        }
    }
}