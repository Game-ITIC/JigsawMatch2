using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Extensions.ZLinq;
using Models;
using Newtonsoft.Json;
using R3;
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
        [SerializeField] private GameObject CompletePanel;

        private Models.StarModel _starModel;
        private CountryConfig _countryConfig;

        private List<string> _purchasedItemIds = new List<string>();
        private List<ShopItemView> _shopItemViews = new List<ShopItemView>();
        private CompositeDisposable disposable = new();

        public void Initialize(Models.StarModel starModel, CountryConfig countryConfig)
        {
            _starModel = starModel;
            _countryConfig = countryConfig;


            SetupCloseButton();

            LoadPurchasedItems();

            UpdateShopItemsLockState();

            CreateShopItemViews();

            _starModel.Stars.Subscribe(UpdatePurchaseAvailability)
                .AddTo(disposable);

            UpdatePurchaseAvailability(_starModel.Stars.Value);

            bool allItemsWereAlreadyPurchased = PlayerPrefs.GetInt("AllItemsPurchased", 0) == 1;
            if (allItemsWereAlreadyPurchased)
            {
                OnAllItemsPurchased();
            }
        }

        private void SetupCloseButton()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void LoadPurchasedItems()
        {
            try
            {
                string saveKey = _countryConfig.GetCountryKeyToSave();
                string savedJson = PlayerPrefs.GetString(saveKey, string.Empty);

                _purchasedItemIds = string.IsNullOrEmpty(savedJson)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(savedJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка загрузки купленных предметов: {ex.Message}");
                _purchasedItemIds = new List<string>();
            }
        }

        private void UpdateShopItemsLockState()
        {
            foreach (var shopItem in shopItems)
            {
                if (_purchasedItemIds.Contains(shopItem.itemId))
                {
                    shopItem.UnlockBuildings();
                }
                else
                {
                    shopItem.LockBuildings();
                }
            }
        }

        private void CreateShopItemViews()
        {
            ClearShopItemViews();

            IEnumerable<ShopItem> availableItems = GetAvailableShopItems()
                .Take(maxItemsInShop)
                .AsEnumerable();

            foreach (ShopItem shopItem in availableItems)
            {
                CreateShopItemView(shopItem);
            }
        }

        private IEnumerable<ShopItem> GetAvailableShopItems()
        {
            return shopItems
                .AsValueEnumerable()
                .Where(item => !_purchasedItemIds.Contains(item.itemId))
                .AsEnumerable();
        }

        private void ClearShopItemViews()
        {
            foreach (var view in _shopItemViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _shopItemViews.Clear();
        }

        private void CreateShopItemView(ShopItem shopItem)
        {
            ShopItemView shopItemView = Object.Instantiate(shopItemViewPrefab, shopItemParent);
            string itemId = shopItem.itemId;

            shopItemView.Warmup();
            shopItemView.Init(shopItem);
            shopItemView.OnBuyClicked += () => BuyItem(itemId);

            _shopItemViews.Add(shopItemView);
        }

        private void UpdatePurchaseAvailability(int starCount)
        {
            foreach (var shopItemView in _shopItemViews)
            {
                shopItemView.BuyButton.interactable = starCount >= shopItemView.ShopItem.cost;
            }
        }

        private void BuyItem(string itemId)
        {
            ShopItemView itemView = _shopItemViews.FirstOrDefault(view => view.ShopItem.itemId == itemId);
            if (itemView == null) return;

            if (itemView.ShopItem.cost > _starModel.Stars.Value) return;

            _starModel.Decrease(itemView.ShopItem.cost);
            itemView.ShopItem.UnlockBuildings();
            itemView.ShopItem.SpawnBuildings();

            _purchasedItemIds.Add(itemId);
            SavePurchasedItems();

            _shopItemViews.Remove(itemView);
            Destroy(itemView.gameObject);

            RefreshShopAfterPurchase();

            if (!GetAvailableShopItems().Any())
            {
                OnAllItemsPurchased();
            }
        }

        private void RefreshShopAfterPurchase()
        {
            if (_shopItemViews.Count >= maxItemsInShop) return;

            var displayedItemIds = _shopItemViews.Select(view => view.ShopItem.itemId).ToList();

            ShopItem? nextItemNullable = GetAvailableShopItems()
                .Where(item => !displayedItemIds.Contains(item.itemId))
                .Cast<ShopItem?>()
                .FirstOrDefault();

            if (nextItemNullable != null)
            {
                ShopItem nextItem = nextItemNullable.Value;

                CreateShopItemView(nextItem);

                UpdatePurchaseAvailability(_starModel.Stars.Value);
            }
        }

        private void SavePurchasedItems()
        {
            try
            {
                string saveKey = _countryConfig.GetCountryKeyToSave();
                string jsonData = JsonConvert.SerializeObject(_purchasedItemIds);
                PlayerPrefs.SetString(saveKey, jsonData);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error when saving purchased items: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            disposable.Dispose();

            ClearShopItemViews();
        }

        private void OnAllItemsPurchased()
        {
            CompletePanel.SetActive(true);
            PlayerPrefs.SetInt("AllItemsPurchased", 1);
            PlayerPrefs.Save();
        }
    }
}