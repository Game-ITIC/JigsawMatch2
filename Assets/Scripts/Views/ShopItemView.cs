using System;
using Cysharp.Threading.Tasks;
using Interfaces;
using Monobehaviours.Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class ShopItemView : MonoBehaviour, IPreload
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private TextView cost;
        [SerializeField] private TextView itemName;

        private ShopItem _shopItem;
        
        public Button BuyButton => buyButton;
        public ShopItem ShopItem => _shopItem;
        
        public event Action OnBuyClicked;

        public async UniTask Warmup()
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                OnBuyClicked?.Invoke(); 
                Debug.Log("clicked");
            });
        }

        public void Init(ShopItem shopItem)
        {
            _shopItem = shopItem;
            cost.SetText(shopItem.cost.ToString());
            itemName.SetText(shopItem.itemName);
        }
    }
}