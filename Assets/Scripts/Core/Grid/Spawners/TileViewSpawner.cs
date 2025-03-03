using Core.Grid.Configs;
using Core.Grid.Interfaces;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using Interfaces;
using UnityEngine;

namespace Core.Grid.Spawners
{
    public class TileViewSpawner : ITileViewSpawner, IPreload
    {
        private readonly ITileViewFactory _tileViewFactory;
        private readonly TileViewSpawnConfig _config;
        private Transform _parentTransform;

        public TileViewSpawner(ITileViewFactory tileViewFactory,
            TileViewSpawnConfig config
        )
        {
            _tileViewFactory = tileViewFactory;
            _config = config;
        }

        public async UniTask WarmUp()
        {
            if (string.IsNullOrEmpty(_config.parentObjectName))
            {
                _config.parentObjectName = "GridParent";
            }

            var parentObj = GameObject.Find(_config.parentObjectName);
            if (!parentObj)
            {
                parentObj = new GameObject(_config.parentObjectName);
            }

            _parentTransform = parentObj.transform;

            await UniTask.Yield();
        }

        public async UniTask<GameObject> SpawnTileView(ICell cell, IGrid grid)
        {
            if (cell == null || !cell.IsEnabled || cell.Tile == null) return null;

            var tileId = cell.Tile.TileId;
            if (string.IsNullOrEmpty(tileId))
            {
                Debug.LogWarning("Tile id is empty in " + cell);
                return null;
            }

            GameObject tileGo = _tileViewFactory.GetTileView(tileId);

            tileGo.transform.SetParent(_parentTransform, false);

            var targetPos = CalculateCellPosition(cell, grid);

            tileGo.transform.localPosition = targetPos;
            tileGo.transform.localScale = Vector3.zero;

            await tileGo.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack).AsyncWaitForCompletion();

            return tileGo;
        }

        private Vector3 CalculateCellPosition(ICell cell, IGrid grid)
        {
            var x = _config.offsetX + (cell.Column * _config.cellWidth);
            var y = _config.offsetY - (cell.Row * _config.cellHeight);

            return new Vector3(x, y, 0);
        }

        public void ReleaseTileView(GameObject tileGo)
        {
            _tileViewFactory.ReleaseTileView(tileGo);
        }
    }
}