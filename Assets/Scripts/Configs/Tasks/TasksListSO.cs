using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs.Tasks
{
    [CreateAssetMenu(fileName = nameof(TasksListSO), menuName = "Configs/Tasks/" + nameof(TasksListSO), order = 0)]
    public class TasksListSO : ScriptableObject
    {
        [Header("General")]
        public int simultaneousQuestsCount = 3;
        public int hoursUntilRefresh = 24;

        [Header("Task Templates")]
        [TableList] public List<TaskSO> availableQuestTemplates = new();

        [Header("Debug")]
        public bool enableDebugLogs = false;
    }
}
