using UnityEngine;

namespace Core.Grid.Interfaces
{
    public interface IGrid
    {
        int Rows { get; }
        int Columns { get; }
        Vector3 StartPosition { get; }
        Vector2 CellSize { get; }
        bool IsValidIndex(int row, int column);
        ICell GetCell(int row, int column);
    }
}