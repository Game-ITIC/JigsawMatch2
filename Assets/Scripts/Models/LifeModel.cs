using Configs;
using Utils.Reactive;

namespace Models
{
    public class LifeModel
    {
        private readonly GameConfig _gameConfig;
        public ReactiveProperty<int> Lifes = new();

        public LifeModel(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public bool HasLifes()
        {
            return Lifes.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Lifes.Value + value;

            if (targetValue > _gameConfig.MaxLifes)
            {
                targetValue = _gameConfig.MaxLifes;
            }

            Lifes.Value = targetValue;
        }

        public void Decrease(int value)
        {
            int targetValue = Lifes.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Lifes.Value = targetValue;
        }
    }
}