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

    [field: SerializeField] public bool IsActive { get; private set; } = false;
    
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
        if (!IsActive) return; 
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
        if (!IsActive) return;
        
        _interstital.LoadInterstitialAd();
    }

    public void ShowInterstital()
    {
        if (!IsActive) return;
        _interstital.ShowInterstitialAd();
        
    }
}