using System.Collections.Generic;
using Gley.EasyIAP;
using UnityEngine;
using UnityEngine.Serialization;
using Views;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(InAppConfig), menuName = nameof(Configs) + "/" + nameof(InAppConfig))]
    public class InAppConfig : ScriptableObject
    {
        [field: SerializeField] public InAppProductView ProductViewPrefab { get; private set; } 
        [field: SerializeField] public List<InAppProduct> InAppProducts { get; private set; }
    }

    [System.Serializable]
    public struct InAppProduct
    {
        public ShopProductNames product;
        public string productName;
        public Sprite icon;
        public int amount;
    }
}