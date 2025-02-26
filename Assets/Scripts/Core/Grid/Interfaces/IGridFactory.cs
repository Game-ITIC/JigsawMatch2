namespace Core.Grid.Interfaces
{
    public interface IGridFactory
    {
        IGrid CreateGrid(int rows, int columns);
    }
}