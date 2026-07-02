using System;
using System.Collections.Generic;
using System.Linq;
using Gley.DailyRewards.API;
using Gley.DailyRewards.Internal;
using Models;
using Providers;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using Views;
using ZLinq;
using Object = UnityEngine.Object;

namespace Presenters
{
    public class DailyCardsPresenter : IInitializable, IDisposable
    {
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly BoostersProvider _boostersProvider;

        private readonly List<DailyCardView> _cards = new();
        private readonly CompositeDisposable _disposables = new();

        public DailyCardsPresenter(
            CoinModel coinModel,
            GemModel gemModel,
            BoostersProvider boostersProvider)
        {
            _coinModel = coinModel;
            _gemModel = gemModel;
            _boostersProvider = boostersProvider;
        }

        public void Initialize()
        {
            Calendar.AddClickListener(OnDayClaimed);
            BindCards();

            if(_cards.Count == 0)
            {
                return;
            }

            RefreshCards();

            Observable.Interval(TimeSpan.FromSeconds(0.5f))
                .Subscribe(_ => RefreshCards())
                .AddTo(_disposables);
        }

        private void BindCards()
        {
            _cards.Clear();

            var cardViews = Object.FindObjectsByType<DailyCardView>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(card => card.transform.GetSiblingIndex())
                .ToList();

            if(cardViews.Count == 0)
            {
                return;
            }

            _ = Calendar.GetRemainingTimeSpan();

            var settings = Resources.Load<DailyRewardsData>(Constants.DATA_NAME_RUNTIME);
            if(settings == null || settings.allDays == null || settings.allDays.Count == 0)
            {
                Debug.LogWarning("Daily rewards data is missing. Configure Gley Daily Rewards.");
                return;
            }

            var daysToBind = Mathf.Min(cardViews.Count, settings.allDays.Count);

            for(var i = 0; i < daysToBind; i++)
            {
                var card = cardViews[i];
                var day = settings.allDays[i];

                card.Bind(i + 1, day.dayTexture, day.rewardValue);
                WireClaimButton(card);
                _cards.Add(card);
            }
        }

        private void WireClaimButton(DailyCardView card)
        {
            if(card.ClaimButton == null)
            {
                return;
            }

            card.ClaimButton.onClick.RemoveAllListeners();
            card.ClaimButton.onClick.AddListener(() => TryClaim(card));
        }

        private void TryClaim(DailyCardView card)
        {
            var manager = CalendarManager.Instance;
            var currentDay = manager.GetCurrentDay();
            var timeExpired = manager.TimeExpired();

            if(!card.CanClaim(currentDay, timeExpired))
            {
                return;
            }

            manager.ButtonClick(card.DayNumber, card.RewardValue, card.RewardSprite);
        }

        private void RefreshCards()
        {
            if(_cards.Count == 0)
            {
                return;
            }

            var manager = CalendarManager.Instance;
            var currentDay = manager.GetCurrentDay();
            var timeExpired = manager.TimeExpired();

            foreach (var card in _cards)
            {
                card.ApplyState(currentDay, timeExpired);
            }
        }

        private void OnDayClaimed(int day, int value, Sprite icon)
        {
            switch (day)
            {
                case 1:
                    _coinModel.Increase(value);
                    break;
                case 2:
                    _gemModel.Increase(value);
                    break;
                case 3:
                    _coinModel.Increase(value);
                    break;
                case 4:
                {
                    var booster = _boostersProvider.BoostersModels
                        .AsValueEnumerable()
                        .First(v => v.Type == BoostType.Bomb);
                    booster.Add(value);
                }
                    break;
                case 5:
                {
                    var booster = _boostersProvider.BoostersModels
                        .AsValueEnumerable()
                        .First(v => v.Type == BoostType.Shovel);
                    booster.Add(value);
                }
                    break;
                case 6:
                    _coinModel.Increase(value);
                    break;
                case 7:
                    _gemModel.Increase(value);
                    break;
            }

            RefreshCards();
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
