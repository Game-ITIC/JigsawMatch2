using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Interfaces;
using Providers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class MenuTabs : IPreload
    {
        private readonly MenuNavigationProvider _menuNavigationProvider;
        private readonly List<UnityAction> _navigationActions = new();

        private RectTransform _resolvedPanelsParent;
        private NavigationPanels _currentTab;

        public MenuTabs(
            MenuNavigationProvider menuNavigationProvider
        )
        {
            _menuNavigationProvider = menuNavigationProvider;
        }

        public async UniTask Warmup()
        {
            if(!CanWarmup())
            {
                await UniTask.Yield();
                return;
            }

            for (var i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var localIndex = i;
                var button = _menuNavigationProvider.NavigationButtons[i];

                while (_navigationActions.Count <= i)
                {
                    _navigationActions.Add(null);
                }

                if(button == null)
                {
                    continue;
                }

                if(_navigationActions[i] != null)
                {
                    button.onClick.RemoveListener(_navigationActions[i]);
                }

                UnityAction action = () => { SwitchPanel((NavigationPanels)localIndex).Forget(); };

                _navigationActions[i] = action;

                button.onClick.AddListener(action);
            }

            JumpToPanel(NavigationPanels.Main);

            await UniTask.Yield();
        }

        private bool CanWarmup()
        {
            return _menuNavigationProvider != null &&
                   _menuNavigationProvider.SceneCanvas != null &&
                   _menuNavigationProvider.NavigationButtons != null &&
                   _menuNavigationProvider.NavigationButtons.Length > 0;
        }

        private async UniTask SwitchPanel(NavigationPanels navigationPanel)
        {
            if(_currentTab == navigationPanel) return;

            _currentTab = navigationPanel;

            var canvasWidth = GetPanelWidth();
            var targetX = -(int)navigationPanel * canvasWidth;

            AnimateButtonsVisual();

            var panelsParent = GetPanelsParent();

            if(panelsParent != null)
            {
                panelsParent.DOKill();

                if(_menuNavigationProvider.PanelSlideDuration <= 0f)
                {
                    panelsParent.anchoredPosition = new Vector2(targetX, 0);
                    return;
                }

                await panelsParent
                    .DOAnchorPosX(targetX, _menuNavigationProvider.PanelSlideDuration)
                    .SetEase(_menuNavigationProvider.PanelSlideCurve ?? CurvedUIPanelAnimator.CreateDefaultCloseCurve())
                    .SetUpdate(true)
                    .AsyncWaitForCompletion()
                    .AsUniTask();
            }
        }

        private void JumpToPanel(NavigationPanels navigationPanel)
        {
            if(_currentTab == navigationPanel) return;

            _currentTab = navigationPanel;

            var canvasWidth = GetPanelWidth();
            var targetX = -(int)navigationPanel * canvasWidth;

            var panelsParent = GetPanelsParent();

            if(panelsParent != null)
            {
                panelsParent.DOKill();
                panelsParent.anchoredPosition = new Vector2(targetX, 0);
            }

            UpdateButtonsVisual();
        }

        private RectTransform GetPanelsParent()
        {
            if(_menuNavigationProvider.PanelsParent != null)
            {
                return _menuNavigationProvider.PanelsParent;
            }

            if(_resolvedPanelsParent != null)
            {
                return _resolvedPanelsParent;
            }

            var canvasTransform = _menuNavigationProvider.SceneCanvas.transform as RectTransform;

            if(canvasTransform == null)
            {
                return null;
            }

            _resolvedPanelsParent = FindChildRectTransform(
                canvasTransform,
                "Panels",
                "PanelsParent",
                "Menu Tabs Panels Parent");

            return _resolvedPanelsParent;
        }

        private static RectTransform FindChildRectTransform(Transform parent, params string[] names)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                for (var nameIndex = 0; nameIndex < names.Length; nameIndex++)
                {
                    if(string.Equals(child.name, names[nameIndex], System.StringComparison.OrdinalIgnoreCase))
                    {
                        return child as RectTransform;
                    }
                }

                var match = FindChildRectTransform(child, names);

                if(match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private float GetPanelWidth()
        {
            var scaler = _menuNavigationProvider.SceneCanvas.GetComponent<CanvasScaler>();

            if(scaler != null && scaler.referenceResolution.x > 0)
            {
                return scaler.referenceResolution.x;
            }

            var canvasRect = _menuNavigationProvider.SceneCanvas.transform as RectTransform;
            return canvasRect != null && canvasRect.rect.width > 0 ? canvasRect.rect.width : Screen.width;
        }

        private void UpdateButtonsVisual()
        {
            for (int i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var button = _menuNavigationProvider.NavigationButtons[i];

                if(button == null)
                {
                    continue;
                }

                var isActive = (int)_currentTab == i;

                button.transform.localScale = isActive ? new Vector3(1, 1.1f, 1) : Vector3.one;

                if(button.image != null)
                {
                    var currentColor = button.image.color;
                    button.image.color = new Color(currentColor.r, currentColor.g, currentColor.b, isActive ? 1f : 0.7f);
                }
            }
        }

        private void AnimateButtonsVisual()
        {
            for (int i = 0; i < _menuNavigationProvider.NavigationButtons.Length; i++)
            {
                var button = _menuNavigationProvider.NavigationButtons[i];

                if(button == null)
                {
                    continue;
                }

                var isActive = (int)_currentTab == i;
                var duration = _menuNavigationProvider.ButtonAnimationDuration;

                button.transform.DOKill();
                button.transform
                    .DOScaleY(isActive ? 1.1f : 1f, duration)
                    .SetEase(_menuNavigationProvider.ButtonScaleCurve ?? CurvedUIPanelAnimator.CreateDefaultOpenCurve())
                    .SetUpdate(true);

                if(button.image != null)
                {
                    var currentColor = button.image.color;
                    var targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, isActive ? 1f : 0.7f);
                    button.image.DOKill();
                    button.image
                        .DOColor(targetColor, duration)
                        .SetEase(_menuNavigationProvider.ButtonFadeCurve ?? CurvedUIPanelAnimator.CreateDefaultFadeCurve())
                        .SetUpdate(true);
                }
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