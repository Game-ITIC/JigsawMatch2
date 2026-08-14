using System;

namespace Systems.CurrencySystem
{
    [Serializable]
    public class CurrencyWithMax : ICurrency
    {
        public float Value { get; private set; }
        public float Max { get; private set; }

        public CurrencyWithMax(float current, float max)
        {
            Max = Math.Max(0f, max);
            Value = Math.Max(0f, Math.Min(current, Max));
        }

        public void SetCurrentWithClamp(float value) => Value = Math.Max(0f, Math.Min(value, Max));
        public void SetCurrent(float value) => Value = value;
        public void SetMax(float max)
        {
            Max = Math.Max(0f, max);
            Value = Math.Min(Value, Max);
        }
    }
}
