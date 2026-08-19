using System;
using System.Collections.Generic;
using Models.Tasks;
using UnityEngine;

namespace Services.Tasks
{
    public class TaskStorage
    {
        private const string TASK_DATA_KEY = "TaskData";
        private const string LAST_REFRESH_KEY = "TaskLastRefresh";

        [Serializable]
        private class SerializableTaskData
        {
            public List<TaskModel> tasks = new();
            public string lastRefreshTime;
        }

        public void Save(List<TaskModel> tasks, DateTime lastRefreshTime)
        {
            var data = new SerializableTaskData
            {
                tasks = tasks,
                lastRefreshTime = lastRefreshTime.ToBinary().ToString()
            };
            PlayerPrefs.SetString(TASK_DATA_KEY, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        public List<TaskModel> Load()
        {
            if (!PlayerPrefs.HasKey(TASK_DATA_KEY)) return new List<TaskModel>();
            var data = JsonUtility.FromJson<SerializableTaskData>(PlayerPrefs.GetString(TASK_DATA_KEY));
            return data?.tasks ?? new List<TaskModel>();
        }

        public DateTime GetLastRefreshTime()
        {
            if (!PlayerPrefs.HasKey(TASK_DATA_KEY)) return DateTime.MinValue;
            var data = JsonUtility.FromJson<SerializableTaskData>(PlayerPrefs.GetString(TASK_DATA_KEY));
            return long.TryParse(data?.lastRefreshTime, out var binary) ? DateTime.FromBinary(binary) : DateTime.MinValue;
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(TASK_DATA_KEY);
            PlayerPrefs.Save();
        }
    }
}
