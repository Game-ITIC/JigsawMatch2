using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Providers
{
    public class MenuNavigationProvider : MonoBehaviour
    {
        [field: SerializeField] public Canvas SceneCanvas { get; private set; }
        [field: SerializeField] public Button[] NavigationButtons { get; private set; }
        [field: SerializeField] public RectTransform PanelsParent { get; private set; }
        [field: SerializeField, Min(0f)] public float PanelSlideDuration { get; private set; } = 0.3f;
        [field: SerializeField, Min(0f)] public float ButtonAnimationDuration { get; private set; } = 0.2f;
        [field: SerializeField] public AnimationCurve PanelSlideCurve { get; private set; } = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
        [field: SerializeField] public AnimationCurve ButtonScaleCurve { get; private set; } = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
        [field: SerializeField] public AnimationCurve ButtonFadeCurve { get; private set; } = CurvedUIPanelAnimator.CreateDefaultFadeCurve();
    }
}
