using System;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace Monobehaviours.Buildings
{
    [Serializable]
    public struct ShopItem
    {
        public string itemId;
        public string itemName;
        public int cost;
        public Building[] buildingsToUnlock;
        public bool isPurchased;
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

        public void SpawnBuildings()
        {
            foreach (var building in buildingsToUnlock)
            {
                building.Spawn();
            }
        }
        //
        // public void Warmup()
        // {
        //     buyButton.onClick.RemoveAllListeners();
        //     buyButton.onClick.AddListener(() =>
        //     {
        //         OnBought?.Invoke(itemId);
        //     });
        // }
    }
}