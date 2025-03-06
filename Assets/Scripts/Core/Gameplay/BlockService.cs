using Core.Grid.Interfaces;
using UnityEngine;

namespace Core.Gameplay
{
    public class BlockService
    {
        public bool HitBlockByLine(ICell cell)
        {
            if (cell == null) return false;

            switch (cell.State)
            {
                case CellState.Pink:
                case CellState.DoublePink:
                case CellState.Honey:
                    cell.BlockHitsRemaining--;
                    if (cell.BlockHitsRemaining <= 0)
                    {
                        cell.BlockHitsRemaining = 0;
                        cell.State = CellState.Normal;
                        Debug.Log($"BlockService: Removed a layer from ({cell.Row}, {cell.Column})");
                        return true;
                    }
                    else
                    {
                        UpdateStateAfterHit(cell);
                        return false;
                    }
                default:
                    return false;
            }
        }

        public bool HitBlockByAdjacent(ICell cell)
        {
            if (cell == null) return false;

            switch (cell.State)
            {
                case CellState.Blocked:
                    cell.BlockHitsRemaining--;
                    if (cell.BlockHitsRemaining <= 0)
                    {
                        cell.BlockHitsRemaining = 0;
                        cell.State = CellState.Normal;
                        return true;
                    }
                    else
                    {
                        UpdateStateAfterHit(cell);
                        return false;
                    }
                default:
                    return false;
            }
        }

        private void UpdateStateAfterHit(ICell cell)
        {
            if (cell.State == CellState.DoublePink && cell.BlockHitsRemaining == 1)
            {
                cell.State = CellState.DoublePink;
            }
            else if (cell.State == CellState.Honey && cell.BlockHitsRemaining < 5)
            {
            }
            else if (cell.State == CellState.Blocked && cell.BlockHitsRemaining > 0)
            {
            }
        }
    }
}