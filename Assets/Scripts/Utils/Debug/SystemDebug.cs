using Meta.Quests.Events;
using Sirenix.OdinInspector;
using Systems;
using UnityEngine;
using VContainer;

namespace Utils.Debug
{
    public class SystemDebug : MonoBehaviour
    {
        [Inject] private HealthSystem _healthSystem;
        
        [Button("Collect Quest Item")]
        public void CollectQuestItem(string itemId, int amount)
        {
            QuestEvents.ItemCollected(itemId, amount);
        }
        [Button("Health")]
        public void Health(int i)
        {
            _healthSystem.AddLives(i);
        }
    }
}