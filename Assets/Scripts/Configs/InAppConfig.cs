using System.Collections.Generic;
using Gley.EasyIAP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Views;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(InAppConfig), menuName = nameof(Configs) + "/" + nameof(InAppConfig))]
    public class InAppConfig : ScriptableObject
    {
        [FormerlySerializedAs("<ProductViewPrefab>k__BackingField")]
        [SerializeField] public InAppProductView ProductViewPrefab;
        [SerializeField] public InAppProductView ProductViewMediumPrefab;
        [SerializeField] public InAppProductView ProductViewLargePrefab;
        [SerializeField] public Transform HLGParentPrefab;

        [TableList]
        [FormerlySerializedAs("<InAppProducts>k__BackingField")]
        public List<InAppProduct> InAppProducts;
    }

    [System.Serializable]
    public struct InAppProduct
    {
        public ShopProductNames product;
        public string productName;
        public Sprite icon;
        public string priceLabel;
        public int gems;
        public int bombs;
        public int butterflies;
        public int extraMoves;
        public int lives;
        public int unlimitedLivesMinutes;
        public bool oneTimePurchase;
    }
}
