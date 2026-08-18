using Systems.CurrencySystem.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceView : MonoBehaviour, ICurrencyView
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private Image _image;
    [SerializeField] private Button _addMoreButton;

    public Button AddMoreButton => _addMoreButton;
    public Image Icon => _image;
    public TMP_Text TextComponent => _healthText;

    public void SetAmount(int amount)
    {
        if (_healthText != null)
        {
            _healthText.SetText(amount.ToString());
        }
    }

    public void SetAmount(string text)
    {
        if (_healthText != null)
        {
            _healthText.SetText(text);
        }
    }

    public void SetText(string text)
    {
        if (_healthText != null)
        {
            _healthText.SetText(text);
        }
    }
}