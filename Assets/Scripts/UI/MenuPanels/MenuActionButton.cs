using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuActionButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _subLabel;

    public Button Button
    {
        get
        {
            if (_button != null) return _button;
            _button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
            return _button;
        }
    }

    public TMP_Text Label
    {
        get
        {
            if (_label != null) return _label;
            _label = GetComponentInChildren<TMP_Text>(true);
            return _label;
        }
    }

    public TMP_Text SubLabel => _subLabel;

    public void SetLabel(string text)
    {
        var label = Label;
        if (label != null) label.text = text;
    }

    public void SetSubLabel(string text)
    {
        if (_subLabel != null) _subLabel.text = text;
    }
}
