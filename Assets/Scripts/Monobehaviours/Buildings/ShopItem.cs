using System;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Monobehaviours.Buildings
{
    public class ShopItem : MonoBehaviour, IPreloader
    {
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private int cost;
        [SerializeField] private Building[] buildingsToUnlock;
        [SerializeField] private int requiredPlayerLevel;
        [SerializeField] private Building[] requiredBuildings;

        [HideInInspector] public bool isPurchased = false;
        [SerializeField] private Button buyButton;

        public string ItemId => itemId;
        public int Cost => cost;
        public Button BuyButton => buyButton;

        public event Action<string> OnBought;
        
        public void UnlockBuildings()
        {
            foreach (var building in buildingsToUnlock)
            {
                building.Unlock();
            }
        }
        
        public void LockBuildings()
        {
            foreach (var building in buildingsToUnlock)
            {
                building.Lock();
            }
        }

        public void Warmup()
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                OnBought?.Invoke(itemId);
            });
        }
    }
}