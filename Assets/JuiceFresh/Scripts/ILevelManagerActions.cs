using System.Collections;
using UnityEngine;

namespace JuiceFresh
{
    public interface ILevelManagerActions
    {
        // Methods needed by the states
        void GenerateLevel();
        void GenerateOutline();
        void GenerateNewItems(bool falling);
        Square GetSquare(int col, int row, bool safe = false);
        IEnumerator InitBombs();

        void CreateCollectableTarget(GameObject parentTransform, Target tar, bool ForDialog = true);
        // Add other methods as needed
    }
}