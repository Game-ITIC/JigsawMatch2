using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using JuiceFresh.Scripts.GoogleAds;
using UnityEngine;

public class AdmobManager : MonoBehaviour
{
    public static AdmobManager Instance;

    [SerializeField] private Banner _banner;
    [SerializeField] private AdMobInterstital _interstital;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus =>
        {
            _banner.LoadAd();
            LoadInterstital();
        });
    }

    public void OnDestroy()
    {
        _banner.DestroyAd();
    }

    public void LoadInterstital()
    {
        _interstital.LoadInterstitialAd();
    }

    public void ShowInterstital()
    {
        _interstital.ShowInterstitialAd();
    }
}