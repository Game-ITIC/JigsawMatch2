using UnityEngine;

namespace Configs
{
    
    [CreateAssetMenu(fileName = nameof(GameConfig), menuName = nameof(Configs) + "/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        public int MaxLifes = 5;
        public int CoinRewardForLevelPass = 100;
    }
}