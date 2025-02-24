using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [SerializeField] private Button loadInter;
    [SerializeField] private Button showInter;
    [SerializeField] private Button loadBan;
    [SerializeField] private Button showBan;

    private void Start()
    {
        loadInter.onClick.AddListener(() => { IronSourceManager.Instance.LoadInterstitial(); });

        showInter.onClick.AddListener(() => { IronSourceManager.Instance.ShowInterstitial(); });

        loadBan.onClick.AddListener(() => { IronSourceManager.Instance.LoadBannerAd(); });

        showBan.onClick.AddListener(() => { IronSourceManager.Instance.ShowBannerAd(); });
    }
}