using System;
using System.Collections.Generic;
using Configs.Tasks;
using Meta.Quests.Interfaces;
using Meta.Quests.Models;
using Random = UnityEngine.Random;

namespace Meta.Quests.Services
{
    public class QuestGenerator : IQuestGenerator
    {
        private readonly TasksListSO _settings;

        public QuestGenerator(TasksListSO settings)
        {
            _settings = settings;
        }

        public List<DailyQuest> GenerateQuests(int count)
        {
            var quests = new List<DailyQuest>();
            if (!_settings || _settings.availableQuestTemplates == null) return quests;

            var availableTemplates = new List<TaskSO>(_settings.availableQuestTemplates);

            for (var i = 0; i < count && availableTemplates.Count > 0; i++)
            {
                var template = SelectWeightedRandom(availableTemplates);
                if (template)
                {
                    var quest = GenerateQuestFromTemplate(template);
                    quests.Add(quest);
                }

                availableTemplates.Remove(template);
            }

            return quests;
        }

        private TaskSO SelectWeightedRandom(List<TaskSO> templates)
        {
            var totalWeight = 0f;
            foreach (var template in templates)
            {
                if (template) totalWeight += template.selectionWeight;
            }

            var randomValue = Random.Range(0f, 1f) * totalWeight;
            var currentWeight = 0f;

            foreach (var template in templates)
            {
                if (!template) continue;
                currentWeight += template.selectionWeight;
                if (randomValue <= currentWeight) return template;
            }

            return templates.Count > 0 ? templates[templates.Count - 1] : null;
        }

        private DailyQuest GenerateQuestFromTemplate(TaskSO template)
        {
            var quest = new DailyQuest
            {
                id = Guid.NewGuid().ToString(),
                type = template.questType,
                targetAmount = Random.Range(template.amountRange.x, template.amountRange.y + 1),
                currentProgress = 0,
                isCompleted = false,
                sprite = template.icon
            };

            if (template.availableTargetIds != null && template.availableTargetIds.Count > 0)
                quest.targetItemId = template.availableTargetIds[Random.Range(0, template.availableTargetIds.Count)];

            quest.reward = new QuestReward
            {
                type = template.rewardType,
                amount = Random.Range(template.rewardRange.x, template.rewardRange.y + 1)
            };

            if (template.rewardType == RewardType.Boosters && template.possibleBoosterIds != null && template.possibleBoosterIds.Count > 0)
                quest.reward.itemId = template.possibleBoosterIds[Random.Range(0, template.possibleBoosterIds.Count)];

            quest.localizationKey = $"{template.localizationKeyPrefix}_{quest.targetItemId}";

            return quest;
        }
    }
}