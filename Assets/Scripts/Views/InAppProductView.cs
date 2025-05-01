using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InAppProductView : MonoBehaviour
    {
        [SerializeField] private Button buyButton;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text productName;
        [SerializeField] private TMP_Text price;
        [SerializeField] private TMP_Text amount;

        public Button BuyButton => buyButton;

        public void Init(string productName, Sprite icon, string price, string amount)
        {
            this.productName.text = productName;
            image.sprite = icon;
            this.price.text = price;
            this.amount.text = amount;
        }
    }
}