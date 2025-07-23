using System;
using Configs;
using Interfaces;
using Models;
using Providers;
using R3;
using Systems;
using UnityEngine;
using VContainer.Unity;

namespace Services
{
    public class AdRewardService : IInitializable, IDisposable
    {
        private readonly AdEventModel _adEventModel;
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly BoostersProvider _boostersProvider;
        private readonly HealthSystem _healthSystem;
        private readonly GameConfig _gameConfig;

        private AdRewardType _adRewardType;
        private bool _isTypeSet;
        private BoostType _boostType;
        private Sprite _icon;
        private string _description;

        private CompositeDisposable _compositeDisposable = new();
        public ReactiveCommand OnContinueRewardGranted = new ReactiveCommand();

        public AdRewardService(
            AdEventModel adEventModel,
            CoinModel coinModel,
            GemModel gemModel,
            BoostersProvider boostersProvider,
            HealthSystem healthSystem,
            GameConfig gameConfig
        )
        {
            _adEventModel = adEventModel;
            _coinModel = coinModel;
            _gemModel = gemModel;
            _boostersProvider = boostersProvider;
            _healthSystem = healthSystem;
            _gameConfig = gameConfig;
        }

        public void Initialize()
        {
            _adEventModel.OnRewardedReward
                .Subscribe(GrantReward)
                .AddTo(_compositeDisposable);
        }

        public void GrantReward(Unit _)
        {
            if(!_isTypeSet) return;

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
                case AdRewardType.X2:
                    _coinModel.Increase(_gameConfig.CoinRewardForLevelPass * 2);
                    break;
                case AdRewardType.Life:
                    _healthSystem.AddLives(1);
                    break;
                case AdRewardType.Continue:
                    OnContinueRewardGranted?.Execute(Unit.Default);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _adEventModel.OnRewardGranted.OnNext(Unit.Default);
            _isTypeSet = false;
        }

        public void SetAdRewardType(AdRewardType adRewardType, BoostType? boostType = null)
        {
            _isTypeSet = true;
            _adRewardType = adRewardType;

            if(boostType != null)
            {
                _boostType = (BoostType)boostType;
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public void SetInfo(Sprite icon, string description)
        {
            _icon = icon;
            _description = description;
        }

        public RewardInfo GetInfo()
        {
            return new RewardInfo
            {
                Icon = _icon, Description = _description
            };
        }
    }

    public class RewardInfo
    {
        public Sprite Icon;
        public string Description;
    }

    public enum AdRewardType
    {
        Coin,
        Gem,
        Booster,
        X2,
        Life,
        Continue
    }
}