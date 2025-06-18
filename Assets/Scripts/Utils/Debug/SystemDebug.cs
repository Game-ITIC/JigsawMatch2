using Meta.Quests.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils.Debug
{
    public class SystemDebug : MonoBehaviour
    {
        [Button("Collect Quest Item")]
        public void CollectQuestItem(string itemId, int amount)
        {
            QuestEvents.ItemCollected(itemId, amount);
        }
    }
}