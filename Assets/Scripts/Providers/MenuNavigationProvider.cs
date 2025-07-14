using UnityEngine;
using UnityEngine.UI;

namespace Providers
{
    public class MenuNavigationProvider : MonoBehaviour
    {
        [field: SerializeField] public Canvas SceneCanvas { get; private set; }
        [field: SerializeField] public Button[] NavigationButtons { get; private set; }
        [field: SerializeField] public RectTransform PanelsParent { get; private set; }
    }
}