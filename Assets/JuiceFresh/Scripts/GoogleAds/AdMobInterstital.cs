using GoogleMobileAds.Api;
using UnityEngine;

namespace JuiceFresh.Scripts.GoogleAds
{
    public class AdMobInterstital : MonoBehaviour
    {
        // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-4981577085737170/8858607849";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
  private string _adUnitId = "unused";
#endif

        private InterstitialAd _interstitialAd;

        private bool _isShowRequested = false;

        /// <summary>
        /// Loads the interstitial ad.
        /// </summary>
        public void LoadInterstitialAd()
        {
            // Clean up the old ad before loading a new one.
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            InterstitialAd.Load(_adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitialAd = ad;

                    RegisterReloadHandler(_interstitialAd);


                    if (_isShowRequested)
                    {
                        ShowInterstitialAd();
                    }
                });
        }

        /// <summary>
        /// Shows the interstitial ad.
        /// </summary>
        public void ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log("Показываем межстраничное объявление.");
                _interstitialAd.Show();
                _isShowRequested = false;
            }
            else
            {
                Debug.Log("Межстраничное объявление ещё не готово. Оно будет показано после загрузки.");
                _isShowRequested = true;
            }
        }

        private void RegisterReloadHandler(InterstitialAd interstitialAd)
        {
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += HandleAdClosed;
            interstitialAd.OnAdFullScreenContentFailed += HandleAdFailed;
        }

        private void HandleAdClosed()
        {
            Debug.Log("Межстраничное объявление закрыто.");
            LoadInterstitialAd();
        }

        private void HandleAdFailed(AdError error)
        {
            Debug.LogError("Ошибка показа межстраничного объявления: " + error);
            LoadInterstitialAd();
        }
    }
}