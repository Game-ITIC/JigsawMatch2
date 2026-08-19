using System;
using UnityEngine;

public class MenuActionPanel : MonoBehaviour
{
    [SerializeField] private MenuActionButton _playButton;
    [SerializeField] private MenuActionButton _buildButton;

    public MenuActionButton PlayButton
    {
        get
        {
            if (_playButton != null) return _playButton;
            _playButton = FindChildActionButton("PlayButton", "Play Button", "Play", "StartGame", "Start Game");
            return _playButton;
        }
    }

    public MenuActionButton BuildButton
    {
        get
        {
            if (_buildButton != null) return _buildButton;
            _buildButton = FindChildActionButton("BuildButton", "Build Button", "Build");
            return _buildButton;
        }
    }

    private MenuActionButton FindChildActionButton(params string[] names)
    {
        var actionButtons = GetComponentsInChildren<MenuActionButton>(true);
        foreach (var actionButton in actionButtons)
        {
            foreach (var name in names)
            {
                if (string.Equals(actionButton.gameObject.name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return actionButton;
                }
            }
        }

        if (actionButtons.Length > 0)
        {
            return actionButtons[0];
        }

        return null;
    }
}
