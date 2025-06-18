using System;
using System.Collections.Generic;
using Meta.Quests.Models;

namespace Meta.Quests.Interfaces
{
    public interface IQuestDataStorage
    {
        void SaveQuestData(List<DailyQuest> quests, DateTime lastRefreshTime);
        List<DailyQuest> LoadQuestData();
        DateTime GetLastRefreshTime();
        void ClearData();
    }
}