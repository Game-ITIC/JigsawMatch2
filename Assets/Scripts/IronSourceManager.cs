using System;
using com.unity3d.mediation;
using UnityEngine;

namespace DefaultNamespace
{
    public class IronSourceManager : MonoBehaviour
    {
        public static IronSourceManager Instance;


#if UNITY_ANDROID
        string appKey = "207f4e275";
        string _bannerId = "yp2w87ryfdpbw9qr";
        string _interstitialId = "y7hr7f67s5rjhawl";
#elif UNITY_IPHONE
    string appKey = "8545d445";
    string _bannerId = "iep3rxsyp9na3rw8";
    string _interstitialId = "wmgt0712uuux8ju4";
#else
    string appKey = "unexpected_platform";
    string bannerAdUnitId = "unexpected_platform";
    string interstitialAdUnitId = "unexpected_platform";
#endif

        private LevelPlayInterstitialAd _interstitial;
        private LevelPlayBannerAd _banner;

        // Максимальное количество попыток
        private const int MaxRetryAttempts = 3;
        private int _retryAttempts = 0;
        private const float RetryDelay = 5f; // Задержка между попытками в секундах


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeLevelPlay();
        }

        private void InitializeLevelPlay()
        {
            IronSource.Agent.validateIntegration();
            Debug.Log("Start Init");
            LevelPlay.Init(appKey, null, new[]
            {
                LevelPlayAdFormat.BANNER,
                LevelPlayAdFormat.INTERSTITIAL
            });

            LevelPlay.OnInitSuccess += LevelPlayOnInitSuccess;
            LevelPlay.OnInitFailed += LevelPlayOnInitFailed;
        }

        private void LevelPlayOnInitSuccess(LevelPlayConfiguration obj)
        {
            Debug.Log("Init Success");
            _retryAttempts = 0;
            RegisterInterstitial();
            RegisterBanner();
        }

        private void LevelPlayOnInitFailed(LevelPlayInitError obj)
        {
            Debug.Log("Init Failed");

            if (_retryAttempts < MaxRetryAttempts)
            {
                _retryAttempts++;
                Debug.Log(
                    $"Повторная попытка инициализации через {RetryDelay} секунд. Попытка {_retryAttempts}/{MaxRetryAttempts}");
                Invoke(nameof(InitializeLevelPlay), RetryDelay);
            }
            else
            {
                Debug.LogError(
                    "Достигнуто максимальное количество попыток инициализации. Проверьте интернет или настройки.");
            }
        }

        private void RegisterBanner()
        {
            Debug.Log("Register Banner");
            var adSize = LevelPlayAdSize.CreateAdaptiveAdSize();

            _banner = new LevelPlayBannerAd(_bannerId, adSize);

            _banner.OnAdLoaded += BannerOnAdLoadedEvent;
            _banner.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            _banner.OnAdDisplayed += BannerOnAdDisplayedEvent;
            _banner.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
            _banner.OnAdClicked += BannerOnAdClickedEvent;
            _banner.OnAdCollapsed += BannerOnAdCollapsedEvent;
            _banner.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            _banner.OnAdExpanded += BannerOnAdExpandedEvent;
        }

        #region banner

        public void LoadBannerAd()
        {
            Debug.Log("Load Banner");
            _banner.LoadAd();
        }

        public void ShowBannerAd()
        {
            Debug.Log("Show Banner");
            _banner.ShowAd();
        }

        private void BannerOnAdLoadedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner Loaded");

            ShowBannerAd();
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError obj)
        {
            Debug.Log("Banner Failed");

            Invoke(nameof(LoadBannerAd), RetryDelay);
        }

        private void BannerOnAdDisplayedEvent(LevelPlayAdInfo obj)
        {
        }

        private void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError obj)
        {
        }

        private void BannerOnAdClickedEvent(LevelPlayAdInfo obj)
        {
        }

        private void BannerOnAdCollapsedEvent(LevelPlayAdInfo obj)
        {
        }

        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo obj)
        {
        }

        private void BannerOnAdExpandedEvent(LevelPlayAdInfo obj)
        {
        }

        public void DestroyBannerAd()
        {
            _banner.DestroyAd();
        }

        #endregion

        #region Interstital

        private void RegisterInterstitial()
        {
            _interstitial = new LevelPlayInterstitialAd(_interstitialId);
            
            _interstitial.OnAdLoaded += InterstitialOnAdLoadedEvent;
            _interstitial.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            _interstitial.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            _interstitial.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            _interstitial.OnAdClicked += InterstitialOnAdClickedEvent;
            _interstitial.OnAdClosed += InterstitialOnAdClosedEvent;
            _interstitial.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
        }

        public void LoadInterstitial()
        {
            _interstitial.LoadAd();
        }

        public void ShowInterstitial()
        {
            _interstitial.ShowAd();
        }

        public void DestroyInterstitial()
        {
            _interstitial.DestroyAd();
        }

        #endregion

        #region InterstitialRegistration

        private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo obj)
        {
        }

        private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError obj)
        {
            Invoke(nameof(LoadInterstitial), RetryDelay);
        }

        private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo obj)
        {
        }

        private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError obj)
        {
        }

        private void InterstitialOnAdClickedEvent(LevelPlayAdInfo obj)
        {
        }

        private void InterstitialOnAdClosedEvent(LevelPlayAdInfo obj)
        {
            LoadInterstitial();
        }

        private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo obj)
        {
        }

        #endregion

        void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        private void OnDestroy()
        {
            LevelPlay.OnInitSuccess -= LevelPlayOnInitSuccess;
            LevelPlay.OnInitFailed -= LevelPlayOnInitFailed;

            DestroyBannerAd();
            DestroyInterstitial();
        }
    }
}