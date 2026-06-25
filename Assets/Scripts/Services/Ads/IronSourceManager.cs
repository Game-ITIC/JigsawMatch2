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
    private bool _rewardedLoaded = false;

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
        return _isInitialized && (_banner != null) && (_interstitial != null) && (_rewarded != null);
    }

    public void InitializeLevelPlay()
    {
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
        _retryAttempts = 0;
        _isInitialized = true;

        RegisterBanner();
        RegisterInterstitial();
        RegisterRewarded();
    }

    private void LevelPlayOnInitFailed(LevelPlayInitError obj)
    {
        if (_retryAttempts < MaxRetryAttempts)
        {
            _retryAttempts++;
            Invoke(nameof(InitializeLevelPlay), RetryDelay);
        }
    }

    private void RegisterBanner()
    {
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
        if (_banner == null) return;
        _bannerLoaded = false;
        _banner.LoadAd();
    }

    public void ShowBannerAd()
    {
        if (_banner != null)
        {
            _banner.ShowAd();
        }
    }

    private void BannerOnAdLoadedEvent(LevelPlayAdInfo obj)
    {
        _bannerLoaded = true;
        ShowBannerAd();
    }

    private void BannerOnAdLoadFailedEvent(LevelPlayAdError obj)
    {
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
        if (_banner != null)
        {
            _banner.DestroyAd();
            _banner = null;
        }
    }

    #endregion

    #region Interstital

    private void RegisterInterstitial()
    {
        _interstitial = new LevelPlayInterstitialAd(_config.InterstitialId);

        _interstitial.OnAdLoaded += InterstitialOnAdLoadedEvent;
        _interstitial.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        _interstitial.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        _interstitial.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        _interstitial.OnAdClicked += InterstitialOnAdClickedEvent;
        _interstitial.OnAdClosed += InterstitialOnAdClosedEvent;
        _interstitial.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

        LoadInterstitial();
    }

    public void LoadInterstitial()
    {
        if (_interstitial == null) return;
        _interstitialLoaded = false;
        _interstitial.LoadAd();
    }


    public void ShowInterstitial()
    {
        if (_interstitial == null)
        {
            return;
        }

        if (_interstitial.IsAdReady())
        {
            _interstitial.ShowAd();
        }
        else
        {
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
    }

    private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo obj)
    {
        _interstitialLoaded = true;
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

    #region Rewarded

    private void RegisterRewarded()
    {
        _rewarded = new LevelPlayRewardedAd(_config.RewardedId);

        _rewarded.OnAdLoaded += RewardedOnAdLoadedEvent;
        _rewarded.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
        _rewarded.OnAdDisplayed += RewardedOnAdDisplayedEvent;
        _rewarded.OnAdDisplayFailed += RewardedOnAdDisplayFailedEvent;
        _rewarded.OnAdClicked += RewardedOnAdClickedEvent;
        _rewarded.OnAdClosed += RewardedOnAdClosedEvent;
        _rewarded.OnAdRewarded += RewardedOnAdRewarded;
        _rewarded.OnAdInfoChanged += RewardedOnAdInfoChangedEvent;

        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        if (_rewarded == null) return;
        _rewardedLoaded = false;
        _rewarded.LoadAd();
    }

    public void ShowRewardedAd()
    {
        if (_rewarded == null)
        {
            return;
        }

        if (_rewarded.IsAdReady())
        {
            _rewarded.ShowAd();
        }
        else
        {
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
    }

    private void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
        _rewardedLoaded = true;
    }

    private void RewardedOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
    {
        Invoke(nameof(LoadRewardedAd), RetryDelay);
    }

    private void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
    }

    private void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
    }

    private void RewardedOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
    {
    }

    private void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
        LoadRewardedAd();
    }

    private void RewardedOnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
    {
        _adEventModel.InvokeOnRewardedReward();
    }

    private void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
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
        DestroyRewardedAd();
    }
}
