using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Itic.Scopes;
using JetBrains.Annotations;
using Monobehaviours;
using Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Views;

namespace Initializers
{
    [UsedImplicitly]
    public class GameInitializer : IInitializable, IAsyncStartable, ITickable
        // , IDisposable
    {
        // private readonly IronSourceManager _ironSourceManager;
        private readonly IGameEvents _gameEvents;
        private readonly Button _noAds;
        // private readonly InternetState _internetState;
        private readonly GameCompleteView _gameCompleteView;
        private readonly GamePauseView _gamePauseView;
        private readonly GameOverView _gameOverView;
        private readonly SceneLoader _sceneLoader;
        private readonly LevelManager _levelManager;

        public GameInitializer(
            // IronSourceManager ironSourceManager,
            IGameEvents gameEvents,
            Button noAds,
            // InternetState internetState,
            GameCompleteView gameCompleteView,
            GamePauseView gamePauseView,
            GameOverView gameOverView,
            SceneLoader sceneLoader,
            LevelManager levelManager
        )
        {
            // _ironSourceManager = ironSourceManager;
            _gameEvents = gameEvents;
            _noAds = noAds;
            // _internetState = internetState;
            _gameCompleteView = gameCompleteView;
            _gamePauseView = gamePauseView;
            _gameOverView = gameOverView;
            _sceneLoader = sceneLoader;
            _levelManager = levelManager;
        }

        public void Initialize()
        {
            Debug.Log("I'm here");
            _gameCompleteView.Home.onClick.RemoveAllListeners();
            _gameCompleteView.Home.onClick.AddListener(() => { _sceneLoader.LoadRegionAsync().Forget(); });

            _gameCompleteView.Next.onClick.RemoveAllListeners();
            _gameCompleteView.Next.onClick.AddListener(() =>
            {
                var currentLevel = PlayerPrefs.GetInt("OpenLevel", 1);

                PlayerPrefs.SetInt("OpenLevel", currentLevel + 1);
                _sceneLoader.LoadGameAsync().Forget();
            });

            _gamePauseView.Home.onClick.RemoveAllListeners();
            _gamePauseView.Home.onClick.AddListener(() => { _sceneLoader.LoadRegionAsync().Forget(); });

            _gamePauseView.ContinueButton.onClick.RemoveAllListeners();
            _gamePauseView.ContinueButton.onClick.AddListener(() => { _gamePauseView.Hide(); });

            _gamePauseView.RestartButton.onClick.RemoveAllListeners();
            _gamePauseView.RestartButton.onClick.AddListener(() => { _sceneLoader.LoadGameAsync().Forget(); });


            _gameOverView.RestartButton.onClick.RemoveAllListeners();
            _gameOverView.RestartButton.onClick.AddListener(() => { _sceneLoader.LoadGameAsync().Forget(); });

            _gameOverView.Home.onClick.RemoveAllListeners();
            _gameOverView.Home.onClick.AddListener(() => { _sceneLoader.LoadRegionAsync().Forget(); });
            
            _levelManager.InvokeStart();
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            // _gameEvents.OnGameLost += ShowInterstitial;
            // _gameEvents.OnGameWon += ShowInterstitial;
            // _gameEvents.OnEnterGame += GameStart;

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

        // private void GameStart()
        // {
        //     if (_internetState.HasInternet && !_internetState.HasRemoveAds)
        //         _ironSourceManager.LoadInterstitial();
        //     // _ironSourceManager.LoadBannerAd();
        // }
        //
        // private void ShowInterstitial()
        // {
        //     if (_internetState.HasInternet && !_internetState.HasRemoveAds)
        //         _ironSourceManager.ShowInterstitial();
        // }
        //
        // public void Dispose()
        // {
        //     _gameEvents.OnGameLost -= ShowInterstitial;
        //     _gameEvents.OnGameWon -= ShowInterstitial;
        //     _gameEvents.OnEnterGame -= GameStart;
        // }
        public void Tick()
        {
            _levelManager.InvokeUpdate();
        }
    }
}