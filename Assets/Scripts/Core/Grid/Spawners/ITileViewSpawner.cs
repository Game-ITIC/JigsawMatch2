using Core.Grid.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Grid.Spawners
{
    public interface ITileViewSpawner
    {
        UniTask<GameObject> SpawnTileView(ICell cell, IGrid grid);
        void ReleaseTileView(GameObject tileGo);
    }
}