using Meta.Quests.Views;
using UnityEngine;

namespace Meta.Quests.Providers
{
    public class DailyQuestProvider : MonoBehaviour
    {
        [field: SerializeField] public DailyQuestView DailyQuestViewPrefab { get; private set; }
        [field: SerializeField] public Transform DailyQuestsParent { get; private set; }
    }
}