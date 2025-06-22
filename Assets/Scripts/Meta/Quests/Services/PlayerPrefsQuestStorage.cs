using System;
using System.Collections.Generic;
using Meta.Quests.Interfaces;
using Meta.Quests.Models;
using UnityEngine;

namespace Meta.Quests.Services
{
    public class PlayerPrefsQuestStorage : IQuestDataStorage
    {
        private const string QUEST_DATA_KEY = "DailyQuestData";
        private const string LAST_REFRESH_KEY = "DailyQuestLastRefresh";

        public void SaveQuestData(List<DailyQuest> quests, DateTime lastRefreshTime)
        {
            var data = new SerializableQuestData
            {
                quests = quests,
                lastRefreshTime = lastRefreshTime.ToBinary().ToString()
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(QUEST_DATA_KEY, json);
            PlayerPrefs.Save();
        }

        public List<DailyQuest> LoadQuestData()
        {
            if (!PlayerPrefs.HasKey(QUEST_DATA_KEY))
                return new List<DailyQuest>();

            string json = PlayerPrefs.GetString(QUEST_DATA_KEY);
            var data = JsonUtility.FromJson<SerializableQuestData>(json);
            return data?.quests ?? new List<DailyQuest>();
        }

        public DateTime GetLastRefreshTime()
        {
            if (!PlayerPrefs.HasKey(QUEST_DATA_KEY))
                return DateTime.MinValue;

            string json = PlayerPrefs.GetString(QUEST_DATA_KEY);
            var data = JsonUtility.FromJson<SerializableQuestData>(json);
            
            if (long.TryParse(data?.lastRefreshTime, out long binary))
                return DateTime.FromBinary(binary);

            return DateTime.MinValue;
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteKey(QUEST_DATA_KEY);
            PlayerPrefs.Save();
        }
    }
}