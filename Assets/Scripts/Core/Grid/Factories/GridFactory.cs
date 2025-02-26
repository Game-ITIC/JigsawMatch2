using Core.Grid.Interfaces;
using UnityEngine;

namespace Core.Grid.Factories
{
    public class GridFactory : IGridFactory
    {
        public IGrid CreateGrid(int rows, int columns)
        {
            var grid = new Entities.Grid(rows, columns, Vector3.zero, Vector3.one);
            grid.Initialize();
            return grid;
        }
    }
}