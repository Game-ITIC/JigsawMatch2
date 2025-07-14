using UnityEngine;
using Views;

namespace Providers
{
    public class RegionUIProvider : MonoBehaviour
    {
        [SerializeField] private RegionUIView regionUIViewPrefab;
        [SerializeField] private Transform regionUIViewParent;

        public RegionUIView RegionUIViewPrefab => regionUIViewPrefab;
        public Transform RegionUIViewParent => regionUIViewParent;
    }
}