using System.Collections;
using System.Collections.Generic;
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
        List<Item> GetRandomItems(int count);
        // Add other methods as needed
    }
}