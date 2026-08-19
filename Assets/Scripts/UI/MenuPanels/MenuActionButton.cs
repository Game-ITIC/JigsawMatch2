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

    public TMP_Text SubLabel
    {
        get
        {
            if (_subLabel != null) return _subLabel;
            var texts = GetComponentsInChildren<TMP_Text>(true);
            if (texts != null && texts.Length > 1)
            {
                foreach (var text in texts)
                {
                    if (text != _label)
                    {
                        _subLabel = text;
                        break;
                    }
                }
            }
            return _subLabel;
        }
    }

    public void SetLabel(string text)
    {
        var label = Label;
        if (label != null) label.text = text;
    }

    public void SetSubLabel(string text)
    {
        var subLabel = SubLabel;
        if (subLabel != null) subLabel.text = text;
    }
}
