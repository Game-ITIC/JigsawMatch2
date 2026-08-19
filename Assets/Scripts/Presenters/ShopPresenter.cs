using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer.Unity;
using Views;

namespace Presenters
{
    public class ShopPresenter : IInitializable, IDisposable
    {
        private readonly InAppView _inAppView;
        private readonly Button _shopButton;
        private readonly List<Button> _closeOrNavButtons = new();
        private readonly List<(Button Button, UnityAction Action)> _boundActions = new();

        public ShopPresenter(InAppView inAppView, Button shopButton, IEnumerable<Button> closeOrNavButtons = null)
        {
            _inAppView = inAppView;
            _shopButton = shopButton;
            if (closeOrNavButtons != null)
            {
                _closeOrNavButtons.AddRange(closeOrNavButtons.Where(b => b != null));
            }
        }

        public void Initialize()
        {
            if (_inAppView == null) return;

            if (_shopButton != null)
            {
                UnityAction openAction = OnShopButtonClicked;
                _shopButton.onClick.RemoveListener(openAction);
                _shopButton.onClick.AddListener(openAction);
                _boundActions.Add((_shopButton, openAction));
            }

            foreach (var navButton in _closeOrNavButtons)
            {
                if (navButton == null || navButton == _shopButton) continue;

                UnityAction closeAction = OnCloseOrNavButtonClicked;
                navButton.onClick.RemoveListener(closeAction);
                navButton.onClick.AddListener(closeAction);
                _boundActions.Add((navButton, closeAction));
            }
        }

        private void OnShopButtonClicked()
        {
            _inAppView.Show();
        }

        private void OnCloseOrNavButtonClicked()
        {
            _inAppView.Hide();
        }

        public void Dispose()
        {
            foreach (var (button, action) in _boundActions)
            {
                if (button != null && action != null)
                {
                    button.onClick.RemoveListener(action);
                }
            }

            _boundActions.Clear();
        }
    }
}
