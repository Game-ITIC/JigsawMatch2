using Interfaces;
using UnityEngine;

namespace Core.Grid.Interfaces
{
    public interface ITileViewFactory : IPreload
    {
        GameObject GetTileView(string tileId);
        void ReleaseTileView(GameObject tileGameObject);
    }
}