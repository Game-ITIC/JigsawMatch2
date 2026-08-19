using Systems.CurrencySystem.Interfaces;
using TMPro;
using UnityEngine;

namespace Views
{
    public class TextView : MonoBehaviour, ICurrencyView
    {
        [SerializeField] private TMP_Text text;

        public void SetAmount(int amount)
        {
            if (text != null)
            {
                text.SetText(amount.ToString());
            }
        }

        public void SetAmount(string newText)
        {
            if (text != null)
            {
                text.SetText(newText);
            }
        }

        public void SetText(string newText)
        {
            if (text != null)
            {
                text.SetText(newText);
            }
        }
    }
}