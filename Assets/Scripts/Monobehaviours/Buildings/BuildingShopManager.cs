using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Extensions.ZLinq;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Views;
using ZLinq;
using Object = UnityEngine.Object;

namespace Monobehaviours.Buildings
{
    public class BuildingShopManager : MonoBehaviour
    {
        [SerializeField] private List<ShopItem> shopItems;
        [SerializeField] private int maxItemsInShop;
        [SerializeField] private ShopItemView shopItemViewPrefab;
        [SerializeField] private Transform shopItemParent;
        [SerializeField] private Button closeButton;

        private Models.StarModel _starModel;
        private CountryConfig _countryConfig;

        private List<string> _openedItemsId;

        private IDisposable _starsSubscribeDisposable;

        private List<ShopItemView> _shopItemViews = new();

        public void Initialize(Models.StarModel starModel, CountryConfig countryConfig)
        {
            _starModel = starModel;
            _countryConfig = countryConfig;

            _starsSubscribeDisposable = _starModel.Stars.Subscribe(UpdateBuyVisual);
            LoadBuildings();

            CreateShopItemViews();

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
            UpdateBuyVisual(starModel.Stars.Value);
        }

        private void CreateShopItemViews()
        {
            var lockedShopItems =
                shopItems
                    .AsValueEnumerable()
                    .Where(item => !_openedItemsId.Contains(item.itemId))
                    .AsEnumerable();
            

            foreach (var item in lockedShopItems)
            {
                if (_shopItemViews.Count < maxItemsInShop)
                {
                    var shopItemView = Object.Instantiate(shopItemViewPrefab, shopItemParent);
                    var itemId = item.itemId;
                    shopItemView.Warmup();
                    shopItemView.Init(item);
                    shopItemView.OnBuyClicked += () => { BuyItem(itemId); };
                    _shopItemViews.Add(shopItemView);
                }
                else
                {
                    break;
                }
            }
        }

        private void UpdateBuyVisual(int newValue)
        {
            foreach (var shopItemView in _shopItemViews)
            {
                shopItemView.BuyButton.interactable = newValue >= shopItemView.ShopItem.cost;
            }
        }

        private void BuyItem(string itemId)
        {
            var item = _shopItemViews.AsValueEnumerable()
                .First(item => item.ShopItem.itemId == itemId);

            if (item.ShopItem.cost <= _starModel.Stars.Value)
            {
                _starModel.Decrease(item.ShopItem.cost);
                item.ShopItem.UnlockBuildings();
                _openedItemsId.Add(itemId);

                PlayerPrefs.SetString(_countryConfig.GetCountryKeyToSave(),
                    JsonConvert.SerializeObject(_openedItemsId));
            }
        }

        private void LoadBuildings()
        {
            var saveValue = PlayerPrefs.GetString(_countryConfig.GetCountryKeyToSave(), "empty");

            _openedItemsId = saveValue == "empty"
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(saveValue);

            var shopItemsZ = shopItems.AsValueEnumerable();

            foreach (var id in _openedItemsId)
            {
                var item = shopItemsZ.First(item => item.itemId == id);
                item.UnlockBuildings();
            }
        }

        private void OnDestroy()
        {
            _starsSubscribeDisposable.Dispose();
        }
    }
}