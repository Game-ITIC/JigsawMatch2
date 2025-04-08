using Utils.Reactive;

namespace Models
{
    public class StarModel
    {
        public ReactiveProperty<int> Stars = new(1);
        
        public bool HasStars()
        {
            return Stars.Value > 0;
        }

        public void Increase(int value)
        {
            int targetValue = Stars.Value + value;

            Stars.Value = targetValue;
        }

        public void Decrease(int value)
        {
            int targetValue = Stars.Value - value;

            if (targetValue <= 0)
            {
                targetValue = 0;
            }

            Stars.Value = targetValue;
        }
    }
}