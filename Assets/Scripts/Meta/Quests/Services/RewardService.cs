using Meta.Quests.Models;
using Models;
using Providers;
using ZLinq;

namespace Meta.Quests.Services
{
    public class RewardService
    {
        private readonly CoinModel _coinModel;
        private readonly GemModel _gemModel;
        private readonly BoostersProvider _boostersProvider;

        public RewardService(
            CoinModel coinModel,
            GemModel gemModel,
            BoostersProvider boostersProvider
        )
        {
            _coinModel = coinModel;
            _gemModel = gemModel;
            _boostersProvider = boostersProvider;
        }

        public void GiveReward(QuestReward reward)
        {
            switch (reward.type)
            {
                case RewardType.Coins:
                    _coinModel.Increase(reward.amount);
                    break;
                case RewardType.Gems:
                    _gemModel.Increase(reward.amount);
                    break;
                case RewardType.Boosters:
                    // _boostersProvider.BoostersModels
                        // .AsValueEnumerable()
                        // .First(v => v.Type == reward.)
                    break;
            }
        }
    }
}