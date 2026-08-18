using System;
using Meta.Quests.Models;
using UnityEngine;

namespace Models.Tasks
{
    [Serializable]
    public class TaskModel
    {
        public string id;
        public QuestType type;
        public string targetItemId;
        public int targetAmount;
        public int currentProgress;
        public QuestReward reward;
        public string title;
        public bool isCompleted;
        public bool isClaimed;
        [NonSerialized] public Sprite sprite;

        public float ProgressNormalized => targetAmount > 0 ? (float)currentProgress / targetAmount : 0f;

        public TaskModel() { }

        public void UpdateProgress(int newProgress)
        {
            currentProgress = Mathf.Clamp(newProgress, 0, targetAmount);
            if (currentProgress >= targetAmount) isCompleted = true;
        }
    }
}
