using Configs;
using Models;
using Unity.Services.LevelPlay;
using UnityEngine;
using VContainer;
using LevelPlayAdFormat = com.unity3d.mediation.LevelPlayAdFormat;

public class IronSourceManager : MonoBehaviour
{
    public static IronSourceManager Instance;

    private LevelPlayInterstitialAd _interstitial;
    private LevelPlayBannerAd _banner;
    private LevelPlayRewardedAd _rewarded;

    private const int MaxRetryAttempts = 2;
    private int _retryAttempts = 0;
    private const float RetryDelay = 5f;

    private bool _isInitialized = false;
    private bool _bannerLoaded = false;
    private bool _interstitialLoaded = false;
    private bool _rewardedLoaded = false; // Добавлен флаг для rewarded

    private IronSourceConfigSO _config;
    private AdEventModel _adEventModel;
    
    [Inject]
    void Construct(
        IronSourceConfigSO config,
        AdEventModel adEventModel
        )
    {
        _config = config;
        _adEventModel = adEventModel;
    }

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
        // Включен rewarded в проверку готовности
        return _isInitialized && (_banner != null) && (_interstitial != null) && (_rewarded != null);
    }

    public void InitializeLevelPlay()
    {
        Debug.Log("Start Init IronSource");

        LevelPlay.ValidateIntegration();

        LevelPlay.OnInitSuccess += LevelPlayOnInitSuccess;
        LevelPlay.OnInitFailed += LevelPlayOnInitFailed;

        LevelPlay.Init(_config.AppKey, null, new[]
        {
            LevelPlayAdFormat.BANNER,
            LevelPlayAdFormat.INTERSTITIAL,
            LevelPlayAdFormat.REWARDED
        });
    }

    private void LevelPlayOnInitSuccess(LevelPlayConfiguration obj)
    {
        Debug.Log("IronSource Init Success");
        _retryAttempts = 0;
        _isInitialized = true;

        RegisterBanner();
        RegisterInterstitial();
        RegisterRewarded(); // Добавлена регистрация rewarded
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
        com.unity3d.mediation.LevelPlayAdSize adSize = com.unity3d.mediation.LevelPlayAdSize.CreateAdaptiveAdSize();

        var config = new LevelPlayBannerAd.Config.Builder().SetSize(adSize).Build();
        
        _banner = new LevelPlayBannerAd(_config.BannerId);

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
        _interstitial = new LevelPlayInterstitialAd(_config.InterstitialId);

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

    #region Rewarded

    // Переименован метод для соответствия паттерну
    private void RegisterRewarded()
    {
        Debug.Log("Register Rewarded");
        
        //Create RewardedAd instance
        _rewarded = new LevelPlayRewardedAd(_config.RewardedId);

        //Subscribe RewardedAd events
        _rewarded.OnAdLoaded += RewardedOnAdLoadedEvent;
        _rewarded.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
        _rewarded.OnAdDisplayed += RewardedOnAdDisplayedEvent;
        _rewarded.OnAdDisplayFailed += RewardedOnAdDisplayFailedEvent;
        _rewarded.OnAdClicked += RewardedOnAdClickedEvent;
        _rewarded.OnAdClosed += RewardedOnAdClosedEvent;
        _rewarded.OnAdRewarded += RewardedOnAdRewarded;
        _rewarded.OnAdInfoChanged += RewardedOnAdInfoChangedEvent;

        // Сразу загрузим
        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        if (_rewarded == null) return;
        Debug.Log("Load Rewarded");
        _rewardedLoaded = false;
        //Load or reload RewardedAd     
        _rewarded.LoadAd();
    }

    public void ShowRewardedAd()
    {
        if (_rewarded == null)
        {
            Debug.LogWarning("Rewarded not created yet.");
            return;
        }

        //Show RewardedAd, check if the ad is ready before showing
        if (_rewarded.IsAdReady())
        {
            Debug.Log("Show Rewarded");
            _rewarded.ShowAd();
        }
        else
        {
            Debug.LogWarning("Rewarded not loaded yet, retry load...");
            LoadRewardedAd();
        }
    }

    public void DestroyRewardedAd()
    {
        if (_rewarded != null)
        {
            _rewarded.DestroyAd();
            _rewarded = null;
        }

        Debug.Log("Rewarded destroyed.");
    }

    //Implement RewardedAd events
    private void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Loaded");
        _rewardedLoaded = true;
    }

    private void RewardedOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
    {
        Debug.LogError("Rewarded Load Failed: " + ironSourceError.ErrorMessage);
        Invoke(nameof(LoadRewardedAd), RetryDelay);
    }

    private void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Clicked");
    }

    private void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Displayed");
    }

    private void RewardedOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
    {
        Debug.LogError("Rewarded Display Failed: " + adInfoError.LevelPlayError.ErrorMessage);
    }

    private void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded Closed. Reloading...");
        LoadRewardedAd();
    }

    private void RewardedOnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
    {
        Debug.Log($"Rewarded Ad - User earned reward: {adReward.Amount} {adReward.Name}");
        // Здесь добавить логику выдачи награды игроку
        _adEventModel.InvokeOnRewardedReward();
    }

    private void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
        // При изменении параметров рекламы
        Debug.Log("Rewarded AdInfo Changed");
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
        DestroyRewardedAd(); // Добавлено уничтожение rewarded
    }
}