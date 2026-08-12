using System.Collections.Generic;
using Gley.EasyIAP;
using Sirenix.OdinInspector;
using UnityEngine;
using Views;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(InAppConfig), menuName = nameof(Configs) + "/" + nameof(InAppConfig))]
    public class InAppConfig : ScriptableObject
    {
        [field: SerializeField] public InAppProductView ProductViewPrefab { get; private set; } 
        [SerializeField] public InAppProductView ProductViewMediumPrefab;
        [SerializeField] public InAppProductView ProductViewLargePrefab;
        [TableList] [field: SerializeField] public List<InAppProduct> InAppProducts { get; private set; }
    }

    [System.Serializable]
    public struct InAppProduct
    {
        public ShopProductNames product;
        public string productName;
        public Sprite icon;
        public int amount;
        public string priceLabel;
        [TextArea] public string rewardText;
        public int gems;
        public int bombs;
        public int butterflies;
        public int extraMoves;
        public int lives;
        public int unlimitedLivesMinutes;
        public bool oneTimePurchase;
    }
}
