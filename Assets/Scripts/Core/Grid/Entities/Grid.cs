using Core.Grid.Interfaces;
using UnityEngine;

namespace Core.Grid.Entities
{
    public class Grid : IGrid
    {
        private readonly ICell[,] _cells;
        
        public int Rows { get; }
        public int Columns { get; }
        public Vector3 StartPosition { get; }
        public Vector2 CellSize { get; }

        public Grid(int rows, int columns, Vector3 startPosition, Vector2 cellSize)
        {
            Rows = rows;
            Columns = columns;
            StartPosition = startPosition;
            CellSize = cellSize;

            _cells = new ICell[rows, columns];
        }

        public void Initialize()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    _cells[r, c] = new Cell(r, c);
                }
            }
        }
        public bool IsValidIndex(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }

        public ICell GetCell(int row, int column)
        {
            if (!IsValidIndex(row, column)) return null;
            
            return _cells[row, column];
        }
    }
}