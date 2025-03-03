using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Loaders;
using Core;
using Core.Grid.Interfaces;
using Core.Grid.Spawners;
using JetBrains.Annotations;
using UnityEngine;
using Grid = Core.Grid.Entities.Grid;

namespace Services
{
    [UsedImplicitly]
    public class LevelService : ILevelService
    {
        private readonly ILevelAssetLoader _levelAssetLoader;
        private readonly IGridFactory _gridFactory;
        private readonly ITileFactory _tileFactory;
        private readonly ITileViewSpawner _tileViewSpawner;

        private readonly Dictionary<ICell, GameObject> _cellViews = new();
        public IGrid CurrentGrid { get; private set; }

        private GameObject _gridParent;

        public LevelService(ILevelAssetLoader levelAssetLoader,
            IGridFactory gridFactory,
            ITileFactory tileFactory,
            ITileViewSpawner tileViewSpawner
        )
        {
            _levelAssetLoader = levelAssetLoader;
            _gridFactory = gridFactory;
            _tileFactory = tileFactory;
            _tileViewSpawner = tileViewSpawner;
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
                //TODO: Need to think about a Normal tile which created. Maybe there's already a solution.

                if (!string.IsNullOrEmpty(cd.tileId))
                {
                    cell.Tile = _tileFactory.CreateTile(cd.tileId);
                }
                else
                {
                    cell.Tile = _tileFactory.CreateRandomTile();
                }
            }

            for (int r = 0; r < definition.rows; r++)
            {
                for (int c = 0; c < definition.columns; c++)
                {
                    var cell = CurrentGrid.GetCell(r, c);
                    if (cell == null) continue;
                    if (!cell.IsEnabled || cell.Tile == null) continue;

                    var tileGo = await _tileViewSpawner.SpawnTileView(cell, CurrentGrid);
                    if (tileGo != null)
                    {
                        _cellViews[cell] = tileGo;
                    }
                }
            }

            return CurrentGrid;
        }

        public void ClearLevel()
        {
            foreach (var kvp in _cellViews)
            {
                var go = kvp.Value;
                if (go != null)
                {
                    _tileViewSpawner.ReleaseTileView(go);
                }
            }

            _cellViews.Clear();

            CurrentGrid = null;
        }

        public async UniTask UpdateCellView(ICell cell)
        {
            if (_cellViews.TryGetValue(cell, out var oldGO))
            {
                _tileViewSpawner.ReleaseTileView(oldGO);
                _cellViews.Remove(cell);
            }

            if (cell != null && cell.IsEnabled && cell.Tile != null)
            {
                var newGo = await _tileViewSpawner.SpawnTileView(cell, CurrentGrid);
                if (newGo != null)
                {
                    _cellViews[cell] = newGo;
                }
            }
        }

        public GameObject GetCellView(ICell cell)
        {
            return _cellViews.GetValueOrDefault(cell);
        }
    }
}