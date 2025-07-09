using System;
using Interfaces;
using Models;
using R3;
using VContainer.Unity;

namespace Services
{
    public class AdRewardService : IInitializable, IDisposable
    {
        private readonly AdEventModel _adEventModel;
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;

        private AdRewardType _adRewardType;
        private bool _isTypeSet;

        private CompositeDisposable _compositeDisposable = new();

        public AdRewardService(
            AdEventModel adEventModel,
            CoinModel coinModel,
            GemModel gemModel
        )
        {
            _adEventModel = adEventModel;
            _coinModel = coinModel;
            _gemModel = gemModel;
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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _isTypeSet = false;
        }

        public void SetAdRewardType(AdRewardType adRewardType)
        {
            _isTypeSet = true;
            _adRewardType = adRewardType;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    public enum AdRewardType
    {
        Coin,
        Gem
    }
}