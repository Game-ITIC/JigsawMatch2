using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(LevelConfig), menuName = nameof(Configs) + "/" + nameof(LevelConfig))]
    public class LevelConfig : ScriptableObject
    {
        public bool Testing;
        public int LevelToPlay;
    }
}