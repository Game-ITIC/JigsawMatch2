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
    string appKey = "207f4e275";
    string _bannerId = "iep3rxsyp9na3rw8";
    string _interstitialId = "wmgt0712uuux8ju4";
#else
    string appKey = "unexpected_platform";
    string bannerAdUnitId = "unexpected_platform";
    string interstitialAdUnitId = "unexpected_platform";
#endif

        private LevelPlayInterstitialAd _interstitial;
        private LevelPlayBannerAd _banner;

        private const int MaxRetryAttempts = 3;
        private int _retryAttempts = 0;
        private const float RetryDelay = 5f;

        private bool _isInitialized = false; 
        private bool _bannerLoaded = false;  
        private bool _interstitialLoaded = false; 

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

        public bool AreAdsReady()
        {
            return _isInitialized && (_banner != null) && (_interstitial != null);
        }

        public void InitializeLevelPlay()
        {
            Debug.Log("Start Init IronSource");

            IronSource.Agent.validateIntegration();
            
            LevelPlay.OnInitSuccess += LevelPlayOnInitSuccess;
            LevelPlay.OnInitFailed += LevelPlayOnInitFailed;

            LevelPlay.Init(appKey, null, new[]
            {
                LevelPlayAdFormat.BANNER,
                LevelPlayAdFormat.INTERSTITIAL
            });
        }

        private void LevelPlayOnInitSuccess(LevelPlayConfiguration obj)
        {
            Debug.Log("IronSource Init Success");
            _retryAttempts = 0;
            _isInitialized = true;

            RegisterBanner();
            RegisterInterstitial();
        }

        private void LevelPlayOnInitFailed(LevelPlayInitError obj)
        {
            Debug.LogError("IronSource Init Failed: " + obj.ErrorMessage);

            if (_retryAttempts < MaxRetryAttempts)
            {
                _retryAttempts++;
                Debug.Log(
                    $"Повторная попытка инициализации через {RetryDelay} секунд. Попытка {_retryAttempts}/{MaxRetryAttempts}"
                );
                Invoke(nameof(InitializeLevelPlay), RetryDelay);
            }
            else
            {
                Debug.LogError("Достигнуто максимальное число попыток. Проверяем интернет или AppKey.");
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

            LoadBannerAd();
        }

        #region banner

        public void LoadBannerAd()
        {
            Debug.Log("Load Banner");
            if (_banner == null) return;
            _bannerLoaded = false;
            _banner.LoadAd();
        }

        public void ShowBannerAd()
        {
            if (_banner != null)
            {
                Debug.Log("Show Banner");
                _banner.ShowAd();
            }
        }

        private void BannerOnAdLoadedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner Loaded");
            _bannerLoaded = true;
            // Отобразим сразу
            ShowBannerAd();
        }

        private void BannerOnAdLoadFailedEvent(LevelPlayAdError obj)
        {
            Debug.LogError("Banner Failed: " + obj.ErrorMessage);
            Invoke(nameof(LoadBannerAd), RetryDelay);
        }

        private void BannerOnAdDisplayedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner displayed");
        }

        private void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError obj)
        {
            Debug.LogError("Banner display failed: " + obj.LevelPlayError.ErrorMessage);
        }

        private void BannerOnAdClickedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner clicked");
        }

        private void BannerOnAdCollapsedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner collapsed");
        }

        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner left app");
        }

        private void BannerOnAdExpandedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Banner expanded");
        }

        public void DestroyBannerAd()
        {
            if (_banner != null)
            {
                _banner.DestroyAd();
                _banner = null;
            }
            Debug.Log("Banner destroyed.");
        }

        #endregion

        #region Interstital

        private void RegisterInterstitial()
        {
            Debug.Log("Register Interstitial");
            _interstitial = new LevelPlayInterstitialAd(_interstitialId);

            _interstitial.OnAdLoaded += InterstitialOnAdLoadedEvent;
            _interstitial.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            _interstitial.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            _interstitial.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
            _interstitial.OnAdClicked += InterstitialOnAdClickedEvent;
            _interstitial.OnAdClosed += InterstitialOnAdClosedEvent;
            _interstitial.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

            // Сразу загрузим
            LoadInterstitial();
        }

        public void LoadInterstitial()
        {
            if (_interstitial == null) return;
            Debug.Log("Load Interstitial");
            _interstitialLoaded = false;
            _interstitial.LoadAd();
        }


         public void ShowInterstitial()
        {
            if (_interstitial == null) 
            {
                Debug.LogWarning("Interstitial not created yet.");
                return;
            }
            if (_interstitial.IsAdReady())
            {
                Debug.Log("Show Interstitial");
                _interstitial.ShowAd();
            }
            else
            {
                Debug.LogWarning("Interstitial not loaded yet, retry load...");
                LoadInterstitial();
            }
        }

        public void DestroyInterstitial()
        {
            if (_interstitial != null)
            {
                _interstitial.DestroyAd();
                _interstitial = null;
            }
            Debug.Log("Interstitial destroyed.");
        }

        private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Interstitial Loaded");
            _interstitialLoaded = true;
        }

        private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError obj)
        {
            Debug.LogError("Interstitial Load Failed: " + obj.ErrorMessage);
            Invoke(nameof(LoadInterstitial), RetryDelay);
        }

        private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Interstitial Displayed");
        }

        private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError obj)
        {
            Debug.LogError("Interstitial Display Failed: " + obj.LevelPlayError.ErrorMessage);
        }

        private void InterstitialOnAdClickedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Interstitial Clicked");
        }

        private void InterstitialOnAdClosedEvent(LevelPlayAdInfo obj)
        {
            Debug.Log("Interstitial Closed. Reloading...");
            LoadInterstitial();
        }

        private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo obj)
        {
            // При изменении параметров рекламы
            Debug.Log("Interstitial AdInfo Changed");
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

            // Обязательно уничтожаем рекламу, чтобы освободить ресурсы
            DestroyBannerAd();
            DestroyInterstitial();
        }
    }
}