using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuNavPanel : MonoBehaviour
{
    [SerializeField] private Button _islandButton;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _shopButton;

    public Button IslandButton
    {
        get
        {
            if (_islandButton != null) return _islandButton;
            _islandButton = FindChildButton("IslandButton", "Island Button", "Island", "RegionButton", "Region Button");
            return _islandButton;
        }
    }

    public Button MenuButton
    {
        get
        {
            if (_menuButton != null) return _menuButton;
            _menuButton = FindChildButton("MainMenuButton", "Main Button", "MainMenu Button", "MenuButton", "Menu", "Main");
            return _menuButton;
        }
    }

    public Button ShopButton
    {
        get
        {
            if (_shopButton != null) return _shopButton;
            _shopButton = FindChildButton("ShopButton", "Shop Button", "Shop", "InAppButton");
            return _shopButton;
        }
    }

    public Button[] NavigationButtons => new[] { IslandButton, MenuButton, ShopButton };

    private Button FindChildButton(params string[] names)
    {
        var buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            foreach (var name in names)
            {
                if (string.Equals(button.gameObject.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return button;
                }
            }
        }

        return null;
    }
}
