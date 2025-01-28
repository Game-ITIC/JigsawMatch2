using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(IronSourceConfigSO), menuName = nameof(Configs) + "/" + nameof(IronSourceConfigSO),
        order = 0)]
    public class IronSourceConfigSO : ScriptableObject
    {
        [field: SerializeField] public string AppKey { get; private set; } = "207f4e275";
        [field: SerializeField] public string BannerId { get; private set; } = "yp2w87ryfdpbw9qr";
        [field: SerializeField] public string InterstitialId { get; private set; } = "y7hr7f67s5rjhawl";
    }
}