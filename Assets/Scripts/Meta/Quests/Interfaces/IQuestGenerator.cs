using System.Collections.Generic;
using Meta.Quests.Models;

namespace Meta.Quests.Interfaces
{
    public interface IQuestGenerator
    {
        List<DailyQuest> GenerateQuests(int count);
    }
}