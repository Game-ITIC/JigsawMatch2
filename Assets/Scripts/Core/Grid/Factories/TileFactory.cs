using System.Collections.Generic;
using Core.Grid.Entities;
using Core.Grid.Interfaces;
using UnityEngine;

namespace Core.Grid.Factories
{
    public class TileFactory : ITileFactory
    {
        private readonly List<string> _possibleTileIds;

        public TileFactory(List<string> possibleTileIds)
        {
            _possibleTileIds = possibleTileIds;
        }

        public ITile CreateTile(string tileId)
        {
            return new ColorTile(tileId);
        }

        public ITile CreateRandomTile()
        {
            if (_possibleTileIds == null || _possibleTileIds.Count == 0)
            {
                // Fallback — если не задали
                return new ColorTile("Red");
            }

            int index = Random.Range(0, _possibleTileIds.Count);
            string tileId = _possibleTileIds[index];
            return new ColorTile(tileId);
        }
    }
}