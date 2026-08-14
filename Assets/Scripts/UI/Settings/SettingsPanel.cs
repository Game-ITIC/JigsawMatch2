using System;
using System.Collections.Generic;
using TMPro;
using UI;
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

    [Header("Animation Settings")]
    [SerializeField] private bool animatePanelTransitions = true;
    [SerializeField, Min(0f)] private float panelOpenDuration = 0.28f;
    [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
    [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
    [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
    [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

    public IReadOnlyList<Button> CloseButtons => _closeButtons ?? Array.Empty<Button>();
    public SettingsToggleView SoundToggleView => _soundToggleView;
    public SettingsToggleView MusicToggleView => _musicToggleView;
    public SettingsToggleView VibrationToggleView => _vibrationToggleView;
    public SettingsToggleView NotificationToggleView => _notificationToggleView;

    public void Show()
    {
        CurvedUIPanelAnimator.Show(
            gameObject,
            animatePanelTransitions ? panelOpenDuration : 0f,
            panelOpenCurve,
            panelFadeCurve);
    }

    public void Hide()
    {
        CurvedUIPanelAnimator.Hide(
            gameObject,
            animatePanelTransitions ? panelCloseDuration : 0f,
            panelCloseCurve,
            panelFadeCurve);
    }

    public void HideImmediate()
    {
        CurvedUIPanelAnimator.HideImmediate(gameObject);
    }
}
