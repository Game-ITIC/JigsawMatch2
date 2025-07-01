namespace Meta.Quests.Interfaces
{
    public interface IQuestProgressTracker
    {
        void TrackItemCollected(string itemId, int amount = 1);
        void TrackLevelCompleted(bool isPerfect = false);
        void TrackItemUsed(string itemId, int amount = 1);
    }
}