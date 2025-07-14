using System;
using Models;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Views
{
    public class BoostShopView : MonoBehaviour
    {
        [SerializeField] private Text currentCoinsText;

        [Inject] public CoinModel CoinModel;

        private CompositeDisposable _compositeDisposable = new();
        
        private void Start()
        {   
            currentCoinsText.text = CoinModel.Coins.Value.ToString();
            
            CoinModel.Coins.Subscribe(v =>
            {
                currentCoinsText.text = v.ToString();
            }).AddTo(_compositeDisposable);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            _compositeDisposable.Dispose();
        }
    }
}