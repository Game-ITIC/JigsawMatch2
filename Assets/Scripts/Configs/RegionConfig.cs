using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(RegionConfig), menuName = nameof(Configs) + "/" + nameof(RegionConfig), order = 0)]
    public class RegionConfig : ScriptableObject
    {
        [field: SerializeField] public List<string> Regions { get; private set; }
    }

    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        public GameObject regionPrefab;
        public int maxLevels; 
        public Sprite regionIcon; 
    }
}