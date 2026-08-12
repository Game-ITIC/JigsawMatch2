using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InAppProductView : MonoBehaviour
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image image;
        [SerializeField] private IconWtihTextView[] _productGives;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text price;

        public Button BuyButton => buyButton;
        public IconWtihTextView[] ProductGives => _productGives;

        public void Init(string productName, Sprite icon, string price, string amount)
        {
            if (_productGives != null && _productGives.Length > 0 && _productGives[0] != null)
            {
                _productGives[0].SetText(amount);
                _productGives[0].SetImage(icon);
            }

            if (image != null && icon != null)
                image.sprite = icon;

            if (this.price != null)
                this.price.text = price;

            if (_label != null)
                _label.text = productName;
        }
    }
}
