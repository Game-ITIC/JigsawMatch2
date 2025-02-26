using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Loaders;
using Core;
using Core.Grid.Interfaces;
using UnityEngine;
using Grid = Core.Grid.Entities.Grid;

namespace Services
{
    public class LevelService : ILevelService
    {
        private readonly ILevelAssetLoader _levelAssetLoader;
        private readonly IGridFactory _gridFactory;
        private readonly ITileFactory _tileFactory;
        private readonly ITileViewFactory _tileViewFactory;

        private readonly Dictionary<ICell, GameObject> _cellViews = new();
        public IGrid CurrentGrid { get; private set; }

        private GameObject _gridParent;

        public LevelService(ILevelAssetLoader levelAssetLoader,
            IGridFactory gridFactory,
            ITileFactory tileFactory,
            ITileViewFactory tileViewFactory
        )
        {
            _levelAssetLoader = levelAssetLoader;
            _gridFactory = gridFactory;
            _tileFactory = tileFactory;
            _tileViewFactory = tileViewFactory;
        }

        public async UniTask<IGrid> InitializeLevel(int levelId)
        {
            var definition = await _levelAssetLoader.LoadLevelAsync(levelId);
            
            CurrentGrid = _gridFactory.CreateGrid(definition.rows, definition.columns);

            foreach (var cd in definition.cells)
            {
                var cell = CurrentGrid.GetCell(cd.row, cd.column);
                if (cell == null) continue;

                cell.IsEnabled = cd.isEnabled;
                cell.State = cd.state;

                if (!cell.IsEnabled || cell.State != CellState.Normal)
                {
                    cell.Tile = null;
                    continue;
                }

                if (!string.IsNullOrEmpty(cd.tileId))
                {
                    cell.Tile = _tileFactory.CreateTile(cd.tileId);
                }
                else
                {
                    cell.Tile = _tileFactory.CreateRandomTile();
                }
            }

            if (!_gridParent)
            {
                _gridParent = new GameObject("GridParent");
            }

            float cellWidth = 1f;
            float cellHeight = 1f;
            float startX = -(definition.columns / 2f);
            float startY = (definition.rows / 2f);

            for (int r = 0; r < definition.rows; r++)
            {
                for (int c = 0; c < definition.columns; c++)
                {
                    var cell = CurrentGrid.GetCell(r, c);
                    if (cell == null) continue;
                    if (!cell.IsEnabled || cell.Tile == null) continue;

                    string tileId = cell.Tile.TileId;

                    GameObject tileGO = _tileViewFactory.GetTileView(tileId);

                    tileGO.transform.SetParent(_gridParent.transform, false);

                    float x = startX + c * cellWidth;
                    float y = startY - r * cellHeight;
                    tileGO.transform.localPosition = new Vector3(x, y, 0);

                    _cellViews[cell] = tileGO;
                }
            }

            return CurrentGrid;
        }

        public void ClearLevel()
        {
            foreach (var kvp in _cellViews)
            {
                var cell = kvp.Key;
                var go = kvp.Value;
                if (go != null)
                {
                    _tileViewFactory.ReleaseTileView(go);
                }

                if (cell != null)
                {
                    cell.Tile = null;
                }
            }

            _cellViews.Clear();

            if (_gridParent != null)
            {
                Object.Destroy(_gridParent);
            }

            CurrentGrid = null;
            _gridParent = null;
        }

        public GameObject GetCellView(ICell cell)
        {
            return _cellViews.GetValueOrDefault(cell);
        }

        public void UpdateCellView(ICell cell)
        {
            if (_cellViews.TryGetValue(cell, out var oldGO))
            {
                _cellViews.Remove(cell);
                if (oldGO != null)
                {
                    _tileViewFactory.ReleaseTileView(oldGO);
                }
            }

            if (!cell.IsEnabled || cell.Tile == null)
            {
                return;
            }

            string tileId = cell.Tile.TileId;
            GameObject tileGO = _tileViewFactory.GetTileView(tileId);
            tileGO.transform.SetParent(_gridParent.transform, false);

            float cellWidth = 1f;
            float cellHeight = 1f;
            float startX = -(CurrentGrid.Columns / 2f);
            float startY = (CurrentGrid.Rows / 2f);

            float x = startX + cell.Column * cellWidth;
            float y = startY - cell.Row * cellHeight;
            tileGO.transform.localPosition = new Vector3(x, y, 0);

            // Запоминаем
            _cellViews[cell] = tileGO;
        }
    }
}