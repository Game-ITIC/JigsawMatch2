using System;
using System.Collections.Generic;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using VContainer;

namespace Monobehaviours.Buildings
{
    public class BuildingShopManager : MonoBehaviour
    {
        [SerializeField] private List<ShopItem> shopItems;
        [SerializeField] private int maxItemsInShop;

        private Models.StarModel _starModel;
        private string _countryId;

        private List<string> _openedItemsId;

        private IDisposable _starsSubscrieDisposable;

        [Inject]
        public void Construct(Models.StarModel starModel, string countryId)
        {
            _starModel = starModel;
            _countryId = countryId;

            Initialize();
        }

        private void Initialize()
        {
            _starsSubscrieDisposable = _starModel.Stars.Subscribe(UpdateBuyVisual);
            LoadBuildings();


            foreach (var shopItem in shopItems)
            {
                if (_openedItemsId.Contains(shopItem.ItemId)) shopItem.UnlockBuildings();

                shopItem.OnBought += BuyItem;
                shopItem.BuyButton.interactable = _starModel.Stars.Value >= shopItem.Cost;
                shopItem.LockBuildings();
            }
        }

        private void UpdateBuyVisual(int newValue)
        {
            foreach (var shopItem in shopItems)
            {
                shopItem.BuyButton.interactable = newValue >= shopItem.Cost;
                shopItem.Warmup();
            }
        }

        private void BuyItem(string itemId)
        {
            Debug.Log(itemId);
            var item = shopItems.Find(item => item.ItemId == itemId);
            
            Debug.Log(item);
            if (item.Cost <= _starModel.Stars.Value)
            {
                _starModel.Decrease(item.Cost);
                item.UnlockBuildings();
                _openedItemsId.Add(itemId);

                PlayerPrefs.SetString(_countryId, JsonConvert.SerializeObject(_openedItemsId));
            }
        }

        private void LoadBuildings()
        {
            var saveValue = PlayerPrefs.GetString(_countryId, "empty");

            _openedItemsId = saveValue == "empty"
                ? new List<string>()
                : JsonConvert.DeserializeObject<List<string>>(saveValue);
        }

        private void OnDestroy()
        {
            _starsSubscrieDisposable.Dispose();
        }
    }
}