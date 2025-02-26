using Core.Grid.Interfaces;

namespace Core.Grid.Entities
{
    public class ColorTile : ITile
    {
        public string TileId { get; }

        public ColorTile(string tileId)
        {
            TileId = tileId;
        }
        public void OnRemove()
        {
            
        }
    }
}