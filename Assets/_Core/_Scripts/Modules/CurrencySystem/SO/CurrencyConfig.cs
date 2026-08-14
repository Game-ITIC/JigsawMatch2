using UnityEngine;

namespace Systems.CurrencySystem
{
    [CreateAssetMenu(
        fileName = "CurrencyConfig",
        menuName = "Game/Currency/Config")]
    public sealed class CurrencyConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _initialCash;
        [SerializeField, Min(0f)] private float _initialEnergy = 10f;
        [SerializeField, Min(1f)] private float _maxEnergy = 10f;
        [SerializeField, Min(0.01f)] private float _energyRecoveryIntervalSeconds = 300f;

        public float InitialCash => Mathf.Max(0f, _initialCash);
        public float InitialEnergy => Mathf.Clamp(_initialEnergy, 0f, MaxEnergy);
        public float MaxEnergy => Mathf.Max(1f, _maxEnergy);
        public float EnergyRecoveryIntervalSeconds => Mathf.Max(0.01f, _energyRecoveryIntervalSeconds);

        public static CurrencyConfig CreateRuntimeDefault()
        {
            var config = CreateInstance<CurrencyConfig>();
            config.hideFlags = HideFlags.HideAndDontSave;
            return config;
        }
    }
}
