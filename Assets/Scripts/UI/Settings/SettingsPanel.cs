using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text tMP_Text;
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private SettingsToggleView _soundToggleView;
    [SerializeField] private SettingsToggleView _musicToggleView;
    [SerializeField] private SettingsToggleView _vibrationToggleView;
    [SerializeField] private SettingsToggleView _notificationToggleView;
    // [SerializeField] private Transform _settingsToggleViewParent;
}
