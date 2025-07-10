using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Gley.EasyIAP;
using Itic.Scopes;
using JetBrains.Annotations;
using Monobehaviours;
using Services;
using Services.InApp;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Views;

namespace Initializers
{
    [UsedImplicitly]
    public class GameInitializer : IInitializable, IAsyncStartable, ITickable, IDisposable
    // , IDisposable
    {
        // private readonly IronSourceManager _ironSourceManager;
        private readonly IGameEvents _gameEvents;
        private readonly InternetState _internetState;
        private readonly GameCompleteView _gameCompleteView;
        private readonly GamePauseView _gamePauseView;
        private readonly GameOverView _gameOverView;
        private readonly SceneLoader _sceneLoader;
        private readonly LevelManager _levelManager;
        private readonly AdRewardService _adRewardService;
        private readonly IronSourceManager _ironSourceManager;

        public GameInitializer(
            // IronSourceManager ironSourceManager,
            IGameEvents gameEvents,
            InternetState internetState,
            GameCompleteView gameCompleteView,
            GamePauseView gamePauseView,
            GameOverView gameOverView,
            SceneLoader sceneLoader,
            LevelManager levelManager,
            AdRewardService adRewardService,
            IronSourceManager ironSourceManager
        )
        {
            // _ironSourceManager = ironSourceManager;
            _gameEvents = gameEvents;
            _internetState = internetState;
            _gameCompleteView = gameCompleteView;
            _gamePauseView = gamePauseView;
            _gameOverView = gameOverView;
            _sceneLoader = sceneLoader;
            _levelManager = levelManager;
            _adRewardService = adRewardService;
            _ironSourceManager = ironSourceManager;
        }

        public void Initialize()
        {
            _gameCompleteView.Home.onClick.RemoveAllListeners();
            _gameCompleteView.Home.onClick.AddListener(() =>
            {
                Debug.Log("gameComplete woriking");
                var currentLevel = PlayerPrefs.GetInt("OpenLevel", 1);
                PlayerPrefs.SetInt("OpenLevel", currentLevel + 1);

                _sceneLoader.LoadLastSceneAsync().Forget();
            });

            _gameCompleteView.Next.onClick.RemoveAllListeners();
            _gameCompleteView.Next.onClick.AddListener(() =>
            {
                var currentLevel = PlayerPrefs.GetInt("OpenLevel", 1);

                PlayerPrefs.SetInt("OpenLevel", currentLevel + 1);
                _sceneLoader.LoadGameAsync().Forget();
            });

            _gameCompleteView.AdsButton.onClick.RemoveAllListeners();
            _gameCompleteView.AdsButton.onClick.AddListener(() =>
            {
                _adRewardService.SetAdRewardType(AdRewardType.Coin);
                IronSourceManager.Instance.ShowRewardedAd();
            });

            _gamePauseView.Home.onClick.RemoveAllListeners();
            _gamePauseView.Home.onClick.AddListener(BackToBack);

            _gamePauseView.ContinueButton.onClick.RemoveAllListeners();
            _gamePauseView.ContinueButton.onClick.AddListener(() =>
            {
                _gamePauseView.Hide();
                _levelManager.ResumeGame();
            });

            _gamePauseView.RestartButton.onClick.RemoveAllListeners();
            _gamePauseView.RestartButton.onClick.AddListener(RestartGame);

            _gameOverView.RestartButton.onClick.RemoveAllListeners();
            _gameOverView.RestartButton.onClick.AddListener(RestartGame);

            _gameOverView.Home.onClick.RemoveAllListeners();
            _gameOverView.Home.onClick.AddListener(BackToBack);


            _levelManager.InvokeStart();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            _sceneLoader.LoadGameAsync().Forget();
        }

        public void BackToBack()
        {
            Time.timeScale = 1f;
            _sceneLoader.LoadLastSceneAsync().Forget();
        }

        public async UniTask StartAsync(CancellationToken cancellation = new CancellationToken())
        {
            _gameEvents.OnGameLost += ShowInterstitial;
            _gameEvents.OnGameWon += ShowInterstitial;
            _gameEvents.OnEnterGame += GameStart;

            await UniTask.Yield();
        }

        private void GameStart()
        {
            //     if (_internetState.HasInternet && !_internetState.HasRemoveAds)
            //         _ironSourceManager.LoadInterstitial();
            //     // _ironSourceManager.LoadBannerAd();
        }

        //
        private void ShowInterstitial()
        {
            //     if (_internetState.HasInternet && !_internetState.HasRemoveAds)
            //         _ironSourceManager.ShowInterstitial();
        }

        //
        public void Dispose()
        {
            _gameEvents.OnGameLost -= ShowInterstitial;
            _gameEvents.OnGameWon -= ShowInterstitial;
            _gameEvents.OnEnterGame -= GameStart;
        }


        public void Tick()
        {
            _levelManager.InvokeUpdate();
        }
    }
}