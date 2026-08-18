using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Meta.Quests.Interfaces;
using Meta.Quests.Models;
using Meta.Quests.Services;
using Meta.Quests.Views;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace Presenters
{
    public class DailyQuestPresenter : IInitializable, IDisposable
    {
        private readonly IDailyQuestService _questService;
        private readonly RewardService _rewardService;
        private readonly DailyQuestView _questViewPrefab;
        private readonly Transform _questsParent;

        private readonly Dictionary<string, DailyQuestView> _questViews = new();
        private readonly Dictionary<string, ReactiveDailyQuest> _reactiveQuests = new();
        private readonly Dictionary<string, CompositeDisposable> _questDisposables = new();

        private readonly CompositeDisposable _globalDisposables = new();

        public DailyQuestPresenter(
            IDailyQuestService questService,
            RewardService rewardService,
            DailyQuestView questViewPrefab = null,
            Transform questsParent = null
        )
        {
            _questService = questService;
            _rewardService = rewardService;
            _questViewPrefab = questViewPrefab;
            _questsParent = questsParent;
        }

        public void Initialize()
        {
            SubscribeToServiceEvents();
            _questService.Initialize();

            if (_questService.ActiveQuests.Count > 0 && _questViewPrefab && _questsParent)
                CreateQuestViews(_questService.ActiveQuests);
        }

        private void SubscribeToServiceEvents()
        {
            Observable.FromEvent<IReadOnlyList<DailyQuest>>(
                    handler => _questService.OnQuestsRefreshed += handler,
                    handler => _questService.OnQuestsRefreshed -= handler)
                .Subscribe(OnQuestsRefreshed)
                .AddTo(_globalDisposables);

            Observable.FromEvent<DailyQuest>(
                    handler => _questService.OnQuestCompleted += handler,
                    handler => _questService.OnQuestCompleted -= handler)
                .Subscribe(OnQuestCompleted)
                .AddTo(_globalDisposables);
        }

        private void OnQuestsRefreshed(IReadOnlyList<DailyQuest> quests)
        {
            ClearAllQuestViews();
            if (_questViewPrefab && _questsParent) CreateQuestViews(quests);
        }

        private void CreateQuestViews(IReadOnlyList<DailyQuest> quests)
        {
            foreach (var quest in quests)
                CreateQuestView(quest);
        }

        private void CreateQuestView(DailyQuest quest)
        {
            if (_questViews.ContainsKey(quest.id) || !_questViewPrefab || !_questsParent) return;

            var reactiveQuest = new ReactiveDailyQuest(quest);
            _reactiveQuests[quest.id] = reactiveQuest;

            var questView = UnityEngine.Object.Instantiate(_questViewPrefab, _questsParent);
            questView.Init(quest);
            _questViews[quest.id] = questView;

            SetupQuestBindings(reactiveQuest, questView);
        }

        private void SetupQuestBindings(ReactiveDailyQuest reactiveQuest, DailyQuestView questView)
        {
            var questDisposables = new CompositeDisposable();
            _questDisposables[reactiveQuest.id] = questDisposables;

            reactiveQuest.CurrentProgress
                .CombineLatest(reactiveQuest.ProgressNormalized, (progress, normalized) => new { progress, normalized })
                .Subscribe(data =>
                {
                    if (questView.Slider) questView.Slider.value = data.normalized;
                    if (questView.SliderText) questView.SliderText.text = $"{data.progress} / {reactiveQuest.targetAmount}";
                })
                .AddTo(questDisposables);

            reactiveQuest.IsCompleted
                .Subscribe(isCompleted =>
                {
                    if (questView.CollectButton) questView.CollectButton.interactable = isCompleted;
                    if (isCompleted) PlayQuestCompletionEffect(questView);
                })
                .AddTo(questDisposables);

            if (questView.CollectButton)
            {
                questView.CollectButton.OnClickAsObservable()
                    .Where(_ => reactiveQuest.IsCompleted.CurrentValue)
                    .Subscribe(_ => CollectQuestReward(reactiveQuest))
                    .AddTo(questDisposables);
            }

            Observable.Interval(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => SyncWithService(reactiveQuest))
                .AddTo(questDisposables);
        }

        private void SyncWithService(ReactiveDailyQuest reactiveQuest)
        {
            var serviceQuest = _questService.ActiveQuests.FirstOrDefault(q => q.id == reactiveQuest.id);
            if (serviceQuest != null && serviceQuest.currentProgress != reactiveQuest.CurrentProgress.CurrentValue)
                reactiveQuest.UpdateProgress(serviceQuest.currentProgress);
        }

        private void PlayQuestCompletionEffect(DailyQuestView questView)
        {
            const float animationDuration = 0.5f;
            Observable.Timer(TimeSpan.Zero)
                .Subscribe(_ => questView.transform.DOScale(1.1f, animationDuration).SetLoops(2, LoopType.Yoyo))
                .AddTo(_globalDisposables);
        }

        private void OnQuestCompleted(DailyQuest quest)
        {
            if (_reactiveQuests.TryGetValue(quest.id, out var reactiveQuest))
                reactiveQuest.UpdateProgress(quest.currentProgress);
        }

        private void CollectQuestReward(ReactiveDailyQuest quest)
        {
            if (!quest.IsCompleted.CurrentValue) return;

            _rewardService?.GiveReward(quest.reward);
            AnimateRewardCollection(quest.id);
        }

        private void AnimateRewardCollection(string questId)
        {
            if (_questViews.TryGetValue(questId, out var questView))
            {
                Observable.Timer(TimeSpan.FromSeconds(0.3f))
                    .Subscribe(_ => RemoveQuestView(questId))
                    .AddTo(_globalDisposables);
            }
        }

        private void RemoveQuestView(string questId)
        {
            if (_questDisposables.TryGetValue(questId, out var disposables))
            {
                disposables.Dispose();
                _questDisposables.Remove(questId);
            }

            if (_reactiveQuests.TryGetValue(questId, out var reactiveQuest))
            {
                reactiveQuest.Dispose();
                _reactiveQuests.Remove(questId);
            }

            if (_questViews.TryGetValue(questId, out var questView))
            {
                if (questView) UnityEngine.Object.Destroy(questView.gameObject);
                _questViews.Remove(questId);
            }
        }

        private void ClearAllQuestViews()
        {
            foreach (var disposables in _questDisposables.Values)
                disposables.Dispose();
            _questDisposables.Clear();

            foreach (var reactiveQuest in _reactiveQuests.Values)
                reactiveQuest.Dispose();
            _reactiveQuests.Clear();

            foreach (var questView in _questViews.Values)
            {
                if (questView) UnityEngine.Object.Destroy(questView.gameObject);
            }
            _questViews.Clear();
        }

        public void Dispose()
        {
            ClearAllQuestViews();
            _globalDisposables.Dispose();
        }
    }
}