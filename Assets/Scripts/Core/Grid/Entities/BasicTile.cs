using Core.Grid.Interfaces;

namespace Core.Grid.Entities
{
    public class BasicTile : ITile
    {
        public string TileId { get; }

        public BasicTile(string tileId)
        {
            TileId = tileId;
        }

        public void OnRemove()
        {
            // Something happening here VFX or others.
        }
    }
}