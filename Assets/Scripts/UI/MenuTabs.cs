using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interfaces;
using Providers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MenuTabs : IPreload
    {
        private readonly MenuNavigationProvider _menuNavigationProvider;

        private NavigationPanels _currentTab;

        public MenuTabs(
            MenuNavigationProvider menuNavigationProvider
        )
        {
            _menuNavigationProvider = menuNavigationProvider;
        }

        public async UniTask Warmup()
        {
            for (var i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var localIndex = i;
                _menuNavigationProvider.NavigationButtons[i].onClick.RemoveAllListeners();
                _menuNavigationProvider.NavigationButtons[i].onClick.AddListener(() =>
                {
                    SwitchPanel((NavigationPanels)localIndex).Forget();
                });
            }

            JumpToPanel(NavigationPanels.Main);

            await UniTask.Yield();
        }

        private async UniTask SwitchPanel(NavigationPanels navigationPanel)
        {
            if (_currentTab == navigationPanel) return;

            _currentTab = navigationPanel;

            var canvasWidth = _menuNavigationProvider.SceneCanvas.pixelRect.width;
            var targetX = -(int)navigationPanel * canvasWidth;

            AnimateButtonsVisual();

            await _menuNavigationProvider.PanelsParent
                .DOAnchorPosX(targetX, 0.3f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion()
                .AsUniTask();
        }

        private void JumpToPanel(NavigationPanels navigationPanel)
        {
            if (_currentTab == navigationPanel) return;

            _currentTab = navigationPanel;

            var canvasWidth = _menuNavigationProvider.SceneCanvas.pixelRect.width;
            var targetX = -(int)navigationPanel * canvasWidth;

            _menuNavigationProvider.PanelsParent.anchoredPosition = new Vector2(targetX, 0);
            UpdateButtonsVisual();
        }

        private void UpdateButtonsVisual()
        {
            for (int i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var button = _menuNavigationProvider.NavigationButtons[i];
                var isActive = (int)_currentTab == i;
                
                button.transform.localScale = isActive ? new Vector3(1, 1.1f, 1) : Vector3.one;

                var currentColor = button.image.color;
                button.image.color = new Color(currentColor.r, currentColor.g, currentColor.b, isActive ? 1f : 0.7f);
            }
        }

        private void AnimateButtonsVisual()
        {
            for (int i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var button = _menuNavigationProvider.NavigationButtons[i];
                var isActive = (int)_currentTab == i;

                button.transform.DOScaleY(isActive ? 1.1f : 1f, 0.2f).SetEase(Ease.OutQuad);

                var currentColor = button.image.color;
                var targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, isActive ? 1f : 0.7f);
                button.image.DOColor(targetColor, 0.2f);
            }
        }

        private enum NavigationPanels
        {
            Region,
            Main,
            Shop
        }
    }
}