using TMPro;
using UnityEngine;

namespace Meta.Quests.Views
{
    public class DailyQuestsView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text amount;

        public void SetAmount(int amount)
        {
            this.amount.text = amount.ToString();
        }
    }
}