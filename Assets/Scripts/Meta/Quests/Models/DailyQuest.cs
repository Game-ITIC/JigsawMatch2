using System;
using UnityEngine;

namespace Meta.Quests.Models
{
    [Serializable]
    public class DailyQuest
    {
        public string id;
        public QuestType type;
        public string targetItemId; // ID предмета/уровня
        public int targetAmount;
        public int currentProgress;
        public QuestReward reward;
        public string localizationKey;
        public bool isCompleted;

        public float ProgressNormalized => targetAmount > 0 ? (float)currentProgress / targetAmount : 0f;

        public void UpdateProgress(int newProgress)
        {
            currentProgress = Mathf.Clamp(newProgress, 0, targetAmount);
            if (currentProgress >= targetAmount && !isCompleted)
            {
                isCompleted = true;
            }
        }

        public string GetLocalizedDescription()
        {
            // Заглушка для локализации
            // В реальном проекте здесь будет вызов системы локализации
            return $"Quest_{localizationKey}";
        }
    }
}