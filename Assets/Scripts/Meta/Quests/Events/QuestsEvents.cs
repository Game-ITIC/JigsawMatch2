using System;

namespace Meta.Quests.Events
{
    public static class QuestEvents
    {
        public static event Action<string, int> OnItemCollected;
        public static event Action<bool> OnLevelCompleted;
        public static event Action<string, int> OnItemUsed;

        public static void ItemCollected(string itemId, int amount = 1)
        {
            OnItemCollected?.Invoke(itemId, amount);
        }

        public static void LevelCompleted(bool isPerfect = false)
        {
            OnLevelCompleted?.Invoke(isPerfect);
        }

        public static void ItemUsed(string itemId, int amount = 1)
        {
            OnItemUsed?.Invoke(itemId, amount);
        }
    }
}