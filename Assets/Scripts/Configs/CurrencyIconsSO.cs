using UnityEngine;

namespace Configs
{
    public enum CurrencyIconType
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

        public Sprite GetIcon(CurrencyIconType type)
        {
            return type switch
            {
                CurrencyIconType.Gems => _gemsIcon,
                CurrencyIconType.Coins => _coinsIcon,
                CurrencyIconType.Stars => _starsIcon,
                CurrencyIconType.Lives => _livesIcon,
                CurrencyIconType.UnlimitedLives => _unlimitedLivesIcon,
                CurrencyIconType.Bomb => _bombIcon,
                CurrencyIconType.Shovel => _shovelIcon,
                CurrencyIconType.ExtraMoves => _extraMovesIcon,
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
