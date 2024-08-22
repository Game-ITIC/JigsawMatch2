using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum BoostType
{
    Shovel,
    Energy, 
    Bomb,
    ExtraMoves
}

public class BoostShop : MonoBehaviour
{
    public Sprite[] icons;
    public string[] descriptions;
    public int[] prices;
    public Image icon;
    public Text description;

    BoostType boostType;

    public List<BoostProduct> boostProducts = new List<BoostProduct>();

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
    }

    // Update is called once per frame
    public void SetBoost(BoostType _boostType)
    {
        boostType = _boostType;
        gameObject.SetActive(true);
        //icon.sprite = boostProducts[(int)_boostType].icon;
        //description.text = boostProducts[(int)_boostType].description;
        //for (int i = 0; i < 1; i++)
        //{
        //    transform.Find("Image/BuyBoost" + (i + 1) + "/Count").GetComponent<Text>().text = "x" + boostProducts[(int)_boostType].count[i];
        //    transform.Find("Image/BuyBoost" + (i + 1) + "/Price").GetComponent<Text>().text = "" + boostProducts[(int)_boostType].GemPrices[i];
        //}
        var boostProduct = boostProducts[(int)_boostType];
        icon.sprite = boostProduct.icon;
        description.text = boostProduct.description;
        var countTextTransform = transform.Find("Image/BuyBoost1/Count");
        var priceTextTransform = transform.Find("Image/BuyBoost1/Price");

        if (countTextTransform != null)
        {
            var countText = countTextTransform.GetComponent<Text>();
            if (countText != null)
            {
                countText.text = "x" + boostProduct.count[0]; // Access the first element
            }
        }

        if (priceTextTransform != null)
        {
            var priceText = priceTextTransform.GetComponent<Text>();
            if (priceText != null)
            {
                priceText.text = "" + boostProduct.GemPrices[0]; // Access the first element
            }
        }
    }

    public void BuyBoost(GameObject button)
    {
        int count = int.Parse(button.transform.Find("Count").GetComponent<Text>().text.Replace("x", ""));
        int price = int.Parse(button.transform.Find("Price").GetComponent<Text>().text);
        GetComponent<AnimationManager>().BuyBoost(boostType, price, count);
    }
}

[System.Serializable]
public class BoostProduct
{
    public Sprite icon;
    public string description;
    public int[] count;
    public int[] GemPrices;
}