using System.Collections.Generic;
using Meta.Quests.Models;
using UnityEngine;

namespace Meta.Quests.Configs
{
    [CreateAssetMenu(fileName = nameof(QuestTemplate), menuName = nameof(Quests.Configs) + "/" + nameof(QuestTemplate), order = 0)]
    public class QuestTemplate : ScriptableObject
    {
        public QuestType questType;
        public List<string> availableTargetIds = new List<string>();
        public int minAmount = 1;
        public int maxAmount = 10;
        public float selectionWeight = 1f;
        
        [Header("Rewards")]
        public RewardType rewardType = RewardType.Coins;
        public int minRewardAmount = 10;
        public int maxRewardAmount = 100;
        public List<string> possibleBoosterIds = new List<string>(); // Для наград-бустеров
        
        [Header("Localization")]
        public string localizationKeyPrefix = "quest_collect";
    }
}