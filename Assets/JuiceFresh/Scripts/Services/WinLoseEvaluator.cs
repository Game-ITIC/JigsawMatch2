using JuiceFresh;

namespace JuiceFresh.Scripts
{
    public enum WinLoseEvaluation
    {
        None,
        Win,
        Lose
    }

    public class WinLoseEvaluator
    {
        private readonly LevelManager _levelManager;

        public WinLoseEvaluator(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public int GetRestIngredients()
        {
            int count = 0;

            for (int i = 0; i < _levelManager.ingrTarget.Count; i++)
                count += _levelManager.ingrTarget[i].count;

            return count;
        }

        public int GetScoresOfTargetStars()
        {
            return _levelManager.RequiredStars switch
            {
                1 => _levelManager.star1,
                2 => _levelManager.star2,
                3 => _levelManager.star3,
                _ => _levelManager.star1
            };
        }

        public bool IsObjectiveCompleted()
        {
            if (_levelManager.target == Target.SCORE)
                return LevelManager.Score >= GetScoresOfTargetStars();

            if (LevelManager.Score < _levelManager.star1)
                return false;

            return _levelManager.target switch
            {
                Target.BLOCKS => _levelManager.TargetBlocks <= 0,
                Target.CAGES => _levelManager.TargetCages <= 0,
                Target.BOMBS => _levelManager.TargetBombs >= _levelManager.bombsCollect,
                Target.COLLECT or Target.ITEMS => GetRestIngredients() <= 0,
                _ => false
            };
        }

        public WinLoseEvaluation Evaluate()
        {
            if (IsObjectiveCompleted())
                return WinLoseEvaluation.Win;

            if (_levelManager.Limit <= 0)
                return WinLoseEvaluation.Lose;

            return WinLoseEvaluation.None;
        }
    }
}
