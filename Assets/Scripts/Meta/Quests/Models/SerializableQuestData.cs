using System;
using System.Collections.Generic;

namespace Meta.Quests.Models
{
    [Serializable]
    public class SerializableQuestData
    {
        public List<DailyQuest> quests = new List<DailyQuest>();
        public string lastRefreshTime;
    }
}