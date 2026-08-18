using System;
using System.Collections.Generic;
using Meta.Quests.Models;
using Models.Tasks;

namespace Services.Tasks
{
    public interface ITaskService
    {
        IReadOnlyList<TaskModel> Tasks { get; }
        event Action<IReadOnlyList<TaskModel>> OnTasksChanged;
        event Action<TaskModel> OnTaskClaimed;

        void Initialize();
        void ForceRefreshTasks();
        void ClaimTask(string taskId);
        void TrackProgress(QuestType type, string targetId = null, int amount = 1);
    }
}
