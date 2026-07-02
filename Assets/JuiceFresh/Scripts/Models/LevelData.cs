using System.Collections.Generic;

namespace JuiceFresh.Scripts
{
    public class LevelData
    {
        public Target Target;
        public int MaxCols;
        public int MaxRows;
        public SquareBlocks[] LevelSquares;
        public LIMIT LimitType;
        public int Limit;
        public int ColorLimit;
        public int Star1;
        public int Star2;
        public int Star3;
        public List<CollectedIngredients> IngrTarget = new List<CollectedIngredients>();
        public CollectItems[] CollectItems = new CollectItems[6];
        public int CageHp;
        public int BombsCollect;
        public int BombTimer;
        public CollectStars StarsTargetCount;
        public int TargetBlocks;
        public int TargetCages;
    }
}
