using System.Collections.Generic;
using UnityEngine;
using Views;

namespace Providers
{
    public class IslandProvider : MonoBehaviour
    {
        [SerializeField] private List<IslandView> islandViews;

        public List<IslandView> IslandViews => islandViews;
    }
}