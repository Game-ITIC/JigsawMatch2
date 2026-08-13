using System.Collections.Generic;
using Gley.EasyIAP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Views;

namespace Configs
{
    public enum ProductColumnWidth
    {
        Small,
        Medium,
        Large
    }

    public static class ProductColumnWidthExtensions
    {
        public static int GetUnits(this ProductColumnWidth width)
        {
            switch (width)
            {
                case ProductColumnWidth.Medium: return 2;
                case ProductColumnWidth.Large: return 3;
                case ProductColumnWidth.Small:
                default: return 1;
            }
        }
    }

    [CreateAssetMenu(fileName = nameof(InAppConfig), menuName = nameof(Configs) + "/" + nameof(InAppConfig))]
    public class InAppConfig : ScriptableObject
    {
        [FormerlySerializedAs("<ProductViewPrefab>k__BackingField")]
        [SerializeField] public InAppProductView ProductViewPrefab;
        [SerializeField] public InAppProductView ProductViewMediumPrefab;
        [SerializeField] public InAppProductView ProductViewLargePrefab;
        [SerializeField] public Transform HLGParentPrefab;
        [SerializeField] public bool SmartFilling;

        [TableList]
        [FormerlySerializedAs("<InAppProducts>k__BackingField")]
        public List<InAppProduct> InAppProducts;

        public InAppProductView GetProductViewPrefab(ProductColumnWidth columnWidth)
        {
            switch (columnWidth)
            {
                case ProductColumnWidth.Medium:
                    return ProductViewMediumPrefab != null ? ProductViewMediumPrefab : ProductViewPrefab;
                case ProductColumnWidth.Large:
                    return ProductViewLargePrefab != null ? ProductViewLargePrefab : ProductViewPrefab;
                case ProductColumnWidth.Small:
                default:
                    return ProductViewPrefab;
            }
        }
    }

    [System.Serializable]
    public struct InAppProduct
    {
        public ShopProductNames product;
        public string productName;
        public ProductColumnWidth columnWidth;
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
