using System;
using Interfaces;
using Models;
using Providers;
using R3;
using VContainer.Unity;

namespace Services
{
    public class AdRewardService : IInitializable, IDisposable
    {
        private readonly AdEventModel _adEventModel;
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly BoostersProvider _boostersProvider;

        private AdRewardType _adRewardType;
        private bool _isTypeSet;
        private BoostType _boostType;

        private CompositeDisposable _compositeDisposable = new();

        public AdRewardService(
            AdEventModel adEventModel,
            CoinModel coinModel,
            GemModel gemModel,
            BoostersProvider boostersProvider
        )
        {
            _adEventModel = adEventModel;
            _coinModel = coinModel;
            _gemModel = gemModel;
            _boostersProvider = boostersProvider;
        }

        public void Initialize()
        {
            _adEventModel.OnRewardedReward
                .Subscribe(GrantReward)
                .AddTo(_compositeDisposable);
        }

        public void GrantReward(Unit _)
        {
            if (!_isTypeSet) return;

            switch (_adRewardType)
            {
                case AdRewardType.Coin:
                    _coinModel.Increase(50);
                    break;
                case AdRewardType.Gem:
                    _gemModel.Increase(50);
                    break;
                case AdRewardType.Booster:
                    var booster = _boostersProvider.GetBoosterModel(_boostType);
                    booster.Add(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _isTypeSet = false;
        }

        public void SetAdRewardType(AdRewardType adRewardType, BoostType? boostType = null)
        {
            _isTypeSet = true;
            _adRewardType = adRewardType;

            if (boostType != null)
            {
                _boostType = (BoostType)boostType;
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    public enum AdRewardType
    {
        Coin,
        Gem,
        Booster
    }
}