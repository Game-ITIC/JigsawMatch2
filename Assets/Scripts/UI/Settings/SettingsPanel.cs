using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private SettingsToggleView _settingsToggleViewPrefab;
    [SerializeField] private Transform _settingsToggleViewParent;
}
