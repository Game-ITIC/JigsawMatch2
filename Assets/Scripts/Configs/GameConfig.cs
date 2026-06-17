using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(GameConfig), menuName = nameof(Configs) + "/" + nameof(GameConfig))]
    public class GameConfig : ScriptableObject
    {
        public int MaxLifes = 5;
        public float LifeRegenTimeMinutes = 20f;
        public int CoinRewardForLevelPass = 100;
        public int CoinRewardForLevelLose = 25;
        public int CoinRewardFor3StarPass = 25;
        public int AmountOfTimesToPlayToShowVideo = 3;
    }
}
