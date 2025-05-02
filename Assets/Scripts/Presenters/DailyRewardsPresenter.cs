using Gley.DailyRewards.API;
using Models;
using Providers;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;
using ZLinq;

namespace Presenters
{
    public class DailyRewardsPresenter : IInitializable
    {
        private readonly Button _dailyButton;
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly BoostersProvider _boostersProvider;

        public DailyRewardsPresenter(
            Button dailyButton,
            CoinModel coinModel,
            GemModel gemModel,
            BoostersProvider boostersProvider
            
        )
        {
            _dailyButton = dailyButton;
            _coinModel = coinModel;
            _gemModel = gemModel;
            _boostersProvider = boostersProvider;
        }

        public void Initialize()
        {
            _dailyButton.onClick.RemoveAllListeners();
            _dailyButton.onClick.AddListener(OpenCalendar);
            
            Calendar.AddClickListener(OnDayClick);
        }

        private void OnDayClick(int day, int value, Sprite icon)
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
        }


        private void OpenCalendar()
        {
            Calendar.Show();
        }
    }
}