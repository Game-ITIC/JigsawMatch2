namespace Core.Grid.Interfaces
{
    public interface ITileFactory
    {
        ITile CreateTile(string tileId);
        ITile CreateRandomTile();
    }
}