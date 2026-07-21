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

            ApplyShopCardLayout();
        }

        private void ApplyShopCardLayout()
        {
            var rectTransform = transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(292f, 176f);
            }

            var cardImage = GetComponent<Image>();
            if(cardImage != null)
            {
                cardImage.color = Color.white;
                cardImage.type = Image.Type.Sliced;
                cardImage.raycastTarget = false;
            }

            ConfigureName();
            ConfigureIcon();
            ConfigureAmount();
            ConfigureBuyButton();
            ConfigurePrice();
        }

        private void ConfigureName()
        {
            if(productName == null)
            {
                return;
            }

            var rectTransform = productName.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0f, -12f);
                rectTransform.sizeDelta = new Vector2(-28f, 34f);
            }

            productName.color = new Color(0.36f, 0.19f, 0.67f, 1f);
            productName.fontSize = 21f;
            productName.fontStyle = FontStyles.Bold;
            productName.alignment = TextAlignmentOptions.Center;
            productName.enableAutoSizing = true;
            productName.fontSizeMin = 14f;
            productName.fontSizeMax = 21f;
            productName.overflowMode = TextOverflowModes.Ellipsis;
            productName.enableWordWrapping = false;
            productName.raycastTarget = false;
        }

        private void ConfigureIcon()
        {
            if(image == null)
            {
                return;
            }

            var rectTransform = image.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(58f, -86f);
                rectTransform.sizeDelta = new Vector2(68f, 68f);
            }

            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        private void ConfigureAmount()
        {
            if(amount == null)
            {
                return;
            }

            var rectTransform = amount.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(34f, -48f);
                rectTransform.sizeDelta = new Vector2(-112f, 68f);
            }

            amount.color = new Color(0.48f, 0.32f, 0.72f, 1f);
            amount.fontSize = 16f;
            amount.fontStyle = FontStyles.Bold;
            amount.alignment = TextAlignmentOptions.TopLeft;
            amount.enableAutoSizing = true;
            amount.fontSizeMin = 10f;
            amount.fontSizeMax = 16f;
            amount.overflowMode = TextOverflowModes.Ellipsis;
            amount.enableWordWrapping = true;
            amount.raycastTarget = false;
        }

        private void ConfigureBuyButton()
        {
            if(buyButton == null)
            {
                return;
            }

            var rectTransform = buyButton.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 0f);
                rectTransform.pivot = new Vector2(0.5f, 0f);
                rectTransform.anchoredPosition = new Vector2(0f, 16f);
                rectTransform.sizeDelta = new Vector2(162f, 48f);
            }

            var buttonImage = buyButton.GetComponent<Image>();
            if(buttonImage != null)
            {
                buttonImage.color = Color.white;
                buttonImage.type = Image.Type.Sliced;
            }
        }

        private void ConfigurePrice()
        {
            if(price == null)
            {
                return;
            }

            var rectTransform = price.transform as RectTransform;
            if(rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            price.color = new Color(0.36f, 0.19f, 0.67f, 1f);
            price.fontSize = 24f;
            price.fontStyle = FontStyles.Bold;
            price.alignment = TextAlignmentOptions.Center;
            price.enableAutoSizing = true;
            price.fontSizeMin = 16f;
            price.fontSizeMax = 24f;
            price.overflowMode = TextOverflowModes.Ellipsis;
            price.enableWordWrapping = false;
            price.raycastTarget = false;
        }
    }
}
