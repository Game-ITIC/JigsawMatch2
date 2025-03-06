using Core.Grid.Interfaces;

namespace Core.Grid.Entities
{
    public class Cell : ICell
    {
        public int Row { get; }
        public int Column { get; }
        public CellState State { get; set; }

        public int BlockHitsRemaining { get; set; }
        public bool IsEnabled { get; set; }
        public ITile Tile { get; set; }

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}