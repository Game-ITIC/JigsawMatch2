using System;

namespace Meta.Quests.Models
{
    [Serializable]
    public class QuestReward
    {
        public RewardType type;
        public string itemId; // Для бустеров
        public int amount;
    }
}