using System.Collections;
using System.Collections.Generic;
using Gley.EasyIAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InappTest : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button removeAdsButton;
    [SerializeField] private TMP_Text removeAdsText;

    void Start()
    {
        API.Initialize(InitializeIAP);
    }

    private void InitializeIAP(IAPOperationStatus status, string message)
    {
        if (status == IAPOperationStatus.Fail)
        {
            statusText.text = "Initialization error";
        }
        else if (status == IAPOperationStatus.Success)
        {
            statusText.text = "Initialization Success";
            removeAdsButton.onClick.AddListener(BuyRemoveAds);
        }
    }

    private void BuyRemoveAds()
    {
        if (API.IsActive(ShopProductNames.RemoveAds))
        {
            statusText.text = "Remove ads is already bought";
            return;
        }

        API.BuyProduct(ShopProductNames.RemoveAds, OnProductBought);
    }

    private void OnProductBought(IAPOperationStatus status, string message, StoreProduct product)
    {
        if (status == IAPOperationStatus.Success)
        {
            if (product.productName == ShopProductNames.RemoveAds.ToString())
            {
                removeAdsText.text = "Ads removed";
            }
        }
    }
}