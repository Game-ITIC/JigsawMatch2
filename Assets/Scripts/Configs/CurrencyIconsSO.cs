using UnityEngine;

namespace Configs
{
    public enum CurrencyType
    {
        Gems,
        Coins,
        Stars,
        Lives,
        UnlimitedLives,
        Bomb,
        Shovel,
        ExtraMoves
    }

    [CreateAssetMenu(fileName = nameof(CurrencyIconsSO), menuName = nameof(Configs) + "/" + nameof(CurrencyIconsSO))]
    public class CurrencyIconsSO : ScriptableObject
    {
        [Header("Currencies")]
        [SerializeField] private Sprite _gemsIcon;
        [SerializeField] private Sprite _coinsIcon;
        [SerializeField] private Sprite _starsIcon;
        [SerializeField] private Sprite _livesIcon;
        [SerializeField] private Sprite _unlimitedLivesIcon;

        [Header("Boosters")]
        [SerializeField] private Sprite _bombIcon;
        [SerializeField] private Sprite _shovelIcon;
        [SerializeField] private Sprite _extraMovesIcon;

        public Sprite GemsIcon => _gemsIcon;
        public Sprite CoinsIcon => _coinsIcon;
        public Sprite StarsIcon => _starsIcon;
        public Sprite LivesIcon => _livesIcon;
        public Sprite UnlimitedLivesIcon => _unlimitedLivesIcon;
        public Sprite BombIcon => _bombIcon;
        public Sprite ShovelIcon => _shovelIcon;
        public Sprite ExtraMovesIcon => _extraMovesIcon;

        public Sprite GetIcon(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Gems => _gemsIcon,
                CurrencyType.Coins => _coinsIcon,
                CurrencyType.Stars => _starsIcon,
                CurrencyType.Lives => _livesIcon,
                CurrencyType.UnlimitedLives => _unlimitedLivesIcon,
                CurrencyType.Bomb => _bombIcon,
                CurrencyType.Shovel => _shovelIcon,
                CurrencyType.ExtraMoves => _extraMovesIcon,
                _ => null
            };
        }

        public Sprite GetBoosterIcon(BoostType boostType)
        {
            return boostType switch
            {
                BoostType.Bomb => _bombIcon,
                BoostType.Shovel => _shovelIcon,
                BoostType.ExtraMoves => _extraMovesIcon,
                _ => null
            };
        }
    }
}
