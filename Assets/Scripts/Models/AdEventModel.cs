using R3;
using Services;

namespace Models
{
    public class AdEventModel
    {
        public Subject<Unit> OnRewardedReward = new();

        public void InvokeOnRewardedReward()
        {
            OnRewardedReward.OnNext(Unit.Default);
        }

    }
}