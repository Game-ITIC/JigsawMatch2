using System;
using Meta.Quests.Models;
using R3;
using UnityEngine;

namespace Meta.Quests.Models
{
    [Serializable]
    public class ReactiveDailyQuest : IDisposable
    {
        [SerializeField] private string _id;
        [SerializeField] private QuestType _type;
        [SerializeField] private string _targetItemId;
        [SerializeField] private int _targetAmount;
        [SerializeField] private string _localizationKey;
        [SerializeField] private Sprite _sprite;
        [SerializeField] private QuestReward _reward;

        // Реактивные свойства
        private readonly ReactiveProperty<int> _currentProgress = new(0);
        private readonly ReactiveProperty<bool> _isCompleted = new(false);

        // Публичные свойства для чтения
        public string id => _id;
        public QuestType type => _type;
        public string targetItemId => _targetItemId;
        public int targetAmount => _targetAmount;
        public string localizationKey => _localizationKey;
        public Sprite sprite => _sprite;
        public QuestReward reward => _reward;

        // Реактивные свойства
        public ReadOnlyReactiveProperty<int> CurrentProgress => _currentProgress;
        public ReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;
        
        // Вычисляемое свойство для нормализованного прогресса
        public ReadOnlyReactiveProperty<float> ProgressNormalized { get; private set; }

        public ReactiveDailyQuest()
        {
            // Настраиваем вычисляемое свойство для нормализованного прогресса
            ProgressNormalized = _currentProgress
                .Select(progress => _targetAmount > 0 ? Mathf.Clamp01((float)progress / _targetAmount) : 0f)
                .ToReadOnlyReactiveProperty();
                
            // Автоматически обновляем статус завершения
            _currentProgress
                .Select(progress => progress >= _targetAmount)
                .Subscribe(completed => _isCompleted.Value = completed);
        }

        // Конструктор для создания из обычного DailyQuest
        public ReactiveDailyQuest(DailyQuest quest) : this()
        {
            _id = quest.id;
            _type = quest.type;
            _targetItemId = quest.targetItemId;
            _targetAmount = quest.targetAmount;
            _localizationKey = quest.localizationKey;
            _sprite = quest.sprite;
            _reward = quest.reward;
            
            _currentProgress.Value = quest.currentProgress;
            _isCompleted.Value = quest.isCompleted;
        }

        public void UpdateProgress(int newProgress)
        {
            _currentProgress.Value = Mathf.Max(0, Mathf.Min(newProgress, _targetAmount));
        }

        public void AddProgress(int amount)
        {
            UpdateProgress(_currentProgress.Value + amount);
        }

        // Конвертация обратно в обычный DailyQuest для сохранения
        public DailyQuest ToNonReactive()
        {
            return new DailyQuest
            {
                id = _id,
                type = _type,
                targetItemId = _targetItemId,
                targetAmount = _targetAmount,
                localizationKey = _localizationKey,
                sprite = _sprite,
                reward = _reward,
                currentProgress = _currentProgress.Value,
                isCompleted = _isCompleted.Value
            };
        }

        public string GetLocalizedDescription()
        {
            // Здесь можно использовать систему локализации
            return $"{_localizationKey}: {_currentProgress.Value}/{_targetAmount}";
        }

        public void Dispose()
        {
            _currentProgress?.Dispose();
            _isCompleted?.Dispose();
            ProgressNormalized?.Dispose();
        }
    }
}