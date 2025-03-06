namespace Core.Grid.Interfaces
{
    public interface ICell
    {
        int Row { get; }
        int Column { get; }
        bool IsEnabled { get; set; }
        CellState State { get; set; }
        int BlockHitsRemaining { get; set; }
        ITile Tile { get; set; }
    }
}