using System;
using System.Collections.Generic;
using Meta.Quests.Models;

namespace Meta.Quests.Interfaces
{
    public interface IDailyQuestService
    {
        IReadOnlyList<DailyQuest> ActiveQuests { get; }
        event Action<DailyQuest> OnQuestCompleted;
        event Action<IReadOnlyList<DailyQuest>> OnQuestsRefreshed;
        void Initialize();
        void ForceRefreshQuests();
        bool CheckAndRefreshIfNeeded();
    }
}