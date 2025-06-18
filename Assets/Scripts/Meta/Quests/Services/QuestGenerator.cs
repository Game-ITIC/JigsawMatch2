using System;
using System.Collections.Generic;
using Meta.Quests.Configs;
using Meta.Quests.Interfaces;
using Meta.Quests.Models;
using Random = UnityEngine.Random;

namespace Meta.Quests.Services
{
    public class QuestGenerator : IQuestGenerator
    {
        private readonly DailyQuestSettings _settings;

        public QuestGenerator(DailyQuestSettings settings)
        {
            _settings = settings;
        }

        public List<DailyQuest> GenerateQuests(int count)
        {
            var quests = new List<DailyQuest>();
            var availableTemplates = new List<QuestTemplate>(_settings.availableQuestTemplates);

            for (int i = 0; i < count && availableTemplates.Count > 0; i++)
            {
                var template = SelectWeightedRandom(availableTemplates);
                var quest = GenerateQuestFromTemplate(template);
                quests.Add(quest);

                availableTemplates.Remove(template);
            }

            return quests;
        }

        private QuestTemplate SelectWeightedRandom(List<QuestTemplate> templates)
        {
            float totalWeight = 0f;
            foreach (var template in templates)
                totalWeight += template.selectionWeight;

            float randomValue = UnityEngine.Random.Range(0f, 1f) * totalWeight;
            float currentWeight = 0f;

            foreach (var template in templates)
            {
                currentWeight += template.selectionWeight;
                if (randomValue <= currentWeight)
                    return template;
            }

            return templates[templates.Count - 1];
        }

        private DailyQuest GenerateQuestFromTemplate(QuestTemplate template)
        {
            var quest = new DailyQuest
            {
                id = Guid.NewGuid().ToString(),
                type = template.questType,
                targetAmount = UnityEngine.Random.Range(template.minAmount, template.maxAmount + 1),
                currentProgress = 0,
                isCompleted = false
            };

            // Выбираем случайный целевой предмет
            if (template.availableTargetIds.Count > 0)
            {
                quest.targetItemId = template.availableTargetIds[Random.Range(0, template.availableTargetIds.Count)];
            }

            // Генерируем награду
            quest.reward = new QuestReward
            {
                type = template.rewardType,
                amount = Random.Range(template.minRewardAmount, template.maxRewardAmount + 1)
            };

            if (template.rewardType == RewardType.Boosters && template.possibleBoosterIds.Count > 0)
            {
                quest.reward.itemId = template.possibleBoosterIds[Random.Range(0, template.possibleBoosterIds.Count)];
            }

            // Ключ локализации
            quest.localizationKey = $"{template.localizationKeyPrefix}_{quest.targetItemId}";

            return quest;
        }
    }
}