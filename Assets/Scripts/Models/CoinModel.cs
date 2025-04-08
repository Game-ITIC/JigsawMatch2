using Utils.Reactive;

namespace Models
{
    public class CoinModel
    {
        public ReactiveProperty<int> Coins = new();

        public bool HasCoins()
        {
            return Coins.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Coins.Value + value;

            Coins.Value = targetValue;
        }

        public void Decrease(int value)
        {
            int targetValue = Coins.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Coins.Value = targetValue;
        }
    }
}