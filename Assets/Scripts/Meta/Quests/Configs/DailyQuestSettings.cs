using System.Collections.Generic;
using UnityEngine;

namespace Meta.Quests.Configs
{
    [CreateAssetMenu(fileName = nameof(DailyQuestSettings), menuName = nameof(Quests.Configs) + "/" + nameof(DailyQuestSettings), order = 0)]
    public class DailyQuestSettings : ScriptableObject
    {
        [Header("General")]
        public int simultaneousQuestsCount = 3;
        public int hoursUntilRefresh = 24;
        
        [Header("Quest Templates")]
        public List<QuestTemplate> availableQuestTemplates = new List<QuestTemplate>();
        
        [Header("Debug")]
        public bool enableDebugLogs = false;
    }
}