using System;
using System.Collections.Generic;
using Configs.Tasks;
using Meta.Quests.Models;
using Models.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services.Tasks
{
    public static class TaskGenerator
    {
        public static List<TaskModel> GenerateTasks(TasksListSO config)
        {
            var result = new List<TaskModel>();
            if (!config || config.availableQuestTemplates == null || config.availableQuestTemplates.Count == 0)
                return result;

            var templates = new List<TaskSO>(config.availableQuestTemplates);
            var count = Mathf.Min(config.simultaneousQuestsCount, templates.Count);

            for (var i = 0; i < count && templates.Count > 0; i++)
            {
                var template = SelectWeightedRandom(templates);
                if (template) result.Add(CreateTask(template));
                templates.Remove(template);
            }

            return result;
        }

        private static TaskSO SelectWeightedRandom(List<TaskSO> templates)
        {
            var totalWeight = 0f;
            foreach (var t in templates)
            {
                if (t) totalWeight += t.selectionWeight;
            }

            var randomValue = Random.Range(0f, 1f) * totalWeight;
            var currentWeight = 0f;

            foreach (var t in templates)
            {
                if (!t) continue;
                currentWeight += t.selectionWeight;
                if (randomValue <= currentWeight) return t;
            }

            return templates.Count > 0 ? templates[templates.Count - 1] : null;
        }

        private static TaskModel CreateTask(TaskSO template)
        {
            var task = new TaskModel
            {
                id = Guid.NewGuid().ToString(),
                type = template.questType,
                targetAmount = Random.Range(template.amountRange.x, template.amountRange.y + 1),
                currentProgress = 0,
                isCompleted = false,
                isClaimed = false,
                sprite = template.icon,
                title = !string.IsNullOrEmpty(template.title) ? template.title : template.localizationKeyPrefix,
                reward = new QuestReward
                {
                    type = template.rewardType,
                    amount = Random.Range(template.rewardRange.x, template.rewardRange.y + 1)
                }
            };

            if (template.availableTargetIds != null && template.availableTargetIds.Count > 0)
                task.targetItemId = template.availableTargetIds[Random.Range(0, template.availableTargetIds.Count)];

            if (template.rewardType == RewardType.Boosters && template.possibleBoosterIds != null && template.possibleBoosterIds.Count > 0)
                task.reward.itemId = template.possibleBoosterIds[Random.Range(0, template.possibleBoosterIds.Count)];

            return task;
        }
    }
}
