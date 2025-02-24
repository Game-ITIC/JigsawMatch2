namespace Core.Grid.Interfaces
{
    public interface ICell
    {
        int Row { get; }
        int Column { get; }
        CellState State { get; set; }
        bool IsEnabled { get; set; }
        ITile Tile { get; set; }
    }
}