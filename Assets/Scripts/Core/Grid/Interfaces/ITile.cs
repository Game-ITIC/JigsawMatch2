namespace Core.Grid.Interfaces
{
    public interface ITile
    {
        string TileId { get; }
        void OnRemove();
    }
}